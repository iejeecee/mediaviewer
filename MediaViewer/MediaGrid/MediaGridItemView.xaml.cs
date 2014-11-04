using MediaViewer.ImagePanel;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.Torrent;
using MediaViewer.VideoPanel;
using MediaViewer.VideoPreviewImage;
using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;

namespace MediaViewer.MediaGrid
{
    /// <summary>
    /// Interaction logic for MediaGridItemView.xaml
    /// </summary>
    public partial class MediaGridItemView : UserControl
    {      
        IEventAggregator EventAggregator { get; set; }
       
        public MediaGridItemView()
        {
            InitializeComponent();
            selectAllMenuItem.IsEnabled = false;

            EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;
        }

        public SelectableMediaFileItem SelectableMediaFileItem
        {
            get { return (SelectableMediaFileItem)GetValue(SelectableMediaFileItemProperty); }
            set { SetValue(SelectableMediaFileItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectableMediaFileItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectableMediaFileItemProperty =
            DependencyProperty.Register("SelectableMediaFileItem", typeof(SelectableMediaFileItem), typeof(MediaGridItemView), new PropertyMetadata(null));

        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {          
            MediaFileItem item = SelectableMediaFileItem.Item;

            if (MediaViewer.Model.Utils.MediaFormatConvert.isImageFile(item.Location))
            {
                Shell.ShellViewModel.navigateToImageView(item.Location);                
            }
            else if (MediaFormatConvert.isVideoFile(item.Location))
            {
                Shell.ShellViewModel.navigateToVideoView(item.Location);      
            }

        }

        public MediaStateSortMode ExtraInfoType
        {
            get { return (MediaStateSortMode)GetValue(ExtraInfoTypeProperty); }
            set { SetValue(ExtraInfoTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtraInfoType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtraInfoTypeProperty =
            DependencyProperty.Register("ExtraInfoType", typeof(MediaStateSortMode), typeof(MediaGridItemView), new PropertyMetadata(MediaStateSortMode.Name, imageGridItemView_ExtraInfoTypeChangedCallback));

        private static void imageGridItemView_ExtraInfoTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView view = d as MediaGridItemView;
            view.extraInfo.InfoType = (MediaStateSortMode)e.NewValue;
        }

        public bool IsGridLoaded
        {
            get { return (bool)GetValue(IsGridLoadedProperty); }
            set { SetValue(IsGridLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGridLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGridLoadedProperty =
            DependencyProperty.Register("IsGridLoaded", typeof(bool), typeof(MediaGridItemView), new PropertyMetadata(true, isGridLoadedChangedCallback));

        private static void isGridLoadedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaGridItemView item = d as MediaGridItemView;

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
            MediaFileItem item = SelectableMediaFileItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(location);
        }

        private void openInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MediaFileItem item = SelectableMediaFileItem.Item;

            String location = FileUtils.getPathWithoutFileName(item.Location);

            Process.Start(location);
        }

     

        private void imageGridItem_Checked(object sender, RoutedEventArgs e)
        {
           
            if (SelectableMediaFileItem.IsSelected == true) return;

            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                MediaStateCollectionView cv = (this.Tag as MediaStateCollectionView);
                cv.deselectAll();         
            }

            SelectableMediaFileItem.IsSelected = true;     

           
        }

        private void imageGridItem_Unchecked(object sender, RoutedEventArgs e)
        {
          
            SelectableMediaFileItem.IsSelected = false;     
            
        }

        
    }
}
