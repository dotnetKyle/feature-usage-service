using System;
using System.Threading.Tasks;
using FeatureUsage;
using FeatureUsage.DAO;

namespace FeatureReportTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing Report...");

            var reportsRepo = new FeatureUsageReportsRepoAsync(AppSettings.GetSettings());

            await reportsRepo.CalculateEachFeatureCountAsync(
                startTime: DateTime.UtcNow.AddHours(-24),
                stopTime: DateTime.UtcNow
            );

            
        }
    }
}
