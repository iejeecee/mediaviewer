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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;

namespace MediaViewer.VideoPanel
{
    /// <summary>
    /// Interaction logic for VideoToolbarView.xaml
    /// </summary>
    [Export]
    public partial class VideoToolbarView : UserControl, INavigationAware
    {
        VideoDebugViewModel VideoDebugViewModel { get; set; }

        public VideoToolbarView()
        {
            InitializeComponent();
        }

        private void debugInfoToggleButton_Click(object sender, RoutedEventArgs e)
        {
            debugInfoToggleButton.IsEnabled = false;
         
            VideoDebugView debugWindow = new VideoDebugView();

            if (VideoDebugViewModel == null)
            {
                VideoDebugViewModel = new VideoDebugViewModel((DataContext as VideoViewModel).VideoPlayer);
            }

            debugWindow.DataContext = VideoDebugViewModel;

            debugWindow.Closed += debugWindow_Closed;

            debugWindow.Show();
            
        }

        void debugWindow_Closed(object sender, EventArgs e)
        {
            debugInfoToggleButton.IsChecked = false;
            debugInfoToggleButton.IsEnabled = true;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            DataContext = navigationContext.Parameters["viewModel"];
        }
    }
}
