using FeatureUsage;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureUsageTests
{
    public class Tests
    {
        MockUserInfoService _userInfoSvc;
        FeatureUsageService service;
        MockFeatureUsageRepoAsync repo;

        string randString() => Guid.NewGuid().ToString();

        [SetUp]
        public void Setup()
        {
            // create new instances every time
            _userInfoSvc = new MockUserInfoService();
            repo = new MockFeatureUsageRepoAsync();
            service = new FeatureUsageService(_userInfoSvc, repo);
        }

        [Test]
        public async Task UserName_ShouldMatchRecordCurrentlyLoggedInUser()
        {
            // Act
            service.RecordUsage(randString());
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            var entity = repo._entities.FirstOrDefault();

            Assert.IsNotNull(entity, "Service did not record the feature in the database");
            Assert.AreEqual(_userInfoSvc.UserLogin, entity.UserName, "Service did not record the user name correctly");
        }

        [Test]
        public async Task FeatureName_ShouldMatchRecordUsage()
        {
            // Arrange 
            var featureName = randString();
            service.RecordUsage(featureName);

            // Act
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            Assert.IsNotNull(repo._entities.FirstOrDefault(), "Service did not record the feature in the database");
            Assert.AreEqual(featureName, repo._entities.First().FeatureName, "Service did not record the feature name correctly");
        }

        [Test]
        public async Task MultipleRecordsSaves_HasCorrectRecords()
        {
            // Arrange 
            var feature1UsedOnce = randString();
            var feature2UsedTwice = randString();
            var feature3UsedThreeTimes = randString();
            var feature1BenchmarkedOnce = randString();
            var feature2BenmarkedTwice = randString();

            // Act
            using(service.Benchmark(feature1BenchmarkedOnce))
            {
                service.RecordUsage(feature1UsedOnce);
                service.RecordUsage(feature2UsedTwice);
                await service.ForceSendUsageToDatabaseAsync();
                service.RecordUsage(feature3UsedThreeTimes);

                Thread.Sleep(35);
                using (service.Benchmark(feature2BenmarkedTwice))
                {
                    service.RecordUsage(feature3UsedThreeTimes);
                    Thread.Sleep(20);
                    service.RecordUsage(feature2UsedTwice);
                }
                using (service.Benchmark(feature2BenmarkedTwice))
                {
                    Thread.Sleep(15);
                    service.RecordUsage(feature3UsedThreeTimes);
                }
            }
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            var usageData = repo._entities;
            var feature1 = usageData.FirstOrDefault(r => r.FeatureName == feature1UsedOnce);
            var feature2 = usageData.FirstOrDefault(r => r.FeatureName == feature2UsedTwice);
            var feature3 = usageData.FirstOrDefault(r => r.FeatureName == feature3UsedThreeTimes);
            var benchmark1 = usageData.FirstOrDefault(r => r.FeatureName == feature1BenchmarkedOnce);
            var benchmark2 = usageData.FirstOrDefault(r => r.FeatureName == feature2BenmarkedTwice);

            Assert.AreEqual(1, feature1.UsageData.Length, "Feature1 was supposed recorded once");
            Assert.AreEqual(2, feature2.UsageData.Length, "Feature2 was supposed recorded twice");
            Assert.AreEqual(3, feature3.UsageData.Length, "Feature3 was supposed recorded three times");
            Assert.AreEqual(1, benchmark1.UsageData.Length, "Benchmark1 was supposed recorded once");
            Assert.AreEqual(2, benchmark2.UsageData.Length, "Benchmark2 was supposed recorded twice");
        }


        [Test]
        public async Task MultipleRecords_ForMultipleUsers_Saves_HasCorrectRecords()
        {
            // Arrange 
            var user1 = randString();
            var user2 = randString();

            var feature1UsedOnce = randString();
            var feature2UsedTwice = randString();
            var feature1BenchmarkedOnce = randString();
            _userInfoSvc.LoginUser(user1);

            // Act
            using (service.Benchmark(feature1BenchmarkedOnce))
            {
                service.RecordUsage(feature1UsedOnce);
                service.RecordUsage(feature2UsedTwice);
                await service.ForceSendUsageToDatabaseAsync();

                // note that the benchmark started recording time before we switch to user2
                // this is as intended, if a benchmark is to record login time, it needs to
                // grab the username at the end of the benchmark
                _userInfoSvc.LoginUser(user2); 

                service.RecordUsage(feature2UsedTwice);
                Thread.Sleep(35);
            }
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            var usageData = repo._entities;
            var feature1_user1 = usageData.FirstOrDefault(r => r.FeatureName == feature1UsedOnce && r.UserName == user1);
            var feature2_user1 = usageData.FirstOrDefault(r => r.FeatureName == feature2UsedTwice && r.UserName == user1);
            var feature2_user2 = usageData.FirstOrDefault(r => r.FeatureName == feature2UsedTwice && r.UserName == user2);
            var benchmark1_user1 = usageData.FirstOrDefault(r => r.FeatureName == feature1BenchmarkedOnce && r.UserName == user1);
            var benchmark1_user2 = usageData.FirstOrDefault(r => r.FeatureName == feature1BenchmarkedOnce && r.UserName == user2);

            Assert.AreEqual(1, feature1_user1.UsageData.Length, "Feature1 was supposed recorded once for user1");
            Assert.AreEqual(1, feature2_user1.UsageData.Length, "Feature1 was supposed recorded once for user1");
            Assert.AreEqual(1, feature2_user2.UsageData.Length, "Feature1 was supposed recorded once for user2");
            Assert.AreEqual(1, benchmark1_user2.UsageData.Length, "Benchmark2 was supposed recorded once for user2");
        }


        [Test]
        public void HasRecords_ShouldBeTrueAfterAdding_UsageRecord()
        {
            // Act
            service.RecordUsage(randString());

            // Assert
            Assert.IsTrue(service.HasRecords, "No in-Memory records were stored after adding a record.");
        }

        [Test]
        public async Task RecordsShouldBeClearedOutAfterSaving()
        {
            // Arrange
            service.RecordUsage(randString());

            // Act
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            Assert.IsFalse(service.HasRecords, "In-Memory records were not cleared after saving.");
        }

        [Test]
        public void HasRecords_ShouldBeTrueAfterAdding_Benchmark()
        {
            // Act
            using(var benchmark = service.Benchmark(randString()))
            {
                Thread.Sleep(45);
            }

            // Assert
            Assert.IsTrue(service.HasRecords, "No in-Memory records were stored after the benchmark.");
        }

    }
}