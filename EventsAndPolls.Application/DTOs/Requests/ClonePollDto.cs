namespace EventsAndPolls.Application.DTOs.Requests;

public class ClonePollDto
{
     public int SourcePollId { get; set; }
     public int? TargetEventId { get; set; }
     public bool DeepClone { get; set; } = true;
     public string? NewQuestion { get; set; }
}