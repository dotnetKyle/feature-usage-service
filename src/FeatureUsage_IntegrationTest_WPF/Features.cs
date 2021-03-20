using FeatureUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureUsage_IntegrationTest_WPF
{
    public static class Features
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
        /// <summary> This Feature is not implemented yet </summary>
        public static class SecondWindow
        {
            public static Feature Open { get; set; }
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
            MainWindow.LongRunningService = new Feature(serviceReference, $"{mainWindow_}Long Running Service");

            // MainWindow EditMenu Features
            MainWindow.EditMenu.Cut = new Feature(serviceReference, $"{mainWindEditMenu_}Cut");
            MainWindow.EditMenu.Copy = new Feature(serviceReference, $"{mainWindEditMenu_}Copy");
            MainWindow.EditMenu.Paste = new Feature(serviceReference, $"{mainWindEditMenu_}Paste");

            SecondWindow.Open = new Feature(serviceReference, secondWindow + $"{secondWindow_}Open");
        }
    }
}
