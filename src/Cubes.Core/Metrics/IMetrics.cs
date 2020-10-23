using Prometheus;

namespace Cubes.Core.Metrics
{
    public interface IMetrics
    {
        Counter GetCounter(string name);
        Counter RegisterCounter(string name, string help, params string[] labels);

        Histogram GetHistogram(string name);
        Histogram RegisterHistogram(string name, string help, double[] buckets, params string[] labels);
    }
}