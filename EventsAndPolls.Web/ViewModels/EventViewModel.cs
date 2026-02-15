namespace EventsAndPolls.Web.ViewModels;

public class EventViewModel
{
     public int Id { get; set; }
     public string Title { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public DateTime StartDate { get; set; }
     public DateTime EndDate { get; set; }
     public string Location { get; set; } = string.Empty;
     public int PollCount { get; set; }
}

public class CreateEventViewModel
{
     public string Title { get; set; } = string.Empty;
     public string Description { get; set; } = string.Empty;
     public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);
     public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1).AddHours(2);
     public string Location { get; set; } = string.Empty;
     public int MaxParticipants { get; set; } = 100;
}