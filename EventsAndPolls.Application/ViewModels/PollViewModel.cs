namespace EventsAndPolls.Application.ViewModels;

public class PollViewModel
{
     public int Id { get; set; }
     public string Question { get; set; } = string.Empty;
     public int EventId { get; set; }
     public bool IsActive { get; set; }
     public bool AllowMultipleChoices { get; set; }
     public int TotalVotes { get; set; }
     public List<PollOptionViewModel> Options { get; set; } = new();
}

public class PollOptionViewModel
{
     public int Id { get; set; }
     public string Text { get; set; } = string.Empty;
     public int VoteCount { get; set; }
}

public class CreatePollViewModel
{
     public int EventId { get; set; }
     public string Question { get; set; } = string.Empty;
     public List<string> Options { get; set; } = new();
     public bool AllowMultipleChoices { get; set; }
}

public class VoteViewModel
{
     public int Id { get; set; }
     public string UserId { get; set; } = string.Empty;
     public int PollId { get; set; }
     public List<int> SelectedOptionIds { get; set; } = new();
     public DateTime CreatedAt { get; set; }
}

public class PollResultsViewModel
{
     public int PollId { get; set; }
     public string Question { get; set; } = string.Empty;
     public int TotalVotes { get; set; }
     public List<PollOptionResultViewModel> OptionResults { get; set; } = new();
}

public class PollOptionResultViewModel
{
     public int OptionId { get; set; }
     public string Text { get; set; } = string.Empty;
     public int VoteCount { get; set; }
     public decimal Percentage { get; set; }
}