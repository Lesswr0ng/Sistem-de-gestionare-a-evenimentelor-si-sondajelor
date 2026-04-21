using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Domain.Composite;

namespace EventsAndPolls.Application.Facade;

// Facade interface — exposes a simplified API over the poll subsystem.
// Controllers depend on this instead of juggling IPollService + IVoteService separately.
public interface IPollFacade
{
    // Creates a poll and immediately returns its composite option tree
    Task<PollCreationResult> CreateAndPublishPollAsync(CreatePollDto dto);

    // Casts a vote after performing all validation in one call
    Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId);

    // Returns the poll with its full results + composite tree in one shot
    Task<PollSummary> GetPollSummaryAsync(int pollId);

    // Clones a poll and returns the new one ready to use
    Task<PollDto> ClonePollAsync(ClonePollDto dto);
}

public class PollCreationResult
{
    public PollDto Poll { get; set; } = null!;
    public string OptionTree { get; set; } = string.Empty; // Rendered composite tree
}

public class PollSummary
{
    public PollDto Poll { get; set; } = null!;
    public int TotalVotes { get; set; }
    public bool HasUserVoted { get; set; }
    public string OptionTree { get; set; } = string.Empty;
}
