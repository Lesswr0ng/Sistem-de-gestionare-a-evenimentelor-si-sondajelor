using EventsAndPolls.Application.DTOs.Responses;

namespace EventsAndPolls.Application.Export;

// The data object passed to any export adapter.
// Adapters transform this into their target format.
public class ExportData
{
    public string Title { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    // Either an event with its polls, or a standalone poll
    public EventDto? Event { get; set; }
    public IEnumerable<PollDto> Polls { get; set; } = Enumerable.Empty<PollDto>();
}
