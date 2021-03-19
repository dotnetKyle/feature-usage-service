using FeatureUsage;
using FeatureUsage.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeatureUsage_IntegrationTest_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FeatureUsageService featureUsage;

        public MainWindow()
        {
            featureUsage = new FeatureUsageService(new UserInfoService(), new FeatureUsageRepoAsync(AppSettings.GetSettings()));
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                featureUsage.RecordUsage("MainWindow|Close");
                await featureUsage.ForceSendUsageToDatabaseAsync();
            }
            catch(Exception ex)
            {

            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            featureUsage.RecordUsage("MainWindow|Load");
        }

        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            featureUsage.RecordUsage("Edit Menu|Cut");
        }
        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            featureUsage.RecordUsage("Edit Menu|Copy");
        }
        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            featureUsage.RecordUsage("Edit Menu|Paste");
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                featureUsage.RecordUsage("MainForm|Save");
                await featureUsage.ForceSendUsageToDatabaseAsync();
            }
            catch(Exception ex)
            {

            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            featureUsage.RecordUsage("MainForm|Cancel");
        }

        
    }
}
