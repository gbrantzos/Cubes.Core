using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cubes.Core.Scheduling
{
    public class SchedulerStatus
    {
        public enum State
        {
            StandBy,
            Active,
            FailedToLoad
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public State SchedulerState          { get; set; }
        public DateTime ServerTime           { get; set; }
        public List<SchedulerJobStatus> Jobs { get; set; }
    }

    public class SchedulerJobStatus
    {
        public string Name                      { get; set; }
        public bool Active                      { get; set; }
        public bool RefireIfMissed              { get; set; }
        public string CronExpression            { get; set; }
        public string CronExpressionDescription { get; set; }
        public string JobType                   { get; set; }
        public DateTime? PreviousFireTime       { get; set; }
        public DateTime? NextFireTime           { get; set; }
        public bool LastExecutionFailed         { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastExecutionMessage      { get; set; }

        public Dictionary<string, string> JobParameters { get; set; }
    }
}
