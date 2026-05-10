using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;

namespace EventsAndPolls.Application.Services;

public class PollService : IPollService
{
     private readonly IPollRepository _pollRepository;
     private readonly IVoteRepository _voteRepository;
     private readonly IEventRepository _eventRepository;

     public PollService(IPollRepository pollRepository, IVoteRepository voteRepository, IEventRepository eventRepository)
     {
          _pollRepository = pollRepository;
          _voteRepository = voteRepository;
          _eventRepository = eventRepository;
     }

     public async Task<PollDto> CreatePollAsync(CreatePollDto createDto)
     {
          //factory method implementation
          /*
          PollCreator creator = createDto.AllowMultipleChoices
              ? new MultipleChoicePollCreator()
              : new SingleChoicePollCreator();

          var poll = creator.CreateAndSetupPoll(
              createDto.Question,
              createDto.EventId,
              createDto.Options) as Poll;

          if (poll == null)
               throw new Exception("Failed to create poll");

          await _pollRepository.AddAsync(poll);

          return MapToDto(poll);*/
          
          var poll = Poll.CreateBuilder(createDto.Question, createDto.EventId)
              .WithOptions(createDto.Options)
              .AllowMultipleSelections(createDto.AllowMultipleChoices)
              .Build();

          await _pollRepository.AddAsync(poll);
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
               throw new ArgumentException($"Poll with ID {pollId} not found");

          return MapToDto(poll);
     }

     public async Task DeletePollAsync(int id)
     {
          await _pollRepository.DeleteAsync(id);
     }

     // Mapping method
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
                        ? (decimal)Math.Round((poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0) * 100.0 / totalVotes, 2)
                        : 0
               }).ToList() ?? new List<PollOptionDto>()
          };
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
               if (targetEvent == null)
                    throw new Exception("Eveniment țintă negăsit");

               clonedPoll.SetEventId(cloneDto.TargetEventId.Value);
          }

          if (!string.IsNullOrWhiteSpace(cloneDto.NewQuestion))
          {
               clonedPoll.SetQuestion(cloneDto.NewQuestion);
          }

          await _pollRepository.AddAsync(clonedPoll);
          return MapToDto(clonedPoll);
     }
}
