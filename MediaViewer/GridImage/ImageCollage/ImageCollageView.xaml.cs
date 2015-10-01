using MediaViewer.Model.Mvvm;
using MediaViewer.Model.Media.File.Watcher;
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
using MediaViewer.Model.Settings;

namespace MediaViewer.GridImage.ImageCollage
{
    /// <summary>
    /// Interaction logic for ImageCollageView.xaml
    /// </summary>
    public partial class ImageCollageView : Window
    {
        public ImageCollageViewModel ViewModel { get; protected set; }

        public ImageCollageView()
        {
            InitializeComponent();

            ViewModel = new ImageCollageViewModel(MediaFileWatcher.Instance);

            ViewModel.ClosingRequest += vm_ClosingRequest;

            DataContext = ViewModel;
        }

        private void vm_ClosingRequest(object sender, CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
