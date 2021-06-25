using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;

namespace FeatureUsage.DAO
{
    public static class DefaultClusterConfigurator
    {
        public static void Configure(ClusterBuilder cb)
        {
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

            cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e =>
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
        }
    }
}
