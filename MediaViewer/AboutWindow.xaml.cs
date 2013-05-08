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
using System.Windows.Shapes;

namespace MediaViewer
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
     

        public string ProgramInfo
        {
            get {

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string info = "MediaViewer v" + version.ToString();

                return info; 
            }
   
        }

        public string CopyrightInfo
        {
            get
            {
                Version version = Assembly.GetEntryAssembly().GetName().Version;
                string info = "(c) 2013 IJC";

                return info;
            }

        }

        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
