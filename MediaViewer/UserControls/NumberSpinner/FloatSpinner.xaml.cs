using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
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

namespace MediaViewer.UserControls.NumberSpinner
{
    /// <summary>
    /// Interaction logic for FloatSpinner.xaml
    /// </summary>
    public partial class FloatSpinner : UserControl
    {
        static Timers.DefaultTimer timer;

        static FloatSpinner()
        {
            timer = new Timers.DefaultTimer();
            timer.Tick += timer_Tick;
            timer.AutoReset = true;
        }

        public FloatSpinner()
        {
            InitializeComponent();
                                    
            var descriptor = DependencyPropertyDescriptor.FromProperty(Button.IsPressedProperty, typeof(Button));
            descriptor.AddValueChanged(upButton, new EventHandler(button_IsPressedChanged));
          
            descriptor.AddValueChanged(downButton, new EventHandler(button_IsPressedChanged));
           
        }

        private void button_IsPressedChanged(object sender, EventArgs e)
        {
            if (downButton.IsPressed == true)
            {
                subtractValue();
                timer.Interval = 1000;
                timer.Tag = this;
                timer.start();
            } 
            else if(upButton.IsPressed == true)
            {
                addValue();
                timer.Interval = 1000;
                timer.Tag = this;
                timer.start();
            }
            else
            {
                timer.stop();
            }
        }

        static void timer_Tick(Object sender, EventArgs e)
        {
           FloatSpinner spinner = (FloatSpinner)(sender as Timers.DefaultTimer).Tag;

           spinner.Dispatcher.BeginInvoke(new Action(() =>
           {
               if (spinner.downButton.IsPressed == true)
               {
                   spinner.subtractValue();
               }
               else if (spinner.upButton.IsPressed == true)
               {
                   spinner.addValue();
               }
           }));

           timer.Interval = 200;
        }
        
        public Nullable<float> Value
        {
            get { return (Nullable<float>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Nullable<float>), typeof(FloatSpinner),
            new FrameworkPropertyMetadata()
            {
                DefaultValue = null,
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = new PropertyChangedCallback(valueChangedCallback),
                CoerceValueCallback = new CoerceValueCallback(coerceValueCallback),
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged                      
            });
 
        private static object coerceValueCallback(DependencyObject d, object baseValue)
        {
            float max = (float)d.GetValue(MaxProperty);
            float min = (float)d.GetValue(MinProperty);

            Nullable<float> value = (Nullable<float>)baseValue;

            if (value != null)
            {
                if (value.Value > max)
                {
                    return (new Nullable<float>(max));
                }

                if (value.Value < min)
                {
                    return (new Nullable<float>(min));
                }
            }

            return (value);
        }

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FloatSpinner control = (FloatSpinner)d;

            Nullable<float> value = (Nullable<float>)e.NewValue;

            control.valueTextBox.Text = value == null ? "" : value.Value.ToString();          
        }


        public float Max
        {
            get { return (float)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(float), typeof(FloatSpinner), new PropertyMetadata(float.MaxValue));



        public float SpinValue
        {
            get { return (float)GetValue(SpinValueProperty); }
            set { SetValue(SpinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpinValueProperty =
            DependencyProperty.Register("SpinValue", typeof(float), typeof(FloatSpinner), new PropertyMetadata(1.0f));

        


        public float Min
        {
            get { return (float)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(float), typeof(FloatSpinner), new PropertyMetadata(float.MinValue));

        private void addValue()
        {          
            if (Value == null)
            {
                Value = Min;
            }
            else
            {
                Value += SpinValue;
            }          
        }

        private void subtractValue()
        {           
            if (Value == null)
            {
                Value = Min;
            }
            else
            {
                Value -= SpinValue;
            }          
        }
     
        private void valueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                String input = valueTextBox.Text;

                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input) || input.Equals("-"))
                {
                    Value = null;
                }
                else
                {
                    float value = float.Parse(input);
                    Value = new Nullable<float>(value);
                }
            }
            catch (Exception)
            {
                Value = null;
                valueTextBox.Text = "";
            }
        }

        private void valueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!"-0123456789,".Contains(c))
                {
                    e.Handled = true;
                    SystemSounds.Beep.Play();
                    break;
                }
            }
        }
        
    }
}

