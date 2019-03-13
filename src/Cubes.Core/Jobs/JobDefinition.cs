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

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static JobParameter FromJson(string json)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<JobParameter>(json, settings);
        }
    }
}