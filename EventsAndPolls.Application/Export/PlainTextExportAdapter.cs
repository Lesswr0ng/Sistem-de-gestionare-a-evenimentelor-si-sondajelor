using System.Text;

namespace EventsAndPolls.Application.Export;

// Concrete Adapter — adapts ExportData into a plain-text report (UTF-8 bytes).
//
// In a real project this would wrap a PDF library (e.g. QuestPDF, iTextSharp)
// as the adaptee. Here we produce a formatted text report to keep the project
// free of heavy NuGet dependencies, but the adapter contract is identical —
// swap the implementation and the rest of the app is unaffected.
public class PlainTextExportAdapter : IExportAdapter
{
    public string Format => "txt";
    public string ContentType => "text/plain";
    public string FileExtension => "txt";

    public byte[] Export(ExportData data)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine($"  {data.Title}");
        sb.AppendLine($"  Exported: {data.ExportedAt:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();

        // Event section
        if (data.Event != null)
        {
            var e = data.Event;
            sb.AppendLine("EVENT DETAILS");
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine($"Title       : {e.Title}");
            sb.AppendLine($"Description : {e.Description}");
            sb.AppendLine($"Location    : {e.Location}");
            sb.AppendLine($"Starts      : {e.StartDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Ends        : {e.EndDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Capacity    : {e.MaxParticipants}");
            sb.AppendLine($"Status      : {(e.IsActive ? "Active" : "Inactive")}");
            sb.AppendLine();
        }

        // Polls section
        if (data.Polls.Any())
        {
            sb.AppendLine("POLLS");
            sb.AppendLine("-".PadRight(40, '-'));

            int pollIndex = 1;
            foreach (var poll in data.Polls)
            {
                sb.AppendLine($"{pollIndex}. {poll.Question}");
                sb.AppendLine($"   Type    : {(poll.AllowMultipleChoices ? "Multiple choice" : "Single choice")}");
                sb.AppendLine($"   Status  : {(poll.IsActive ? "Active" : "Closed")}");
                sb.AppendLine($"   Votes   : {poll.TotalVotes}");

                if (poll.ClosesAt.HasValue)
                    sb.AppendLine($"   Closes  : {poll.ClosesAt:yyyy-MM-dd HH:mm}");

                if (poll.Options.Any())
                {
                    sb.AppendLine("   Options:");
                    foreach (var opt in poll.Options)
                    {
                        var bar = BuildBar(opt.Percentage);
                        sb.AppendLine($"     [{bar}] {opt.Percentage,5:F1}%  {opt.Text} ({opt.VoteCount})");
                    }
                }

                sb.AppendLine();
                pollIndex++;
            }
        }

        sb.AppendLine("=".PadRight(60, '='));

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string BuildBar(decimal percentage)
    {
        var filled = (int)(percentage / 5); // 20 chars = 100%
        return new string('#', filled).PadRight(20);
    }
}
