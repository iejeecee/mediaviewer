using Microsoft.Practices.Prism.Regions;
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
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Media.File;
using MediaViewer.UserControls.Pager;
using Microsoft.Practices.Prism.Mvvm;
using System.ComponentModel;
using MediaViewer.Infrastructure.Global.Events;
using MediaViewer.Model.Media.Base;
using MediaViewer.Model.Utils;

namespace MediaViewer.UserControls.MediaStackPanel
{
    /// <summary>
    /// Interaction logic for MediaStackPanelView.xaml
    /// </summary>
    public partial class MediaStackPanelView : UserControl
    {
        public event EventHandler<SelectableMediaItem> MediaItemClick;

        public ScrollViewer scrollViewer;
        int scrollToIndex;
  
        public MediaStackPanelView()
        {
            InitializeComponent();
            
            itemsControl.Height = itemsControl.Height + SystemParameters.HorizontalScrollBarHeight;
                      
            scrollToIndex = -1;                                 
        }
       
        private void scrollViewer_Loaded(object sender, EventArgs e)
        {
            scrollViewer = (ScrollViewer)sender;

            if (scrollToIndex != -1)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollToIndex);                
            }         
        }

        public ContextMenu MediaItemContextMenu
        {
            get { return (ContextMenu)GetValue(MediaItemContextMenuProperty); }
            set { SetValue(MediaItemContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaItemContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaItemContextMenuProperty =
            DependencyProperty.Register("MediaItemContextMenu", typeof(ContextMenu), typeof(MediaStackPanelView), new PropertyMetadata(null));
        
        public MediaStateCollectionView MediaStateCollectionView
        {
            get { return (MediaStateCollectionView)GetValue(MediaStateCollectionViewProperty); }
            set { SetValue(MediaStateCollectionViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateCollectionView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateCollectionViewProperty =
            DependencyProperty.Register("MediaStateCollectionView", typeof(MediaStateCollectionView), typeof(MediaStackPanelView), new PropertyMetadata(null, mediaStateCollectionViewChanged));

        private static void mediaStateCollectionViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaStackPanelView view = (MediaStackPanelView)d;

            if (e.OldValue != null)
            {
                MediaStateCollectionView collectionView = (MediaStateCollectionView)e.NewValue;

                collectionView.SelectionChanged -= view.mediaStateCollectionView_SelectionChanged;
            }

            if (e.NewValue != null)
            {
                MediaStateCollectionView collectionView = (MediaStateCollectionView)e.NewValue;

                collectionView.SelectionChanged += view.mediaStateCollectionView_SelectionChanged;
            }
        }

        private void mediaStateCollectionView_SelectionChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {

                ICollection<MediaItem> selectedItems = MediaStateCollectionView.getSelectedItems();
                if (selectedItems.Count == 0) return;

                scrollToIndex = MediaStateCollectionView.Media.IndexOf(new SelectableMediaItem(selectedItems.ElementAt(0)));

                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollToIndex);
                }

            }));
        }

        private void mediaGridItem_Click(object sender, SelectableMediaItem selectableItem)
        {
            if (MediaItemClick != null)
            {
                MediaItemClick(this, selectableItem);
            }            
        }

        private void mediaGridItem_Checked(object sender, SelectableMediaItem selectableItem)
        {       
            if (selectableItem.IsSelected == true) return;

            MediaStateCollectionView.deselectAll();

            selectableItem.IsSelected = true;
        }

        private void mediaGridItem_Unchecked(object sender, SelectableMediaItem selectableItem)
        {           
            selectableItem.IsSelected = false;         
        }

        private void itemsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scrollViewer != null)
            {
                if (e.Delta < 0)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 1);
                }
                else
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 1);
                }
            }

            e.Handled = true;
        }
    }
}
