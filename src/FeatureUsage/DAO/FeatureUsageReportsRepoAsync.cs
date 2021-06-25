using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using FeatureUsage.Entities;
using MongoDB.Bson;

namespace FeatureUsage.DAO
{
    public interface IFeatureUsageReportsRepoAsync
    { 

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
        //public async Task CalculateEachFeatureCountAsync(DateTime startTime, DateTime stopTime, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var pipeline = new EmptyPipelineDefinition<FeatureUsageRecord>()
        //        .Unwind(f => f.UsageData)
        //        .Match(
        //            Builders<BsonDocument>.Filter.And(
        //                Builders<BsonDocument>.Filter.Gte("", startTime),
        //                Builders<BsonDocument>.Filter.Lt("", stopTime)
        //            )
        //        )
        //        .Group(
        //        )
        //        ;

        //    var cursor = await _collection.AggregateAsync(pipeline, null, cancellationToken);

        //    var list = await cursor.ToListAsync(cancellationToken);


        //}

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
}
