using EventsAndPolls.Application.Export;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(IExportService exportService, ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    // GET api/export/event/5?format=json
    // GET api/export/event/5?format=txt
    [HttpGet("event/{id}")]
    public async Task<IActionResult> ExportEvent(int id, [FromQuery] string format = "json")
    {
        try
        {
            var result = await _exportService.ExportEventAsync(id, format);
            return File(result.Data, result.ContentType, result.FileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred during export" });
        }
    }

    // GET api/export/poll/3?format=txt
    [HttpGet("poll/{id}")]
    public async Task<IActionResult> ExportPoll(int id, [FromQuery] string format = "json")
    {
        try
        {
            var result = await _exportService.ExportPollAsync(id, format);
            return File(result.Data, result.ContentType, result.FileName);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting poll {PollId}", id);
            return StatusCode(500, new { error = "An error occurred during export" });
        }
    }
}
