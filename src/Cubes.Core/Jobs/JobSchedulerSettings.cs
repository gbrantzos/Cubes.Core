using System.Collections.Generic;
using Cubes.Core.Settings;

namespace Cubes.Core.Jobs
{
    [SettingsPrefix("Core")]
    public class JobSchedulerSettings
    {
        public int MonitoringIntervalSecs { get; set; } = 180;
        public HistoryRetentionOptions RetentionOptions { get; set; } = new HistoryRetentionOptions();
        public List<JobDefinition> Jobs { get; set; } = new List<JobDefinition>();
    }
}
