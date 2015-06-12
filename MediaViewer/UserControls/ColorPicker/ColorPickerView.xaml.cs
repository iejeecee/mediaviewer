using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace MediaViewer.UserControls.ColorPicker
{
    /// <summary>
    /// Interaction logic for ColorPickerView.xaml
    /// </summary>
    public partial class ColorPickerView : UserControl
    {
        public ColorPickerView()
        {
            InitializeComponent();
         
            colorsComboBox.ItemsSource = typeof(System.Windows.Media.Colors).GetProperties();
            colorsComboBox.SelectedItem = getPropertyInfoFromColor(Colors.White);
        
            colorsComboBox.SelectionChanged += colorsComboBox_SelectionChanged;

        }

        void colorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Color = (Color)(colorsComboBox.SelectedItem as PropertyInfo).GetValue(null, null);
        }

        public System.Windows.Media.Color Color
        {
            get { return (System.Windows.Media.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(System.Windows.Media.Color), typeof(ColorPickerView), new FrameworkPropertyMetadata(System.Windows.Media.Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, colorChanged));

        private static void colorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerView view = (ColorPickerView)d;

            view.colorsComboBox.SelectedItem = getPropertyInfoFromColor((Color)e.NewValue);
        }

        private static PropertyInfo getPropertyInfoFromColor(Color color)
        {                 
            PropertyInfo[] colorProperties = typeof(Colors).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            foreach (PropertyInfo colorProperty in colorProperties)
            {
                if ((Color)colorProperty.GetValue(null, null) == color)
                {
                    return colorProperty;
                }
            }

            return null;
        }
                      
    }
}
