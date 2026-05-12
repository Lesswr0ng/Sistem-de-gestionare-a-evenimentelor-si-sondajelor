using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.ChainOfResponsibility;

// The request object that flows through the chain
public class VoteRequest
{
     public CastVoteDto Dto { get; init; } = null!;
     public string UserId { get; init; } = string.Empty;

     // Enriched by handlers as the request travels down the chain
     public DTOs.Responses.PollDto? ResolvedPoll { get; set; }
     public bool UserHasVoted { get; set; }
}

// Base handler — each handler can process the request or pass it along
public abstract class VoteHandler
{
     private VoteHandler? _next;

     public VoteHandler SetNext(VoteHandler next)
     {
          _next = next;
          return next; // allows chaining: h1.SetNext(h2).SetNext(h3)
     }

     // Each handler calls this to pass control to the next in chain
     protected Task PassToNextAsync(VoteRequest request)
     {
          if (_next != null)
               return _next.HandleAsync(request);

          return Task.CompletedTask;
     }

     public abstract Task HandleAsync(VoteRequest request);
}

// Handler 1 — resolve the poll; block if it doesn't exist
public class PollExistsHandler : VoteHandler
{
     private readonly Services.IPollService _pollService;
     private readonly ILogger<PollExistsHandler> _logger;

     public PollExistsHandler(Services.IPollService pollService, ILogger<PollExistsHandler> logger)
     {
          _pollService = pollService;
          _logger = logger;
     }

     public override async Task HandleAsync(VoteRequest request)
     {
          var poll = await _pollService.GetPollByIdAsync(request.Dto.PollId);

          if (poll == null)
          {
               _logger.LogWarning("[Chain] BLOCKED at PollExists — Poll #{PollId} not found.", request.Dto.PollId);
               throw new ArgumentException($"Poll #{request.Dto.PollId} does not exist.");
          }

          request.ResolvedPoll = poll; // enrich the request for downstream handlers
          _logger.LogInformation("[Chain] PollExists PASSED for Poll #{PollId}", request.Dto.PollId);
          await PassToNextAsync(request);
     }
}

// Handler 2 — poll must be active and not expired
public class PollActiveHandler : VoteHandler
{
     private readonly ILogger<PollActiveHandler> _logger;

     public PollActiveHandler(ILogger<PollActiveHandler> logger)
     {
          _logger = logger;
     }

     public override async Task HandleAsync(VoteRequest request)
     {
          var poll = request.ResolvedPoll!;

          if (!poll.IsActive)
          {
               _logger.LogWarning("[Chain] BLOCKED at PollActive — Poll #{PollId} is inactive.", poll.Id);
               throw new InvalidOperationException("Cannot vote on an inactive poll.");
          }

          if (poll.ClosesAt.HasValue && poll.ClosesAt.Value < DateTime.UtcNow)
          {
               _logger.LogWarning("[Chain] BLOCKED at PollActive — Poll #{PollId} closed at {ClosedAt}.",
                   poll.Id, poll.ClosesAt);
               throw new InvalidOperationException($"This poll closed on {poll.ClosesAt.Value:yyyy-MM-dd HH:mm} UTC.");
          }

          _logger.LogInformation("[Chain] PollActive PASSED for Poll #{PollId}", poll.Id);
          await PassToNextAsync(request);
     }
}

// Handler 3 — user must not have already voted
public class DuplicateVoteHandler : VoteHandler
{
     private readonly Domain.Interfaces.IVoteRepository _voteRepository;
     private readonly ILogger<DuplicateVoteHandler> _logger;

     public DuplicateVoteHandler(Domain.Interfaces.IVoteRepository voteRepository, ILogger<DuplicateVoteHandler> logger)
     {
          _voteRepository = voteRepository;
          _logger = logger;
     }

