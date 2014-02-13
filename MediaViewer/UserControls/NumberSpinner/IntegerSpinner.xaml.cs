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
    /// Interaction logic for IntegerSpinner.xaml
    /// </summary>
    public partial class IntegerSpinner : UserControl
    {
        static Timers.DefaultTimer timer;
        const int initialRepeatDelayMS = 800;
        const int repeatDelayMS = 50;

        static IntegerSpinner()
        {
            timer = new Timers.DefaultTimer();
            timer.Tick += timer_Tick;
            timer.AutoReset = true;
        }

        public IntegerSpinner()
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
                timer.Interval = initialRepeatDelayMS;
                timer.Tag = this;
                timer.start();
            } 
            else if(upButton.IsPressed == true)
            {
                addValue();
                timer.Interval = initialRepeatDelayMS;
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
           IntegerSpinner spinner = (IntegerSpinner)(sender as Timers.DefaultTimer).Tag;

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

           timer.Interval = repeatDelayMS;
        }
        
        public Nullable<int> Value
        {
            get { return (Nullable<int>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Nullable<int>), typeof(IntegerSpinner),
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
            int max = (int)d.GetValue(MaxProperty);
            int min = (int)d.GetValue(MinProperty);

            Nullable<int> value = (Nullable<int>)baseValue;

            if (value != null)
            {
                if (value.Value > max)
                {
                    return (new Nullable<int>(max));
                }

                if (value.Value < min)
                {
                    return (new Nullable<int>(min));
                }
            }

            return (value);
        }

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntegerSpinner control = (IntegerSpinner)d;

            Nullable<int> value = (Nullable<int>)e.NewValue;

            control.valueTextBox.Text = value == null ? "" : value.Value.ToString();          
        }


        public int Max
        {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(int), typeof(IntegerSpinner), new PropertyMetadata(int.MaxValue));


        public int Min
        {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(int), typeof(IntegerSpinner), new PropertyMetadata(int.MinValue));

        private void addValue()
        {          
            if (Value == null)
            {
                Value = Min;
            }
            else
            {
                Value += 1;
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
                Value -= 1;
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
                    int value = int.Parse(input);
                    Value = new Nullable<int>(value);
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
                if (!"-0123456789".Contains(c))
                {
                    e.Handled = true;
                    SystemSounds.Beep.Play();
                    break;
                }
            }
        }

    

      

        
        
    }
}
