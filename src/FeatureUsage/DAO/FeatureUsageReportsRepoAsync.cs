using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using FeatureUsage.Entities;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Translators;
using MongoDB.Bson.Serialization.Attributes;

namespace FeatureUsage.DAO
{
    public interface IFeatureUsageReportsRepoAsync
    {
        Task<List<FeatureTotalCount>> CalculateEachFeatureCountAsync(DateTime startTime, DateTime stopTime, CancellationToken cancellationToken = default(CancellationToken));
    }
    public class FeatureUsageReportsRepoAsync : IFeatureUsageReportsRepoAsync
    {
        IMongoClient _client;
        IMongoDatabase _db;
        IMongoCollection<FeatureUsageRecord> _collection;

        public FeatureUsageReportsRepoAsync(AppSettings appSettings)
        {
            var settings = MongoClientSettings.FromConnectionString(appSettings.DbConnectionString);
            settings.ClusterConfigurator = cb => DefaultClusterConfigurator.Configure(cb);

            _client = new MongoClient(settings);
            _db = _client.GetDatabase(appSettings.DatabaseName);
            _collection = _db.GetCollection<FeatureUsageRecord>("featureUsage");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        /// <param name="cancellationToken"></param>
        public async Task<List<FeatureTotalCount>> CalculateEachFeatureCountAsync(
            DateTime startTime, 
            DateTime stopTime, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            /* Report to calculate the number of times each feature has been used.
            JSON:

            db.featureUsage.aggregate([
                { $unwind: "$usageData" },
                { $group: { _id: "$featureName",
                    usageCount: { $sum: 1 }
                  }
                },
                { $sort: { usageCount: -1 } }
            ])
             */

            var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
                .Unwind<FeatureUsageRecord, FeatureUsageRecord, UnwoundFeatureUsageRecord>(
                    f => f.UsageData
                )
                .Match(f => 
                           f.UsageData.UsageDtgUTC >= startTime 
                        && f.UsageData.UsageDtgUTC < stopTime
                )
                .Group(e => e.FeatureName, 
                    e => new FeatureTotalCount
                    { 
                        FeatureName = e.Key,
                        UsageCount = e.Count()
                    }
                )
                .Sort(
                    Builders<FeatureTotalCount>.Sort.Descending(e => e.UsageCount)
                );

            var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

            var list = await cursor.ToListAsync(cancellationToken);

            return list;
        }

        public async Task<List<FeatureTotalCount>> CalculateSpecificFeatureCountAsync(
            string featureName, 
            DateTime startTime, 
            DateTime stopTime, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            /* Report to calculate the number of times a specific feature has been used.
             * 
            JSON
            db.featureUsage.aggregate([
                { $unwind: "$usageData" },
                { $match: { featureName:"Main Window|Long Running Service" } },
                { $group: { _id: "$featureName",
                    usageCount: { $sum: 1 }
                  }
                }
            ])
            */

            var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
                .Unwind(e => e.UsageData, new AggregateUnwindOptions<UnwoundFeatureUsageRecord>())
                .Match(e => 
                       e.FeatureName == featureName
                    && e.UsageData.UsageDtgUTC >= startTime
                    && e.UsageData.UsageDtgUTC < stopTime
                )
                .Group(e => e.FeatureName,
                    e => new FeatureTotalCount
                    {
                        FeatureName = e.Key,
                        UsageCount = e.Count()
                    }
                )
                .Sort(
                    Builders<FeatureTotalCount>.Sort.Descending(e => e.UsageCount)
                );

            var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

            var list = await cursor.ToListAsync(cancellationToken);

            return list;
        }

        public async Task<List<FeatureBenchmarkAggregate>> BenchmarkAverageForAllBenchmarkedFeaturesAsync(
            DateTime startTime, 
            DateTime stopTime, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            /* Report to calculate the average benchmark for all features that are benchmarked.
             * 
            JSON
            db.featureUsage.aggregate([
                { $unwind: "$usageData" },                
                { $match: {
                        "benchmarkMs": { $exists: true, $ne: [] }
                }}, 
                { $group: {
                        _id: "$featureName",
                        benchmarkAverage: {
                            $avg: "$benchmarkMs"
                        }
                }}
            ])
            */

            var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
                // might have to aggregate first
                .Unwind(e => e.UsageData, new AggregateUnwindOptions<UnwoundBenchmarkUsageRecord>())
                .Match(
                    Builders<UnwoundBenchmarkUsageRecord>.Filter.And(
                        // only if benchmark exists
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Exists(e => e.BenchmarkMs),
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Gte(e => e.UsageData.UsageDtgUTC, startTime),
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Lt (e => e.UsageData.UsageDtgUTC, stopTime)
                    )
                )
                .Group(e => e.FeatureName,
                    e => new FeatureBenchmarkAggregate
                    {
                        FeatureName = e.Key,
                        // average the benchmarks for this period
                        BenchmarkMs = e.Average(x => x.BenchmarkMs)
                    }
                )
                .Sort(
                    // sort slowest features up top
                    Builders<FeatureBenchmarkAggregate>.Sort.Descending(e => e.BenchmarkMs)
                );

            var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

            var list = await cursor.ToListAsync(cancellationToken);

            return list;
        }

        public async Task<List<FeatureBenchmarkAggregate>> BenchmarkAverageForSpecificFeatureAsync(
            string featureName, 
            DateTime startTime, 
            DateTime stopTime, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            /* Report to calculate the average benchmark time for a specific feature.
             * 
            JSON
            db.featureUsage.aggregate([
                { $match: { featureName:"Main Window|Long Running Service" } },
                { $project: {
                    _id:0,
                    user:"$userName",
                    featureName:"$featureName",
                    benchmarkMs:"$usageData.benchmarkMs"
                  } 
                },
                { $unwind: "$benchmarkMs" },
                { $group: {
                    _id: "$featureName",
                    benchmarksAvg: { $avg: "$benchmarkMs" }
                  }
                }
            ])
            */

            var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
                .Unwind(e => e.UsageData, new AggregateUnwindOptions<UnwoundBenchmarkUsageRecord>())
                .Match(
                    Builders<UnwoundBenchmarkUsageRecord>.Filter.And(
                        // only if benchmark exists
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Exists(e => e.BenchmarkMs),
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Gte(e => e.UsageData.UsageDtgUTC, startTime),
                        Builders<UnwoundBenchmarkUsageRecord>.Filter.Lt(e => e.UsageData.UsageDtgUTC, stopTime)
                    )
                )
                .Group(e => e.FeatureName,
                    e => new FeatureBenchmarkAggregate
                    {
                        FeatureName = e.Key,
                        // average the benchmarks for this period
                        BenchmarkMs = e.Average(x => x.BenchmarkMs)
                    }
                )
                .Sort(
                    // sort slowest features up top
                    Builders<FeatureBenchmarkAggregate>.Sort.Descending(e => e.BenchmarkMs)
                );

            var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

            var list = await cursor.ToListAsync(cancellationToken);

            return list;
        }
    }

    public class UnwoundFeatureUsageRecord
    {
        [BsonId(Order = 3)]
        public ObjectId Id { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("featureName")]
        public string FeatureName { get; set; }

        [BsonElement("usageData")]
        public UsageData UsageData { get; set; }

        public override string ToString()
        {
            return $"{FeatureName} {UserName} UTC:[{UsageData.UsageDtgUTC}]";
        }
    }
    public class FeatureTotalCount
    {
        public string FeatureName { get; set; }
        public int UsageCount { get; set; }

        public override string ToString()
        {
            return $"{FeatureName} ({UsageCount})";
        }
    }
    public class UnwoundBenchmarkUsageRecord : UnwoundFeatureUsageRecord
    {
        [BsonIgnoreIfNull]
        [BsonElement("benchmarkMs")]
        public int BenchmarkMs { get; set; }

        public override string ToString()
        {
            return $"{FeatureName} {UserName} Ms:[{BenchmarkMs}]";
        }
    }
    public class FeatureBenchmarkAggregate
    {
        public string FeatureName { get; set; }
        public double BenchmarkMs { get; set; }

        public override string ToString()
        {
            return $"{FeatureName} ({BenchmarkMs}ms)";
        }
    }

}
