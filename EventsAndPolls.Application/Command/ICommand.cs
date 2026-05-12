using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Command;

// Command interface — every action in the system implements Execute + Undo
public interface ICommand
{
     string CommandName { get; }
     Task ExecuteAsync();
     Task UndoAsync();
}

// The invoker — queues, executes and tracks commands for undo/redo
public class PollCommandInvoker
{
     private readonly Stack<ICommand> _history = new();
     private readonly Microsoft.Extensions.Logging.ILogger<PollCommandInvoker> _logger;

     public PollCommandInvoker(Microsoft.Extensions.Logging.ILogger<PollCommandInvoker> logger)
     {
          _logger = logger;
     }

     public async Task ExecuteAsync(ICommand command)
     {
          _logger.LogInformation("[Command] Executing: {Command}", command.CommandName);
          await command.ExecuteAsync();
          _history.Push(command);
          _logger.LogInformation("[Command] '{Command}' pushed to history (depth: {Depth})",
              command.CommandName, _history.Count);
     }

     public async Task UndoLastAsync()
     {
          if (!_history.TryPop(out var command))
          {
               _logger.LogWarning("[Command] Undo requested but history is empty.");
               return;
          }

          _logger.LogInformation("[Command] Undoing: {Command}", command.CommandName);
          await command.UndoAsync();
     }

     public int HistoryDepth => _history.Count;
     public IEnumerable<string> HistoryNames => _history.Select(c => c.CommandName);
}