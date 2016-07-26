using Elders.Multithreading.Scheduler;
using Netflix.Hystrix;
using System;

namespace Hystrix.NET.MetricsEventStream
{
    public class SamplerWork : IWork
    {
        readonly HystrixMetricsSampler sampler;

        public SamplerWork(HystrixMetricsSampler sampler)
        {
            this.sampler = sampler;
        }

        public DateTime ScheduledStart { get; set; }

        public void Start()
        {
            try
            {
                sampler.DoSample();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }
    }
}
