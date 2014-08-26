using MediaViewer.Settings;
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

namespace MediaViewer.Torrent
{
    /// <summary>
    /// Interaction logic for TorrentCreationView.xaml
    /// </summary>
    public partial class TorrentCreationView : Window
    {
        TorrentCreationViewModel viewModel;

        internal TorrentCreationViewModel ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; }
        }

        public TorrentCreationView()
        {
            InitializeComponent();
            DataContext = ViewModel = new TorrentCreationViewModel(AppSettings.Instance);

            ViewModel.ClosingRequest += ViewModel_ClosingRequest;
        }

        private void ViewModel_ClosingRequest(object sender, MvvmFoundation.Wpf.CloseableObservableObject.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
