using MediaViewer.Model.Global.Events;
using MediaViewer.ImagePanel;
using MediaViewer.MediaFileBrowser;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.VideoPanel;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Model.Media.State.CollectionView;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Settings;
using MediaViewer.Infrastructure;
using MediaViewer.MediaFileStackPanel;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.Base;

namespace MediaViewer
{
    public class ShellViewModel : BindableBase
    {
        public IRegionManager RegionManager { get; set; }
        public IEventAggregator EventAggregator { get; set; }

        ImageViewModel imageViewModel;

        public ImageViewModel ImageViewModel
        {
            get { return imageViewModel; }
            private set
            {
                SetProperty(ref imageViewModel, value);            
            }
        }

        MediaFileStackPanelViewModel imageMediaStackPanelViewModel;

        public MediaFileStackPanelViewModel ImageMediaStackPanelViewModel
        {
            get { return imageMediaStackPanelViewModel; }
            set
            {
                SetProperty(ref imageMediaStackPanelViewModel, value);             
            }
        }

        VideoViewModel videoViewModel;

        public VideoViewModel VideoViewModel
        {
            get { return videoViewModel; }
            private set
            {
                SetProperty(ref videoViewModel, value);              
            }
        }

        MediaFileStackPanelViewModel videoMediaStackPanelViewModel;

        public MediaFileStackPanelViewModel VideoMediaStackPanelViewModel
        {
            get { return videoMediaStackPanelViewModel; }
            set
            {
                SetProperty(ref videoMediaStackPanelViewModel, value);           
            }
        }

        MediaFileBrowserViewModel mediaFileBrowserViewModel;

        public MediaFileBrowserViewModel MediaFileBrowserViewModel
        {
            get { return mediaFileBrowserViewModel; }
            private set
            {
                SetProperty(ref mediaFileBrowserViewModel, value);            
            }
        }

    

        public ShellViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            RegionManager = regionManager;
            EventAggregator = eventAggregator;

            ImageViewModel = new ImagePanel.ImageViewModel(eventAggregator);
            ImageViewModel.SelectedScaleMode = UserControls.ImagePanel.ScaleMode.UNSCALED;
          
            imageMediaStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileWatcher.Instance.MediaFileState, EventAggregator);
            imageMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaFilterMode.Images);

            VideoViewModel = new VideoPanel.VideoViewModel(AppSettings.Instance, EventAggregator);

            videoMediaStackPanelViewModel = new MediaFileStackPanelViewModel(MediaFileWatcher.Instance.MediaFileState, EventAggregator);
            videoMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaFilterMode.Video);

            MediaFileBrowserViewModel = new MediaFileBrowserViewModel(mediaFileWatcher, regionManager, eventAggregator, AppSettings.Instance);
        
        }

        public void navigateToMediaStackPanelView(MediaFileStackPanelViewModel viewModel, String location = null)
        {
            Uri ImageViewUri = new Uri(typeof(MediaFileStackPanelView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", viewModel);          
            navigationParams.Add("location", location);
           
            RegionManager.RequestNavigate(RegionNames.MainMediaSelectionRegion, ImageViewUri, navigationParams);
        }


        public void navigateToImageView(MediaItem item = null)
        {
            String newLocation = item == null ? null : item.Location;

            if (RegionManager.Regions[RegionNames.MainContentRegion].ActiveViews.FirstOrDefault() is ImageView &&
                String.Compare(newLocation, ImageViewModel.Location) == 0)
            {
                //active view is already imageview
                return;
            }

            Uri ImageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", ImageViewModel);
            navigationParams.Add("item", item);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, ImageViewUri, navigationParams);

            Uri ImageToolbarViewUri = new Uri(typeof(ImageToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, ImageToolbarViewUri, navigationParams);

            navigateToMediaStackPanelView(imageMediaStackPanelViewModel, newLocation);

        }

        public void navigateToMediaFileBrowser()
        {
            if (RegionManager.Regions[RegionNames.MainContentRegion].ActiveViews.FirstOrDefault() is MediaFileBrowserView)
            {
                //active view is already mediafilebrowser
                return;
            }

            Uri MediaFileBrowserViewUri = new Uri(typeof(MediaFileBrowserView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", MediaFileBrowserViewModel);
           
            RegionManager.RequestNavigate(RegionNames.MainContentRegion, MediaFileBrowserViewUri, navigationParams);

            Uri MediaFileBrowserToolbarViewUri = new Uri(typeof(MediaFileBrowserToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, MediaFileBrowserToolbarViewUri, navigationParams);
           
        }

        public void navigateToVideoView(MediaItem item = null, int? offsetSeconds = null, MediaItem audio = null)
        {
            String currentLocation = VideoViewModel.CurrentItem.IsEmpty ? null : VideoViewModel.CurrentItem.Video.Location;
            String newLocation = item == null ? null : item.Location;

            if (RegionManager.Regions[RegionNames.MainContentRegion].ActiveViews.FirstOrDefault() is VideoView &&
                String.Compare(currentLocation, newLocation) == 0)
            {
                //active view is already videoview
                return;
            }

            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", VideoViewModel);
            navigationParams.Add("item", item);
            navigationParams.Add("offsetSeconds", offsetSeconds);
            navigationParams.Add("audio", audio);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, VideoViewUri, navigationParams);

            navigateToMediaStackPanelView(videoMediaStackPanelViewModel, newLocation);

            Uri VideoToolbarViewUri = new Uri(typeof(VideoToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, VideoToolbarViewUri);
        }
    }
}
