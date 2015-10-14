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

namespace MediaViewer.VideoPanel
{
    /// <summary>
    /// Interaction logic for VideoOpenLocationView.xaml
    /// </summary>
    public partial class VideoOpenLocationView : Window
    {
        public VideoOpenLocationViewModel ViewModel { get; set; }

        public VideoOpenLocationView()
        {
            InitializeComponent();
            ViewModel = new VideoOpenLocationViewModel();
            DataContext = ViewModel;

            ViewModel.ClosingRequest += ViewModel_ClosingRequest;
        }

        void ViewModel_ClosingRequest(object sender, Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            DialogResult = e.DialogMode == MediaViewer.Model.Mvvm.CloseableBindableBase.DialogMode.CANCEL ? false : true;
            Close();
        }
    }
}
