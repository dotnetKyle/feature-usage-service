#define SEEDING

using Features;
using FeatureUsage;
using FeatureUsage.DAO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace FeatureSeedDataCreator
{
    class Program
    {
        /* This program is designed to not overwhelm a fre tier mongodb server
         * by sending a million rows at once */
        static bool serviceIsStopping = false;
        static bool serviceIsRunning = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Seeding Features...");

            Console.WriteLine("What date range would you like to seed the features for?");
            GetUtcTimeRanges(out DateTime startTime, out DateTime endTime);

            var userService = new MockUserInfoService();
            var featureUsage = new FeatureUsageService(userService, new FeatureUsageRepoAsync(AppSettings.GetSettings()));
            FeaturesList.Init(featureUsage);

            Console.WriteLine("Press 'q' to quit.");

            await Run(featureUsage, userService, startTime, endTime);
        }

        static void GetUtcTimeRanges(out DateTime startTime, out DateTime endTime)
        {
            PrintDateHelp();
            Console.WriteLine("\r\nChoose a start time...");
            startTime = GetLocalDateTime()
                .ToUniversalTime();

            Console.WriteLine("\r\nChoose an end time...");
            endTime = GetLocalDateTime()
                .ToUniversalTime();

            if (startTime == endTime)
            {
                Console.WriteLine("Start Time/End Time can't be the same.");
                Console.WriteLine("Try Again..");
                GetUtcTimeRanges(out startTime, out endTime);
            }
            else if(startTime > endTime)
            {
                Console.WriteLine("Start Time can't be after End Time.");
                Console.WriteLine("Try Again..");
                GetUtcTimeRanges(out startTime, out endTime);
            }

            // modify the console title so the user can remember what he/she entered
            var localStart = $"{startTime.ToLocalTime().ToShortDateString()} {startTime.ToLocalTime().ToShortTimeString()}";
            var localEnd = $"{startTime.ToLocalTime().ToShortDateString()} {startTime.ToLocalTime().ToShortTimeString()}";
            var utcStart = $"{startTime.ToShortDateString()} {startTime.ToShortTimeString()}";
            var utcEnd = $"{startTime.ToShortDateString()} {startTime.ToShortTimeString()}";

            Console.Title = $"TimeRange: {localStart}-{localEnd} | UTC:{utcStart}-{utcEnd}";
        }

        static void PrintDateHelp()
        {
            Console.WriteLine("How to add a date:");
            Console.WriteLine($"   Type a date \"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\", or..");
            Console.WriteLine("    Zero for right now");
            Console.WriteLine("    -1 for 1 hour in the past");
            Console.WriteLine("    3 for 3 hours in the future");
        }
        static DateTime GetLocalDateTime()
        {
            int trys = 0;
            DateTime? startTime = null;
            while (startTime.HasValue == false)
            {
                var startTimeStr = Console.ReadLine();

                int hours;
                DateTime dtg;
                if (int.TryParse(startTimeStr, out hours))
                {
                    startTime = DateTime.Now.AddHours(hours);
                }
                else if (DateTime.TryParse(startTimeStr, out dtg))
                {
                    startTime = dtg;
                }
                else
                {
                    trys++;
                    Console.WriteLine("Couldn't read value, try again..");
                    if (trys > 1)
                        PrintDateHelp();
                }
            }

            Console.WriteLine($"Date Entered: {startTime.Value}");
            return startTime.Value;
        }

        static async Task Stop(FeatureUsageService service, MockUserInfoService userService)
        {
            Console.WriteLine("\r\nStopping...");
            Console.Write("Sending recorded features to MongoDB...");
            await service.ForceSendUsageToDatabaseAsync();
            Console.WriteLine(" Done!");
            Console.WriteLine($"Features sent as {userService.LastUser}");

            serviceIsRunning = false;

            Console.WriteLine("Goodbye!");
            Thread.Sleep(1000);
        }
        static async Task Run(FeatureUsageService service, MockUserInfoService userService, DateTime startTimeRange, DateTime stopTimeRange)
        {
#if SEEDING

            try
            {
                serviceIsRunning = true;

                var rand = new Random();

                var listOfFeatures = new List<Feature> { 
                    FeaturesList.MainWindow.Cancel,
                    FeaturesList.MainWindow.Open,
                    FeaturesList.MainWindow.Close,
                    FeaturesList.MainWindow.EditMenu.Cut,
                    //FeaturesList.MainWindow.EditMenu.Copy,
                    FeaturesList.MainWindow.EditMenu.Copy,
                    FeaturesList.MainWindow.EditMenu.Paste,
                    //FeaturesList.MainWindow.EditMenu.Paste,
                    //FeaturesList.MainWindow.EditMenu.Paste,
                    FeaturesList.MainWindow.Save,
                    //FeaturesList.MainWindow.Save,
                    //FeaturesList.MainWindow.Save,
                    FeaturesList.SecondWindow.Open,
                    FeaturesList.SecondWindow.Close,
                    FeaturesList.MainWindow.LongRunningService,
                    FeaturesList.MainWindow.LongRunningService,
                    FeaturesList.MainWindow.LongRunningService,
                    //FeaturesList.Users.Administrator.ApproveAccount,
                    FeaturesList.Users.Administrator.DisableAccount,
                    FeaturesList.Users.Administrator.DisableAccount,
                    FeaturesList.Users.Administrator.DisableAccount,
                    //FeaturesList.Users.Administrator.ResetPassword,
                    //FeaturesList.Users.Administrator.ResetPassword,
                    //FeaturesList.Users.Administrator.ResetPassword,
                    //FeaturesList.Users.Administrator.UnlockAccount,
                    //FeaturesList.Users.Administrator.UnlockAccount,
                    //FeaturesList.Users.Administrator.UnlockAccount,
                    //FeaturesList.Users.Administrator.UnlockAccount,
                    FeaturesList.Users.Reports.Report1,
                    //FeaturesList.Users.Reports.Report2,
                    //FeaturesList.Users.Reports.Report3,
                    //FeaturesList.Users.Reports.Report4,
                    //FeaturesList.Users.Reports.Report4,
                    //FeaturesList.Users.Reports.Report4,
                    FeaturesList.Users.Reports.NightReport5,
                    FeaturesList.Users.Reports.NightReport5,
                    FeaturesList.Users.Reports.NightReport5,
                    FeaturesList.Users.Reports.NightReport5,
                };

                int count = 0;
                int overallCount = 0;

                while (serviceIsRunning)
                {
                    // force send features every 5-ish features
                    if(count > rand.Next(3, 8))
                    {
                        Console.Write("Sending recorded features to MongoDB...");
                        await service.ForceSendUsageToDatabaseAsync();
                        Console.WriteLine(" Done!");
                        Console.WriteLine($"Features sent as {userService.LastUser}");

                        count = 0;
                    }

                    var timespan = stopTimeRange - startTimeRange;
                    var dblTotalHours = timespan.TotalHours;
                    var randDbl = rand.NextDouble();
                    var randTotalHours = dblTotalHours * randDbl;
                    var randTime = startTimeRange.AddHours(randTotalHours);
                    var randMockTimeUTC = randTime.ToUniversalTime();

                    var randomIndex = rand.Next(0, listOfFeatures.Count);

                    var feature = listOfFeatures[randomIndex];

                    if(feature.IsBenchmark)
                    {
                        using (feature.BenchmarkFeatureUse(randMockTimeUTC))
                        {
                            Thread.Sleep(rand.Next(10, 2000));
                        }
                        Console.WriteLine($"{overallCount+1} Benchmark recorded ({feature.FeatureName})");
                    }
                    else
                    {
                        feature.RecordFeatureUse(randMockTimeUTC);
                        Console.WriteLine($"{overallCount + 1} Record recorded ({feature.FeatureName})");
                    }

                    // increment sent feature count
                    count++;
                    overallCount++;

                    // don't wait for the user's keypress
                    var t = Task.Run(() => {
                        var key = Console.ReadKey().KeyChar;
                        if (key == 'q')
                            serviceIsStopping = true;
                        else
                            Console.WriteLine("Type 'q' to quit.");
                    });

                    // sleep for a bit before continuing
                    var randMs = rand.Next(3800, 4201);
                    Thread.Sleep(randMs);

                    if(serviceIsStopping)
                    {
                        await Stop(service, userService);
                    }
                }
            }
            catch(Exception ex)
            {
                // show error
                Console.WriteLine("There was an unexpected error:");
                Console.WriteLine(ex);

                // stop the app here so the user can read the error
                Console.WriteLine("\r\n\r\nPress enter to exit.");
                Console.ReadLine();
            }

#endif
        }
    }
}