     public override async Task HandleAsync(VoteRequest request)
     {
          var hasVoted = await _voteRepository.HasUserVotedAsync(request.Dto.PollId, request.UserId);

          if (hasVoted)
          {
               _logger.LogWarning("[Chain] BLOCKED at DuplicateVote — User {UserId} already voted on Poll #{PollId}.",
                   request.UserId, request.Dto.PollId);
               throw new InvalidOperationException("You have already voted on this poll.");
          }

          request.UserHasVoted = false;
          _logger.LogInformation("[Chain] DuplicateVote PASSED for User {UserId}", request.UserId);
          await PassToNextAsync(request);
     }
}

// Handler 4 — enforce single/multiple choice rules
public class VoteLimitHandler : VoteHandler
{
     private readonly ILogger<VoteLimitHandler> _logger;

     public VoteLimitHandler(ILogger<VoteLimitHandler> logger)
     {
          _logger = logger;
     }

     public override async Task HandleAsync(VoteRequest request)
     {
          var poll = request.ResolvedPoll!;

          if (!poll.AllowMultipleChoices && request.Dto.SelectedOptionIds.Count > 1)
          {
               _logger.LogWarning("[Chain] BLOCKED at VoteLimit — Poll #{PollId} is single-choice only.", poll.Id);
               throw new InvalidOperationException("This poll only allows a single selection.");
          }

          if (request.Dto.SelectedOptionIds.Count == 0)
          {
               _logger.LogWarning("[Chain] BLOCKED at VoteLimit — No options selected for Poll #{PollId}.", poll.Id);
               throw new InvalidOperationException("You must select at least one option.");
          }

          _logger.LogInformation("[Chain] VoteLimit PASSED for Poll #{PollId}", poll.Id);
          await PassToNextAsync(request);
     }
}

// Handler 5 — all selected option IDs must actually belong to the poll
public class ValidOptionHandler : VoteHandler
{
     private readonly ILogger<ValidOptionHandler> _logger;

     public ValidOptionHandler(ILogger<ValidOptionHandler> logger)
     {
          _logger = logger;
     }

     public override async Task HandleAsync(VoteRequest request)
     {
          var poll = request.ResolvedPoll!;
          var validIds = poll.Options.Select(o => o.Id).ToHashSet();
          var invalid = request.Dto.SelectedOptionIds.Where(id => !validIds.Contains(id)).ToList();

          if (invalid.Any())
          {
               _logger.LogWarning("[Chain] BLOCKED at ValidOption — Invalid option IDs [{Ids}] for Poll #{PollId}.",
                   string.Join(", ", invalid), poll.Id);
               throw new InvalidOperationException($"Option ID(s) {string.Join(", ", invalid)} do not belong to this poll.");
          }

          _logger.LogInformation("[Chain] ValidOption PASSED for Poll #{PollId}", poll.Id);
          await PassToNextAsync(request);
     }
}

// Factory — assembles the chain in the correct order
public static class VoteHandlerChainFactory
{
     public static VoteHandler Build(
         Services.IPollService pollService,
         Domain.Interfaces.IVoteRepository voteRepository,
         ILoggerFactory loggerFactory)
     {
          var pollExists = new PollExistsHandler(pollService, loggerFactory.CreateLogger<PollExistsHandler>());
          var pollActive = new PollActiveHandler(loggerFactory.CreateLogger<PollActiveHandler>());
          var noDuplicate = new DuplicateVoteHandler(voteRepository, loggerFactory.CreateLogger<DuplicateVoteHandler>());
          var voteLimit = new VoteLimitHandler(loggerFactory.CreateLogger<VoteLimitHandler>());
          var validOption = new ValidOptionHandler(loggerFactory.CreateLogger<ValidOptionHandler>());

          // Wire up: pollExists → pollActive → noDuplicate → voteLimit → validOption
          pollExists.SetNext(pollActive).SetNext(noDuplicate).SetNext(voteLimit).SetNext(validOption);

          return pollExists; // return the head of the chain
     }
}