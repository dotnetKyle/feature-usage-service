using FeatureUsage;
using System;

namespace FeatureUsage_IntegrationTest_WPF
{
    public class UserInfoService : IUserInfoService
    {
        public UserInfoService() { }

        public string UserLogin
        {
            get
            {
                return Environment.UserName;
            }
        }
    }
}
