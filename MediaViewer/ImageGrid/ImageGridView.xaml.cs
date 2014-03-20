// intresting stuff:
// Lazy<T> for lazy initialization
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using MediaViewer.Utils.WPF;
using MediaViewer.MediaFileModel.Watcher;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    public partial class ImageGridView : UserControl
    {
        public ImageGridView()
        {           
            InitializeComponent();                               
        }
        
        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)e.OriginalSource;
           
            GlobalMessenger.Instance.NotifyColleagues("MainWindowViewModel.ViewMediaCommand", item.Tag);
           
        }

        private void selectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            ImageGridViewModel vm = (ImageGridViewModel)DataContext;
            vm.MediaState.selectAllUIState();
        }

        private void deselectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            ImageGridViewModel vm = (ImageGridViewModel)DataContext;
            vm.MediaState.deselectAllUIState();          

        }

        private void browseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)e.OriginalSource;

            String location = Utils.FileUtils.getPathWithoutFileName((string)item.Tag);

            GlobalMessenger.Instance.NotifyColleagues("MediaFileBrowser_SetPath", location);   
        }

        private void openInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)e.OriginalSource;

            String location = Utils.FileUtils.getPathWithoutFileName((string)item.Tag);

            Process.Start(location);
        }
       
    }
}
