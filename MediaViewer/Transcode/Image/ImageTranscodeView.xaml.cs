using MediaViewer.Model.Settings;
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

namespace MediaViewer.Transcode.Image
{
    /// <summary>
    /// Interaction logic for ImageTranscodeView.xaml
    /// </summary>
    public partial class ImageTranscodeView : Window
    {
        public ImageTranscodeViewModel ViewModel { get; set; }

        public ImageTranscodeView()
        {
            InitializeComponent();

            DataContext = ViewModel = new ImageTranscodeViewModel();

            ViewModel.ClosingRequest += viewModel_ClosingRequest;
        }

        private void viewModel_ClosingRequest(object sender, Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
