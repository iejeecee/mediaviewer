using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MediaViewer.UserControls.DateTimePicker
{
    /// <summary>
    /// Interaction logic for DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker : UserControl
    {
        static Timers.DefaultTimer timer;
        const int initialRepeatDelayMS = 800;
        const int repeatDelayMS = 50;
        static DateTime minDateTime = new DateTime(1753, 1, 1);
        static String format = "dd-MM-yyyy HH:mm:ss";

        static DateTimePicker()
        {
            timer = new Timers.DefaultTimer();
            timer.Tick += timer_Tick;
            timer.AutoReset = true;
        }

        int caretIndex;

        public DateTimePicker()
        {
            InitializeComponent();
                              
            var descriptor = DependencyPropertyDescriptor.FromProperty(Button.IsPressedProperty, typeof(Button));
            descriptor.AddValueChanged(upButton, new EventHandler(button_IsPressedChanged));          
            descriptor.AddValueChanged(downButton, new EventHandler(button_IsPressedChanged));

            caretIndex = 0;
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

        DateTime DefaultDateTime
        {
            get
            {
                DateTime defDateTime = new DateTime(DateTime.Now.Year, 1, 1);
                return defDateTime;
            }
        }

        static void timer_Tick(Object sender, EventArgs e)
        {
            DateTimePicker spinner = (DateTimePicker)(sender as Timers.DefaultTimer).Tag;

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
        
        public Nullable<DateTime> Value
        {
            get { return (Nullable<DateTime>)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Nullable<DateTime>), typeof(DateTimePicker),
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
            Nullable<DateTime> value = (Nullable<DateTime>)baseValue;

            if (value != null)
            {
                if (value.Value < minDateTime)
                {
                    return (new Nullable<DateTime>(minDateTime));
                }
            
            }

            return (value);
        }

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimePicker control = (DateTimePicker)d;

            Nullable<DateTime> value = (Nullable<DateTime>)e.NewValue;
      
            if (value != null)
            {
                control.valueTextBox.Text = value.Value.ToString(format);
            }
            else
            {
                control.valueTextBox.Text = "";
            }

                   
        }

        void spinAtCaretPosition(int val)
        {
            int nrDateItems = 0;
            int nrTimeItems = 0;
            int nrWhiteSpace = 0;

            for (int i = caretIndex; i < valueTextBox.Text.Length; i++)
            {
                if (valueTextBox.Text[i] == '-')
                {
                    nrDateItems++;
                }
                else if (valueTextBox.Text[i] == ':')
                {
                    nrTimeItems++;

                } else if(Char.IsWhiteSpace(valueTextBox.Text[i])) {

                    nrWhiteSpace++;
                }
            }

            if (nrDateItems == 2)
            {
                Value = Value.Value.AddDays(val);
            }
            else if (nrDateItems == 1)
            {
                Value = Value.Value.AddMonths(val);
            }
            else if (nrDateItems == 0 && nrWhiteSpace > 0)
            {
                Value = Value.Value.AddYears(val);
            }
            else if (nrTimeItems == 2)
            {
                Value = Value.Value.AddHours(val);
            }
            else if (nrTimeItems == 1)
            {
                Value = Value.Value.AddMinutes(val);
            }
            else if (nrTimeItems == 0)
            {
                Value = Value.Value.AddSeconds(val);
            }

        }


        private void addValue()
        {          
            if (Value == null)
            {
                Value = DefaultDateTime;
            }
            else
            {
                spinAtCaretPosition(1);
            }          
        }

        private void subtractValue()
        {           
            if (Value == null)
            {
                Value = DefaultDateTime;
            }
            else
            {
                spinAtCaretPosition(-1);
            }          
        }
     
        private void valueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!"0123456789-:".Contains(c))
                {
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    break;
                }
                else if(Value == null)
                {
                    e.Handled = true;
                    Value = DefaultDateTime;
                }
                
            }
        }

        private void valueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            caretIndex = valueTextBox.CaretIndex;

            try
            {
                String input = valueTextBox.Text;

                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
                {
                    Value = null;
                }
                else
                {
                    DateTime value = DateTime.Parse(input);                    
                    Value = new Nullable<DateTime>(value);
                }
            }
            catch (Exception)
            {
                Value = null;
                valueTextBox.Text = "";
            }
        }

        void selectAtCaretPos()
        {
            int start = valueTextBox.CaretIndex;
            
            while (start != 0 && !"-:".Contains(valueTextBox.Text[start - 1]) && !Char.IsWhiteSpace(valueTextBox.Text[start - 1]))
            {
                start--;
            }

            int end = valueTextBox.CaretIndex;

            while (end < valueTextBox.Text.Count() && !"-:".Contains(valueTextBox.Text[end]) && !Char.IsWhiteSpace(valueTextBox.Text[end]))
            {
                end++;
            }

            valueTextBox.SelectionStart = start;
            valueTextBox.SelectionLength = end - start;

        }

        private void valueTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (valueTextBox.SelectionLength == 0)
            {
                selectAtCaretPos();
            }
        }

        private void valueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(valueTextBox.Text) || String.IsNullOrWhiteSpace(valueTextBox.Text))
            {
                Value = null;
            }
        }        
        
    }

}
