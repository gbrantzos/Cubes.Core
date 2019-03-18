using System.Collections.Generic;

namespace Cubes.Core.Commands.Middleware.ExecutionHistory
{
    public interface ICommandExecutionHistory
    {
        void Add(CommandExecution commandExecution);
        IEnumerable<CommandExecution> Get(string commandType);
        int Delete(string commandType, HistoryRetentionOptions options);
    }

    public class HistoryRetentionOptions
    {
        public int? KeepLastDays { get; set; } = 7;
        public int? KeepLastTimes { get; set; } = -1;
    }
}
