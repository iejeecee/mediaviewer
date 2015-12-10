//https://diptimayapatra.wordpress.com/2010/03/05/grouping-in-datagrid-in-wpf/
using Aga.Controls.Tree;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.Base.State;
using MediaViewer.Model.Media.Base.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Utils.WPF;
using MediaViewer.TagEditor;
using MediaViewer.UserControls.Layout;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.Filter
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TagFilterView : UserControl, IRegionMemberLifetime, INavigationAware, ITabbedExpanderAware
    {
        TagItemList tagsList;        
        bool extendTimer;
        
        Timer timer;
        int timerInterval = 100;
        
        public TagFilterView()
        {
            InitializeComponent();
                                   
            timer = new Timer();
            timer.AutoReset = false;
            timer.Interval = timerInterval;
            timer.Elapsed += timer_Elapsed;

            extendTimer = false;
            
            tagsList = new TagItemList();
            tagsList.IsFilterChanged += tagsList_IsFilterChanged;
            dataGrid.ItemsSource = tagsList;

            ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Count", ListSortDirection.Descending));
            dataGrid.ColumnFromDisplayIndex(1).SortDirection = ListSortDirection.Descending;

            TabName = "Tag Filter";
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(2);
            TabBorderBrush = ClassicBorderDecorator.ClassicBorderBrush;            
            TabIsSelected = true;
        }
     
        MediaStateCollectionView mediaCollectionView;

        public MediaStateCollectionView MediaCollectionView
        {
            get { return (mediaCollectionView); }
            set {

                if (ReferenceEquals(value, mediaCollectionView))
                {
                    return;
                }

                if (mediaCollectionView != null)
                {
                    mediaCollectionView.ItemPropertyChanged -= mediaCollection_ItemPropertyChanged;
                    mediaCollectionView.NrItemsInStateChanged -= mediaCollection_NrItemsInStateChanged;
                    mediaCollectionView.Cleared -= mediaCollectionView_Cleared;
                }
                
                if (value != null)
                {
                    value.ItemPropertyChanged += mediaCollection_ItemPropertyChanged;
                    value.NrItemsInStateChanged += mediaCollection_NrItemsInStateChanged;
                    value.Cleared += mediaCollectionView_Cleared;
                }

                mediaCollectionView = value;

                buildTagList();
               
            }
        }

        void mediaCollectionView_Cleared(object sender, EventArgs e)
        {
            mediaCollectionView.TagFilter.Clear();

            App.Current.Dispatcher.BeginInvoke(new Action(() => {

                ToggleButton includedClearToggleButton =
                    VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "includeClearToggleButton");

                ToggleButton excludedClearToggleButton =
                    VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "excludeClearToggleButton");

                if (includedClearToggleButton != null && excludedClearToggleButton != null)
                {
                
                        includedClearToggleButton.IsEnabled = false;
                        includedClearToggleButton.IsChecked = false;
                
                }
            }));
            
        }

        private void mediaCollection_NrItemsInStateChanged(object sender, MediaStateCollectionViewChangedEventArgs e)
        {
            if (e.Action == MediaStateChangedAction.Clear)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(tagsList.Clear));                                                       
            }
      
            if (timer.Enabled) 
            {
                extendTimer = true;
            }
            else 
            {                
                timer.Enabled = true;
            }
        }

        private void mediaCollection_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (timer.Enabled)
            {
                extendTimer = true;
            } 
            else 
            {                           
                timer.Enabled = true;
            }
            
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (extendTimer)
            {
                //System.Diagnostics.Debug.Print("Extend timer " + System.Threading.Thread.CurrentThread.ManagedThreadId);
                extendTimer = false;
                timer.Enabled = true;                
                return;
            }

            //System.Diagnostics.Debug.Print("Stop timer " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            buildTagList();

            
        }

        

        void addTags(SelectableMediaItem item, List<TagItem> tags)
        {
            if (item.Item.Metadata == null) return;

            item.Item.EnterReadLock();
            try
            {                
                foreach (Tag tag in item.Item.Metadata.Tags)
                {
                    TagItem tagItem = new TagItem(tag, mediaCollectionView);

                    int index = tags.IndexOf(tagItem);

                    if (index == -1)
                    {
                        tags.Add(tagItem);
                    }
                    else
                    {
                        tags[index].Count += 1;
                    }
                }
            }
            finally
            {
                item.Item.ExitReadLock();
            }
        }

        void buildTagList()
        {
            List<TagItem> tagItems = new List<TagItem>();            

            MediaCollectionView.EnterReadLock();
            try
            {
                foreach (SelectableMediaItem media in MediaCollectionView)
                {
                    if (media.Item.ItemState == MediaItemState.LOADED)
                    {
                        addTags(media, tagItems);                        
                    }
                }               
            }
            finally
            {
                MediaCollectionView.ExitReadLock();
            }

            App.Current.Dispatcher.BeginInvoke(new Action(() => {

                tagsList.Clear();
                tagsList.AddRange(tagItems);               

                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                view.Refresh();

            }));

/*          
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SortDescription? sortDescription = null;

                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view != null && view.SortDescriptions.Count > 0)
                {
                    sortDescription = view.SortDescriptions[0];                    
                }

                dataGrid.ItemsSource = tagsList.tagItems;

                view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (sortDescription == null)
                {
                    sortDescription = new SortDescription("Count", ListSortDirection.Descending);
                }
                
                view.SortDescriptions.Add(sortDescription.Value);
                int index = dataGridColumnIndex(sortDescription.Value.PropertyName);

                dataGrid.ColumnFromDisplayIndex(index).SortDirection = sortDescription.Value.Direction;

                view.Refresh();

            }));*/
        }

        int dataGridColumnIndex(string propertyName)
        {
            int i = 0;

            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (column.Header.Equals(propertyName))
                {
                    return (i);
                }

                i++;
            }

            return (-1);
        }

        public bool KeepAlive
        {
            get { return (true); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MediaCollectionView = (MediaStateCollectionView)navigationContext.Parameters["MediaStateCollectionView"];
        }
  
        void tagsList_IsFilterChanged(object sender, EventArgs e)
        {
            ToggleButton includedClearToggleButton =
                VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "includeClearToggleButton");

            ToggleButton excludedClearToggleButton =
                VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "excludeClearToggleButton");

            TagItem item = (TagItem)sender;

            if (item.IsIncluded || item.IsExcluded)
            {
                mediaCollectionView.TagFilter.Add(item);

                if (item.IsIncluded)
                {
                    includedClearToggleButton.IsEnabled = true;
                    includedClearToggleButton.IsChecked = true;
                }
                else
                {
                    excludedClearToggleButton.IsEnabled = true;
                    excludedClearToggleButton.IsChecked = true;
                }
            }
            else
            {
                mediaCollectionView.TagFilter.Remove(item);

                int nrIncluded = 0, nrExcluded = 0;

                foreach (TagItem tagItem in mediaCollectionView.TagFilter)
                {
                    if (tagItem.IsIncluded) nrIncluded++;
                    if (tagItem.IsExcluded) nrExcluded++;
                }

                if (nrExcluded == 0)
                {

                    excludedClearToggleButton.IsEnabled = false;
                    excludedClearToggleButton.IsChecked = false;
                }

                if (nrIncluded == 0)
                {
                    includedClearToggleButton.IsEnabled = false;
                    includedClearToggleButton.IsChecked = false;
                }
            }

            MediaCollectionView.refresh();
        }
      
        private void includeClearToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MediaCollectionView.TagFilter.RemoveAll((i) => i.IsIncluded == true);
            foreach (TagItem item in tagsList)
            {
                item.IsIncluded = false;
            }

            MediaCollectionView.refresh();

            ToggleButton includedClearToggleButton =
                VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "includeClearToggleButton");

            includedClearToggleButton.IsEnabled = false;
            includedClearToggleButton.IsChecked = false;
        }

        private void excludeClearToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            
            MediaCollectionView.TagFilter.RemoveAll((i) => i.IsExcluded == true);
            foreach (TagItem item in tagsList)
            {
                item.IsExcluded = false;
            }

            MediaCollectionView.refresh();

            ToggleButton excludedClearToggleButton =
                VisualTreeUtils.findVisualChildByName<ToggleButton>(dataGrid, "excludeClearToggleButton");

            excludedClearToggleButton.IsEnabled = false;
            excludedClearToggleButton.IsChecked = false;
        }

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }       
        public Thickness TabBorderThickness { get; set; }       
        public Brush TabBorderBrush { get; set; }
        
    }
}
