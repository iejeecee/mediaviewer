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
        private MetaDataViewModel ViewModel
        {
            get { return this.Resources["viewModel"] as MetaDataViewModel; }
        }

        public MetaDataView()
        {
            InitializeComponent();

            dynamicElements = new List<UIElement>();
            dynamicRows = new List<RowDefinition>();
                     
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

                ViewModel.ItemList = MetaDataList;
                displayDynamicProperties(ViewModel.DynamicProperties);
                                        
            }));

        }
            
        List<RowDefinition> dynamicRows;
        List<UIElement> dynamicElements;

        void displayDynamicProperties(List<Tuple<String, String>> additionalProps)
        {
                        
            foreach (RowDefinition row in dynamicRows)
            {
                mainGrid.RowDefinitions.Remove(row);               
            }

            dynamicRows.Clear();

            foreach (UIElement elem in dynamicElements)
            {
                mainGrid.Children.Remove(elem);               
            }

            dynamicElements.Clear();

            foreach(Tuple<String,String> prop in additionalProps) {

                RowDefinition row;

                if (String.IsNullOrEmpty(prop.Item1))
                {
                    row = new RowDefinition();
                    row.Height = GridLength.Auto;

                    mainGrid.RowDefinitions.Add(row);
                    dynamicRows.Add(row);

                    Separator seperator = new Separator();
                    dynamicElements.Add(seperator);

                    mainGrid.Children.Add(seperator);
                    Grid.SetRow(seperator, mainGrid.RowDefinitions.Count - 1);
                    Grid.SetColumnSpan(seperator, 3);

                }

                row = new RowDefinition();
                row.Height = GridLength.Auto;

                mainGrid.RowDefinitions.Add(row); 
                dynamicRows.Add(row);

                Label label = new Label();
                label.Style = Resources["labelStyle"] as Style;
                label.Content = prop.Item1;             

                mainGrid.Children.Add(label);
                dynamicElements.Add(label);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, mainGrid.RowDefinitions.Count - 1);

                TextBlock value = new TextBlock();
                value.Text = prop.Item2;
                value.Margin = new Thickness(5, 5, 5, 5);

                mainGrid.Children.Add(value);
                dynamicElements.Add(value);
                Grid.SetColumn(value, 1);
                Grid.SetRow(value, mainGrid.RowDefinitions.Count - 1);
                
            }
        }
        
    }
}
