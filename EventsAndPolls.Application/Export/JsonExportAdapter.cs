using System.Text;
using System.Text.Json;

namespace EventsAndPolls.Application.Export;

// Concrete Adapter — adapts ExportData into JSON bytes.
// The "adaptee" here is System.Text.Json, which has its own API
// that we wrap behind the IExportAdapter interface.
public class JsonExportAdapter : IExportAdapter
{
    public string Format => "json";
    public string ContentType => "application/json";
    public string FileExtension => "json";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public byte[] Export(ExportData data)
    {
        // Adaptee call: JsonSerializer.SerializeToUtf8Bytes
        // We shape the output into a clean export envelope
        var envelope = new
        {
            exportedAt = data.ExportedAt,
            title = data.Title,
            @event = data.Event,
            polls = data.Polls.Select(p => new
            {
                p.Id,
                p.Question,
                p.IsActive,
                p.AllowMultipleChoices,
                p.TotalVotes,
                p.ClosesAt,
                options = p.Options.Select(o => new
                {
                    o.Id,
                    o.Text,
                    o.VoteCount,
                    o.Percentage
                })
            })
        };

        return JsonSerializer.SerializeToUtf8Bytes(envelope, _options);
    }
}
