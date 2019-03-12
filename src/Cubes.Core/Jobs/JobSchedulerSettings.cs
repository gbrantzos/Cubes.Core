using System.Collections.Generic;
using Cubes.Core.Settings;

namespace Cubes.Core.Jobs
{
    [SettingsPrefix("Core")]
    public class JobSchedulerSettings
    {
        public List<JobDefinition> Jobs { get; set; } = new List<JobDefinition>();
    }
}
