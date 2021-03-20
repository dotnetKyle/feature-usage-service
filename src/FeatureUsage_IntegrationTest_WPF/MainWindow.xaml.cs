using FeatureUsage;
using FeatureUsage.DAO;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FeatureUsage_IntegrationTest_WPF
{
    public partial class MainWindow : Window
    {
        FeatureUsageService featureUsage;
        Random rand;

        public MainWindow()
        {
            featureUsage = new FeatureUsageService(new UserInfoService(), new FeatureUsageRepoAsync(AppSettings.GetSettings()));
            Features.Init(featureUsage);

            InitializeComponent();
            
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            rand = new Random();
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Features.MainWindow.Close.RecordFeatureUse();
                await featureUsage.ForceSendUsageToDatabaseAsync();
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Features.MainWindow.Open.RecordFeatureUse();
        }

        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            Features.MainWindow.EditMenu.Cut.RecordFeatureUse();
        }
        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            Features.MainWindow.EditMenu.Copy.RecordFeatureUse();
        }
        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            Features.MainWindow.EditMenu.Paste.RecordFeatureUse();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Features.MainWindow.Save.RecordFeatureUse();
                await featureUsage.ForceSendUsageToDatabaseAsync();
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Features.MainWindow.Cancel.RecordFeatureUse();
        }

        private async void btnLongRunningProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // using the benchmark
                using (Features.MainWindow.LongRunningService.BenchmarkFeatureUse()) 
                {
                    Dispatcher.Invoke(() => btnLongRunningProcess.Content = "Running...");
                    await Task.Delay(rand.Next(0, 500));
                    Dispatcher.Invoke(() => btnLongRunningProcess.Content = "Running..");
                    await Task.Delay(rand.Next(0, 1000));
                    Dispatcher.Invoke(() => btnLongRunningProcess.Content = "Running.");
                    await Task.Delay(rand.Next(0, 1000));
                    Dispatcher.Invoke(() => btnLongRunningProcess.Content = "Complete!");
                    await Task.Delay(rand.Next(0, 500));
                    Dispatcher.Invoke(() => btnLongRunningProcess.Content = "Start Long Running Process");
                } // <-- it will stop the time here
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
