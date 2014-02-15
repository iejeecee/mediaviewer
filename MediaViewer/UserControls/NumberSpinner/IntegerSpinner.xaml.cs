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
    public partial class IntegerSpinner : IntegerSpinnerBase
    {
        
        public IntegerSpinner()
        {
            InitializeComponent();

            initializeElems(upButton, downButton, valueTextBox);
           
        }

        protected override void addValue()
        {          
            if (Value == null)
            {
                Value = Min == null ? 0 : Min;
            }
            else
            {
                Value += 1;
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
