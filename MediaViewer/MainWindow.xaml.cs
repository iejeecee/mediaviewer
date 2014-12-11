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



namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
            
        public MainWindow()
        {
            Close();

            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);
       
            InitializeComponent();
          
            /*Version version = Assembly.GetEntryAssembly().GetName().Version;
            Logger.Log.Info("Starting MediaViewer v" + version.ToString());

            AppDomain currentDomain = AppDomain.CurrentDomain;
            PrintLoadedAssemblies(currentDomain);
            currentDomain.AssemblyLoad += new AssemblyLoadEventHandler(assemblyLoadEventHandler);

          

            Settings = AppSettings.Instance;
             */
        
           // initializeVideoView();
           // initializeImageView();      
            //initializeMediaFileBrowser();

            //mainWindowViewModel = new MainWindowViewModel(Settings);
            //DataContext = mainWindowViewModel;
                    
            //mainWindowViewModel.PropertyChanged += new PropertyChangedEventHandler(mainWindowViewModel_PropertyChanged);

           // GlobalMessenger.Instance.Register<String>("MainWindow_SetTitle", new Action<String>(mainWindow_SetTitle));            
            //GlobalMessenger.Instance.Register("ToggleFullScreen", new Action(toggleFullScreen));

            //mediaFileBrowser.Loaded += new RoutedEventHandler(mediaFileBrowser_Loaded);

            //MediaDatabase.Test.test();
         

        }

   
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
             //OpenFileDialog openFileDialog = Utils.Windows.FileDialog.createOpenMediaFileDialog(false);

             //if (openFileDialog.ShowDialog() == true)
             //{

                 //imageViewModel.LoadImageAsyncCommand.DoExecute(openFileDialog.FileName);
             //}
        

        }

    

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       

       

        private void imageToolBarButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        
      
        



       

    

 

        
    }
}
