namespace EventsAndPolls.Application.DTOs.Responses;

public class VoteResultDto
{
     public bool Success { get; set; }
     public string Message { get; set; } = string.Empty;
     public int PollId { get; set; }
     public DateTime Timestamp { get; set; }
     public int TotalVotes { get; set; }
}