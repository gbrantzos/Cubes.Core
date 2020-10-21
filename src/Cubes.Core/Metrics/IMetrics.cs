using Prometheus;

namespace Cubes.Core.Metrics
{
    public interface IMetrics
    {
        Counter GetCounter(string name);

        // TODO Setup counter functionality
    }
}