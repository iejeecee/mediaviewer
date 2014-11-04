using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Mvvm;
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
    /// Interaction logic for VideoPreviewImageView.xaml
    /// </summary>
    public partial class VideoPreviewImageView : Window
    {
        VideoPreviewImageViewModel viewModel;

        internal VideoPreviewImageViewModel ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; }
        }

        public VideoPreviewImageView()
        {
            InitializeComponent();

            DataContext = viewModel = new VideoPreviewImageViewModel(MediaFileWatcher.Instance);

            viewModel.ClosingRequest += new EventHandler<CloseableBindableBase.DialogEventArgs>((s, e) =>
                {
                    this.Close();
                });
        }
    }
}
