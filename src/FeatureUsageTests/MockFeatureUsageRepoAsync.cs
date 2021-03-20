using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FeatureUsage.DAO;
using FeatureUsage.Entities;

namespace FeatureUsageTests
{
    class MockFeatureUsageRepoAsync : IFeatureUsageRepoAsync
    {
        internal List<FeatureUsageRecord> _entities;
        public MockFeatureUsageRepoAsync(IEnumerable<FeatureUsageRecord> records = null)
        {
            _entities = (records == null)
                ? new List<FeatureUsageRecord>()
                : new List<FeatureUsageRecord>(records);
        }

        internal HashSet<string> UsernamesSentToRepo = new HashSet<string>();

        public Task SendAllFeatureUsageDataAsync(IEnumerable<FeatureUsageRecord> records, CancellationToken cancellationToken)
        {
            foreach(var record in records)
            {
                var existingFeatureForUser = _entities
                    .FirstOrDefault(e => e.FeatureName == record.FeatureName
                        && e.UserName == record.UserName);

                if(existingFeatureForUser != null)
                {
                    existingFeatureForUser.UsageData = existingFeatureForUser
                        .UsageData
                        .Concat(record.UsageData)
                        .ToArray();
                }
                else
                {
                    _entities.Add(record);
                }
            }

            return Task.CompletedTask;
        }
    }
}
