using FeatureUsage;
using System;

namespace FeatureSeedDataCreator
{
    public class MockUserInfoService : IUserInfoService
    {
        Random _rand;
        string[] _users;
        public MockUserInfoService() 
        {
            _rand = new Random();
            _users = new string[]
            {
                "ncMichaelsFG",
                "ncSimpsonKR",
                //Environment.UserName,
                //"SmithJA",
                //"FranklinC",
                //"JohnsonPL",
                //"HoganLA",
                //"ThompsonJ",
                //"CampbellR",
                //"SmithTY",
                //"CruiseRL",
            };
        }

        public string UserLogin
        {
            get
            {
                var randIndex = _rand.Next(0, _users.Length);

                var user = _users[randIndex];

                LastUser = user;

                return user;
            }
        }

        public string LastUser { get; private set; }
    }
}
