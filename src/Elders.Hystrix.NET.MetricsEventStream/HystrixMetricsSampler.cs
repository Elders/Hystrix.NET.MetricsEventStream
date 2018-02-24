// Copyright 2013 Loránd Biró
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Elders.Hystrix.NET.MetricsEventStream
{
    using System;
    using System.Collections.Generic;
    using CircuitBreaker;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ThreadPool;
    using Util;

    /// <summary>
    /// Samples Hystrix metrics (<see cref="HystrixCommandMetrics.Instances"/>, <see cref="HystrixThreadPoolMetrics.Instances"/>)
    /// and outputs them as JSON formatted strings. Sampling and processing done in a different thread
    /// which can be started and stopped with the <see cref="Start"/> and <see cref="Stop"/> methods. Receiving the
    /// formatted data is possible through the <see cref="SampleDataAvailable"/> event.
    /// </summary>
    public class HystrixMetricsSampler : StoppableBackgroundWorker
    {
        /// <summary>
        /// The name of the sampler thread.
        /// </summary>
        private const string ThreadName = "Hystrix-MetricsEventStream-Sampler";
        
        /// <summary>
        /// The backing field for the <see cref="SampleInterval"/> property. May be access only through the property.
        /// </summary>
        private TimeSpan sampleInterval;

        private readonly ISampleDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixMetricsSampler" /> class.
        /// </summary>
        /// <param name="sampleInterval">The time interval between sampling.</param>
        public HystrixMetricsSampler(TimeSpan sampleInterval, ISampleDataProvider dataProvider)
            : base(ThreadName)
        {
            this.SampleInterval = sampleInterval;
            this.dataProvider = dataProvider ?? new HystrixSampleDataProvider();
        }

        /// <summary>
        /// When the sampler is running, it will repeatedly broadcast the sample data through this event.
        /// You should not do long operations in the event handler, since it would block the operation of
        /// the sampler.
        /// </summary>
        public event EventHandler<SampleDataAvailableEventArgs> SampleDataAvailable;

        /// <summary>
        /// Gets or sets the interval between sampling, it must be a positive time.
        /// </summary>
        public TimeSpan SampleInterval
        {
            get
            {
                return this.sampleInterval;
            }

            set
            {
                if (value.Ticks <= 0)
                {
                    throw new ArgumentException("Sample interval must be greater than zero.");
                }

                this.sampleInterval = value;
            }
        }

        /// <inheritdoc />
        protected override void DoWork()
        {
            List<string> data = new List<string>();
            while (true)
            {
                bool shouldStop = this.SleepAndGetShouldStop(this.SampleInterval);
                if (shouldStop)
                {
                    break;
                }

                foreach (var item in dataProvider.GetSampleData())
                {
                    data.Add(item);
                }

                EventHandler<SampleDataAvailableEventArgs> handler = this.SampleDataAvailable;
                if (handler != null)
                {
                    handler(this, new SampleDataAvailableEventArgs(data));
                }

                data.Clear();
            }
        } 
    }
}
