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
using System.Windows.Shapes;

namespace MediaViewer.MVImage.Rotate
{
    /// <summary>
    /// Interaction logic for RotateWindow.xaml
    /// </summary>
    public partial class RotateWindow : Window, INotifyPropertyChanged
    {
        public RotateWindow()
        {
            InitializeComponent();
            rotation = 0;
         
        }

        private double rotation;

        public double Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Rotation"));
                RotationChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler RotationChanged;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void clampAngleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if(clampAngleCheckBox.IsChecked.Value == true) {

                degreesSlider.IsSnapToTickEnabled = true;

            } else {

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

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            Rotation = 0;
        }

 
    }
}
