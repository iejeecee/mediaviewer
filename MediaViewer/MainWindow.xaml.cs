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
using MediaViewer.Timers;
using MediaViewer.Utils.Windows;
using System.Windows.Threading;
using MediaViewer.MediaFileModel.Watcher;
using System.IO;

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
    
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);
       
            InitializeComponent();
          
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            log.Info("Starting MediaViewer v" + version.ToString());

            AppDomain currentDomain = AppDomain.CurrentDomain;
            PrintLoadedAssemblies(currentDomain);
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(assemblyLoadEventHandler);

            XMPLib.MetaData.setLogCallback(new XMPLib.MetaData.LogCallbackDelegate(metaData_logCallback));

            initializeImageView();      
            initializeMediaFileBrowser();

            mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;
            

            mainWindowViewModel.PropertyChanged += new PropertyChangedEventHandler(mainWindowViewModel_PropertyChanged);
       
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_PathSelected", new Action<String>(browsingDirectory_IsSelected));

            mediaFileBrowser.Loaded += new RoutedEventHandler(mediaFileBrowser_Loaded);

            //MediaDatabase.Test.test();
            MediaFileModel.Watcher.MediaFileWatcher.Instance.MediaState.ItemIsSelectedChanged += new EventHandler((o,e) =>
            {
                if (mediaFileBrowser.Visibility == Visibility.Visible)
                {
                    List<MediaFileItem> selected = MediaFileModel.Watcher.MediaFileWatcher.Instance.MediaState.getSelectedItems();
                 
                    if (selected.Count > 0)
                    {
                        setTitle(mediaFileBrowserViewModel.BrowsePath + " - " + selected.Count.ToString() + " Item(s) Selected");
                    }
                    else
                    {
                        setTitle(mediaFileBrowserViewModel.BrowsePath);
                    }
                }
            });
        
          
        }

        private void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("Unhandled exception: " + e.ExceptionObject.ToString() + " Terminating: " + e.IsTerminating.ToString());            
        }

    
        void mediaFileBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Args.Length != 0)
            {               
                loadAndDisplayMedia(App.Args[0]);               
                mediaFileBrowserViewModel.BrowsePath = App.Args[0];          
               
            }

           

           // VideoTestWindow test = new VideoTestWindow();

            //test.Show();
            //test.Activate();
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
            });
            rotationView.DataContext = imageViewModel;

            scaleView = new ScaleView();
            scaleView.Closing += new CancelEventHandler((sender, e) =>
            {
                e.Cancel = true;
                ((Window)sender).Hide();               
            });
            scaleView.DataContext = imageViewModel;

            imageToolBar.DataContext = imageViewModel;
           
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
                mainWindowViewModel.WindowTitle = "MediaViewer";
            }
            else
            {
                mainWindowViewModel.WindowTitle = info + " - MediaViewer";
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
            VideoPlayerViewModel videoPlayerViewModel = (VideoPlayerViewModel)videoView.DataContext;
            videoPlayerViewModel.open(location);
            videoPlayerViewModel.PlayCommand.DoExecute();
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
             OpenFileDialog openFileDialog = Utils.Windows.FileDialog.createOpenMediaFileDialog(false);

             if (openFileDialog.ShowDialog() == true)
             {

                 imageViewModel.LoadImageAsyncCommand.DoExecute(openFileDialog.FileName);
             }
        

        }

    

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MediaViewer.Settings.AppSettings.save();        
            Dispatcher.InvokeShutdown();
        }

        private void imageToolBarButton_Click(object sender, RoutedEventArgs e)
        {
           
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
      
        private void showMediaFileBrowser()
        {

            imageView.Visibility = Visibility.Hidden;
            videoView.Visibility = Visibility.Hidden;
            mediaFileBrowser.Visibility = Visibility.Visible;
         
            videoToolbarCheckBox.IsChecked = false;
            imageToolbarCheckBox.IsChecked = false;

            imageToolBar.Visibility = Visibility.Hidden;
            mediaFileBrowserToolBar.Visibility = Visibility.Visible;
         
            setTitle(mediaFileBrowserViewModel.BrowsePath);
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

            setTitle(System.IO.Path.GetFileName(location));
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

            setTitle(System.IO.Path.GetFileName(location));
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
