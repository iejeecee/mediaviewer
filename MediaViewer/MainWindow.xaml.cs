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
using MediaViewer.ImagePanel;
using System.ComponentModel;
using MediaViewer.About;
using MediaViewer.DirectoryBrowser;
using MediaViewer.VideoPanel;
using MediaViewer.MediaFileBrowser;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config",Watch=true)]

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private RotationView rotationView;
        private ScaleView scaleView;

        private ImageViewModel imageViewModel;
        private MainWindowViewModel mainWindowViewModel;
        private MediaFileBrowserViewModel mediaFileBrowserViewModel;

        private VideoViewModel videoViewModel;

        private string currentBrowsingDirectory;

        public MainWindow()
        {
           
            InitializeComponent();
          
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            log.Info("Starting MediaViewer v" + version.ToString());

            AppDomain currentDomain = AppDomain.CurrentDomain;
            PrintLoadedAssemblies(currentDomain);
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(assemblyLoadEventHandler);

            XMPLib.MetaData.setLogCallback(new XMPLib.MetaData.LogCallbackDelegate(metaData_logCallback));

            initializeImageView();
            initializeVideoView();
            initializeMediaFileBrowser();

            mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;

            mainWindowViewModel.PropertyChanged += new PropertyChangedEventHandler(mainWindowViewModel_PropertyChanged);

            currentBrowsingDirectory = "";
       
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_PathSelected", new Action<String>(browsingDirectory_IsSelected));

            mediaFileBrowser.Loaded += new RoutedEventHandler(mediaFileBrowser_Loaded);

            VideoTestWindow test = new VideoTestWindow();
                       
            test.Show();
            test.Activate();
            
        }

        void mediaFileBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Args.Length != 0)
            {               
                loadAndDisplayMedia(App.Args[0]);
                mediaFileBrowserViewModel.BrowsePath = App.Args[0];
            } 
        }

        void browsingDirectory_IsSelected(String path)
        {
            setTitle(path);
        }

        void initializeImageView()
        {
            imageViewModel = new ImageViewModel();
            imageView.DataContext = imageViewModel;

            rotationView = new RotationView();
            rotationView.Closing += new CancelEventHandler((sender, e) =>
            {
                e.Cancel = true;
                ((Window)sender).Hide();
                rotateImageCheckBox.IsChecked = false;
            });
            rotationView.DataContext = imageViewModel;

            scaleView = new ScaleView();
            scaleView.Closing += new CancelEventHandler((sender, e) =>
            {
                e.Cancel = true;
                ((Window)sender).Hide();
                scaleImageCheckBox.IsChecked = false;
            });
            scaleView.DataContext = imageViewModel;

            imageToolBar.DataContext = imageViewModel;
        }

        void initializeVideoView()
        {
            videoViewModel = new VideoViewModel();
            videoView.DataContext = videoViewModel;
        }

        void initializeMediaFileBrowser()
        {
            mediaFileBrowserViewModel = (MediaFileBrowserViewModel)mediaFileBrowser.DataContext;

            mediaFileBrowserToolBar.DataContext = mediaFileBrowserViewModel;
        }

        void mainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("CurrentImageLocation"))
            {
                if (mainWindowViewModel.CurrentImageLocation != null)
                {
                    loadAndDisplayImage(mainWindowViewModel.CurrentImageLocation);
                }
            }
            else if (e.PropertyName.Equals("CurrentVideoLocation"))
            {
                if (mainWindowViewModel.CurrentVideoLocation != null)
                {
                    loadAndDisplayVideo(mainWindowViewModel.CurrentVideoLocation);
                }
            }

        }

        void setTitle(string info = "")
        {
            if (string.IsNullOrEmpty(info))
            {
                mainWindow.Title = "MediaViewer";
            }
            else
            {
                mainWindow.Title = info + " - MediaViewer";
            }
        }

        private void loadAndDisplayMedia(string media)
        {
            if (Utils.MediaFormatConvert.isImageFile(media))
            {
                loadAndDisplayImage(media);

            }
            else if (Utils.MediaFormatConvert.isVideoFile(media))
            {
                loadAndDisplayVideo(media);
            }

        }

        private void loadAndDisplayImage(string location)
        {
            showImageView(location);
            imageViewModel.LoadImageAsyncCommand.DoExecute(location);
        }

        private void loadAndDisplayVideo(string location)
        {
            showVideoView(location);
            videoViewModel.VideoLocation = location;
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

                 imageViewModel.LoadImageAsyncCommand.DoExecute(openFileDialog.FileName);
             }
        

        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutView about = new AboutView();
            AboutViewModel aboutViewModel = new AboutViewModel();
            about.DataContext = aboutViewModel;

            about.ShowDialog();

        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            log4net.Appender.IAppender[] appenders = log4net.LogManager.GetRepository().GetAppenders();
            VisualAppender appender = (VisualAppender)(appenders[0]);

            LogView logView = new LogView();
            logView.DataContext = appender.LogViewModel;

            logView.Show();
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
                //imageView.Stretch = Stretch.Uniform;
            }
            else
            {
                //imageView.Stretch = Stretch.None;
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
                rotationView.Show();
            }
            else
            {
                rotationView.Hide();
            }
        }

        private void scaleImageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (scaleImageCheckBox.IsChecked.Value == true)
            {
                scaleView.Show();            
            }
            else
            {
                scaleView.Hide();
            }
        }

        private void showMediaFileBrowser()
        {

            imageView.Visibility = Visibility.Hidden;
            videoView.Visibility = Visibility.Hidden;
            mediaFileBrowser.Visibility = Visibility.Visible;
         
            videoToolbarCheckBox.IsChecked = false;
            imageToolbarCheckBox.IsChecked = false;

            imageToolBar.Visibility = Visibility.Hidden;
            mediaFileBrowserToolBar.Visibility = Visibility.Visible;
         
            setTitle(currentBrowsingDirectory);
        }

        private void showImageView(string location)
        {

            imageView.Visibility = Visibility.Visible;
            videoView.Visibility = Visibility.Hidden;
            mediaFileBrowser.Visibility = Visibility.Hidden;

            videoToolbarCheckBox.IsChecked = false;
            mediaFileBrowserToolbarCheckBox.IsChecked = false;

            imageToolBar.Visibility = Visibility.Visible;
            mediaFileBrowserToolBar.Visibility = Visibility.Hidden;

            setTitle(location);
        }

        private void showVideoView(string location)
        {

            imageView.Visibility = Visibility.Hidden;
            videoView.Visibility = Visibility.Visible;
            mediaFileBrowser.Visibility = Visibility.Hidden;

            imageToolbarCheckBox.IsChecked = false;
            mediaFileBrowserToolbarCheckBox.IsChecked = false;

            imageToolBar.Visibility = Visibility.Hidden;
            mediaFileBrowserToolBar.Visibility = Visibility.Hidden;

            setTitle(location);
        }

        private void videoToolbarCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showVideoView(mainWindowViewModel.CurrentVideoLocation);
        }

        private void imageToolbarCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showImageView(mainWindowViewModel.CurrentImageLocation);
        }

        private void mediaFileBrowserToolbarCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showMediaFileBrowser();
        }
    }
}
