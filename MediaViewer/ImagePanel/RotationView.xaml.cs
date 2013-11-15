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
    /// Interaction logic for RotationView.xaml
    /// </summary>
    public partial class RotationView : Window
    {
        public RotationView()
        {
            InitializeComponent();
        }

        private void clampAngleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (clampAngleCheckBox.IsChecked.Value == true)
            {

                degreesSlider.IsSnapToTickEnabled = true;

            }
            else
            {

                degreesSlider.IsSnapToTickEnabled = false;
            }
        }

        private void degreesTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = degreesTextBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }
    }
}
