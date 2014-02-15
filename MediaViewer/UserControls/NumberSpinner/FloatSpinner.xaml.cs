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
    public partial class FloatSpinner : FloatSpinnerBase
    {
      
        public FloatSpinner()
        {
            InitializeComponent();

            initializeElems(upButton, downButton, valueTextBox);         
                      
        }

        public float SpinValue
        {
            get { return (float)GetValue(SpinValueProperty); }
            set { SetValue(SpinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpinValueProperty =
            DependencyProperty.Register("SpinValue", typeof(float), typeof(FloatSpinner), new PropertyMetadata(1.0f));


        protected override void addValue() 
        {          
            if (Value == null)
            {
                Value = Min == null ? 0 : Min;
            }
            else
            {
                Value += SpinValue;
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

