using MediaViewer.MediaDatabase.DataTransferObjects;
using MediaViewer.Model.Utils.WPF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer
{
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args;
        public static SplashScreen SplashScreen;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Args = e.Args;

            SplashScreen = new SplashScreen("Resources/Images/splash.png");

#if !DEBUG
              SplashScreen.Show(false, true);
#endif

            ThemeHelper.SetTheme("classic", "");

            MediaViewerBootstrapper bootstrapper = new MediaViewerBootstrapper();
            bootstrapper.Run();                     

            AutoMapperSetup.Run();
        }

        public static String getAppInfoString()
        {
            return ("MediaViewer v" + Assembly.GetEntryAssembly().GetName().Version.ToString());
        }

       
    }
}
