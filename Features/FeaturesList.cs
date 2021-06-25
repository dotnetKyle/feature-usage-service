using FeatureUsage;

namespace Features
{
    public static class FeaturesList
    {
        /// <summary> The App's Main Window </summary>
        public static class MainWindow
        {
            public static Feature Open { get; set; }
            public static Feature Close { get; set; }

            public static Feature LongRunningService { get; set; }
            public static Feature Cancel { get; set; }
            public static Feature Save { get; set; }

            public static class EditMenu
            {
                public static Feature Cut { get; set; }
                public static Feature Copy { get; set; }
                public static Feature Paste { get; set; }

            }
        }

        public static class SecondWindow
        {
            public static Feature Open { get; set; }
            public static Feature Close { get; set; }
        }

        /// <summary>For seed data only</summary>
        public static class Users
        { 
            public static class Reports
            {
                public static Feature Report1 { get; set; }
                public static Feature Report2 { get; set; }
                public static Feature Report3 { get; set; }
                public static Feature Report4 { get; set; }
                public static Feature NightReport5 { get; set; }
            }

            public static class Administrator
            {
                public static Feature ResetPassword { get; set; }
                public static Feature UnlockAccount { get; set; }
                public static Feature ApproveAccount { get; set; }
                public static Feature DisableAccount { get; set; }
            }
        }


        public static void Init(FeatureUsageService serviceReference)
        {
            // Feature Description Separator
            const string _ = "|";

            const string mainWindow = "Main Window";
            const string secondWindow = "Second Window";
            const string mainWindow_ = mainWindow + _;
            const string secondWindow_ = secondWindow + _;

            const string mainWindEditMenu = mainWindow + _ + "Edit Menu";
            const string mainWindEditMenu_ = mainWindEditMenu + _;

            // Create MainWindow Features
            MainWindow.Open = new Feature(serviceReference, $"{mainWindow_}Open");
            MainWindow.Close = new Feature(serviceReference, $"{mainWindow_}Close");

            MainWindow.Cancel = new Feature(serviceReference, $"{mainWindow_}Cancel");
            MainWindow.Save = new Feature(serviceReference, $"{mainWindow_}Save");
            MainWindow.LongRunningService = new Feature(serviceReference, $"{mainWindow_}Long Running Service"
#if SEEDING
                , true
#endif
                );

            // MainWindow EditMenu Features
            MainWindow.EditMenu.Cut = new Feature(serviceReference, $"{mainWindEditMenu_}Cut");
            MainWindow.EditMenu.Copy = new Feature(serviceReference, $"{mainWindEditMenu_}Copy");
            MainWindow.EditMenu.Paste = new Feature(serviceReference, $"{mainWindEditMenu_}Paste");

            SecondWindow.Open = new Feature(serviceReference, $"{secondWindow_}Open");
            SecondWindow.Close = new Feature(serviceReference, $"{secondWindow_}Close");

            Users.Administrator.ApproveAccount = new Feature(serviceReference, "Users|Administrator|Approve Account");
            Users.Administrator.DisableAccount = new Feature(serviceReference, "Users|Administrator|Disable Account");
            Users.Administrator.ResetPassword = new Feature(serviceReference, "Users|Administrator|Reset Password ");
            Users.Administrator.UnlockAccount = new Feature(serviceReference, "Users|Administrator|Unlock Account ");

            Users.Reports.Report1 = new Feature(serviceReference, "Users|Reports|Daily Financial Report"
#if SEEDING
                , true
#endif
                );
            Users.Reports.Report2 = new Feature(serviceReference, "Users|Reports|Weekly Averages"
#if SEEDING
                , true
#endif
                );
            Users.Reports.Report3 = new Feature(serviceReference, "Users|Reports|Daily Active Users"
#if SEEDING
                , true
#endif
                );
            Users.Reports.Report4 = new Feature(serviceReference, "Users|Reports|Report4"
#if SEEDING
                , true
#endif
                );
            Users.Reports.NightReport5 = new Feature(serviceReference, "Users|Reports|Night Report"
#if SEEDING
                , true
#endif
                );
        }
    }
}
