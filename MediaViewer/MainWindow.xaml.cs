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
using MediaViewer.MVImage.Panel;
using MediaViewer.MVImage.Rotate;
using Microsoft.Win32;
using MediaViewer.MVImage.Scale;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config",Watch=true)]

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private RotateWindow rotate;
        private ScaleWindow scale;

        public MainWindow()
        {

            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            log.Info("Starting MediaViewer v" + version.ToString());

            AppDomain currentDomain = AppDomain.CurrentDomain;
            PrintLoadedAssemblies(currentDomain);
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(assemblyLoadEventHandler);

            XMPLib.MetaData.setLogCallback(new XMPLib.MetaData.LogCallbackDelegate(metaData_logCallback));

            rotate = null;
        }

        private void metaData_logCallback(XMPLib.MetaData.LogLevel level, string message)
        {

            switch (level)
            {

                case XMPLib.MetaData.LogLevel.ERROR:
                    {
                        log.Error(message);
                        break;
                    }
                case XMPLib.MetaData.LogLevel.WARNING:
                    {

                        log.Warn(message);
                        break;
                    }
                case XMPLib.MetaData.LogLevel.INFO:
                    {

                        log.Info(message);
                        break;
                    }
                default:
                    {
                        System.Diagnostics.Debug.Assert(false);
                        break;
                    }
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
             OpenFileDialog openFileDialog = WindowsUtils.createOpenMediaFileDialog(false);

             if (openFileDialog.ShowDialog() == true)
             {

                 imagePanel.loadImage(openFileDialog.FileName);
             }
        

        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //AboutWindow about = new AboutWindow();
            //about.ShowDialog();

            TestWindow test = new TestWindow();
            test.Show();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            log4net.Appender.IAppender[] appenders = log4net.LogManager.GetRepository().GetAppenders();
            VisualAppender appender = (VisualAppender)(appenders[0]);
            appender.LogWindow.Show();
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.InvokeShutdown();
        }

        private void imageToolBarButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void autoScaleImageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (autoScaleImageCheckBox.IsChecked == true)
            {
                imagePanel.Stretch = Stretch.Uniform;
            }
            else
            {
                imagePanel.Stretch = Stretch.None;
            }
        }


        static void PrintLoadedAssemblies(AppDomain domain)
        {          
            foreach (Assembly a in domain.GetAssemblies())
            {
                log.Info("Assembly loaded: " + a.FullName);               
            }        
        }

        static void assemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            log.Info("Assembly loaded: " + args.LoadedAssembly.FullName);           
        }

        private void rotateImageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (rotateImageCheckBox.IsChecked.Value == true)
            {
                rotate = new RotateWindow();
                rotate.RotationChanged += new EventHandler((s, a) => imagePanel.Rotation = rotate.Rotation);
                rotate.Closed += new EventHandler((s, a) => rotateImageCheckBox.IsChecked = false);
                rotate.Rotation = imagePanel.Rotation;
                rotate.Show();
            }
            else
            {
                rotate.Close();
            }
        }

        private void scaleImageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (scaleImageCheckBox.IsChecked.Value == true)
            {
                scale = new ScaleWindow();
                scale.ScaleChanged += new EventHandler((s, a) => imagePanel.Scale = scale.Scale);
                scale.ResetScale += new EventHandler((s, a) =>
                {
                    imagePanel.setDefaultScale();
                    scale.Scale = imagePanel.Scale;
                });
                scale.Closed += new EventHandler((s, a) => scaleImageCheckBox.IsChecked = false);
                scale.Scale = imagePanel.Scale;
                scale.Show();
            }
            else
            {
                scale.Close();
            }
        }

        private void flipImageHorizontalCheckBox_Click(object sender, RoutedEventArgs e)
        {
            imagePanel.FlipX = flipImageHorizontalCheckBox.IsChecked.Value;
        }

        private void flipImageVerticalCheckBox_Click(object sender, RoutedEventArgs e)
        {
            imagePanel.FlipY = flipImageVerticalCheckBox.IsChecked.Value;
        }
    }
}
