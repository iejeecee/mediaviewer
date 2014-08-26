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
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.MediaFileBrowser;
using MediaViewer.ImagePanel;
using MediaViewer.VideoPanel;
using MediaViewer.MediaFileModel.Watcher;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>    
    [Export]   
    public partial class Shell : Window, IPartImportsSatisfiedNotification
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Import(AllowRecomposition = false)]
        public IRegionManager RegionManager;

        public static ShellViewModel ShellViewModel { get; set; }

        public Shell()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);

            InitializeComponent();

            XMPLib.MetaData.setLogCallback(new XMPLib.MetaData.LogCallbackDelegate(metaData_logCallback));

            AppDomain currentDomain = AppDomain.CurrentDomain;
            PrintLoadedAssemblies(currentDomain);
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(assemblyLoadEventHandler);

            this.Closing += Shell_Closing;
        }

        public void OnImportsSatisfied()
        {
            ShellViewModel = new ShellViewModel(MediaFileWatcher.Instance, RegionManager);

            DataContext = ShellViewModel;

            this.RegionManager.RegisterViewWithRegion(RegionNames.MainNavigationToolBarRegion, typeof(ImageNavigationItemView));
            this.RegionManager.RegisterViewWithRegion(RegionNames.MainNavigationToolBarRegion, typeof(VideoNavigationItemView));
            this.RegionManager.RegisterViewWithRegion(RegionNames.MainNavigationToolBarRegion, typeof(MediaFileBrowserNavigationItemView));
         
            String location = App.Args.Count() > 0 ? App.Args[0] : "";

            if (Utils.MediaFormatConvert.isImageFile(location))
            {
                ShellViewModel.navigateToImageView(location);
                MediaFileWatcher.Instance.Path = Utils.FileUtils.getPathWithoutFileName(location);
            }
            else if (Utils.MediaFormatConvert.isVideoFile(location))
            {
                ShellViewModel.navigateToVideoView(location);
                MediaFileWatcher.Instance.Path = Utils.FileUtils.getPathWithoutFileName(location);
            }
            else
            {
                ShellViewModel.navigateToMediaFileBrowser();
            }
                  
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

        private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MediaViewer.Settings.AppSettings.Instance.save();
            Dispatcher.InvokeShutdown();
        }

        private void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error("Unhandled exception: " + e.ExceptionObject.ToString() + " Terminating: " + e.IsTerminating.ToString());
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
    }
}
