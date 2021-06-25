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
        Task CalculateEachFeatureCountAsync(DateTime startTime, DateTime stopTime, CancellationToken cancellationToken = default(CancellationToken));
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
        public async Task CalculateEachFeatureCountAsync(DateTime startTime, DateTime stopTime, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                startTime = DateTime.UtcNow.AddHours(-1);
                stopTime = DateTime.UtcNow.AddHours(1);

                var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
                    .Unwind<FeatureUsageRecord, FeatureUsageRecord, UnwoundFeatureUsageRecord>(
                        f => f.UsageData
                    )
                    .Match(
                        f => 
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
                        new SortDefinitionBuilder<FeatureTotalCount>()
                            .Descending(e => e.UsageCount)
                    );

                var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

                var list = await cursor.ToListAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error:");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /*
            Report to calculate the number of times each feature has been used.

            db.featureUsage.aggregate([
                { $unwind: "$usageData" },
                { $group: { _id: "$featureName",
                    usageCount: { $sum: 1 }
                  }
                },
                { $sort: { usageCount: -1 } }
            ])
            Report to calculate the number of times a specific feature has been used.

            db.featureUsage.aggregate([
                { $match: { featureName:"Main Window|Long Running Service" } },
                { $unwind: "$usageData" },
                { $group: { _id: "$featureName",
                    usageCount: { $sum: 1 }
                  }
                }
            ])


            Benchmark Data Reports


            Report to calculate the average benchmark for all features that are benchmarked.

            db.featureUsage.aggregate([
                { $project: {
                        _id: 0,
                        user: "$userName",
                        featureName: "$featureName",
                        benchmarkMs: "$usageData.benchmarkMs"
                    }
                }, 
                { $match: {
                        "benchmarkMs": { $exists: true, $ne: [] }
                }}, 
                { $unwind: { path: "$benchmarkMs" } }, 
                { $group: {
                        _id: "$featureName",
                        benchmarkAverage: {
                            $avg: "$benchmarkMs"
                        }
                }}
            ])
            Report to calculate the average benchmark time for a specific feature.

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

}
