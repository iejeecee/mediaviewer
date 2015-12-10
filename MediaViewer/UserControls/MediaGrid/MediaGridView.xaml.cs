// intresting stuff:
// Lazy<T> for lazy initialization
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.UserControls.Layout;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel.Composition;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.Base.State.CollectionView;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;

namespace MediaViewer.UserControls.MediaGrid
{   
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>   
    public partial class MediaGridView : UserControl
    {
        public event EventHandler ScrolledToEnd;

        VirtualizingTilePanel panel;
                       
        public MediaGridView()
        {           
            InitializeComponent();         

            panel = null;         
                       
        }
         
        public MediaStateCollectionView MediaStateCollectionView
        {
            get { return (MediaStateCollectionView)GetValue(MediaStateCollectionViewProperty); }
            set { SetValue(MediaStateCollectionViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaStateCollectionView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaStateCollectionViewProperty =
            DependencyProperty.Register("MediaStateCollectionView", typeof(MediaStateCollectionView), typeof(MediaGridView), new PropertyMetadata(null,mediaStateCollectionViewChangedCallback));

        private static void mediaStateCollectionViewChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridView view = (MediaGridView)d;

            if (e.OldValue != null)
            {
                MediaStateCollectionView oldCollectionView = (MediaStateCollectionView)e.OldValue;
                WeakEventManager<MediaStateCollectionView,EventArgs>.RemoveHandler(oldCollectionView, "Cleared", view.mediaGridViewModel_Cleared);  
            }

            if (e.NewValue != null)
            {
                MediaStateCollectionView newCollectionView = (MediaStateCollectionView)e.NewValue;
                WeakEventManager<MediaStateCollectionView, EventArgs>.AddHandler(newCollectionView, "Cleared", view.mediaGridViewModel_Cleared);  
            }
        }

        void mediaGridViewModel_Cleared(object sender, EventArgs e)
        {
            if (panel != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => panel.ScrollOwner.ScrollToVerticalOffset(0)));
            }
        }
        
        public int NrColumns
        {
            get { return (int)GetValue(NrColumnsProperty); }
            set { SetValue(NrColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NrColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NrColumnsProperty =
            DependencyProperty.Register("NrColumns", typeof(int), typeof(MediaGridView), new PropertyMetadata(4));

        public ContextMenu MediaItemContextMenu
        {
            get { return (ContextMenu)GetValue(MediaItemContextMenuProperty); }
            set { SetValue(MediaItemContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaItemContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaItemContextMenuProperty =
            DependencyProperty.Register("MediaItemContextMenu", typeof(ContextMenu), typeof(MediaGridView), new PropertyMetadata(null));

        private void mediaGridItem_Click(object sender, SelectableMediaItem selectableItem)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                selectableItem.IsSelected = !selectableItem.IsSelected;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                MediaStateCollectionView.selectRange(selectableItem.Item);
            }
            else
            {              
                MediaStateCollectionView.selectExclusive(selectableItem.Item);                           
            }
        }
               
        private void imageGridViewModel_PrevPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageUp();              
            }
        }

        private void imageGridViewModel_NextPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.PageDown();
            }
        }

        private void imageGridViewModel_FirstPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.NegativeInfinity);
            }
        }

        private void imageGridViewModel_LastPageCommand(object sender, EventArgs e)
        {
            if (panel != null)
            {
                panel.SetHorizontalOffset(double.PositiveInfinity);
            }
        }

        

        private void virtualizingTilePanel_Loaded(object sender, RoutedEventArgs e)
        {
            panel = sender as VirtualizingTilePanel;
                        
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = (ScrollViewer)sender;
            if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
            {
                if (ScrolledToEnd != null)
                {
                    ScrolledToEnd(this, EventArgs.Empty);
                }
            }
            
        }
        
    }
}
