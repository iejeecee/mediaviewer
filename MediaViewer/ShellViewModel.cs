using MediaViewer.Model.GlobalEvents;
using MediaViewer.ImageGrid;
using MediaViewer.ImagePanel;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.VideoPanel;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.State.CollectionView;

namespace MediaViewer
{
    public class ShellViewModel : ObservableObject
    {
        public IRegionManager RegionManager { get; set; }
        public IEventAggregator EventAggregator { get; set; }

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
            private set { videoViewModel = value;
            NotifyPropertyChanged();
            }
        }

        MediaFileBrowserViewModel mediaFileBrowserViewModel;

        public MediaFileBrowserViewModel MediaFileBrowserViewModel
        {
            get { return mediaFileBrowserViewModel; }
            private set
            {
                mediaFileBrowserViewModel = value;
                NotifyPropertyChanged();
            }
        }

        public ShellViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            ImageViewModel = new ImagePanel.ImageViewModel(mediaFileWatcher.MediaState);
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.NONE;
            ImageViewModel.IsEffectsEnabled = false;

            VideoViewModel = new VideoPanel.VideoViewModel(mediaFileWatcher);
            MediaFileBrowserViewModel = new MediaFileBrowserViewModel(mediaFileWatcher, regionManager, eventAggregator);

        }

        public void navigateToMediaStackPanelView()
        {
            Uri ImageViewUri = new Uri(typeof(ImageStackPanelView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", MediaFileBrowserViewModel.ImageGridViewModel);

            RegionManager.RequestNavigate(RegionNames.MainMediaSelectionRegion, ImageViewUri, navigationParams);
        }


        public void navigateToImageView(String location = null)
        {
            Uri ImageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();
         
            navigationParams.Add("viewModel", ImageViewModel);                          
            navigationParams.Add("location", location);
           
            RegionManager.RequestNavigate(RegionNames.MainContentRegion, ImageViewUri, navigationParams);

            Uri ImageToolbarViewUri = new Uri(typeof(ImageToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, ImageToolbarViewUri, navigationParams);

            MediaBrowserDisplayOptions options = new MediaBrowserDisplayOptions();
          
            options.FilterMode = MediaStateFilterMode.Images;
            options.IsHidden = true;
        
            EventAggregator.GetEvent<MediaBrowserDisplayEvent>().Publish(options);
        }

        public void navigateToMediaFileBrowser()
        {
            Uri MediaFileBrowserViewUri = new Uri(typeof(MediaFileBrowserView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", MediaFileBrowserViewModel);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, MediaFileBrowserViewUri, navigationParams);

            Uri MediaFileBrowserToolbarViewUri = new Uri(typeof(MediaFileBrowserToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, MediaFileBrowserToolbarViewUri, navigationParams); 
        }

        public void navigateToVideoView(String location = null)
        {
            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", VideoViewModel);
            navigationParams.Add("location", location);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, VideoViewUri, navigationParams);

            MediaBrowserDisplayOptions options = new MediaBrowserDisplayOptions();

            options.FilterMode = MediaStateFilterMode.Video;
            options.IsHidden = true;

            EventAggregator.GetEvent<MediaBrowserDisplayEvent>().Publish(options);
            
        }
    }
}
