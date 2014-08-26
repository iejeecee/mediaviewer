using MediaViewer.ImageGrid;
using MediaViewer.Input;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.DirectoryPicker;
using MediaViewer.Pager;
using MediaViewer.Search;
using MediaViewer.Utils;
using MvvmFoundation.Wpf;
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

namespace MediaViewer.MediaFileBrowser
{
   
    public class MediaFileBrowserViewModel : ObservableObject
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
           
        private MediaFileWatcher mediaFileWatcher;
        private delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);
             
        object currentViewModel;

        public object CurrentViewModel
        {
            get { return currentViewModel; }
            set { currentViewModel = value;
            NotifyPropertyChanged();
            }
        }

        ImageViewModel imageViewModel;

        public ImageViewModel ImageViewModel
        {
            get { return imageViewModel; }
            private set { imageViewModel = value;
            NotifyPropertyChanged();
            }
        }

        VideoViewModel videoViewModel;

        public VideoViewModel VideoViewModel
        {
            get { return videoViewModel; }
            set { videoViewModel = value;
            NotifyPropertyChanged();
            }
        }

        ImageGridViewModel imageGridViewModel;

        public ImageGridViewModel ImageGridViewModel
        {
            get { return imageGridViewModel; }
            set { imageGridViewModel = value;
            NotifyPropertyChanged();
            }
        }

        private double maxImageScale;

        public double MaxImageScale
        {
            get { return maxImageScale; }
            set { maxImageScale = value;
            NotifyPropertyChanged();
            }
        }

        private double minImageScale;

        public double MinImageScale
        {
            get { return minImageScale; }
            set { minImageScale = value;
            NotifyPropertyChanged();
            }
        }

        IRegionManager regionManager;
          
        public MediaFileBrowserViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager) {

            this.mediaFileWatcher = mediaFileWatcher;
            this.regionManager = regionManager;

            ImageViewModel = new ImagePanel.ImageViewModel(mediaFileWatcher.MediaState);
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.RELATIVE;
            ImageViewModel.IsEffectsEnabled = false;

            VideoViewModel = new VideoPanel.VideoViewModel(mediaFileWatcher);
            ImageGridViewModel = new ImageGrid.ImageGridViewModel(mediaFileWatcher.MediaState, regionManager);

            DeleteSelectedItemsCommand = new Command(new Action(deleteSelectedItems));
      
            ImportSelectedItemsCommand = new Command(() =>
            {          
                ImportView import = new ImportView();
                import.ShowDialog();          
            });

            ExportSelectedItemsCommand = new Command(async () =>
            {
                List<MediaFileItem> selectedItems = mediaFileWatcher.MediaState.getSelectedItemsUIState();
                if (selectedItems.Count == 0) return;

                CancellableOperationProgressView export = new CancellableOperationProgressView();
                ExportViewModel vm = new ExportViewModel(mediaFileWatcher.MediaState);
                export.DataContext = vm;
                export.Show();            
                await vm.exportAsync(selectedItems);

            });

            ExpandCommand = new Command<MediaFileItem>((item) =>
            {
                String location;

                if (item == null)
                {
                    List<MediaFileItem> selectedItems = mediaFileWatcher.MediaState.getSelectedItemsUIState();
                    if (selectedItems.Count == 0) return;

                    location = selectedItems[0].Location;
                }
                else
                {
                    location = item.Location;
                }

                if (Utils.MediaFormatConvert.isImageFile(location))
                {

                    navigateToImageView(location);
              
                }
                else if (Utils.MediaFormatConvert.isVideoFile(location))
                {
                    navigateToVideoView(location);
                }

                
            });

            ContractCommand = new Command(() =>
                {
                    navigateToImageGrid();

                });

            ContractCommand.CanExecute = false;

            CreateVideoPreviewImagesCommand = new Command(() =>
                {
                    List<MediaFileItem> media = mediaFileWatcher.MediaState.getSelectedItemsUIState();
                    if (media.Count == 0) return;

                    VideoPreviewImageView preview = new VideoPreviewImageView();
                    
                    preview.ViewModel.Media = media;
                    preview.ShowDialog();
                });

      

            CreateTorrentFileCommand = new Command(() =>
                {
                    List<MediaFileItem> media = mediaFileWatcher.MediaState.getSelectedItemsUIState();
                    if (media.Count == 0) return;

                    try
                    {
                        TorrentCreationView torrent = new TorrentCreationView();

                        torrent.ViewModel.Media = media;
                        torrent.ViewModel.PathRoot = mediaFileWatcher.Path;

                        torrent.ShowDialog();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
  
            GlobalMessenger.Instance.Register<String>("MediaFileBrowser_SetPath", (location) =>
            {
                BrowsePath = location;
            });


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
                GlobalMessenger.Instance.NotifyColleagues("MediaFileBrowser_PathSelected", value);

                NotifyPropertyChanged();
            }

            get
            {                
                return (mediaFileWatcher.Path);
            }
        }
     

      

        Command createVideoPreviewImagesCommand;

        public Command CreateVideoPreviewImagesCommand
        {
            get { return createVideoPreviewImagesCommand; }
            set { createVideoPreviewImagesCommand = value; }
        }

        Command createTorrentFileCommand;

        public Command CreateTorrentFileCommand
        {
            get { return createTorrentFileCommand; }
            set { createTorrentFileCommand = value; }
        }

        Command deleteSelectedItemsCommand;

        public Command DeleteSelectedItemsCommand
        {
            get { return deleteSelectedItemsCommand; }
            set { deleteSelectedItemsCommand = value; }
        }

        Command importSelectedItemsCommand;

        public Command ImportSelectedItemsCommand
        {
            get { return importSelectedItemsCommand; }
            set { importSelectedItemsCommand = value; }
        }

        Command exportSelectedItemsCommand;

        public Command ExportSelectedItemsCommand
        {
            get { return exportSelectedItemsCommand; }
            set { exportSelectedItemsCommand = value; }
        }

        Command<MediaFileItem> expandCommand;

        public Command<MediaFileItem> ExpandCommand
        {
            get { return expandCommand; }
            set { expandCommand = value; }
        }

        Command contractCommand;

        public Command ContractCommand
        {
            get { return contractCommand; }
            set { contractCommand = value; }
        }
      
        private void deleteSelectedItems()
        {

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            List<MediaFileItem> selected = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();

            if (selected.Count == 0) return;
           
            if (MessageBox.Show("Are you sure you want to permanently delete " + selected.Count.ToString() + " file(s)?",
                "Delete Media", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
              
                try
                {
                    MediaFileWatcher.Instance.MediaState.delete(selected, tokenSource.Token);
                    
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

            regionManager.RequestNavigate(RegionNames.MediaFileBrowserRightTabRegion, MetaDataViewUri, (result) =>
            {
           
            });

        }

        public void navigateToImageGrid()
        {
            Uri ImageGridViewUri = new Uri(typeof(ImageGridView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();        
            navigationParams.Add("viewModel", ImageGridViewModel);

            regionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, ImageGridViewUri, (result) =>
            {
                CurrentViewModel = ImageGridViewModel;

                ContractCommand.CanExecute = false;
                ExpandCommand.CanExecute = true;
            }, navigationParams);

               
        }

        public void navigateToVideoView(string location)
        {
            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("location", location);          
            navigationParams.Add("viewModel", VideoViewModel);

            regionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, VideoViewUri, (result) =>
            {
                CurrentViewModel = VideoViewModel;

                ContractCommand.CanExecute = true;
                ExpandCommand.CanExecute = false;

            }, navigationParams);

        }

        public void navigateToImageView(string location)
        {
            Uri ImageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
            navigationParams.Add("location", location);
            navigationParams.Add("viewModel", ImageViewModel);
           
            regionManager.RequestNavigate(RegionNames.MediaFileBrowserContentRegion, ImageViewUri, (result) =>
            {
                CurrentViewModel = ImageViewModel;

                ContractCommand.CanExecute = true;
                ExpandCommand.CanExecute = false;

            }, navigationParams);

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

                GlobalMessenger.Instance.NotifyColleagues("MainWindow_SetTitle", value);
            }
        }
    }
}
