namespace EventsAndPolls.Domain.Composite;

// Component — the common interface for both leaves and composites
public interface IPollComponent
{
    int Id { get; }
    string DisplayText { get; }

    // Returns all leaf-level options (flattens groups recursively)
    IEnumerable<IPollComponent> GetAllOptions();

    // Renders the structure as an indented string (useful for debugging / display)
    string Render(int depth = 0);
}
