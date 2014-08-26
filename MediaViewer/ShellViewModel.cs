using MediaViewer.ImagePanel;
using MediaViewer.MediaFileBrowser;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.VideoPanel;
using Microsoft.Practices.Prism.Regions;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class ShellViewModel : ObservableObject
    {
        IRegionManager RegionManager { get; set; }

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

        public ShellViewModel(MediaFileWatcher mediaFileWatcher, IRegionManager regionManager)
        {
            RegionManager = regionManager;

            ImageViewModel = new ImagePanel.ImageViewModel(mediaFileWatcher.MediaState);
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.NONE;
            ImageViewModel.IsEffectsEnabled = false;

            VideoViewModel = new VideoPanel.VideoViewModel(mediaFileWatcher);
            MediaFileBrowserViewModel = new MediaFileBrowserViewModel(mediaFileWatcher, regionManager);

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
            
        }
    }
}
