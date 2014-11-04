using MediaViewer.MediaGrid;
using MediaViewer.Input;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.DirectoryPicker;
using MediaViewer.Pager;
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
using MediaViewer.Export;
using MediaViewer.ImagePanel;
using VideoPlayerControl;
using MediaViewer.VideoPanel;
using MediaViewer.VideoPreviewImage;
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

namespace MediaViewer.MediaFileBrowser
{
   
    public class MediaFileBrowserViewModel : BindableBase
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
           
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

        MediaStackPanelViewModel imageMediaStackPanelViewModel;

        public MediaStackPanelViewModel ImageMediaStackPanelViewModel
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

        MediaStackPanelViewModel videoMediaStackPanelViewModel;

        public MediaStackPanelViewModel VideoMediaStackPanelViewModel
        {
            get { return videoMediaStackPanelViewModel; }
            set {  
            SetProperty(ref videoMediaStackPanelViewModel, value);
            }
        }

        MediaGridViewModel mediaGridViewModel;

        public MediaGridViewModel MediaGridViewModel
        {
            get { return mediaGridViewModel; }
            set {  
            SetProperty(ref mediaGridViewModel, value);
            }
        }

        MediaStackPanelViewModel dummyMediaStackPanelViewModel;

        public MediaStackPanelViewModel DummyMediaStackPanelViewModel
        {
            get { return dummyMediaStackPanelViewModel; }
            set
            {                
                SetProperty(ref dummyMediaStackPanelViewModel, value);
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

            this.mediaFileWatcher = mediaFileWatcher;
            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            ImageViewModel = new ImagePanel.ImageViewModel(eventAggregator);
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.RELATIVE;
            ImageViewModel.IsEffectsEnabled = false;

            ImageMediaStackPanelViewModel = new MediaStackPanelViewModel(mediaFileWatcher.MediaState, EventAggregator);
            ImageMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Images);
            ImageMediaStackPanelViewModel.IsVisible = true;

            VideoViewModel = new VideoPanel.VideoViewModel(Settings.AppSettings.Instance, EventAggregator);

            VideoMediaStackPanelViewModel = new MediaStackPanelViewModel(mediaFileWatcher.MediaState, EventAggregator);
            VideoMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Video);
            VideoMediaStackPanelViewModel.IsVisible = true;

            MediaGridViewModel = new MediaGrid.MediaGridViewModel(mediaFileWatcher.MediaState, EventAggregator);

            DummyMediaStackPanelViewModel = new MediaStackPanelViewModel(mediaFileWatcher.MediaState, EventAggregator);
            DummyMediaStackPanelViewModel.IsEnabled = false;

            DeleteSelectedItemsCommand = new Command(new Action(deleteSelectedItems));
      
            ImportSelectedItemsCommand = new Command(() =>
            {          
                ImportView import = new ImportView();
                import.ShowDialog();          
            });

            ExportSelectedItemsCommand = new Command(async () =>
            {
               
                if (SelectedItems.Count == 0) return;

                CancellableOperationProgressView export = new CancellableOperationProgressView();
                ExportViewModel vm = new ExportViewModel(mediaFileWatcher.MediaState);
                export.DataContext = vm;
                export.Show();            
                await vm.exportAsync(SelectedItems);

            });

            ExpandCommand = new Command<MediaFileItem>((item) =>
            {              
                if (item == null)
                {                   
                    if (SelectedItems.Count == 0) return;

                    item = SelectedItems.ElementAt(0);
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

            CreateVideoPreviewImagesCommand = new Command(() =>
                {                 
                    if (SelectedItems.Count == 0) return;

                    VideoPreviewImageView preview = new VideoPreviewImageView();
                    
                    preview.ViewModel.Media = SelectedItems;
                    preview.ShowDialog();
                });

      

            CreateTorrentFileCommand = new Command(() =>
                {
               
                    if (SelectedItems.Count == 0) return;

                    try
                    {
                        TorrentCreationView torrent = new TorrentCreationView();

                        torrent.ViewModel.Media = SelectedItems;
                        torrent.ViewModel.PathRoot = mediaFileWatcher.Path;

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
           
            NavigateToImageGridCommand = new Command(navigateToMediaGrid);
            NavigateToImageViewCommand = new Command<MediaFileItem>(navigateToImageView);
            NavigateToVideoViewCommand = new Command<MediaFileItem>(navigateToVideoView);

            NavigateBackCommand = NavigateToImageGridCommand;

            SelectedItems = new List<MediaFileItem>();

            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaBatchSelectionEvent>().Subscribe(mediaFileBrowser_MediaBatchSelectionEvent);
            EventAggregator.GetEvent<MediaViewer.Model.Global.Events.MediaSelectionEvent>().Subscribe(mediaFileBrowser_MediaSelectionEvent);

        }

        private void mediaFileBrowser_MediaSelectionEvent(MediaFileItem selectedItem)
        {
            List<MediaFileItem> selectedItems = new List<MediaFileItem>();
            selectedItems.Add(selectedItem);

            SelectedItems = selectedItems;
        }

        private void mediaFileBrowser_MediaBatchSelectionEvent(ICollection<MediaFileItem> selectedItems)
        {
            SelectedItems = selectedItems;
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


        public Command CreateVideoPreviewImagesCommand {get;set;}
        public Command CreateTorrentFileCommand { get; set; }
        public Command DeleteSelectedItemsCommand { get; set; }
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
                    MediaFileWatcher.Instance.MediaState.delete(SelectedItems, tokenSource.Token);
                    
                }
                catch (Exception ex)
                {

                    log.Error("Error deleting file", ex);
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

        public void navigateToMediaGrid()
        {
            Uri mediaGridViewUri = new Uri(typeof(MediaGridView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();        
            navigationParams.Add("viewModel", MediaGridViewModel);

            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, mediaGridViewUri, (result) =>
            {
                CurrentViewModel = MediaGridViewModel;
           
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


            Shell.ShellViewModel.navigateToMediaStackPanelView(VideoMediaStackPanelViewModel, item.Location);
           
        }

        public void navigateToImageView(MediaFileItem item)
        {
            Uri ImageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("location", item != null ? item.Location : null);
            navigationParams.Add("viewModel", ImageViewModel);
           
            RegionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, ImageViewUri, (result) =>
            {
                CurrentViewModel = ImageViewModel;
       
            }, navigationParams);

            Shell.ShellViewModel.navigateToMediaStackPanelView(ImageMediaStackPanelViewModel, item.Location);
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
                navigateToMediaGrid();

                newTitle = BrowsePath;
            }
            else
            {
                if (CurrentViewModel is MediaGridViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(DummyMediaStackPanelViewModel);
                    newTitle = BrowsePath;
                }
                else if (CurrentViewModel is ImageViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(ImageMediaStackPanelViewModel);
                    newTitle = ImageViewModel.CurrentLocation;
                    if (newTitle != null) newTitle = System.IO.Path.GetFileName(newTitle);
                }
                else if (CurrentViewModel is VideoViewModel)
                {
                    Shell.ShellViewModel.navigateToMediaStackPanelView(VideoMediaStackPanelViewModel);
                    newTitle = VideoViewModel.CurrentLocation;
                    if (newTitle != null) newTitle = System.IO.Path.GetFileName(newTitle);
                }
                
                CurrentViewModel.OnNavigatedTo(navigationContext);                
            }

            Title = newTitle;
        }
       
    }
}
