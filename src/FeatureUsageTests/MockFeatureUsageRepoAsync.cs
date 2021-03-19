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
            _entities.AddRange(records);

            return Task.CompletedTask;
        }

        //public Task SendFeatureUsageUpdateAsync(string username, 
        //    string featureName, 
        //    IEnumerable<UsageData> usageData, 
        //    CancellationToken cancellationToken = default)
        //{
        //    _entities.Add(new FeatureUsageRecord
        //    {
        //        UserName = username,
        //        FeatureName = featureName,
        //        UsageData = usageData.ToArray()
        //    });

        //    return Task.CompletedTask;
        //}
    }
}
