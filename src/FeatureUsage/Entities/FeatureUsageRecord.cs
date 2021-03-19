using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FeatureUsage.Entities
{
    public class FeatureUsageRecord
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("featureName")]
        public string FeatureName { get; set; }

        [BsonElement("usageData")]
        public UsageData[] UsageData { get; set; }

        public override string ToString()
        {
            return $"{UserName} {FeatureName} UsageData:[{UsageData.Length}]";
        }
    }
    public class UsageData
    {
        [BsonElement("usageDtgUTC")]
        public DateTime UsageDtgUTC { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("benchmarkMs")]
        public int? BenchmarkMs { get; set; } = null;

        public override string ToString()
        {
            if(BenchmarkMs.HasValue)
            {
                return $"DTG:{UsageDtgUTC}, {BenchmarkMs}ms";
            }
            else
            {
                return $"DTG:{UsageDtgUTC}";
            }
        }
    }
}
