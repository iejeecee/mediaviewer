using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
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
       
        public VideoView()
        {
            InitializeComponent();
                
            timeLineSlider.Maximum = 0;
            
            updateTimeLineSlider = true;
        }

        void VideoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is VideoViewModel)
            {
                timeLineSlider.RemoveHandler(Slider.MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonDownEvent));
                timeLineSlider.RemoveHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonUpEvent));

                timeLineSlider.RemoveHandler(Slider.MouseMoveEvent, new MouseEventHandler(timeLineSlider_MouseMoveEvent));
                timeLineSlider.RemoveHandler(Slider.MouseLeaveEvent, new MouseEventHandler(timeLineSlider_MouseLeaveEvent));

                VideoViewModel viewModel = e.OldValue as VideoViewModel;

                videoPlayer.DoubleClick -= videoPlayer_DoubleClick;

                viewModel.Dispose();
            }

            if (e.NewValue is VideoViewModel)
            {
                VideoViewModel viewModel = e.NewValue as VideoViewModel;

                viewModel.initializeVideoPlayer(videoPlayer.ViewModel);

                videoPlayer.DoubleClick += videoPlayer_DoubleClick;

                timeLineSlider.AddHandler(Slider.MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonDownEvent),
                   true);
                timeLineSlider.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonUpEvent),
                  true);

                timeLineSlider.AddHandler(Slider.MouseMoveEvent, new MouseEventHandler(timeLineSlider_MouseMoveEvent));
                timeLineSlider.AddHandler(Slider.MouseLeaveEvent, new MouseEventHandler(timeLineSlider_MouseLeaveEvent));
             
            }
       
        }

        public Command CloseCommand
        {
            get { return (Command)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(Command), typeof(VideoView), new PropertyMetadata(null, closeCommandChangedCallback));

        private static void closeCommandChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            view.closeButton.Command = (Command)e.NewValue;
        }

        public Command ScreenShotCommand
        {
            get { return (Command)GetValue(ScreenShotCommandProperty); }
            set { SetValue(ScreenShotCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScreenShotCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScreenShotCommandProperty =
            DependencyProperty.Register("ScreenShotCommand", typeof(Command), typeof(VideoView), new PropertyMetadata(null, screenShotCommandChangedCallback));

        private static void screenShotCommandChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            view.screenShotButton.Command = (Command)e.NewValue;
        }

        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMuted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMutedProperty =
            DependencyProperty.Register("IsMuted", typeof(bool), typeof(VideoView), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, isMutedChangedCallback));

        private static void isMutedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;
            view.muteToggleButton.IsChecked = (bool)e.NewValue;                                  
        }

        private void muteToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            IsMuted = true;
        }

        private void muteToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            IsMuted = false;
        }

        public bool HasAudio
        {
            get { return (bool)GetValue(HasAudioProperty); }
            set { SetValue(HasAudioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasAudio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasAudioProperty =
            DependencyProperty.Register("HasAudio", typeof(bool), typeof(VideoView), new PropertyMetadata(true,hasAudioChangedCallback));

        private static void hasAudioChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;
            bool hasAudio = (bool)e.NewValue;

            if (hasAudio == false)
            {
                view.muteToggleButton.IsChecked = true;
                view.muteToggleButton.IsEnabled = false;
            }
            else
            {
                view.muteToggleButton.IsEnabled = true;
            }
               
        }

        public Command FrameByFrameCommand
        {
            get { return (Command)GetValue(FrameByFrameCommandProperty); }
            set { SetValue(FrameByFrameCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrameByFrameCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameByFrameCommandProperty =
            DependencyProperty.Register("FrameByFrameCommand", typeof(Command), typeof(VideoView), new PropertyMetadata(null, frameByFrameCommandChangedCallback));

        private static void frameByFrameCommandChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;
            view.frameByFrameButton.Command = (Command)e.NewValue; 
        }

        public Command PlayCommand
        {
            get { return (Command)GetValue(PlayCommandProperty); }
            set { SetValue(PlayCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayCommandProperty =
            DependencyProperty.Register("PlayCommand", typeof(Command), typeof(VideoView), new PropertyMetadata(null));


        public Command PauseCommand
        {
            get { return (Command)GetValue(PauseCommandProperty); }
            set { SetValue(PauseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PauseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PauseCommandProperty =
            DependencyProperty.Register("PauseCommand", typeof(Command), typeof(VideoView), new PropertyMetadata(null));


        private void playButton_Checked(object sender, RoutedEventArgs e)
        {
            if(PlayCommand != null) PlayCommand.DoExecute();
        }

        private void playButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if(PauseCommand != null) PauseCommand.DoExecute();
        }

        public VideoPlayerControl.VideoState VideoState
        {
            get { return (VideoPlayerControl.VideoState)GetValue(VideoStateProperty); }
            set { SetValue(VideoStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoStateProperty =
            DependencyProperty.Register("VideoState", typeof(VideoPlayerControl.VideoState), typeof(VideoView), new PropertyMetadata(VideoPlayerControl.VideoState.CLOSED, videoStateChangedCallback));

        private static void videoStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            VideoPlayerControl.VideoState state = (VideoPlayerControl.VideoState)e.NewValue;

            switch (state)
            {
                case VideoPlayerControl.VideoState.PLAYING:
                    {
                        if (view.playButton.IsChecked == false)
                        {
                            view.playButton.IsChecked = true;
                        }

                        break;
                    }
                case VideoPlayerControl.VideoState.PAUSED:
                    {
                        if (view.playButton.IsChecked == true)
                        {
                            view.playButton.IsChecked = false;
                        }

                        break;
                    }
                case VideoPlayerControl.VideoState.CLOSED:
                    {
                        if (view.playButton.IsChecked == true)
                        {
                            view.playButton.IsChecked = false;
                        }

                        break;
                    }
            }
        }

        public Command<double> SeekCommand
        {
            get { return (Command<double>)GetValue(SeekCommandProperty); }
            set { SetValue(SeekCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeekCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeekCommandProperty =
            DependencyProperty.Register("SeekCommand", typeof(Command<double>), typeof(VideoView), new PropertyMetadata(null));

        public int PositionSeconds
        {
            get { return (int)GetValue(PositionSecondsProperty); }
            set { SetValue(PositionSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PositionSeconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionSecondsProperty =
            DependencyProperty.Register("PositionSeconds", typeof(int), typeof(VideoView), new PropertyMetadata(0, positionSecondsChangedCallback));

        private static void positionSecondsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            if (view.updateTimeLineSlider == true)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    view.timeLineSlider.Value = view.PositionSeconds;
                    view.updateTimeInfoTextBlock();
                }));
            }
        }

        String formatTimeSeconds(int timeSeconds)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, timeSeconds);

            String timeString = "";

            if (timeSpan.Hours > 0)
            {
                timeString = timeSpan.Hours.ToString() + ":";
            }

            timeString += timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2");

            return (timeString);
        }

        void updateTimeInfoTextBlock()
        {            
            String info = formatTimeSeconds(PositionSeconds) + "/" + formatTimeSeconds(DurationSeconds);

            timeInfoTextBlock.Text = info;
        }

        public int DurationSeconds
        {
            get { return (int)GetValue(DurationSecondsProperty); }
            set { SetValue(DurationSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DurationSeconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationSecondsProperty =
            DependencyProperty.Register("DurationSeconds", typeof(int), typeof(VideoView), new PropertyMetadata(0,durationSecondsChangedCallback));

        private static void durationSecondsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            int durationSeconds = (int)e.NewValue;

            view.timeLineSlider.Maximum = durationSeconds;

            Application.Current.Dispatcher.BeginInvoke(new Action(view.updateTimeInfoTextBlock));
            
        }

        public Command<String> OpenCommand
        {
            get { return (Command<String>)GetValue(OpenCommandProperty); }
            set { SetValue(OpenCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenCommandProperty =
            DependencyProperty.Register("OpenCommand", typeof(Command<String>), typeof(VideoView), new PropertyMetadata(null));

        public int MaxVolume
        {
            get { return (int)GetValue(MaxVolumeProperty); }
            set { SetValue(MaxVolumeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxVolume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxVolumeProperty =
            DependencyProperty.Register("MaxVolume", typeof(int), typeof(VideoView), new PropertyMetadata(1,maxVolumeChangedCallback));

        private static void maxVolumeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;

            view.volumeSlider.Maximum = (int)e.NewValue;
        }

        public int MinVolume
        {
            get { return (int)GetValue(MinVolumeProperty); }
            set { SetValue(MinVolumeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinVolume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinVolumeProperty =
            DependencyProperty.Register("MinVolume", typeof(int), typeof(VideoView), new PropertyMetadata(0,minVolumeChangedCallback));

        private static void minVolumeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;
            view.volumeSlider.Minimum = (int)e.NewValue;
        }

        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Volume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(VideoView), new FrameworkPropertyMetadata(0.0,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ,volumeChangedCallback));

        private static void volumeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoView view = (VideoView)d;
            view.volumeSlider.Value = (double)e.NewValue;
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
                OpenCommand.DoExecute(location);
                PlayCommand.DoExecute();               
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
         
            SeekCommand.DoExecute(sliderValue);

            updateTimeLineSlider = true;    
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = e.NewValue;
        }
                                  
    }
}
