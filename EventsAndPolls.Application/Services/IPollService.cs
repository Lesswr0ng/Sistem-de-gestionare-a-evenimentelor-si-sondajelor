using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Application.Services;

public interface IPollService
{
     Task<PollDto> CreatePollAsync(CreatePollDto createDto);
     Task<PollDto?> GetPollByIdAsync(int id);
     Task<IEnumerable<PollDto>> GetPollsByEventAsync(int eventId);
     Task<VoteResultDto> CastVoteAsync(CastVoteDto voteDto, string userId);
     Task<PollDto> GetPollResultsAsync(int pollId);
     Task DeletePollAsync(int id);
     Task<PollDto> ClonePollAsync(ClonePollDto cloneDto);
     Task<PollDto> UpdatePollAsync(UpdatePollDto updateDto);
}