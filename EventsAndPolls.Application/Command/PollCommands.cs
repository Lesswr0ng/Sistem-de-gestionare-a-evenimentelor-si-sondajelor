using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Command;

// Command 1 — Create a poll (undo = delete it)
public class CreatePollCommand : ICommand
{
     private readonly IPollService _pollService;
     private readonly CreatePollDto _dto;
     private readonly ILogger<CreatePollCommand> _logger;
     private PollDto? _createdPoll;

     public string CommandName => $"CreatePoll(\"{_dto.Question}\")";

     public CreatePollCommand(IPollService pollService, CreatePollDto dto, ILogger<CreatePollCommand> logger)
     {
          _pollService = pollService;
          _dto = dto;
          _logger = logger;
     }

     public async Task ExecuteAsync()
     {
          _createdPoll = await _pollService.CreatePollAsync(_dto);
          _logger.LogInformation("[Command] Poll #{Id} created: \"{Question}\"", _createdPoll.Id, _createdPoll.Question);
     }

     public async Task UndoAsync()
     {
          if (_createdPoll == null)
          {
               _logger.LogWarning("[Command] Cannot undo CreatePoll — poll was never created.");
               return;
          }

          await _pollService.DeletePollAsync(_createdPoll.Id);
          _logger.LogInformation("[Command] Undo: Poll #{Id} deleted.", _createdPoll.Id);
          _createdPoll = null;
     }

     public PollDto? Result => _createdPoll;
}

// Command 2 — Cast a vote (undo = log that reversal is not supported in production,
//             but the slot exists and can be wired to a soft-delete vote table later)
public class CastVoteCommand : ICommand
{
     private readonly IPollService _pollService;
     private readonly CastVoteDto _dto;
     private readonly string _userId;
     private readonly ILogger<CastVoteCommand> _logger;
     private VoteResultDto? _result;

     public string CommandName => $"CastVote(Poll #{_dto.PollId}, User: {_userId})";

     public CastVoteCommand(
         IPollService pollService,
         CastVoteDto dto,
         string userId,
         ILogger<CastVoteCommand> logger)
     {
          _pollService = pollService;
          _dto = dto;
          _userId = userId;
          _logger = logger;
     }

     public async Task ExecuteAsync()
     {
          _result = await _pollService.CastVoteAsync(_dto, _userId);
          _logger.LogInformation("[Command] Vote cast on Poll #{PollId} by {UserId}. Total votes: {Total}",
              _dto.PollId, _userId, _result.TotalVotes);
     }

     public Task UndoAsync()
     {
          // Votes are intentionally hard to retract (audit trail).
          // In a real app, a soft-delete flag on the Vote entity would be flipped here.
          _logger.LogWarning(
              "[Command] Undo requested for CastVote on Poll #{PollId} by {UserId}. " +
              "Vote retraction is recorded for audit but not removed from results.",
              _dto.PollId, _userId);
          return Task.CompletedTask;
     }

     public VoteResultDto? Result => _result;
}

// Command 3 — Delete a poll (undo = not possible post-deletion without a snapshot,
//             so we store a clone DTO and recreate it)
public class DeletePollCommand : ICommand
{
     private readonly IPollService _pollService;
     private readonly int _pollId;
     private readonly ILogger<DeletePollCommand> _logger;
     private PollDto? _snapshotBeforeDelete;

     public string CommandName => $"DeletePoll(#{_pollId})";

     public DeletePollCommand(IPollService pollService, int pollId, ILogger<DeletePollCommand> logger)
     {
          _pollService = pollService;
          _pollId = pollId;
          _logger = logger;
     }

     public async Task ExecuteAsync()
     {
          // Snapshot the poll before destroying it so undo can recreate it
          _snapshotBeforeDelete = await _pollService.GetPollByIdAsync(_pollId);

          if (_snapshotBeforeDelete == null)
               throw new ArgumentException($"Poll #{_pollId} does not exist.");

          await _pollService.DeletePollAsync(_pollId);
          _logger.LogInformation("[Command] Poll #{Id} deleted. Snapshot stored for potential undo.", _pollId);
     }

     public async Task UndoAsync()
     {
          if (_snapshotBeforeDelete == null)
          {
               _logger.LogWarning("[Command] Cannot undo DeletePoll — no snapshot available.");
               return;
          }

          var recreateDto = new CreatePollDto
          {
               Question = _snapshotBeforeDelete.Question,
               EventId = _snapshotBeforeDelete.EventId,
               AllowMultipleChoices = _snapshotBeforeDelete.AllowMultipleChoices,
               ClosesAt = _snapshotBeforeDelete.ClosesAt,
               Options = _snapshotBeforeDelete.Options.Select(o => o.Text).ToList()
          };

          var restored = await _pollService.CreatePollAsync(recreateDto);
          _logger.LogInformation("[Command] Undo: Poll recreated as #{NewId} from snapshot of #{OldId}.",
              restored.Id, _pollId);
     }
}

// Command 4 — Clone a poll (undo = delete the clone)
public class ClonePollCommand : ICommand
{
     private readonly IPollService _pollService;
     private readonly ClonePollDto _dto;
     private readonly ILogger<ClonePollCommand> _logger;
     private PollDto? _clonedPoll;

     public string CommandName => $"ClonePoll(Source #{_dto.SourcePollId})";

     public ClonePollCommand(IPollService pollService, ClonePollDto dto, ILogger<ClonePollCommand> logger)
     {
          _pollService = pollService;
          _dto = dto;
          _logger = logger;
     }

     public async Task ExecuteAsync()
     {
          _clonedPoll = await _pollService.ClonePollAsync(_dto);
          _logger.LogInformation("[Command] Poll #{Source} cloned → new Poll #{Clone}",
              _dto.SourcePollId, _clonedPoll.Id);
     }

     public async Task UndoAsync()
     {
          if (_clonedPoll == null)
          {
               _logger.LogWarning("[Command] Cannot undo ClonePoll — clone was never created.");
               return;
          }

          await _pollService.DeletePollAsync(_clonedPoll.Id);
          _logger.LogInformation("[Command] Undo: Cloned Poll #{Id} deleted.", _clonedPoll.Id);
          _clonedPoll = null;
     }

     public PollDto? Result => _clonedPoll;
}