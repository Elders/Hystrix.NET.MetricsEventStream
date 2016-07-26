using Elders.Multithreading.Scheduler;
using Hystrix.NET.MetricsEventStream.Logging;
using System;

namespace Hystrix.NET.MetricsEventStream
{
    public class Work : IWork
    {
        static ILog log = LogProvider.GetLogger(typeof(Work));

        public DateTime ScheduledStart { get; set; }

        /// <summary>
        /// The metrics data queue size limit. The metrics data will thrown away if the queue exceeds this limit.
        /// </summary>
        private const int QueueSizeWarningLimit = 1000;

        public void Start()
        {
            try
            {

            }
            catch (Exception ex)
            {
                log.FatalException("Failed to execute task.", ex);
            }
            finally
            {
                ScheduledStart = DateTime.UtcNow.AddSeconds(10);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
