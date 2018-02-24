
namespace Elders.Hystrix.NET.MetricsEventStream
{

    using System;
    using System.Collections.Generic;
    using CircuitBreaker;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ThreadPool;
    using Util;
    public class HystrixSampleDataProvider : ISampleDataProvider
    {

        public IEnumerable<string> GetSampleData()
        {
            foreach (HystrixCommandMetrics commandMetrics in HystrixCommandMetrics.Instances)
            {
                yield return CreateCommandSampleData(commandMetrics);
            }

            foreach (HystrixThreadPoolMetrics threadPoolMetrics in HystrixThreadPoolMetrics.Instances)
            {
                yield return CreateThreadPoolSampleData(threadPoolMetrics);
            }
        }

        /// <summary>
        /// Produces JSON formatted metrics data from an instance of <see cref="HystrixCommandMetrics"/>.
        /// </summary>
        /// <param name="commandMetrics">The metrics of a command.</param>
        /// <returns>JSON formatted metrics data.</returns>
        private static string CreateCommandSampleData(HystrixCommandMetrics commandMetrics)
        {
            IHystrixCircuitBreaker circuitBreaker = HystrixCircuitBreakerFactory.GetInstance(commandMetrics.CommandKey);
            HealthCounts healthCounts = commandMetrics.GetHealthCounts();
            IHystrixCommandProperties commandProperties = commandMetrics.Properties;

            JObject data = new JObject(
                new JProperty("type", "HystrixCommand"),
                new JProperty("name", commandMetrics.CommandKey.Name),
                new JProperty("group", commandMetrics.CommandGroup.Name),
                new JProperty("currentTime", GetCurrentTimeForJavascript()),
                circuitBreaker == null ? new JProperty("isCircuitBreakerOpen", false) : new JProperty("isCircuitBreakerOpen", circuitBreaker.IsOpen()),
                new JProperty("errorPercentage", healthCounts.ErrorPercentage), // health counts
                new JProperty("errorCount", healthCounts.ErrorCount),
                new JProperty("requestCount", healthCounts.TotalRequests),
                new JProperty("rollingCountCollapsedRequests", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Collapsed)), // rolling counters
                new JProperty("rollingCountExceptionsThrown", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown)),
                new JProperty("rollingCountFailure", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Failure)),
                new JProperty("rollingCountFallbackFailure", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure)),
                new JProperty("rollingCountFallbackRejection", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection)),
                new JProperty("rollingCountFallbackSuccess", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess)),
                new JProperty("rollingCountResponsesFromCache", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache)),
                new JProperty("rollingCountSemaphoreRejected", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected)),
                new JProperty("rollingCountShortCircuited", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited)),
                new JProperty("rollingCountSuccess", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Success)),
                new JProperty("rollingCountThreadPoolRejected", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected)),
                new JProperty("rollingCountTimeout", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Timeout)),
                new JProperty("currentConcurrentExecutionCount", commandMetrics.CurrentConcurrentExecutionCount),
                new JProperty("latencyExecute_mean", commandMetrics.GetExecutionTimeMean()), // latency percentiles
                new JProperty(
                    "latencyExecute",
                    new JObject(
                        new JProperty("0", commandMetrics.GetExecutionTimePercentile(0)),
                        new JProperty("25", commandMetrics.GetExecutionTimePercentile(25)),
                        new JProperty("50", commandMetrics.GetExecutionTimePercentile(50)),
                        new JProperty("75", commandMetrics.GetExecutionTimePercentile(75)),
                        new JProperty("90", commandMetrics.GetExecutionTimePercentile(90)),
                        new JProperty("95", commandMetrics.GetExecutionTimePercentile(95)),
                        new JProperty("99", commandMetrics.GetExecutionTimePercentile(99)),
                        new JProperty("99.5", commandMetrics.GetExecutionTimePercentile(99.5)),
                        new JProperty("100", commandMetrics.GetExecutionTimePercentile(100)))),
                new JProperty("latencyTotal_mean", commandMetrics.GetTotalTimeMean()),
                new JProperty(
                    "latencyTotal",
                    new JObject(
                        new JProperty("0", commandMetrics.GetTotalTimePercentile(0)),
                        new JProperty("25", commandMetrics.GetTotalTimePercentile(25)),
                        new JProperty("50", commandMetrics.GetTotalTimePercentile(50)),
                        new JProperty("75", commandMetrics.GetTotalTimePercentile(75)),
                        new JProperty("90", commandMetrics.GetTotalTimePercentile(90)),
                        new JProperty("95", commandMetrics.GetTotalTimePercentile(95)),
                        new JProperty("99", commandMetrics.GetTotalTimePercentile(99)),
                        new JProperty("99.5", commandMetrics.GetTotalTimePercentile(99.5)),
                        new JProperty("100", commandMetrics.GetTotalTimePercentile(100)))),
                new JProperty("propertyValue_circuitBreakerRequestVolumeThreshold", commandProperties.CircuitBreakerRequestVolumeThreshold.Get()), // property values for reporting what is actually seen by the command rather than what was set somewhere
                new JProperty("propertyValue_circuitBreakerSleepWindowInMilliseconds", (long)commandProperties.CircuitBreakerSleepWindow.Get().TotalMilliseconds),
                new JProperty("propertyValue_circuitBreakerErrorThresholdPercentage", commandProperties.CircuitBreakerErrorThresholdPercentage.Get()),
                new JProperty("propertyValue_circuitBreakerForceOpen", commandProperties.CircuitBreakerForceOpen.Get()),
                new JProperty("propertyValue_circuitBreakerForceClosed", commandProperties.CircuitBreakerForceClosed.Get()),
                new JProperty("propertyValue_circuitBreakerEnabled", commandProperties.CircuitBreakerEnabled.Get()),
                new JProperty("propertyValue_executionIsolationStrategy", commandProperties.ExecutionIsolationStrategy.Get()),
                new JProperty("propertyValue_executionIsolationThreadTimeoutInMilliseconds", (long)commandProperties.ExecutionIsolationThreadTimeout.Get().TotalMilliseconds),
                new JProperty("propertyValue_executionIsolationThreadInterruptOnTimeout", commandProperties.ExecutionIsolationThreadInterruptOnTimeout.Get()),
                new JProperty("propertyValue_executionIsolationThreadPoolKeyOverride", commandProperties.ExecutionIsolationThreadPoolKeyOverride.Get()),
                new JProperty("propertyValue_executionIsolationSemaphoreMaxConcurrentRequests", commandProperties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Get()),
                new JProperty("propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests", commandProperties.FallbackIsolationSemaphoreMaxConcurrentRequests.Get()),
                new JProperty("propertyValue_metricsRollingStatisticalWindowInMilliseconds", commandProperties.MetricsRollingStatisticalWindowInMilliseconds.Get()),
                new JProperty("propertyValue_requestCacheEnabled", commandProperties.RequestCacheEnabled.Get()),
                new JProperty("propertyValue_requestLogEnabled", commandProperties.RequestLogEnabled.Get()),
                new JProperty("reportingHosts", 1));

            return data.ToString(Formatting.None);
        }

        /// <summary>
        /// Produces JSON formatted metrics data from an instance of <see cref="HystrixThreadPoolMetrics"/>.
        /// </summary>
        /// <param name="threadPoolMetrics">The metrics of a thread pool.</param>
        /// <returns>JSON formatted metrics data.</returns>
        private static string CreateThreadPoolSampleData(HystrixThreadPoolMetrics threadPoolMetrics)
        {
            IHystrixThreadPoolProperties properties = threadPoolMetrics.Properties;

            JObject data = new JObject(
                new JProperty("type", "HystrixThreadPool"),
                new JProperty("name", threadPoolMetrics.ThreadPoolKey.Name),
                new JProperty("currentTime", GetCurrentTimeForJavascript()),
                new JProperty("currentActiveCount", threadPoolMetrics.CurrentActiveCount),
                new JProperty("currentCompletedTaskCount", threadPoolMetrics.CurrentCompletedTaskCount),
                new JProperty("currentCorePoolSize", threadPoolMetrics.CurrentCorePoolSize),
                new JProperty("currentLargestPoolSize", threadPoolMetrics.CurrentLargestPoolSize),
                new JProperty("currentMaximumPoolSize", threadPoolMetrics.CurrentMaximumPoolSize),
                new JProperty("currentPoolSize", threadPoolMetrics.CurrentPoolSize),
                new JProperty("currentQueueSize", threadPoolMetrics.CurrentQueueSize),
                new JProperty("currentTaskCount", threadPoolMetrics.CurrentTaskCount),
                new JProperty("rollingCountThreadsExecuted", threadPoolMetrics.RollingCountThreadsExecuted),
                new JProperty("rollingMaxActiveThreads", threadPoolMetrics.RollingMaxActiveThreads),
                new JProperty("propertyValue_queueSizeRejectionThreshold", properties.QueueSizeRejectionThreshold.Get()),
                new JProperty("propertyValue_metricsRollingStatisticalWindowInMilliseconds", properties.MetricsRollingStatisticalWindowInMilliseconds.Get()),
                new JProperty("reportingHosts", 1));

            return data.ToString(Formatting.None);
        }

        /// <summary>
        /// The date which is used to calculate the current time for JavaScript.
        /// JavaScript measures the time in elapsed milliseconds since 1970.01.01 00:00:00.
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Gets the current time in the format of JavaScript, which is the elapsed
        /// time since 1970.01.01 00:00:00 in milliseconds.
        /// </summary>
        /// <returns>The current time.</returns>
        private static long GetCurrentTimeForJavascript()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }
    }
}
