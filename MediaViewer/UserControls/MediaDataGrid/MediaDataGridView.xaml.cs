using MediaViewer.MediaFileModel.Watcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MediaViewer.UserControls.MediaDataGrid
{
    /// <summary>
    /// Interaction logic for MediaDataGridView.xaml
    /// </summary>
    public partial class MediaDataGridView : UserControl
    {

        public event EventHandler<MediaFileItem> RowDoubleClick;

        public MediaDataGridView()
        {
            InitializeComponent();
            
        }

        public ObservableCollection<MediaFileItem> Media
        {
            get { return (ObservableCollection<MediaFileItem>)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Media.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaProperty =
            DependencyProperty.Register("Media", typeof(ObservableCollection<MediaFileItem>), typeof(MediaDataGridView), new PropertyMetadata(null, mediaChangedCallback));

        private static void mediaChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaDataGridView view = (MediaDataGridView)d;
            view.mediaDataGrid.ItemsSource = (IEnumerable<MediaFileItem>)e.NewValue;
        }
        
        private void row_DoubleClick(Object sender, MouseButtonEventArgs e) {

            DataGridRow row = sender as DataGridRow;
            MediaFileItem item = row.DataContext as MediaFileItem;

            if (RowDoubleClick != null)
            {
                RowDoubleClick(this, item);
            }

        }
    }
}
