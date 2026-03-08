using EventsAndPolls.Application.DTOs.Requests;

namespace EventsAndPolls.Application.DTOs.Responses;

public class PollDto
{
     public int Id { get; set; }
     public string Question { get; set; } = string.Empty;
     public int EventId { get; set; }
     public bool IsActive { get; set; }
     public bool AllowMultipleChoices { get; set; }
     public DateTime? ClosesAt { get; set; }
     public int TotalVotes { get; set; }
     public List<PollOptionDto> Options { get; set; } = new();
     public DateTime CreatedAt { get; set; }
}