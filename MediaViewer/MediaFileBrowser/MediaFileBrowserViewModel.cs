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
using MediaViewer.VideoTranscode;
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

namespace MediaViewer.MediaFileBrowser
{
   
    public class MediaFileBrowserViewModel : BindableBase
    {

        private MediaFileWatcher mediaFileWatcher;
        private delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);

        IMediaFileBrowserContentViewModel currentViewModel;

        public IMediaFileBrowserContentViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { 
            SetProperty(ref currentViewModel, value);
            }
        }

        ImageViewModel imageViewModel;

        public ImageViewModel ImageViewModel
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
        AppSettings Settings { get; set; }

        public MediaFileBrowserViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager, IEventAggregator eventAggregator, AppSettings settings)
        {

            this.mediaFileWatcher = mediaFileWatcher;
            RegionManager = regionManager;
            EventAggregator = eventAggregator;
            Settings = settings;

            BrowsePathHistory = Settings.BrowsePathHistory;            
            FavoriteLocations = Settings.FavoriteLocations;            
                 
            ImageViewModel = new ImagePanel.ImageViewModel(eventAggregator);
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.RELATIVE;
            ImageViewModel.IsEffectsEnabled = false;

            imageMediaStackPanelViewModel = new MediaFileStackPanelViewModel(mediaFileWatcher.MediaFileState, EventAggregator);
            imageMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Images);
            imageMediaStackPanelViewModel.IsVisible = true;

            VideoViewModel = new VideoPanel.VideoViewModel(AppSettings.Instance, EventAggregator);

            videoMediaStackPanelViewModel = new MediaFileStackPanelViewModel(mediaFileWatcher.MediaFileState, EventAggregator);
            videoMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Video);
            videoMediaStackPanelViewModel.IsVisible = true;

            GeotagFileBrowserStackPanelViewModel = new MediaFileStackPanelViewModel(mediaFileWatcher.MediaFileState, EventAggregator);
            GeotagFileBrowserStackPanelViewModel.IsVisible = true;

            GeotagFileBrowserViewModel = new GeotagFileBrowserViewModel(GeotagFileBrowserStackPanelViewModel.MediaStateCollectionView, EventAggregator);

            MediaFileGridViewModel = new MediaFileGridViewModel(mediaFileWatcher.MediaFileState, EventAggregator);

            DummyMediaStackPanelViewModel = new MediaFileStackPanelViewModel(mediaFileWatcher.MediaFileState, EventAggregator);
            DummyMediaStackPanelViewModel.IsEnabled = false;

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

            GeotagFileBrowserCommand = new Command(() =>
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
                transcode.ViewModel.OutputPath = BrowsePath;

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
  
            EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Subscribe((location) =>
            {
                BrowsePath = location;
            });
           
            NavigateToImageGridCommand = new Command(navigateToMediaFileGrid);
            NavigateToImageViewCommand = new Command<MediaFileItem>(navigateToImageView);
            NavigateToVideoViewCommand = new Command<MediaFileItem>(navigateToVideoView);

            NavigateBackCommand = NavigateToImageGridCommand;

            SelectedItems = new List<MediaFileItem>();

            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaBatchSelectionEvent>().Subscribe(mediaFileBrowser_MediaBatchSelectionEvent);
            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Subscribe(mediaFileBrowser_MediaSelectionEvent);

            
        }

        private void mediaFileBrowser_MediaSelectionEvent(MediaItem selectedItem)
        {
            List<MediaFileItem> selectedItems = new List<MediaFileItem>();           
            selectedItems.Add(selectedItem as MediaFileItem);

            SelectedItems = selectedItems;
        }

        private void mediaFileBrowser_MediaBatchSelectionEvent(ICollection<MediaItem> selectedItems)
        {
            List<MediaFileItem> items = new List<MediaFileItem>();
            foreach (MediaItem item in selectedItems)
            {
                items.Add(item as MediaFileItem);
            }

            SelectedItems = items;
        }

         
        public string BrowsePath
        {

            set
            {
              
                FileInfo fileInfo = new FileInfo(value);

                string pathWithoutFileName;

                if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {

                    pathWithoutFileName = value;

                }
                else
                {

                    pathWithoutFileName = FileUtils.getPathWithoutFileName(value);

                }
               
                
                
                if (mediaFileWatcher.Path.Equals(pathWithoutFileName))
                {

                    return;
                }

                mediaFileWatcher.Path = pathWithoutFileName;
               
                Title = value;
                EventAggregator.GetEvent<MediaBrowserPathChangedEvent>().Publish(value);

                OnPropertyChanged("BrowsePath");
            }

            get
            {                
                return (mediaFileWatcher.Path);
            }
        }

        ObservableCollection<String> browsePathHistory;

        public ObservableCollection<String> BrowsePathHistory
        {
            get { return browsePathHistory; }
            set { SetProperty(ref browsePathHistory, value); }
        }

        ObservableCollection<String> favoriteLocations;

        public ObservableCollection<String> FavoriteLocations
        {
            get { return favoriteLocations; }
            set { SetProperty(ref favoriteLocations, value); }
        }

        public Command CreateImageCollageCommand { get; set; }
        public Command CreateVideoPreviewImagesCommand {get;set;}
        public Command CreateTorrentFileCommand { get; set; }
        public Command TranscodeVideoCommand { get; set; }
        public Command DeleteSelectedItemsCommand { get; set; }
        public Command GeotagFileBrowserCommand { get; set; }
        public Command ImportSelectedItemsCommand { get; set; }
        public Command ExportSelectedItemsCommand { get; set; }
        public Command<MediaFileItem> ExpandCommand { get; set; }
        public Command ContractCommand { get; set; }
        public Command NavigateToImageGridCommand { get; set; }
        public Command<MediaFileItem> NavigateToImageViewCommand { get; set; }
        public Command<MediaFileItem> NavigateToVideoViewCommand { get; set; }
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

        public void navigateToMetaData()
        {
            Uri MetaDataViewUri = new Uri(typeof(MetaDataView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserRightTabRegion, MetaDataViewUri, (result) =>
            {
           
            });

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
          
        }

        public void navigateToVideoView(MediaFileItem item)
        {
            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("location", item != null ? item.Location : null);           
            navigationParams.Add("viewModel", VideoViewModel);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, VideoViewUri, (result) =>
            {
                CurrentViewModel = VideoViewModel;
          
            }, navigationParams);


            Shell.ShellViewModel.navigateToMediaStackPanelView(videoMediaStackPanelViewModel, item.Location);
           
        }

        public void navigateToImageView(MediaFileItem item)
        {
            Uri imageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("location", item != null ? item.Location : null);
            navigationParams.Add("viewModel", ImageViewModel);
           
            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, imageViewUri, (result) =>
            {
                CurrentViewModel = ImageViewModel;
       
            }, navigationParams);

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

            Shell.ShellViewModel.navigateToMediaStackPanelView(GeotagFileBrowserStackPanelViewModel);
        }

        private void directoryBrowser_Renamed(System.Object sender, System.IO.RenamedEventArgs e)
        {

            // if(mediaFileWatcher.Path.Equals(e.OldFullPath)) {

            BrowsePath = e.FullPath;
            //}
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
                navigateToMetaData();
                navigateToMediaFileGrid();

                newTitle = BrowsePath;
            }
            else
            {
                if (CurrentViewModel is MediaFileGridViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(DummyMediaStackPanelViewModel);
                    newTitle = BrowsePath;
                }
                else if (CurrentViewModel is ImageViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(imageMediaStackPanelViewModel);
                    newTitle = ImageViewModel.CurrentLocation;
                    if (newTitle != null) newTitle = System.IO.Path.GetFileName(newTitle);
                }
                else if (CurrentViewModel is VideoViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(videoMediaStackPanelViewModel);
                    newTitle = VideoViewModel.CurrentLocation;
                    if (newTitle != null) newTitle = System.IO.Path.GetFileName(newTitle);
                }
                else if (CurrentViewModel is GeotagFileBrowserViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(geotagFileBrowserStackPanelViewModel);
                    newTitle = BrowsePath;
                }
                
                CurrentViewModel.OnNavigatedTo(navigationContext);                
            }

            Title = newTitle;
        }
       
    }
}
