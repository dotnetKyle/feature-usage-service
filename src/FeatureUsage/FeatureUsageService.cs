using FeatureUsage.DAO;
using FeatureUsage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FeatureUsage
{
    public class FeatureUsageService
    {
        System.Timers.Timer _timer;
        IUserInfoService _userInfoService;
        IFeatureUsageRepoAsync _featureUsageRepo;
        List<FeatureUsageDTO> _records { get; set; }

        public FeatureUsageService(IUserInfoService userInfoService, IFeatureUsageRepoAsync featureUsageRepo)
        {
            _userInfoService = userInfoService;
            _featureUsageRepo = featureUsageRepo;
            _records = new List<FeatureUsageDTO>();
            _timer = new System.Timers.Timer();
            _timer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            _timer.Elapsed += saveRecords;
        }

        private async Task saveRecordsAsync()
        {
            if (_records != null && _records.Count > 0)
            {
                var username = _userInfoService.UserLogin;
                // group records by feature name
                var groupings = _records.GroupBy(r => r.FeatureName)
                    .Select(g => new FeatureUsageRecord
                    {
                        UserName = username,
                        FeatureName = g.Key,
                        UsageData = g.Select(item => new UsageData
                        {
                            BenchmarkMs = item.BenchmarkInMs,
                            UsageDtgUTC = item.DtgOfUsageUTC
                        }).ToArray()
                    });

                // bulk write
                await _featureUsageRepo.SendAllFeatureUsageDataAsync(groupings, default(CancellationToken));
            }
        }
        private async void saveRecords(object sender, ElapsedEventArgs e)
        {
            try
            {
                await saveRecordsAsync();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception Thrown:");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public void RecordUsage(string featureName)
        {
            var record = new FeatureUsageDTO
            {
                DtgOfUsageUTC = DateTime.UtcNow, 
                FeatureName = featureName
            };

            _records.Add(record);
        }

        /// <summary>
        /// This method is to force the sending of the usage data immediately.
        /// <para>You should only use this method during the shutdown of the app and maybe Unit Testing</para>
        /// </summary>
        public async Task ForceSendUsageToDatabaseAsync()
        {
            await saveRecordsAsync();
        }
    }
}
