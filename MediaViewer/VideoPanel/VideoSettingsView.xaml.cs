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

namespace MediaViewer.VideoPanel
{
    /// <summary>
    /// Interaction logic for VideoSettingsView.xaml
    /// </summary>
    public partial class VideoSettingsView : Window
    {
        public VideoSettingsViewModel ViewModel { get; set; }

        public VideoSettingsView()
        {
            InitializeComponent();
            ViewModel = new VideoSettingsViewModel(AppSettings.Instance);

            ViewModel.ClosingRequest += ViewModel_ClosingRequest;
        }

        void ViewModel_ClosingRequest(object sender, Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            Close();
        }
    }
}
