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

namespace MediaViewer.VideoPreviewImage
{
    /// <summary>
    /// Interaction logic for VideoPreviewImageProgressView.xaml
    /// </summary>
    public partial class VideoPreviewImageProgressView : Window
    {
        VideoPreviewImageProgressViewModel viewModel;

        public VideoPreviewImageProgressViewModel ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; }
        }

        public VideoPreviewImageProgressView()
        {
            InitializeComponent();
            DataContext = ViewModel = new VideoPreviewImageProgressViewModel();

            ViewModel.ClosingRequest += Vm_ClosingRequest;
        }

        private void Vm_ClosingRequest(object sender, MvvmFoundation.Wpf.CloseableObservableObject.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
