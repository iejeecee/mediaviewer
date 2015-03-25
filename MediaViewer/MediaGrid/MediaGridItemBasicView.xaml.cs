using MediaViewer.Model.Media.File;
using MediaViewer.Model.Global.Events;
using System;
using System.Collections.Generic;
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
using MediaViewer.Model.Utils;
using MediaViewer.Model.Media.State.CollectionView;
using Microsoft.Practices.Prism.PubSubEvents;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.Model.Media.Base;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for MediaGridItemBasicView.xaml
    /// </summary>
    public partial class MediaGridItemBasicView : UserControl
    {
       
        IEventAggregator EventAggregator { get; set; }

        public MediaGridItemBasicView()
        {
            InitializeComponent();

            EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;
        }

        public SelectableMediaItem SelectableMediaItem
        {
            get { return (SelectableMediaItem)GetValue(MediaFileItemProperty); }
            set { SetValue(MediaFileItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectableMediaFileItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaFileItemProperty =
            DependencyProperty.Register("SelectableMediaItem", typeof(SelectableMediaItem), typeof(MediaGridItemBasicView), new PropertyMetadata(null));

        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaItem item = SelectableMediaItem.Item;

            if (MediaViewer.Model.Utils.MediaFormatConvert.isImageFile(item.Location))
            {
                Shell.ShellViewModel.navigateToImageView(item.Location);
            }
            else if (MediaFormatConvert.isVideoFile(item.Location))
            {
                Shell.ShellViewModel.navigateToVideoView(item.Location);
            }

        }
         
        public bool IsGridLoaded
        {
            get { return (bool)GetValue(IsGridLoadedProperty); }
            set { SetValue(IsGridLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGridLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGridLoadedProperty =
            DependencyProperty.Register("IsGridLoaded", typeof(bool), typeof(MediaGridItemBasicView), new PropertyMetadata(true, isGridLoadedChangedCallback));

        private static void isGridLoadedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemBasicView item = d as MediaGridItemBasicView;

            item.selectAllMenuItem.IsEnabled = !(bool)e.NewValue;

        }

        private void selectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.selectAll();
        }

        private void deselectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {

            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.deselectAll();

        }

        private void browseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaItem item = SelectableMediaItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(location);
        }

        private void openInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaItem item = SelectableMediaItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            Process.Start(location);
        }



        private void imageGridItem_Checked(object sender, RoutedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)DataContext;

            if (item.IsSelected == true) return;
          
            MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
            cv.deselectAll();
           
            item.IsSelected = true;
         
        }

        private void imageGridItem_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectableMediaItem item = (SelectableMediaItem)DataContext;

            item.IsSelected = false;
         
          
        }
          
    }
}
