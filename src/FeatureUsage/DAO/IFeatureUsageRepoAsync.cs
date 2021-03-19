using FeatureUsage.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureUsage.DAO
{
    public interface IFeatureUsageRepoAsync
    {
        Task SendAllFeatureUsageDataAsync(IEnumerable<FeatureUsageRecord> records, CancellationToken cancellationToken);
    }
}
