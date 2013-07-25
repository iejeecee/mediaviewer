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


        public VideoView()
        {
            InitializeComponent();        

            this.Loaded += new RoutedEventHandler((s, e) =>
            {
                VideoViewModel videoViewModel = (VideoViewModel)DataContext;
                videoViewModel.VideoPlayer = videoPlayer;
            });

            timeLineSlider.AddHandler(Slider.MouseLeftButtonDownEvent, new MouseButtonEventHandler(timeLineSlider_MouseLeftButtonDownEvent),
                true);
        }



        private void timeLineSlider_MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {
            var slider = (Slider)sender;
            Point position = e.GetPosition(slider);
            double d = 1.0d / slider.ActualWidth * position.X;
            var p = slider.Maximum * d;
           
            int sliderValue = (int)p;

            VideoViewModel videoViewModel = (VideoViewModel)DataContext;

            videoViewModel.SetPositionMillisecondsCommand.DoExecute(sliderValue);

        }

        /*
             private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
             {
            
                 timeLineSlider.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;               
           
             }

     

             private void timeLineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
             {
                 int SliderValue = (int)timeLineSlider.Value;

                 // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
                 // Create a TimeSpan with miliseconds equal to the slider value.
                 TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                 videoPlayer.Position = ts;
             }

        
             private void timeLineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
             {
                 int SliderValue = (int)timeLineSlider.Value;

                 // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
                 // Create a TimeSpan with miliseconds equal to the slider value.
                 TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                 videoPlayer.Position = ts;
          
             }
             private void timeLineSlider_DragStarted(object sender, DragStartedEventArgs e)
             {
     
             }
         */
    }
}
