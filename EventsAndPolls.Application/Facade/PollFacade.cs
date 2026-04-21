using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Domain.Composite;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Facade;

// Facade — hides the complexity of coordinating IPollService, IVoteService,
// IPollRepository (for tree building), and PollTreeBuilder behind simple methods.
// Controllers call one method here instead of making 3-4 service calls themselves.
public class PollFacade : IPollFacade
{
    private readonly IPollService _pollService;
    private readonly IVoteService _voteService;
    private readonly IPollRepository _pollRepository;
    private readonly ILogger<PollFacade> _logger;

    public PollFacade(
        IPollService pollService,
        IVoteService voteService,
        IPollRepository pollRepository,
        ILogger<PollFacade> logger)
    {
        _pollService = pollService;
        _voteService = voteService;
        _pollRepository = pollRepository;
        _logger = logger;
    }

    public async Task<PollCreationResult> CreateAndPublishPollAsync(CreatePollDto dto)
    {
        _logger.LogInformation("Facade: creating poll for event {EventId}", dto.EventId);

        // Step 1 — create via service (handles validation + persistence)
        var poll = await _pollService.CreatePollAsync(dto);

        // Step 2 — load the full entity to build the composite tree
        var entity = await _pollRepository.GetByIdAsync(poll.Id);
        var tree = entity != null
            ? PollTreeBuilder.BuildTree(entity).Render()
            : string.Empty;

        _logger.LogInformation("Facade: poll {PollId} created with {OptionCount} options",
            poll.Id, poll.Options.Count);

        return new PollCreationResult
        {
            Poll = poll,
            OptionTree = tree
        };
    }

    public async Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId)
    {
        _logger.LogInformation("Facade: casting vote on poll {PollId} for user {UserId}",
            dto.PollId, userId);

        // Delegates to PollService which already handles the full voting logic
        var result = await _pollService.CastVoteAsync(dto, userId);

        _logger.LogInformation("Facade: vote recorded, total votes now {TotalVotes}",
            result.TotalVotes);

        return result;
    }

    public async Task<PollSummary> GetPollSummaryAsync(int pollId)
    {
        _logger.LogInformation("Facade: building summary for poll {PollId}", pollId);

        // Coordinate multiple subsystem calls in one place
        var poll = await _pollService.GetPollResultsAsync(pollId);
        var totalVotes = await _voteService.GetVoteCountForPollAsync(pollId);

        // Build the composite tree from the full entity
        var entity = await _pollRepository.GetByIdAsync(pollId);
        var tree = entity != null
            ? PollTreeBuilder.BuildTree(entity).Render()
            : string.Empty;

        return new PollSummary
        {
            Poll = poll,
            TotalVotes = totalVotes,
            OptionTree = tree
        };
    }

    public async Task<PollDto> ClonePollAsync(ClonePollDto dto)
    {
        _logger.LogInformation("Facade: cloning poll {SourcePollId}", dto.SourcePollId);
        return await _pollService.ClonePollAsync(dto);
    }
}
