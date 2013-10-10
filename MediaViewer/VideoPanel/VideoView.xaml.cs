using System;
using System.Collections.Generic;
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

            timeLineSlider.AddHandler(Slider.MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonDownEvent),
                 true);
            timeLineSlider.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonUpEvent),
              true);

            timeLineSlider.AddHandler(Slider.MouseMoveEvent, new MouseEventHandler(timeLineSlider_MouseMoveEvent));
            timeLineSlider.AddHandler(Slider.MouseLeaveEvent, new MouseEventHandler(timeLineSlider_MouseLeaveEvent));

            VideoRender videoRender = new VideoRender();

            updateTimeLineSlider = true;

            VideoCanvas.Scene = videoRender;
            VideoPlayerViewModel viewModel = videoRender.VideoPlayerViewModel;
            DataContext = viewModel;

            viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((s, e) =>
            {

                if (e.PropertyName.Equals("PositionSeconds") && updateTimeLineSlider == true)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(updateTimeLine));
                }               

            });
            
        }

        void updateTimeLine()
        {
            VideoPlayerViewModel viewModel = (VideoPlayerViewModel)DataContext;
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

            VideoPlayerViewModel videoPlayerViewModel = (VideoPlayerViewModel)DataContext;

            videoPlayerViewModel.seek(sliderValue);

            updateTimeLineSlider = true;    
        }

             
    }
}
