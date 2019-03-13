using System;
using Newtonsoft.Json;

namespace Cubes.Core.Jobs
{
    public class JobDefinition
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; }
        public string CronExpression { get; set; }
        public JobParameter ExecutionParameters { get; set; }
        public bool IsActive { get; set; } = true;
        public bool FireIfMissed { get; set; }
    }

    public class JobParameter
    {
        public string CommandType { get; set; }
        public object CommandInstance { get; set; }
    }
}