using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MediaViewer.UserControls.VideoSlider
{
    /// <summary>
    /// Interaction logic for VideoSlider.xaml
    /// </summary>
    public partial class VideoSliderView : UserControl
    {
        TimeAdorner timeAdorner;
        List<MarkerAdorner> markerAdorner;

        public VideoSliderView()
        {
            InitializeComponent();

            slider.AddHandler(Slider.MouseMoveEvent, new MouseEventHandler(slider_MouseMoveEvent));
            slider.AddHandler(Slider.MouseLeaveEvent, new MouseEventHandler(slider_MouseLeaveEvent));

            timeAdorner = new TimeAdorner(slider);
            slider.Minimum = 0;
            slider.Maximum = 0;

            markerAdorner = new List<MarkerAdorner>();
        }

        private void slider_MouseLeaveEvent(object sender, MouseEventArgs e)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            adornerLayer.Remove(timeAdorner);
        }

        private void slider_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (timeAdorner.IsVisible)
            {               
                adornerLayer.Remove(timeAdorner);
            }

            Point currentPos = e.GetPosition(slider);

            // The + 20 part is so your mouse pointer doesn't overlap.
     
            timeAdorner.Location = new Point(currentPos.X, -10);

            double d = 1.0d / slider.ActualWidth * currentPos.X;
            var p = slider.Maximum * d;
       
            timeAdorner.TimeSeconds = (int)p;
            
            adornerLayer.Add(timeAdorner);
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(VideoSliderView), new PropertyMetadata(0.0, maximumChangedCallback));

        private static void maximumChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoSliderView view = (VideoSliderView)d;
            view.slider.Maximum = (double)e.NewValue;
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(VideoSliderView), new PropertyMetadata(0.0,minimumChangedCallback));

        private static void minimumChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoSliderView view = (VideoSliderView)d;
            view.slider.Minimum = (double)e.NewValue;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(VideoSliderView), new PropertyMetadata(0.0, valueChangedCallback));

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VideoSliderView view = (VideoSliderView)d;
            view.slider.Value = (double)e.NewValue;
        }

        
              
    }
}
