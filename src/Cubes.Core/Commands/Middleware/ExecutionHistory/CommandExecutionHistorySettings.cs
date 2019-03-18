using System.Collections.Generic;
using Cubes.Core.Settings;

namespace Cubes.Core.Commands.Middleware.ExecutionHistory
{
    [SettingsPrefix("Core")]
    public class CommandExecutionHistorySettings
    {
        public int MonitoringIntervalSecs { get; set; } = 180;
        public HistoryRetentionOptions RetentionOptions { get; set; } = new HistoryRetentionOptions();
        public List<string> SkipCommands { get; set; } = new List<string>();
    }
}
