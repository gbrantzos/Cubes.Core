using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Cubes.Core.Metrics
{
    public static class CubesCoreMetrics
    {
        public static string CubesCoreRequestsCount    = "cubes_core_requests_count";
        public static string CubesCoreRequestsDuration = "cubes_core_requests_duration_seconds";

        public static string CubesCoreApiCallsCount    = "cubes_core_apicall_count";
        public static string CubesCoreApiCallsDuration = "cubes_core_apicalls_duration_seconds";

        public abstract class BaseMetric
        {
            public string Name { get; set; }
            public string Help { get; set; }
            public string[] Labels { get; set; }
        }

        public class CounterDetails : BaseMetric { }

        public class HistogramDetails : BaseMetric
        {
            public double[] Buckets { get; set; }
        }

        public static List<CounterDetails> Counters { get; } = new List<CounterDetails>();
        public static List<HistogramDetails> Histograms { get; } = new List<HistogramDetails>();

        static CubesCoreMetrics()
        {
            Counters.Add(new CounterDetails
            {
                Name = CubesCoreRequestsCount,
                Help = "Cubes Core requests count",
                Labels = new string[] { "request_type" }
            });

            Histograms.Add(new HistogramDetails
            {
                Name = CubesCoreRequestsDuration,
                Help = "Cubes Core requests duration in seconds",
                Labels = new string[] { "request_type" },
                Buckets = Prometheus.Histogram.ExponentialBuckets(1, 2, 8)
            });

            Counters.Add(new CounterDetails
            {
                Name = CubesCoreApiCallsCount,
                Help = "Cubes Core api calls count",
                Labels = new string[] { "method", "path" }
            });

            Histograms.Add(new HistogramDetails
            {
                Name = CubesCoreApiCallsDuration,
                Help = "Cubes Core api calls duration in seconds",
                Labels = new string[] { "method", "path" },
                Buckets = new double[] { 0.1, 0.2, 0.5, 0.8, 1, 2, 5, 8 }
            });
        }

        public static void AddFromConfiguration(IConfiguration configuration)
        {
            // Add counter details from configuration
            // ...
        }
    }
}
