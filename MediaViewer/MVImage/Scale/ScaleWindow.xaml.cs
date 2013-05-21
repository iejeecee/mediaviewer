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

namespace MediaViewer.MVImage.Scale
{
    /// <summary>
    /// Interaction logic for ScaleWindow.xaml
    /// </summary>
    public partial class ScaleWindow : Window, INotifyPropertyChanged
    {
        public ScaleWindow()
        {
            InitializeComponent();
            scale = 1;
         
        }

        private double scale;

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Scale"));
                ScaleChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ScaleChanged;
        public event EventHandler ResetScale;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void clampScaleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if(clampScaleCheckBox.IsChecked.Value == true) {

                scaleSlider.IsSnapToTickEnabled = true;

            } else {

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

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetScale(this, EventArgs.Empty);
        }

 
    }
}
