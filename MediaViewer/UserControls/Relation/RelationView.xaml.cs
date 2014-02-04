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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.Relation
{
    /// <summary>
    /// Interaction logic for RelationView.xaml
    /// </summary>
    public partial class RelationView : UserControl
    {
        public RelationView()
        {
            InitializeComponent();
            equalRadioButton.IsChecked = true;
        }

        public RelationEnum Value
        {
            get { return (RelationEnum)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(RelationEnum), typeof(RelationView), 
            new FrameworkPropertyMetadata() {
                DefaultValue = RelationEnum.EQUAL, 
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = new PropertyChangedCallback(valueChangedCallback)
                }
        );

        private static void valueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RelationView r = (RelationView)d;

            switch ((RelationEnum)e.NewValue)
            {
                case RelationEnum.EQUAL:
                    {
                        r.equalRadioButton.IsChecked = true;
                        r.greaterRadioButton.IsChecked = false;
                        r.lessRadioButton.IsChecked = false;
                        break;
                    }
                case RelationEnum.GREATER_THAN_OR_EQUAL:
                    {
                        r.equalRadioButton.IsChecked = false;
                        r.greaterRadioButton.IsChecked = true;
                        r.lessRadioButton.IsChecked = false;
                        break;
                    }
                case RelationEnum.LESS_THAN_OR_EQUAL:
                    {
                        r.equalRadioButton.IsChecked = false;
                        r.greaterRadioButton.IsChecked = false;
                        r.lessRadioButton.IsChecked = true;
                        break;
                    }
            }
        }

        private void equalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Value = RelationEnum.EQUAL;
        }

        private void greaterRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Value = RelationEnum.GREATER_THAN_OR_EQUAL;
        }

        private void lessRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Value = RelationEnum.LESS_THAN_OR_EQUAL;
        }

        
    }
}
