using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MediaViewer.ImagePanel
{
    /// <summary>
    /// Interaction logic for ScaleView.xaml
    /// </summary>
    public partial class ScaleView : Window
    {
        public ScaleView()
        {
            InitializeComponent();
        }

        private void clampScaleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (clampScaleCheckBox.IsChecked.Value == true)
            {

                scaleSlider.IsSnapToTickEnabled = true;

            }
            else
            {

                scaleSlider.IsSnapToTickEnabled = false;
            }
        }

        private void scaleTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = scaleTextBox.GetBindingExpression(TextBox.TextProperty);                
                be.UpdateSource();
            }
        }
    }
}
