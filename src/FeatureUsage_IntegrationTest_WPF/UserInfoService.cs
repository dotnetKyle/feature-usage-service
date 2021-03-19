using FeatureUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureUsage_IntegrationTest_WPF
{
    public class UserInfoService : IUserInfoService
    {
        public UserInfoService()
        {

        }
        public string UserLogin
        {
            get
            {
                return Environment.UserName;
            }
        }
    }
}
