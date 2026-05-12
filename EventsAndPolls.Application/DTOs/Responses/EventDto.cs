namespace EventsAndPolls.Application.DTOs.Responses;

public class EventDto
{
     public int Id { get; set; }
     public string Title { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public DateTime StartDate { get; set; }
     public DateTime EndDate { get; set; }
     public string Location { get; set; } = string.Empty;
     public int MaxParticipants { get; set; }
     public int PollCount { get; set; }
     public DateTime CreatedAt { get; set; }
     public bool IsActive { get; set; }
     public string OrganizerId { get; set; } = string.Empty;
}