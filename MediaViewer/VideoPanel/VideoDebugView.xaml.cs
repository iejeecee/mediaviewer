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
    /// Interaction logic for VideoDebug.xaml
    /// </summary>
    public partial class VideoDebugView : Window
    {
        VideoDebugViewModel ViewModel { get; set; }

        public VideoDebugView()
        {
            InitializeComponent();

            DataContextChanged += VideoDebugView_DataContextChanged;
        }

        void VideoDebugView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is VideoDebugViewModel)
            {
                ViewModel.ClosingRequest -= ViewModel_ClosingRequest;
            }

            if (e.NewValue != null && e.NewValue is VideoDebugViewModel)
            {
                ViewModel = e.NewValue as VideoDebugViewModel;
                ViewModel.ClosingRequest += ViewModel_ClosingRequest;
            }
        }

        void ViewModel_ClosingRequest(object sender, Model.Mvvm.CloseableBindableBase.DialogEventArgs e)
        {
            this.Close();
        }
    }
}
