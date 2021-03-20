using System;

namespace FeatureUsage
{
    public class Feature
    {
        FeatureUsageService _serviceReference;

        public Feature(FeatureUsageService serviceReference, string featureName)
        {
            if (serviceReference == null)
                throw new ArgumentNullException(nameof(serviceReference), "The Service Reference was null");
            if (string.IsNullOrWhiteSpace(featureName))
                throw new ArgumentNullException(nameof(featureName), "The Feature Name must not be empty");

            _serviceReference = serviceReference;
            FeatureName = featureName;
        }

        public string FeatureName { get; private set; }

        /// <summary> Record the usage of this feature </summary>
        public void RecordFeatureUse()
            => _serviceReference.RecordUsage(FeatureName);

        /// <summary> Benchmark and record the usage of this feature </summary>
        /// <returns>An IDisposable Benchmark that will be timed when it is disposed</returns>
        public FeatureBenchmark BenchmarkFeatureUse()
            => _serviceReference.Benchmark(FeatureName);
    }
}
