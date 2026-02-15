namespace EventsAndPolls.Application.DTOs.Responses;
public class PollOptionDto
{
     public int Id { get; set; }
     public string Text { get; set; } = string.Empty;
     public int VoteCount { get; set; }
     public decimal Percentage { get; set; }
}