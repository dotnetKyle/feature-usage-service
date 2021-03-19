using System;

namespace FeatureUsage
{
    public class FeatureUsageDTO
    {
        public string FeatureName { get; set; }
        public int BenchmarkInMs { get; set; }
        public DateTime DtgOfUsageUTC { get; set; }
    }
}
