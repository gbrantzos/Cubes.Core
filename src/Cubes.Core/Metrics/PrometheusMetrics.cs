using System;
using System.Collections.Generic;
using Cubes.Core.Utilities;
using Humanizer;
using Prometheus;

namespace Cubes.Core.Metrics
{
    public static class CubesCoreMetrics
    {
        public static string CubesRequests = "cubes_requests";
    }

    public class PrometheusMetrics : IMetrics
    {
        private class CounterSetup
        {
            public string Name { get; }
            public string Help { get; }
            public string[] Labels { get; }

            public CounterSetup(string name, string help, string[] labels)
            {
                Name = name;
                Help = help.IfNullOrEmpty(Name.Humanize(LetterCasing.Sentence));
                Labels = labels ?? new string[] { };
            }
            public CounterSetup(string name, string[] labels) : this(name, String.Empty, labels) { }

            public CounterConfiguration GetConfiguration()
                => Labels.Length > 0 ? new CounterConfiguration { LabelNames = Labels } : null;
        }

        private readonly Dictionary<string, CounterSetup> _counters = new Dictionary<string, CounterSetup>
        {
            {
                CubesCoreMetrics.CubesRequests,
                new CounterSetup(CubesCoreMetrics.CubesRequests,new [] { "request_type" })
            }
        };

        public PrometheusMetrics()
        {
            // Add known counters
            foreach (var counter in _counters)
                Prometheus.Metrics.CreateCounter(counter.Key, counter.Value.Help, counter.Value.GetConfiguration());
        }

        public Counter GetCounter(string name)
        {
            if (_counters.TryGetValue(name, out var counter))
                return Prometheus.Metrics.CreateCounter(counter.Name, counter.Help, counter.GetConfiguration());

            throw new ArgumentException($"Could not find counter with name {name}");
        }
    }
}
