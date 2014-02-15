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
    public partial class DateTimePicker : DateTimePickerBase
    {  
        static DateTime minDateTime = new DateTime(1753, 1, 1);
        static String format = "dd-MM-yyyy HH:mm:ss";

        int caretIndex;

        public DateTimePicker()
        {
            InitializeComponent();

            initializeElems(upButton, downButton, valueTextBox);

            caretIndex = 0;
            //Min = new Nullable<DateTime>(minDateTime);
        }
       
        DateTime DefaultDateTime
        {
            get
            {
                DateTime defDateTime = new DateTime(DateTime.Now.Year, 1, 1);
                return defDateTime;
            }
        }

       
        protected override void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        protected override void addValue()
        {          
            if (Value == null)
            { 
                Value = Min == null ? DefaultDateTime : Min;
            }
            else
            {
                spinAtCaretPosition(1);
            }          
        }

        protected override void subtractValue()
        {           
            if (Value == null)
            {
                Value = Min == null ? DefaultDateTime : Min;
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
