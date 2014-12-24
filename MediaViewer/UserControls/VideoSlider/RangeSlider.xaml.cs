using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        public RangeSlider()
        {
            InitializeComponent();
               
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new PropertyMetadata(0d, minimumChangedCallback));

        private static void minimumChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider rangeSlider = (RangeSlider)d;
            rangeSlider.LowerSlider.Minimum = rangeSlider.UpperSlider.Minimum = (double)e.NewValue;
        }

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), 
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                lowerValueChangedCallback));

        private static void lowerValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider rangeSlider = (RangeSlider)d;
                     
            if (rangeSlider.LowerValue > rangeSlider.UpperValue)
            {
                rangeSlider.UpperValue = rangeSlider.LowerValue;
            }
            rangeSlider.LowerSlider.Value = rangeSlider.LowerValue;
          
        }

        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }
     
        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), 
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                upperValueChangedCallback));

        private static void upperValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider rangeSlider = (RangeSlider)d;
                 
            if (rangeSlider.LowerValue > rangeSlider.UpperValue)
            {
                rangeSlider.UpperValue = rangeSlider.LowerValue;
            }
            rangeSlider.UpperSlider.Value = rangeSlider.UpperValue;
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new PropertyMetadata(1d));

        private static void maximumChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RangeSlider rangeSlider = (RangeSlider)d;
            rangeSlider.LowerSlider.Maximum = rangeSlider.UpperSlider.Maximum = (double)e.NewValue;
        }

      

        
    }
}
