using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

// EF entity for persisting option groups
public class PollOptionGroup : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int PollId { get; private set; }

    // Navigation
    public virtual Poll Poll { get; private set; } = null!;
    public virtual ICollection<PollOption> Options { get; private set; } = new List<PollOption>();

    private PollOptionGroup() { }

    public PollOptionGroup(string name, int pollId)
    {
        Name = name;
        PollId = pollId;
    }
}
