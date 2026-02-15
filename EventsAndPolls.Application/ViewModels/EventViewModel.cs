namespace EventsAndPolls.Application.ViewModels;

public class EventViewModel
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
}

public class CreateEventViewModel
{
     public string Title { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public DateTime StartDate { get; set; }
     public DateTime EndDate { get; set; }
     public string Location { get; set; } = string.Empty;
     public int MaxParticipants { get; set; }
}

public class UpdateEventViewModel
{
     public string Title { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public DateTime StartDate { get; set; }
     public DateTime EndDate { get; set; }
     public string Location { get; set; } = string.Empty;
     public int MaxParticipants { get; set; }
}