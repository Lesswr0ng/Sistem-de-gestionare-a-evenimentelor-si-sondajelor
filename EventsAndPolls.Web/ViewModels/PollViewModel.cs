namespace EventsAndPolls.Web.ViewModels;

public class PollViewModel
{
     public int Id { get; set; }
     public string Question { get; set; } = string.Empty;
     public int EventId { get; set; }
     public bool IsActive { get; set; }
     public bool AllowMultipleChoices { get; set; }
     public List<PollOptionViewModel> Options { get; set; } = new();
     public int TotalVotes { get; set; }
}

public class PollOptionViewModel
{
     public int Id { get; set; }
     public string Text { get; set; } = string.Empty;
     public int VoteCount { get; set; }
     public bool IsSelected { get; set; }
}

public class CreatePollViewModel
{
     public int EventId { get; set; }
     public string Question { get; set; } = string.Empty;
     public List<string> Options { get; set; } = new() { "", "", "" };
     public bool AllowMultipleChoices { get; set; }
}

public class VoteViewModel
{
     public int PollId { get; set; }
     public List<int> SelectedOptionIds { get; set; } = new();
}
