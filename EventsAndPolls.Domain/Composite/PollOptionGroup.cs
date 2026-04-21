namespace EventsAndPolls.Domain.Composite;

// Composite — a named group that holds children (other groups or leaf options)
public class PollOptionGroup : IPollComponent
{
    public int Id { get; }
    public string DisplayText { get; }

    private readonly List<IPollComponent> _children = new();

    public IReadOnlyList<IPollComponent> Children => _children.AsReadOnly();

    public PollOptionGroup(int id, string groupName)
    {
        Id = id;
        DisplayText = groupName;
    }

    public void Add(IPollComponent component)
    {
        _children.Add(component);
    }

    public void Remove(IPollComponent component)
    {
        _children.Remove(component);
    }

    // Recursively flattens all leaf options within this group
    public IEnumerable<IPollComponent> GetAllOptions()
    {
        foreach (var child in _children)
        foreach (var option in child.GetAllOptions())
            yield return option;
    }

    public string Render(int depth = 0)
    {
        var indent = new string(' ', depth * 2);
        var lines = new System.Text.StringBuilder();
        lines.AppendLine($"{indent}[{DisplayText}]");
        foreach (var child in _children)
            lines.AppendLine(child.Render(depth + 1));
        return lines.ToString().TrimEnd();
    }
}
