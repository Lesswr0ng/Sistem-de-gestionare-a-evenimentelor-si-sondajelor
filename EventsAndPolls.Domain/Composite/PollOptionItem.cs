namespace EventsAndPolls.Domain.Composite;

// Leaf — wraps a single poll option, has no children
public class PollOptionItem : IPollComponent
{
    public int Id { get; }
    public string DisplayText { get; }
    public int VoteCount { get; }

    public PollOptionItem(int id, string text, int voteCount = 0)
    {
        Id = id;
        DisplayText = text;
        VoteCount = voteCount;
    }

    // A leaf IS the option — returns itself
    public IEnumerable<IPollComponent> GetAllOptions()
    {
        yield return this;
    }

    public string Render(int depth = 0)
    {
        var indent = new string(' ', depth * 2);
        return $"{indent}- {DisplayText} ({VoteCount} votes)";
    }
}
