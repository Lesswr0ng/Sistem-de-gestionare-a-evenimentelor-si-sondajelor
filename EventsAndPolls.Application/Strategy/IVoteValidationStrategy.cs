namespace EventsAndPolls.Application.Strategy;

// The strategy interface — every validation rule implements this
public interface IVoteValidationStrategy
{
     string StrategyName { get; }
     ValidationResult Validate(VoteValidationContext context);
}

// Context passed into every strategy — carries everything needed to make a decision
public record VoteValidationContext(
    int PollId,
    bool PollIsActive,
    DateTime? PollClosesAt,
    bool AllowMultipleChoices,
    List<int> SelectedOptionIds,
    List<int> ValidOptionIds,
    bool UserHasAlreadyVoted,
    string UserId
);

public record ValidationResult(bool IsValid, string? ErrorMessage = null)
{
     public static ValidationResult Ok() => new(true);
     public static ValidationResult Fail(string message) => new(false, message);
}

// Strategy 1 — poll must be active
public class PollActiveValidationStrategy : IVoteValidationStrategy
{
     public string StrategyName => "PollActive";

     public ValidationResult Validate(VoteValidationContext ctx)
     {
          if (!ctx.PollIsActive)
               return ValidationResult.Fail("Cannot vote on an inactive poll.");

          if (ctx.PollClosesAt.HasValue && ctx.PollClosesAt.Value < DateTime.UtcNow)
               return ValidationResult.Fail($"This poll closed on {ctx.PollClosesAt.Value:yyyy-MM-dd HH:mm} UTC.");

          return ValidationResult.Ok();
     }
}

// Strategy 2 — user must not have already voted
public class NoDuplicateVoteStrategy : IVoteValidationStrategy
{
     public string StrategyName => "NoDuplicateVote";

     public ValidationResult Validate(VoteValidationContext ctx)
     {
          if (ctx.UserHasAlreadyVoted)
               return ValidationResult.Fail("You have already voted on this poll.");

          return ValidationResult.Ok();
     }
}

// Strategy 3 — single choice: exactly one option must be selected
public class SingleChoiceValidationStrategy : IVoteValidationStrategy
{
     public string StrategyName => "SingleChoice";

     public ValidationResult Validate(VoteValidationContext ctx)
     {
          if (ctx.AllowMultipleChoices)
               return ValidationResult.Ok(); // not applicable

          if (ctx.SelectedOptionIds.Count != 1)
               return ValidationResult.Fail("This poll requires exactly one selection.");

          return ValidationResult.Ok();
     }
}

// Strategy 4 — multiple choice: at least one option, no duplicates in selection
public class MultipleChoiceValidationStrategy : IVoteValidationStrategy
{
     public string StrategyName => "MultipleChoice";

     public ValidationResult Validate(VoteValidationContext ctx)
     {
          if (!ctx.AllowMultipleChoices)
               return ValidationResult.Ok(); // not applicable

          if (ctx.SelectedOptionIds.Count == 0)
               return ValidationResult.Fail("You must select at least one option.");

          if (ctx.SelectedOptionIds.Distinct().Count() != ctx.SelectedOptionIds.Count)
               return ValidationResult.Fail("Duplicate options selected.");

          return ValidationResult.Ok();
     }
}

// Strategy 5 — all selected option IDs must actually exist on the poll
public class ValidOptionsStrategy : IVoteValidationStrategy
{
     public string StrategyName => "ValidOptions";

     public ValidationResult Validate(VoteValidationContext ctx)
     {
          var invalidOptions = ctx.SelectedOptionIds.Except(ctx.ValidOptionIds).ToList();

          if (invalidOptions.Any())
               return ValidationResult.Fail(
                   $"Invalid option ID(s): {string.Join(", ", invalidOptions)}.");

          return ValidationResult.Ok();
     }
}