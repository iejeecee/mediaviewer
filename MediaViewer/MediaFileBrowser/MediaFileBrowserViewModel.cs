using MediaViewer.Input;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.DirectoryPicker;
using MediaViewer.UserControls.Pager;
using MediaViewer.Search;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaViewer.Import;
using MediaViewer.ImagePanel;
using VideoPlayerControl;
using MediaViewer.VideoPanel;
using MediaViewer.GridImage.VideoPreviewImage;
using MediaViewer.Torrent;
using MediaViewer.Progress;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Regions;
using MediaViewer.MetaData;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Input;
using Microsoft.Practices.Prism.PubSubEvents;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Utils;
using MediaViewer.Model.Mvvm;
using MediaViewer.GridImage.ImageCollage;
using MediaViewer.Transcode.Video;
using MediaViewer.Model.Settings;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Infrastructure;
using MediaViewer.Infrastructure.Global.Events;
using System.Collections.ObjectModel;
using MediaViewer.UserControls.LocationBox;
using MediaViewer.Model.Media.Base;
using MediaViewer.MediaFileGrid;
using MediaViewer.MediaFileStackPanel;
using MediaViewer.GeotagFileBrowser;
using MediaViewer.MediaFileBrowser.ImagePanel;
using MediaViewer.Transcode.Image;
using MediaViewer.Filter;
using MediaViewer.MediaFileBrowser.DirectoryBrowser;
using MediaViewer.UserControls.MediaPreview;

namespace MediaViewer.MediaFileBrowser
{
   
    public class MediaFileBrowserViewModel : BindableBase
    {

        MediaFileWatcher MediaFileWatcher { get; set; }
        delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);

        IMediaFileBrowserContentViewModel currentViewModel;

        public IMediaFileBrowserContentViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { 
            SetProperty(ref currentViewModel, value);
            }
        }
        
        MediaFileBrowserImagePanelViewModel imageViewModel;

        public MediaFileBrowserImagePanelViewModel ImageViewModel
        {
            get { return imageViewModel; }
            private set {  
            SetProperty(ref imageViewModel, value);
            }
        }

        MediaFileStackPanelViewModel imageMediaStackPanelViewModel;

        public MediaFileStackPanelViewModel ImageMediaStackPanelViewModel
        {
            get { return imageMediaStackPanelViewModel; }
            set {  
            SetProperty(ref imageMediaStackPanelViewModel, value);
            }
        }

        VideoViewModel videoViewModel;

        public VideoViewModel VideoViewModel
        {
            get { return videoViewModel; }
            set {  
            SetProperty(ref videoViewModel, value);
            }
        }

        MediaFileStackPanelViewModel videoMediaStackPanelViewModel;

        public MediaFileStackPanelViewModel VideoMediaStackPanelViewModel
        {
            get { return videoMediaStackPanelViewModel; }
            set {  
            SetProperty(ref videoMediaStackPanelViewModel, value);
            }
        }

        MediaFileGridViewModel mediaFileGridViewModel;

        public MediaFileGridViewModel MediaFileGridViewModel
        {
            get { return mediaFileGridViewModel; }
            set {  
            SetProperty(ref mediaFileGridViewModel, value);
            }
        }
        
        MediaFileStackPanelViewModel dummyMediaStackPanelViewModel;

        public MediaFileStackPanelViewModel DummyMediaStackPanelViewModel
        {
            get { return dummyMediaStackPanelViewModel; }
            set
            {                
                SetProperty(ref dummyMediaStackPanelViewModel, value);
            }
        }

        GeotagFileBrowserViewModel geotagFileBrowserViewModel;

        internal GeotagFileBrowserViewModel GeotagFileBrowserViewModel
        {
            get { return geotagFileBrowserViewModel; }
            set { SetProperty(ref geotagFileBrowserViewModel, value); }
        }

        MediaFileStackPanelViewModel geotagFileBrowserStackPanelViewModel;

        public MediaFileStackPanelViewModel GeotagFileBrowserStackPanelViewModel
        {
            get { return geotagFileBrowserStackPanelViewModel; }
            set
            {
                SetProperty(ref geotagFileBrowserStackPanelViewModel, value);
            }
        }

        private double maxImageScale;

        public double MaxImageScale
        {
            get { return maxImageScale; }
            set {  
                SetProperty(ref maxImageScale, value);
            }
        }

        private double minImageScale;

        public double MinImageScale
        {
            get { return minImageScale; }
            set {  
            SetProperty(ref minImageScale, value);
            }
        }

        ICollection<MediaFileItem> selectedItems;

        public ICollection<MediaFileItem> SelectedItems
        {
            get { return selectedItems; }
            set {  
                SetProperty(ref selectedItems, value);
            }
        }

        IRegionManager RegionManager {get;set;}
        IEventAggregator EventAggregator { get; set; }
      
        public MediaFileBrowserViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
    
            MediaFileWatcher = mediaFileWatcher;
            RegionManager = regionManager;
            EventAggregator = eventAggregator;
          
            MediaFileGridViewModel = new MediaFileGridViewModel(mediaFileWatcher.MediaFileState, EventAggregator);

            DummyMediaStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileGridViewModel.MediaStateCollectionView, EventAggregator);
            DummyMediaStackPanelViewModel.IsEnabled = false;

            ImageViewModel = new MediaFileBrowserImagePanelViewModel(eventAggregator);
            ImageViewModel.SelectedScaleMode = MediaViewer.UserControls.ImagePanel.ScaleMode.FIT_HEIGHT_AND_WIDTH;

            imageMediaStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileGridViewModel.MediaStateCollectionView, EventAggregator);            
            imageMediaStackPanelViewModel.IsVisible = true;

            VideoViewModel = new VideoPanel.VideoViewModel(EventAggregator);

            videoMediaStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileGridViewModel.MediaStateCollectionView, EventAggregator);            
            videoMediaStackPanelViewModel.IsVisible = true;

            GeotagFileBrowserViewModel = new GeotagFileBrowserViewModel(MediaFileGridViewModel.MediaStateCollectionView, EventAggregator);

            GeotagFileBrowserStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileGridViewModel.MediaStateCollectionView, EventAggregator);
            GeotagFileBrowserStackPanelViewModel.IsVisible = true;
                       
            DeleteSelectedItemsCommand = new Command(new Action(deleteSelectedItems));
      
            ImportSelectedItemsCommand = new Command(() =>
            {          
                ImportView import = new ImportView();
                import.DataContext = new ImportViewModel(MediaFileWatcher.Instance);
                import.ShowDialog();          
            });

            ExportSelectedItemsCommand = new Command(() =>
            {
                ImportView export = new ImportView();
                export.DataContext = new ExportViewModel(MediaFileWatcher.Instance);
                export.ShowDialog();
              
            });

            ExpandCommand = new Command<MediaFileItem>((item) =>
            {              
                if (item == null)
                {                   
                    if (SelectedItems.Count == 0) return;

                    item = SelectedItems.ElementAt(0) as MediaFileItem;
                }
               
                if (MediaFormatConvert.isImageFile(item.Location))
                {
                    navigateToImageView(item);              
                }
                else if (MediaFormatConvert.isVideoFile(item.Location))
                {
                    navigateToVideoView(item);
                }
                
            });

            ContractCommand = new Command(() =>
                {
                    NavigateBackCommand.Execute(null);
                   
                });

            ContractCommand.IsExecutable = false;

            NavigateToGeotagFileBrowserCommand = new Command(() =>
                {
                    navigateToGeotagFileBrowser();
                   
                });

            CreateImageCollageCommand = new Command(() =>
            {
                if (SelectedItems.Count == 0) return;

                ImageCollageView collage = new ImageCollageView();

                collage.ViewModel.Media = SelectedItems;
                collage.ShowDialog();
            });

            TranscodeVideoCommand = new Command(() =>
            {
                if (SelectedItems.Count == 0) return;

                VideoTranscodeView transcode = new VideoTranscodeView();
                transcode.ViewModel.Items = SelectedItems;
                transcode.ViewModel.OutputPath = MediaFileWatcher.Path;

                transcode.ShowDialog();
            });

            TranscodeImageCommand = new Command(() =>
            {
                if (SelectedItems.Count == 0) return;

                ImageTranscodeView transcode = new ImageTranscodeView();
                transcode.ViewModel.Items = SelectedItems;
                transcode.ViewModel.OutputPath = MediaFileWatcher.Path;

                transcode.ShowDialog();
            });

            CreateVideoPreviewImagesCommand = new Command(() =>
                {                 
                    if (SelectedItems.Count == 0) return;

                    VideoPreviewImageView preview = new VideoPreviewImageView();
                    
                    preview.ViewModel.Media = SelectedItems;
                    preview.ShowDialog();
                });
      
            CreateTorrentFileCommand = new Command(() =>
                {
               
                    //if (SelectedItems.Count == 0) return;

                    try
                    {
                        TorrentCreationView torrent = new TorrentCreationView();

                        torrent.ViewModel.PathRoot = mediaFileWatcher.Path;
                        torrent.ViewModel.Media = new ObservableCollection<MediaFileItem>(SelectedItems);
                        
                        torrent.ShowDialog();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
  
            
           
            NavigateToMediaFileGridCommand = new Command(navigateToMediaFileGrid);
            NavigateToImageViewCommand = new Command<MediaFileItem>(navigateToImageView);
            NavigateToVideoViewCommand = new Command<MediaFileItem>(navigateToVideoView);

            NavigateBackCommand = NavigateToMediaFileGridCommand;

            SelectedItems = new List<MediaFileItem>();

            EventAggregator.GetEvent<MediaSelectionEvent>().Subscribe(mediaFileBrowser_MediaSelectionEvent);
            
        }
      
        private void mediaFileBrowser_MediaSelectionEvent(MediaSelectionPayload selection)
        {
            List<MediaFileItem> items = new List<MediaFileItem>();
            foreach (MediaItem item in selection.Items)
            {
                items.Add(item as MediaFileItem);
            }

            SelectedItems = items;
        }

        public Command CreateImageCollageCommand { get; set; }
        public Command CreateVideoPreviewImagesCommand {get;set;}
        public Command CreateTorrentFileCommand { get; set; }
        public Command TranscodeVideoCommand { get; set; }
        public Command TranscodeImageCommand { get; set; }
        public Command DeleteSelectedItemsCommand { get; set; }        
        public Command ImportSelectedItemsCommand { get; set; }
        public Command ExportSelectedItemsCommand { get; set; }
        public Command<MediaFileItem> ExpandCommand { get; set; }
        public Command ContractCommand { get; set; }
        public Command NavigateToMediaFileGridCommand { get; set; }
        public Command<MediaFileItem> NavigateToImageViewCommand { get; set; }
        public Command<MediaFileItem> NavigateToVideoViewCommand { get; set; }
        public Command NavigateToGeotagFileBrowserCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }
      
        private void deleteSelectedItems()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();         
            if (SelectedItems.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to permanently delete " + SelectedItems.Count.ToString() + " file(s)?",
                "Delete Media", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {              
                try
                {
                    MediaFileWatcher.Instance.MediaFileState.delete(SelectedItems, tokenSource.Token);                    
                }
                catch (Exception ex)
                {

                    Logger.Log.Error("Error deleting file", ex);
                    MessageBox.Show(ex.Message, "Error deleting file");
                }
                
            }

        }

        void initialize()
        {
            // add metadata view
            Uri metaDataViewUri = new Uri(typeof(MetaDataView).FullName, UriKind.Relative);
            RegionManager.RequestNavigate("mediaMetadataExpander", metaDataViewUri);

            // add tag filter
            Uri tagFilterViewUri = new Uri(typeof(TagFilterView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("MediaStateCollectionView", MediaFileGridViewModel.MediaStateCollectionView);

            RegionManager.RequestNavigate("mediaFilterExpander", tagFilterViewUri, navigationParams);

            // add a directory browser
            Uri directoryBrowserUri = new Uri(typeof(MediaFileBrowserDirectoryBrowserView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate("mediaSearchExpander", directoryBrowserUri);

            // add search view.
            Uri searchViewUri = new Uri(typeof(SearchView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate("mediaSearchExpander", searchViewUri);

            // add media preview
            Uri mediaPreviewUri = new Uri(typeof(MediaPreviewView).FullName, UriKind.Relative);
            RegionManager.RequestNavigate("mediaPreviewExpander", mediaPreviewUri);

            navigateToMediaFileGrid();
        }
           
        public void navigateToMediaFileGrid()
        {
            Uri mediaFileGridViewUri = new Uri(typeof(MediaFileGridView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();        
            navigationParams.Add("viewModel", MediaFileGridViewModel);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, mediaFileGridViewUri, (result) =>
            {
                CurrentViewModel = MediaFileGridViewModel;               
           
            }, navigationParams);

            Shell.ShellViewModel.navigateToMediaStackPanelView(DummyMediaStackPanelViewModel);

            Title = MediaFileWatcher.Path;
        }

        public void navigateToVideoView(MediaFileItem item)
        {
            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("item", item);           
            navigationParams.Add("viewModel", VideoViewModel);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, VideoViewUri, (result) =>
            {
                CurrentViewModel = VideoViewModel;                
          
            }, navigationParams);

            VideoMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaFilterMode.Video);
            Shell.ShellViewModel.navigateToMediaStackPanelView(videoMediaStackPanelViewModel, item.Location);
           
        }

        public void navigateToImageView(MediaFileItem item)
        {
            Uri imageViewUri = new Uri(typeof(MediaFileBrowserImagePanelView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("item", item);
            navigationParams.Add("viewModel", ImageViewModel);
           
            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, imageViewUri, (result) =>
            {
                CurrentViewModel = ImageViewModel;               
       
            }, navigationParams);

            ImageMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaFilterMode.Images);
            Shell.ShellViewModel.navigateToMediaStackPanelView(imageMediaStackPanelViewModel, item != null ? item.Location : null);
        }

        public void navigateToGeotagFileBrowser()
        {
            Uri geotagFileBrowserUri = new Uri(typeof(GeotagFileBrowserView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();           
            navigationParams.Add("viewModel", GeotagFileBrowserViewModel);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, geotagFileBrowserUri, (result) =>
            {
                CurrentViewModel = GeotagFileBrowserViewModel;               

            }, navigationParams);

            GeotagFileBrowserStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaFilterMode.None);
            Shell.ShellViewModel.navigateToMediaStackPanelView(GeotagFileBrowserStackPanelViewModel);
        }

        

        string title;
        public String Title {
            get
            {
                return (title);
            }
            set
            {
                title = value;

                EventAggregator.GetEvent<TitleChangedEvent>().Publish(value);
            }
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            CurrentViewModel.OnNavigatedFrom(navigationContext);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            String newTitle = "";

            if (CurrentViewModel == null)
            {
                initialize();

                newTitle = MediaFileWatcher.Path;
            }
            else
            {
                if (CurrentViewModel is MediaFileGridViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(DummyMediaStackPanelViewModel);
                    newTitle = MediaFileWatcher.Path;
                }
                else if (CurrentViewModel is MediaFileBrowserImagePanelViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(imageMediaStackPanelViewModel);
                    newTitle = ImageViewModel.CurrentItem.Name;                    
                }
                else if (CurrentViewModel is VideoViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(videoMediaStackPanelViewModel);
                    newTitle = VideoViewModel.CurrentItem.Video.Name;                    
                }
                else if (CurrentViewModel is GeotagFileBrowserViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(geotagFileBrowserStackPanelViewModel);
                    newTitle = MediaFileWatcher.Path;
                }
                
                CurrentViewModel.OnNavigatedTo(navigationContext);                
            }

            Title = newTitle;
        }
       
    }
}
