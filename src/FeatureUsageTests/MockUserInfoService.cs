using FeatureUsage;

namespace FeatureUsageTests
{
    class MockUserInfoService : IUserInfoService
    {
        public string UserLogin => "TestUser";
    }
}
