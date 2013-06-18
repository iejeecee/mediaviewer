using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//http://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaViewer.DirectoryBrowser
{
    /// <summary>
    /// Interaction logic for DirectoryBrowserControl.xaml
    /// </summary>
    public partial class DirectoryBrowserControl : UserControl
    {
        public event EventHandler<PathModel> SelectedPathChanged;
       
        public DirectoryBrowserControl()
        {
            InitializeComponent();
    

           // setPathAsync("C:\\game\\XMP-Toolkit-SDK-5.1.2\\samples\\testfiles\\geotag");
    
        }
        /*
       

        

        private void directoryTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {

            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            if (item.DataContext != null && SelectedPathChanged != null)
            {
               
               SelectedPathChanged(this, (PathModel)item.DataContext);
               
            }

            item.BringIntoView();
        }

       
       */
    }
}
