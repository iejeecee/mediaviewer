using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer
{
    public partial class App : Application
    {

        public static string[] Args;

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;        

            InitializeComponent();

            
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Args = e.Args;
           
                  
        }

        private void Application_Exit(object sender, EventArgs e)
        {

          
        }
        
       

    }
}
