using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Composite;

// Assembles a composite tree from a Poll's EF entities.
// This is the bridge between the persistence model and the Composite pattern.
public static class PollTreeBuilder
{
    public static IPollComponent BuildTree(Poll poll)
    {
        // Root group represents the poll itself
        var root = new PollOptionGroup(poll.Id, poll.Question);

        // Options that belong to an explicit group
        var grouped = poll.Options
            .Where(o => o.GroupId.HasValue)
            .GroupBy(o => o.GroupId!.Value);

        foreach (var group in grouped)
        {
            // Find the matching group entity on the poll
            var groupEntity = poll.OptionGroups?.FirstOrDefault(g => g.Id == group.Key);
            var groupName = groupEntity?.Name ?? $"Group {group.Key}";

            var compositeGroup = new PollOptionGroup(group.Key, groupName);
            foreach (var option in group)
            {
                var voteCount = poll.Votes?.Count(v => v.PollOptionId == option.Id) ?? 0;
                compositeGroup.Add(new PollOptionItem(option.Id, option.Text, voteCount));
            }
            root.Add(compositeGroup);
        }

        // Ungrouped options sit directly under the root
        var ungrouped = poll.Options.Where(o => !o.GroupId.HasValue);
        foreach (var option in ungrouped)
        {
            var voteCount = poll.Votes?.Count(v => v.PollOptionId == option.Id) ?? 0;
            root.Add(new PollOptionItem(option.Id, option.Text, voteCount));
        }

        return root;
    }
}
