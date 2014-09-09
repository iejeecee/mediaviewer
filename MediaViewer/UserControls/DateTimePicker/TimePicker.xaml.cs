using MediaViewer.Model.Utils;
using MediaViewer.UserControls.NumberSpinner;
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
    /// Interaction logic for TimePicker.xaml
    /// </summary>
    public partial class TimePicker : TimePickerBase
    {
    
        int caretIndex;

        public TimePicker()
        {
            InitializeComponent();

            initializeElems(upButton, downButton, valueTextBox);

            caretIndex = 7;

        }

        protected override void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            TimePicker control = (TimePicker)d;

            Nullable<long> value = (Nullable<long>)e.NewValue;

            int hours = 0,minutes = 0,seconds = 0;

            if (value != null)
            {

                MiscUtils.parseTimeSeconds(value.Value, out seconds, out minutes, out hours);

                TimeSpan temp = new TimeSpan(hours, minutes, seconds);

                control.valueTextBox.Text = temp.ToString("hh\\:mm\\:ss");
            }
            else
            {
                control.valueTextBox.Text = "";
            }

                   
        }

        int caretIndexMultiplier()
        {
            int index = caretIndex;

            if (index <= 2)
            {
                return (60 * 60);
            }
            if (index > 2 && index <= 5)
            {
                return (60);
            }
            else
            {
                return (1);
            }
        }

        protected override void addValue()
        {          
            if (Value == null)
            {
                Value = Min == null ? 0 : Min;
            }
            else
            {
                Value += 1 * caretIndexMultiplier();
            }          
        }

        protected override void subtractValue()
        {           
            if (Value == null)
            {
                Value = Min == null ? 0 : Min;
            }
            else
            {
                Value -= 1 * caretIndexMultiplier(); 
            }          
        }
     
        private void valueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!"0123456789".Contains(c))
                {
                    e.Handled = true;
                    System.Media.SystemSounds.Beep.Play();
                    break;
                }
                else if(Value == null)
                {
                    e.Handled = true;
                    Value = 0;
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
                    TimeSpan value = TimeSpan.Parse(input);
                    Value = new Nullable<long>((long)value.TotalSeconds);
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
            
            while (start != 0 && valueTextBox.Text[start - 1] != ':')
            {
                start--;
            }

            int end = valueTextBox.CaretIndex;

            while (end < valueTextBox.Text.Count() && valueTextBox.Text[end] != ':')
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
