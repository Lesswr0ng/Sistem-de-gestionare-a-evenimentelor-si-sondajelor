using EventsAndPolls.Application.DTOs.Responses;

namespace EventsAndPolls.Application.Export;

public interface IExportService
{
    Task<ExportResult> ExportEventAsync(int eventId, string format);
    Task<ExportResult> ExportPollAsync(int pollId, string format);
}

public class ExportResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class ExportService : IExportService
{
    private readonly IEnumerable<IExportAdapter> _adapters;
    private readonly Services.IEventService _eventService;
    private readonly Services.IPollService _pollService;

    public ExportService(
        IEnumerable<IExportAdapter> adapters,
        Services.IEventService eventService,
        Services.IPollService pollService)
    {
        _adapters = adapters;
        _eventService = eventService;
        _pollService = pollService;
    }

    public async Task<ExportResult> ExportEventAsync(int eventId, string format)
    {
        var adapter = ResolveAdapter(format);

        var @event = await _eventService.GetEventByIdAsync(eventId)
            ?? throw new ArgumentException($"Event {eventId} not found");

        var polls = await _pollService.GetPollsByEventAsync(eventId);

        var data = new ExportData
        {
            Title = $"Event Report — {@event.Title}",
            Event = @event,
            Polls = polls
        };

        return new ExportResult
        {
            Data = adapter.Export(data),
            ContentType = adapter.ContentType,
            FileName = $"event-{eventId}-export.{adapter.FileExtension}"
        };
    }

    public async Task<ExportResult> ExportPollAsync(int pollId, string format)
    {
        var adapter = ResolveAdapter(format);

        var poll = await _pollService.GetPollByIdAsync(pollId)
            ?? throw new ArgumentException($"Poll {pollId} not found");

        var data = new ExportData
        {
            Title = $"Poll Report — {poll.Question}",
            Polls = new[] { poll }
        };

        return new ExportResult
        {
            Data = adapter.Export(data),
            ContentType = adapter.ContentType,
            FileName = $"poll-{pollId}-export.{adapter.FileExtension}"
        };
    }

    private IExportAdapter ResolveAdapter(string format)
    {
        var adapter = _adapters.FirstOrDefault(a =>
            a.Format.Equals(format, StringComparison.OrdinalIgnoreCase));

        if (adapter == null)
        {
            var supported = string.Join(", ", _adapters.Select(a => a.Format));
            throw new NotSupportedException(
                $"Export format '{format}' is not supported. Supported: {supported}");
        }

        return adapter;
    }
}
