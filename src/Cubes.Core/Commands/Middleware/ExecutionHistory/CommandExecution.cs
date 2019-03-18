using System;

namespace Cubes.Core.Commands.Middleware.ExecutionHistory
{
    public class CommandExecution
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public DateTime ExecutedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public string CommandType { get; set; }
        public CommandExecutionResult Result { get; set; }
        public string Message { get; set; }

    }
}
