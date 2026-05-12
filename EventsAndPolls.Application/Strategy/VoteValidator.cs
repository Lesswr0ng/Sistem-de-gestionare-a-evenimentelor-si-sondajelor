using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Strategy;

// The context that holds and executes a collection of strategies
// Strategies are run in order; first failure stops the chain
public class VoteValidator
{
     private readonly List<IVoteValidationStrategy> _strategies;
     private readonly ILogger<VoteValidator> _logger;

     public VoteValidator(IEnumerable<IVoteValidationStrategy> strategies, ILogger<VoteValidator> logger)
     {
          _strategies = strategies.ToList();
          _logger = logger;
     }

     public ValidationResult Validate(VoteValidationContext context)
     {
          _logger.LogInformation(
              "[Strategy] Running {Count} validation strategies for Poll #{PollId}",
              _strategies.Count, context.PollId);

          foreach (var strategy in _strategies)
          {
               var result = strategy.Validate(context);

               if (!result.IsValid)
               {
                    _logger.LogWarning(
                        "[Strategy] Validation failed at '{Strategy}': {Error}",
                        strategy.StrategyName, result.ErrorMessage);
                    return result;
               }

               _logger.LogDebug("[Strategy] '{Strategy}' passed", strategy.StrategyName);
          }

          _logger.LogInformation("[Strategy] All strategies passed for Poll #{PollId}", context.PollId);
          return ValidationResult.Ok();
     }

     public static VoteValidator CreateDefault(ILogger<VoteValidator> logger)
     {
          var strategies = new List<IVoteValidationStrategy>
        {
            new PollActiveValidationStrategy(),
            new NoDuplicateVoteStrategy(),
            new SingleChoiceValidationStrategy(),
            new MultipleChoiceValidationStrategy(),
            new ValidOptionsStrategy()
        };

          return new VoteValidator(strategies, logger);
     }
}