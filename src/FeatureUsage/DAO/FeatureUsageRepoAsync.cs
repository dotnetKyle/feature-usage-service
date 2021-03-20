using FeatureUsage.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Generic;
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

                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterClosedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterClosingEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterRemovedServerEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ClusterRemovingServerEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandSucceededEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandFailedEvent>(e =>
                {
                    Log.Warning($"Mongo Event:{e.GetType().Name}");
                    Log.Warning($"CMD COMMAND FAILED:{e.CommandName}");
                    Log.Warning($"CMD Exception:{e.Failure}");
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.CommandSucceededEvent>(e =>
                {
                    Log.Debug($"Mongo Event:{e.GetType().Name}");
                    Log.Debug($"CMD Succ! Command Succeeded:{e.CommandName}");
                    Log.Debug($"CMD Succ! Reply:{e.Reply?.ToJson()}");
                    Log.Debug($"CMD Succ! Reply:{e.RequestId}");
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionClosedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionClosingEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionCreatedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpenedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpeningEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionOpeningFailedEvent>(e => Log.Warning($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckedOutConnectionEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckedInConnectionEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolCheckingOutConnectionFailedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ServerClosedEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                cb.Subscribe<MongoDB.Driver.Core.Events.ServerClosingEvent>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));
                //cb.Subscribe<MongoDB.Driver.Core.Events>(e => Log.Debug($"Mongo Event:{e.GetType().Name}"));

                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    Log.Debug($"Mongo Event:{e.GetType().Name}");
                    Log.Debug("CMD Start:" + e.Command?.ToJson());
                    Log.Debug("CMD Name:" + e.CommandName);
                });
                cb.Subscribe<MongoDB.Driver.Core.Events.ConnectionFailedEvent>(e =>
                {
                    Log.Warning($"Mongo Event:{e.GetType().Name}");
                    Log.Warning($"CONN FAILED Ex:{e.Exception}");
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

        public Task SendAllFeatureUsageDataAsync(IEnumerable<FeatureUsageRecord> records, CancellationToken cancellationToken)
        {
            try
            {
                Log.Debug($"Updating Database with {records.Count()} records");

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

                Log.Debug($"Sending Bulk Write...");
                var result = _collection.BulkWrite(updates, new BulkWriteOptions { });

                if (result.IsAcknowledged)
                {
                    if (result.IsModifiedCountAvailable && result.ModifiedCount > 0)
                        Log.Information("Updated several feature usage records for the user.");
                }
                else
                {
                    Log.Error("### FAIL: Feature usage records failed to update.");
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }

            return Task.CompletedTask;            
        }
    }
}
