using System;

namespace FeatureUsage
{
    public class Feature
    {
        FeatureUsageService _serviceReference;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceReference"></param>
        /// <param name="featureName"></param>
#if SEEDING
        /// <param name="isBenchmark">isBenchmark is only for debugging and mock purposes</param>
#endif
        public Feature(FeatureUsageService serviceReference, string featureName
#if SEEDING
            , bool isBenchmark = false
#endif
            )
        {

            if (serviceReference == null)
                throw new ArgumentNullException(nameof(serviceReference), "The Service Reference was null");
            if (string.IsNullOrWhiteSpace(featureName))
                throw new ArgumentNullException(nameof(featureName), "The Feature Name must not be empty");

#if SEEDING
            IsBenchmark = isBenchmark;
#endif

            _serviceReference = serviceReference;
            FeatureName = featureName;
        }

        public string FeatureName { get; private set; }

#if SEEDING
        public bool IsBenchmark { get; private set; }
#endif

        /// <summary> Record the usage of this feature </summary>
        /// <param name="nowUtc">Leave this null if using for real data, nowUtc is for mockData</param>
        public void RecordFeatureUse(DateTime? nowUtc = null)
            => _serviceReference.RecordUsage(FeatureName, nowUtc);

        /// <summary> Benchmark and record the usage of this feature </summary>
        /// <param name="nowUtc">Leave this null if using for real data, nowUtc is for mockData</param>
        /// <returns>An IDisposable Benchmark that will be timed when it is disposed</returns>
        public FeatureBenchmark BenchmarkFeatureUse(DateTime? nowUtc = null)
            => _serviceReference.Benchmark(FeatureName, nowUtc);
    }
}
