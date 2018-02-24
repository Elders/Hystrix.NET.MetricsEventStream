
namespace Elders.Hystrix.NET.MetricsEventStream
{
    using System.Collections.Generic;

    public interface ISampleDataProvider
    {
        IEnumerable<string> GetSampleData();
    }
}
