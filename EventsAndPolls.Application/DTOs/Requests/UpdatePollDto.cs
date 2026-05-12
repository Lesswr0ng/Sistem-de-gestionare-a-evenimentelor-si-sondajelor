namespace EventsAndPolls.Application.DTOs.Requests;

public class UpdatePollDto
{
     public int Id { get; set; }
     public string Question { get; set; } = string.Empty;
     public DateTime? ClosesAt { get; set; }
     public bool IsActive { get; set; }

     // Options to add (new ones, no Id yet)
     public List<string> OptionsToAdd { get; set; } = new();

     // Option Ids to delete (only those with 0 votes will be accepted)
     public List<int> OptionIdsToDelete { get; set; } = new();
}