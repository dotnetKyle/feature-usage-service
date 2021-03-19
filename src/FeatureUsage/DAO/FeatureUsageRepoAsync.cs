using FeatureUsage.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureUsage.DAO
{
    public class FeatureUsageRepoAsync : IFeatureUsageRepoAsync
    {
        IMongoClient _client;
        IMongoDatabase _db;
        IMongoCollection<FeatureUsageRecord> _collection;

        public FeatureUsageRepoAsync(AppSettings appSettings)
        {
            var settings = MongoClientSettings.FromConnectionString(appSettings.DbConnectionString);
            settings.ClusterConfigurator = cb => {

                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterClosedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterClosingEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterRemovedServerEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterRemovingServerEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandSucceededEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandFailedEvent>(e =>
                {
                    Debug.WriteLine($"Mongo Event:{e.GetType().Name}");
                    Debug.WriteLine($"CMD COMMAND FAILED:{e.CommandName}");
                    Debug.WriteLine($"CMD Exception:{e.Failure}");
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandSucceededEvent>(e =>
                {
                    Debug.WriteLine($"Mongo Event:{e.GetType().Name}");
                    Debug.WriteLine($"CMD Succ! Command Succeeded:{e.CommandName}");
                    Debug.WriteLine($"CMD Succ! Reply:{e.Reply?.ToJson()}");
                    Debug.WriteLine($"CMD Succ! Reply:{e.RequestId}");
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionClosedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionClosingEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionCreatedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpenedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpeningEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpeningFailedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckedOutConnectionEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckedInConnectionEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckingOutConnectionFailedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ServerClosedEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ServerClosingEvent>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));
                //cb.Subscribe<MongoDB.Driver.Core.Events>(e => Debug.WriteLine($"Mongo Event:{e.GetType().Name}"));

                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    Debug.WriteLine($"Mongo Event:{e.GetType().Name}");
                    Debug.WriteLine("CMD Start:" + e.Command?.ToJson());
                    Debug.WriteLine("CMD Name:" + e.CommandName);
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionFailedEvent>(e =>
                {
                    Debug.WriteLine($"Mongo Event:{e.GetType().Name}");
                    Debug.WriteLine($"CONN FAILED Ex:{e.Exception}");
                });
            };
            
            _client = new MongoClient(settings);
            _db = _client.GetDatabase(appSettings.DatabaseName);
            _collection = _db.GetCollection<FeatureUsageRecord>("featureUsage");
        }

        public async Task<List<FeatureUsageRecord>> GetAllFeatureUsageForUserAsync(string username)
        {
            var filter = Builders<FeatureUsageRecord>.Filter.Eq(r => r.UserName, username);

            return await _collection.Find(filter, new FindOptions())
                .ToListAsync();
        }

        public async Task SendAllFeatureUsageDataAsync(IEnumerable<FeatureUsageRecord> records, CancellationToken cancellationToken)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Updating Database with {records.Count()} records");

                var updates = new List<UpdateOneModel<FeatureUsageRecord>>();
                foreach(var record in records)
                {
                    var filter = Builders<FeatureUsageRecord>.Filter.And(
                       Builders<FeatureUsageRecord>.Filter.Eq(u => u.UserName, record.UserName),
                       Builders<FeatureUsageRecord>.Filter.Eq(u => u.FeatureName, record.FeatureName)
                    );

                    var update = Builders<FeatureUsageRecord>.Update
                        .SetOnInsert(r => r.UserName, record.UserName)
                        .SetOnInsert(r => r.FeatureName, record.FeatureName)
                        .PushEach(r => r.UsageData, record.UsageData);

                    updates.Add(
                        new UpdateOneModel<FeatureUsageRecord>(filter, update) { IsUpsert = true }
                    );
                }

                System.Diagnostics.Debug.WriteLine($"Sending Bulk Write...");

                var result = await _collection.BulkWriteAsync(updates, new BulkWriteOptions {}, cancellationToken);

                System.Diagnostics.Debug.WriteLine($"Result IsAcknowledged:{result.IsAcknowledged}");

                if (result.IsAcknowledged)
                {
                    System.Diagnostics.Debug.WriteLine($"Result MatchedCount:{result.MatchedCount}");

                        System.Diagnostics.Debug.WriteLine("Inserted several feature usage records for the user.");

                    foreach (var req in result.ProcessedRequests)
                        Console.WriteLine("Processed:" + req.ToJson());

                    if (result.IsModifiedCountAvailable && result.ModifiedCount > 0)
                            System.Diagnostics.Debug.WriteLine("Updated several feature usage records for the user.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("### FAIL: Feature usage records failed to update.");
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception Thrown");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
