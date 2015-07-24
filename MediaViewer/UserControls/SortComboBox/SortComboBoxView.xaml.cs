using System;
using System.Collections;
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

namespace MediaViewer.UserControls.SortComboBox
{
    /// <summary>
    /// Interaction logic for SortComboBoxView.xaml
    /// </summary>
    public partial class SortComboBoxView : UserControl
    {
        public SortComboBoxView()
        {
            InitializeComponent();  
                     
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SortComboBoxView), new PropertyMetadata(null, itemsSourceChanged));

        private static void itemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SortComboBoxView view = (SortComboBoxView)d;
            view.sortComboBox.ItemsSource = (IEnumerable)e.NewValue;                       
        }

        
    }
}
