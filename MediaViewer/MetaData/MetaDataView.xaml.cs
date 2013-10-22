using MediaViewer.ImageGrid;
using MediaViewer.MediaFileModel;
using MediaViewer.MetaData.Tree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace MediaViewer.MetaData
{
    /// <summary>
    /// Interaction logic for MetaDataView.xaml
    /// </summary>
    public partial class MetaDataView : UserControl
    {
      

        public MetaDataView()
        {
            InitializeComponent();

            
        }


        public ObservableCollection<ImageGridItem> MetaDataList
        {
            get { return (ObservableCollection<ImageGridItem>)GetValue(MetaDataListProperty); }
            set { SetValue(MetaDataListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MetaDataListProperty =
            DependencyProperty.Register("MetaDataList", typeof(ObservableCollection<ImageGridItem>), typeof(MetaDataView), new PropertyMetadata(null,
                new PropertyChangedCallback(metaDataList_PropertyChangedCallback)));

        private static void metaDataList_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MetaDataView metaDataView = (MetaDataView)d;

            if (e.OldValue != null)
            {
                var coll = (ObservableCollection<ImageGridItem>)e.OldValue;
                // Unsubscribe from CollectionChanged on the old collection
                coll.CollectionChanged -= metaDataView.metaDataList_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                var coll = (ObservableCollection<ImageGridItem>)e.NewValue;
                // Subscribe to CollectionChanged on the new collection
                coll.CollectionChanged += metaDataView.metaDataList_CollectionChanged;
            }

        }

        private void metaDataList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                List<MetaDataTreeNode> nodes = new List<MetaDataTreeNode>();

                foreach (ImageGridItem item in MetaDataList)
                {
                    if (item.Media != null && item.Media.MetaData != null)
                    {
                        nodes.Add(item.Media.MetaData.Tree);
                        displayMetaDataProperties(item.Media);
                       // metaDataPropertyGrid.SelectedObject = item.Media.MetaData;
                    }
                }

                if (MetaDataList.Count == 0)
                {
                    displayMetaDataProperties(null);
                }
              
                metaDataTreeList.Model = new MetaDataTreeModel(nodes);
                metaDataTreeList.ExpandAll();
               
            }));
        }

        void displayMetaDataProperties(MediaFile media)
        {

            FileMetaData metaData = media == null ? null : media.MetaData;

            fileNameTextBox.Text = media == null ? "" : media.Name;
            authorTextBox.Text = media == null ? "" : metaData.Creator;
            copyrightTextBox.Text = media == null ? "" : metaData.Copyright;
            descriptionTextBox.Text = media == null ? "" : metaData.Description;
            titleNameTextBox.Text = media == null ? "" : metaData.Title;
            creationDatePicker.Text = media == null ? "" : metaData.CreationDate.ToString("R");
            modifiedDatePicker.Text = media == null ? "" : metaData.ModifiedDate.ToString("R");
            metaDataDatePicker.Text = media == null ? "" : metaData.MetaDataDate.ToString("R");
        }
        
    }
}
