using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using MediaViewer.Logging;
using MediaViewer.Utils;
using Microsoft.Win32;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config",Watch=true)]

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            log.Info("Starting MediaViewer v" + version.ToString());
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
             OpenFileDialog openFileDialog = WindowsUtils.createOpenMediaFileDialog(false);

             openFileDialog.ShowDialog();

        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            log4net.Appender.IAppender[] appenders = log4net.LogManager.GetRepository().GetAppenders();
            VisualAppender appender = (VisualAppender)(appenders[0]);
            appender.LogWindow.Show();
        }
    }
}
