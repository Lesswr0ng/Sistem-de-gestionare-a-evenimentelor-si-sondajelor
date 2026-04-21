namespace EventsAndPolls.Application.Export;

// Target interface — what the application knows about.
// Any export format must implement this.
public interface IExportAdapter
{
    // Human-readable format name, e.g. "json", "pdf"
    string Format { get; }

    // Exports the given data object to raw bytes
    byte[] Export(ExportData data);

    // MIME type for the HTTP response
    string ContentType { get; }

    // Suggested file extension (without dot)
    string FileExtension { get; }
}
