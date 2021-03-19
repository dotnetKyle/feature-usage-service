using NUnit.Framework;
using FeatureUsage;
using FeatureUsage.DAO;
using FeatureUsage.Entities;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace FeatureUsageTests
{
    public class Tests
    {
        MockUserInfoService _userInfoSvc;
        FeatureUsageService service;
        MockFeatureUsageRepoAsync repo;

        void wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
        [SetUp]
        public void Setup()
        {
            _userInfoSvc = new MockUserInfoService();
            repo = new MockFeatureUsageRepoAsync();

            service = new FeatureUsageService(_userInfoSvc, repo);
        }

        [Test]
        public async Task TestGenericRecordUsage()
        {
            // Arrange 
            const string featureName = "MyFeature1";
            // Act
            service.RecordUsage(featureName);
            await service.ForceSendUsageToDatabaseAsync();

            // Assert
            var entity = repo._entities.FirstOrDefault();

            Assert.IsNotNull(entity, "Service did not record the feature in the database");
            Assert.AreEqual(featureName, entity.FeatureName, "Service did not record the feature name correctly");
            Assert.AreEqual(_userInfoSvc.UserLogin, entity.UserName, "Service did not record the user name correctly");
        }
    }
}