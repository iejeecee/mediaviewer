using MediaViewer.Infrastructure.Settings;
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

namespace MediaViewer.VideoTranscode
{
    /// <summary>
    /// Interaction logic for VideoTranscodeView.xaml
    /// </summary>
    public partial class VideoTranscodeView : Window
    {
        public VideoTranscodeViewModel ViewModel { get; set; }

        public VideoTranscodeView()
        {
            InitializeComponent();

            ViewModel = new VideoTranscodeViewModel(AppSettings.Instance);

            ViewModel.ClosingRequest += viewModel_ClosingRequest;

            DataContext = ViewModel;

        }

        private void viewModel_ClosingRequest(object sender, Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
