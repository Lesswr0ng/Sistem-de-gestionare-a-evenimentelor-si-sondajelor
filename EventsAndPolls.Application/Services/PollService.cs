using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Observer;
using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;

namespace EventsAndPolls.Application.Services;

public class PollService : IPollService
{
     private readonly IPollRepository _pollRepository;
     private readonly IVoteRepository _voteRepository;
     private readonly IEventRepository _eventRepository;
     private readonly IPollEventPublisher? _publisher;


     public PollService(
         IPollRepository pollRepository,
         IVoteRepository voteRepository,
         IEventRepository eventRepository,
         IPollEventPublisher? publisher = null)
     {
          _pollRepository = pollRepository;
          _voteRepository = voteRepository;
          _eventRepository = eventRepository;
          _publisher = publisher;
     }

     public async Task<PollDto> CreatePollAsync(CreatePollDto createDto)
     {
          var poll = Poll.CreateBuilder(createDto.Question, createDto.EventId)
              .WithOptions(createDto.Options)
              .AllowMultipleSelections(createDto.AllowMultipleChoices)
              .Build();

          await _pollRepository.AddAsync(poll);

          if (_publisher != null)
               await _publisher.NotifyPollCreatedAsync(new PollCreatedEvent(
                   poll.Id, poll.Question, poll.EventId, poll.CreatedAt));

          return MapToDto(poll);
     }

     public async Task<PollDto> UpdatePollAsync(UpdatePollDto updateDto)
     {
          var poll = await _pollRepository.GetByIdAsync(updateDto.Id);
          if (poll == null)
               throw new ArgumentException($"Poll {updateDto.Id} not found");

          // ── Safe field updates ────────────────────────────────────────────
          poll.SetQuestion(updateDto.Question);
          poll.SetClosesAt(updateDto.ClosesAt);

          if (updateDto.IsActive != poll.IsActive)
          {
               if (!updateDto.IsActive) poll.Deactivate();
               else poll.Reactivate();
          }

          // ── Delete options — only those with zero votes ───────────────────
          if (updateDto.OptionIdsToDelete.Any())
          {
               foreach (var optionId in updateDto.OptionIdsToDelete)
               {
                    var voteCount = poll.Votes?.Count(v => v.PollOptionId == optionId) ?? 0;
                    if (voteCount > 0)
                         throw new InvalidOperationException(
                             $"Cannot delete option {optionId} — it has {voteCount} vote(s). " +
                             "Only options with 0 votes can be deleted.");

                    poll.RemoveOption(optionId);
               }
          }

          // ── Add new options ────────────────────────────────────────────────
          foreach (var optionText in updateDto.OptionsToAdd)
          {
               if (!string.IsNullOrWhiteSpace(optionText))
                    poll.AddOption(optionText);
          }

          await _pollRepository.UpdateAsync(poll);
          return MapToDto(poll);
     }

     public async Task<PollDto?> GetPollByIdAsync(int id)
     {
          var poll = await _pollRepository.GetByIdAsync(id);
          return poll == null ? null : MapToDto(poll);
     }

     public async Task<IEnumerable<PollDto>> GetPollsByEventAsync(int eventId)
     {
          var polls = await _pollRepository.GetPollsByEventAsync(eventId);
          return polls.Select(MapToDto);
     }

     public async Task<VoteResultDto> CastVoteAsync(CastVoteDto voteDto, string userId)
     {
          foreach (var optionId in voteDto.SelectedOptionIds)
          {
               var vote = new Vote(userId, voteDto.PollId, optionId);
               await _voteRepository.AddAsync(vote);
          }

          var totalVotes = await _voteRepository.GetVoteCountAsync(voteDto.PollId);

          if (_publisher != null)
          {
               foreach (var optionId in voteDto.SelectedOptionIds)
                    await _publisher.NotifyVoteCastAsync(new VoteCastEvent(
                        voteDto.PollId, optionId, userId, totalVotes, DateTime.UtcNow));
          }

          return new VoteResultDto
          {
               Success = true,
               Message = "Vote recorded successfully",
               PollId = voteDto.PollId,
               Timestamp = DateTime.UtcNow,
               TotalVotes = totalVotes
          };
     }

     public async Task<PollDto> GetPollResultsAsync(int pollId)
     {
          var poll = await _pollRepository.GetByIdAsync(pollId);
          if (poll == null)
               throw new ArgumentException($"Poll {pollId} not found");
          return MapToDto(poll);
     }

     public async Task DeletePollAsync(int id)
     {
          var poll = await _pollRepository.GetByIdAsync(id);
          await _pollRepository.DeleteAsync(id);

          if (_publisher != null && poll != null && poll.IsActive)
          {
               var totalVotes = await _voteRepository.GetVoteCountAsync(id);
               await _publisher.NotifyPollClosedAsync(new PollClosedEvent(
                   id, poll.Question, totalVotes, DateTime.UtcNow));
          }
     }

     public async Task<PollDto> ClonePollAsync(ClonePollDto cloneDto)
     {
          var sourcePoll = await _pollRepository.GetByIdAsync(cloneDto.SourcePollId);
          if (sourcePoll == null)
               throw new Exception("Sondaj sursă negăsit");

          Poll clonedPoll = cloneDto.DeepClone
              ? sourcePoll.DeepClone()
              : sourcePoll.Clone();

          if (cloneDto.TargetEventId.HasValue)
          {
               var targetEvent = await _eventRepository.GetByIdAsync(cloneDto.TargetEventId.Value);
               if (targetEvent == null) throw new Exception("Eveniment țintă negăsit");
               clonedPoll.SetEventId(cloneDto.TargetEventId.Value);
          }

          if (!string.IsNullOrWhiteSpace(cloneDto.NewQuestion))
               clonedPoll.SetQuestion(cloneDto.NewQuestion);

          await _pollRepository.AddAsync(clonedPoll);

          if (_publisher != null)
               await _publisher.NotifyPollCreatedAsync(new PollCreatedEvent(
                   clonedPoll.Id, clonedPoll.Question, clonedPoll.EventId, clonedPoll.CreatedAt));

          return MapToDto(clonedPoll);
     }

     private PollDto MapToDto(Poll poll)
     {
          var totalVotes = poll.Votes?.Count ?? 0;
          return new PollDto
          {
               Id = poll.Id,
               Question = poll.Question,
               EventId = poll.EventId,
               IsActive = poll.IsActive,
               AllowMultipleChoices = poll.AllowMultipleChoices,
               ClosesAt = poll.ClosesAt,
               TotalVotes = totalVotes,
               CreatedAt = poll.CreatedAt,
               Options = poll.Options?.Select(o => new PollOptionDto
               {
                    Id = o.Id,
                    Text = o.Text,
                    VoteCount = poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0,
                    Percentage = totalVotes > 0
                       ? (decimal)Math.Round(
                           (poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0) * 100.0 / totalVotes, 2)
                       : 0
               }).ToList() ?? new()
          };
     }
}