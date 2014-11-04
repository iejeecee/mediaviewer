using MediaViewer.Model.Global.Events;
using MediaViewer.MediaGrid;
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

        MediaStackPanelViewModel imageMediaStackPanelViewModel;

        public MediaStackPanelViewModel ImageMediaStackPanelViewModel
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

        MediaStackPanelViewModel videoMediaStackPanelViewModel;

        public MediaStackPanelViewModel VideoMediaStackPanelViewModel
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
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.NONE;
            ImageViewModel.IsEffectsEnabled = false;

            ImageMediaStackPanelViewModel = new MediaStackPanelViewModel(MediaFileWatcher.Instance.MediaState, EventAggregator);
            ImageMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Images);

            VideoViewModel = new VideoPanel.VideoViewModel(Settings.AppSettings.Instance, EventAggregator);

            VideoMediaStackPanelViewModel = new MediaStackPanelViewModel(MediaFileWatcher.Instance.MediaState, EventAggregator);
            VideoMediaStackPanelViewModel.MediaStateCollectionView.FilterModes.MoveCurrentTo(MediaStateFilterMode.Video);

            MediaFileBrowserViewModel = new MediaFileBrowserViewModel(mediaFileWatcher, regionManager, eventAggregator);
           
        }

        public void navigateToMediaStackPanelView(MediaStackPanelViewModel viewModel, String location = null)
        {
            Uri ImageViewUri = new Uri(typeof(MediaStackPanelView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", viewModel);          
            navigationParams.Add("location", location);
           
            RegionManager.RequestNavigate(RegionNames.MainMediaSelectionRegion, ImageViewUri, navigationParams);
        }


        public void navigateToImageView(String location = null)
        {
            if (RegionManager.Regions[RegionNames.MainContentRegion].ActiveViews.FirstOrDefault() is ImageView)
            {
                //active view is already imageview
                return;
            }

            Uri ImageViewUri = new Uri(typeof(ImageView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", ImageViewModel);
            navigationParams.Add("location", location);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, ImageViewUri, navigationParams);

            Uri ImageToolbarViewUri = new Uri(typeof(ImageToolbarView).FullName, UriKind.Relative);

            RegionManager.RequestNavigate(RegionNames.MainOptionalToolBarRegion, ImageToolbarViewUri, navigationParams);

            navigateToMediaStackPanelView(ImageMediaStackPanelViewModel, location);

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

        public void navigateToVideoView(String location = null)
        {
            if (RegionManager.Regions[RegionNames.MainContentRegion].ActiveViews.FirstOrDefault() is VideoView)
            {
                //active view is already videoview
                return;
            }

            Uri VideoViewUri = new Uri(typeof(VideoView).FullName, UriKind.Relative);

            NavigationParameters navigationParams = new NavigationParameters();

            navigationParams.Add("viewModel", VideoViewModel);
            navigationParams.Add("location", location);

            RegionManager.RequestNavigate(RegionNames.MainContentRegion, VideoViewUri, navigationParams);

            navigateToMediaStackPanelView(VideoMediaStackPanelViewModel, location);

        }
    }
}
