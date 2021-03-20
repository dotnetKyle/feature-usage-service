using FeatureUsage;

namespace FeatureUsageTests
{
    class MockUserInfoService : IUserInfoService
    {
        string _currentUser = "TestUser";
        public string UserLogin => _currentUser;

        public void LoginUser(string username)
        {
            _currentUser = username;
        }
    }
}
