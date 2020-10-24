using System;
using System.Collections.Generic;
using Cubes.Core.Utilities;
using Humanizer;
using Prometheus;

namespace Cubes.Core.Metrics
{
    public class PrometheusMetrics : IMetrics
    {
        private readonly Dictionary<string, Counter> _counters = new Dictionary<string, Counter>();
        private readonly Dictionary<string, Histogram> _histograms = new Dictionary<string, Histogram>();

        public PrometheusMetrics()
        {
            // Add known metrics
            foreach (var counter in CubesCoreMetrics.Counters)
                RegisterCounter(counter.Name, counter.Help, counter.Labels);
            foreach (var histogram in CubesCoreMetrics.Histograms)
                RegisterHistogram(histogram.Name, histogram.Help, histogram.Buckets, histogram.Labels);
        }

        public Counter GetCounter(string name)
        {
            if (_counters.TryGetValue(name, out var counter))
                return counter;

            throw new ArgumentException($"Could not find counter with name {name}");
        }

        public Counter RegisterCounter(string name, string help, params string[] labels)
        {
            if (_counters.TryGetValue(name, out var existing))
                return existing;

            var counter = Prometheus
                .Metrics
                .CreateCounter(name, help.IfNullOrEmpty(name.Humanize(LetterCasing.Sentence)), labels);
            _counters.Add(name, counter);

            return counter;
        }

        public Histogram GetHistogram(string name)
        {
            if (_histograms.TryGetValue(name, out var histogram))
                return histogram;

            throw new ArgumentException($"Could not find histogram with name {name}");
        }

        public Histogram RegisterHistogram(string name, string help, double[] buckets, params string[] labels)
        {
            if (_histograms.TryGetValue(name, out var existing))
                return existing;

            var histogram = Prometheus
                .Metrics
                .CreateHistogram(name,
                    help.IfNullOrEmpty(name.Humanize(LetterCasing.Sentence)),
                    new HistogramConfiguration
                    {
                        LabelNames = labels,
                        Buckets = buckets
                    });
            _histograms.Add(name, histogram);

            return histogram;
        }
    }
}
