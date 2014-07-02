using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.VideoPanel
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView : UserControl
    {

        bool updateTimeLineSlider;
        VideoViewModel viewModel;

        public VideoViewModel ViewModel
        {
            get { return viewModel; }
            private set { viewModel = value; }
        }

        public VideoView()
        {
            InitializeComponent();

            DataContext = viewModel = new VideoViewModel(videoPlayer.ViewModel);

            timeLineSlider.AddHandler(Slider.MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonDownEvent),
                 true);
            timeLineSlider.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonUpEvent),
              true);

            timeLineSlider.AddHandler(Slider.MouseMoveEvent, new MouseEventHandler(timeLineSlider_MouseMoveEvent));
            timeLineSlider.AddHandler(Slider.MouseLeaveEvent, new MouseEventHandler(timeLineSlider_MouseLeaveEvent));
           
            updateTimeLineSlider = true;
                                                     
            videoPlayer.DoubleClick += videoPlayer_DoubleClick;
           
            viewModel.PropertyChanged += videoPlayerViewModel_PropertyChanged;
        }
        
        private void videoPlayerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("PositionSeconds") && updateTimeLineSlider == true)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(updateTimeLine));
            }
            else if (e.PropertyName.Equals("VideoState"))
            {

                switch (viewModel.VideoState)
                {
                    case VideoPlayerControl.VideoState.PLAYING:
                        {
                            if (playButton.IsChecked == false)
                            {
                                playButton.IsChecked = true;
                            }

                            break;
                        }
                    case VideoPlayerControl.VideoState.PAUSED:
                        {
                            if (playButton.IsChecked == true)
                            {
                                playButton.IsChecked = false;
                            }

                            break;
                        }
                    case VideoPlayerControl.VideoState.CLOSED:
                        {
                            if (playButton.IsChecked == true)
                            {
                                playButton.IsChecked = false;
                            }

                            break;
                        }

                }
            }
        }

        private void videoPlayer_DoubleClick(object sender, EventArgs e)
        {
            if (uiGrid.Visibility == Visibility.Visible)
            {
                uiGrid.Visibility = Visibility.Collapsed;               
            }
            else
            {
                uiGrid.Visibility = Visibility.Visible;               
            }

            GlobalMessenger.Instance.NotifyColleagues("ToggleFullScreen");
        }

        public void openAndPlay(String location)
        {
            try
            {                                      
                viewModel.OpenCommand.DoExecute(location);
                viewModel.PlayCommand.DoExecute();               
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void updateTimeLine()
        {           
            timeLineSlider.Value = viewModel.PositionSeconds;
        }

        private void timeLineSlider_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (!timeLineSliderPopup.IsOpen) { timeLineSliderPopup.IsOpen = true; }

            Point currentPos = e.GetPosition(timeLineSlider);

            // The + 20 part is so your mouse pointer doesn't overlap.
            timeLineSliderPopup.HorizontalOffset = currentPos.X + 20;
            timeLineSliderPopup.VerticalOffset = currentPos.Y;

            double d = 1.0d / timeLineSlider.ActualWidth * currentPos.X;
            var p = timeLineSlider.Maximum * d;

            int seconds = (int)p;

            timeLineSliderPopupText.Text = Utils.Misc.formatTimeSeconds(seconds);

        }

        private void timeLineSlider_MouseLeaveEvent(object sender, MouseEventArgs e)
        {
            timeLineSliderPopup.IsOpen = false;
        }

        private void timeLineSlider_MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {
            updateTimeLineSlider = false;

        }

        private void timeLineSlider_MouseLeftButtonUpEvent(object sender, MouseButtonEventArgs e)
        {
           
            var slider = (Slider)sender;
            Point position = e.GetPosition(slider);
            double d = 1.0d / slider.ActualWidth * position.X;
            var p = slider.Maximum * d;

            int sliderValue = (int)p;
         
            viewModel.SeekCommand.DoExecute(sliderValue);

            updateTimeLineSlider = true;    
        }

        private void playButton_Checked(object sender, RoutedEventArgs e)
        {           
            viewModel.PlayCommand.DoExecute();
        }

        private void playButton_Unchecked(object sender, RoutedEventArgs e)
        {
            viewModel.PauseCommand.DoExecute();
        }

             
    }
}
