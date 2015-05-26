//https://diptimayapatra.wordpress.com/2010/03/05/grouping-in-datagrid-in-wpf/
using Aga.Controls.Tree;
using MediaViewer.MediaDatabase;
using MediaViewer.Model.Collections;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.TagEditor;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
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

namespace MediaViewer.UserControls.TagTreePicker
{

    public partial class TagTreePickerView : UserControl
    {
        IEventAggregator EventAggregator { get; set; }
        ObservableCollection<TagItem> Tags;
    
        public TagTreePickerView()
        {
            InitializeComponent();
            //EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;

            Tags = new ObservableCollection<TagItem>();

            dataGrid.ItemsSource = new ListCollectionView(Tags);
        }

        public MediaStateCollectionView MediaStateCollectionView
        {
            get { return (MediaStateCollectionView)GetValue(MediaStateCollectionViewProperty); }
            set { SetValue(MediaStateCollectionViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateCollectionView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateCollectionViewProperty =
            DependencyProperty.Register("MediaStateCollectionView", typeof(MediaStateCollectionView), typeof(TagTreePickerView), new PropertyMetadata(null, mediaStateCollectionViewChangedCallback));

        private static void mediaStateCollectionViewChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagTreePickerView view = (TagTreePickerView)d;

            if (e.OldValue != null)
            {
                MediaStateCollectionView oldCollectionView = (MediaStateCollectionView)e.OldValue;
                oldCollectionView.ItemPropertyChanged -= view.mediaCollectionView_ItemPropertyChanged;
                oldCollectionView.NrItemsInStateChanged -= view.mediaCollectionView_NrItemsInStateChanged;
                //WeakEventManager<MediaStateCollectionView, EventArgs>.RemoveHandler(oldCollectionView, "Cleared", view.mediaGridViewModel_Cleared);
            }

            if (e.NewValue != null)
            {
                MediaStateCollectionView newCollectionView = (MediaStateCollectionView)e.NewValue;
                newCollectionView.ItemPropertyChanged += view.mediaCollectionView_ItemPropertyChanged;
                newCollectionView.NrItemsInStateChanged += view.mediaCollectionView_NrItemsInStateChanged;

                //WeakEventManager<MediaStateCollectionView, EventArgs>.AddHandler(newCollectionView, "Cleared", view.mediaGridViewModel_Cleared);
            }
       
            view.buildTagList();
        }

        private void mediaCollectionView_NrItemsInStateChanged(object sender, MediaStateCollectionViewChangedEventArgs e)
        {
            switch (e.Action)
            {
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Add:
                    foreach (SelectableMediaItem item in e.NewItems)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() => addTags(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Remove:
                    foreach (SelectableMediaItem item in e.OldItems)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(() => removeTags(item)));
                    }
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Clear:
                    App.Current.Dispatcher.BeginInvoke(new Action(() =>  clearTags()));
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Modified:
                    break;
                case MediaViewer.Model.Media.State.MediaStateChangedAction.Replace:
                    break;
                default:
                    break;
            }
        }

        private void mediaCollectionView_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)sender;

            if (e.PropertyName.Equals("Metadata"))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    addTags(item);
                }));

            }
        }

        void clearTags()
        {
            Tags.Clear();
        }

        void removeTags(SelectableMediaItem item)
        {

        }

        void addTags(SelectableMediaItem item)
        {
            if (item.Item.Metadata == null) return;

            item.Item.RWLock.EnterReadLock();
            try
            {                
                foreach (Tag tag in item.Item.Metadata.Tags)
                {
                    TagItem tagItem = new TagItem(tag);

                    int pos = CollectionsSort.itemIndexSortedCollection(Tags,tagItem);

                    if (pos == -1)
                    {
                        CollectionsSort.insertIntoSortedCollection(Tags, new TagItem(tag));
                    }
                    else
                    {
                        Tags[pos].Count++;
                    }
                }
            }
            finally
            {
                item.Item.RWLock.ExitReadLock();
            }
        }

        void buildTagList()
        {
            MediaStateCollectionView.MediaState.UIMediaCollection.EnterReaderLock();
            try
            {
                foreach (SelectableMediaItem media in MediaStateCollectionView.Media)
                {
                    if (media.Item.ItemState == MediaItemState.LOADED)
                    {
                        addTags(media);                        
                    }
                }
            }
            finally
            {
                MediaStateCollectionView.MediaState.UIMediaCollection.ExitReaderLock();
            }
        }

        
    }
}
