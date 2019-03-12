using System;

namespace Cubes.Core.Jobs
{
    public class JobDefinition
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; }
        public string CronExpression { get; set; }
        public string ExecutionParameters { get; set; }
        public bool IsActive { get; set; } = true;
        public bool FireIfMissed { get; set; }
    }
}