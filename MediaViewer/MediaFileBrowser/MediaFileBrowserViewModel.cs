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
using MediaViewer.Plugin;

namespace MediaViewer.MediaFileBrowser
{
    class MediaFileBrowserViewModel : ObservableObject
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        private MediaFileWatcher mediaFileWatcher;
        private delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);       

        ImageGridViewModel imageGridViewModel;

        public ImageGridViewModel ImageGridViewModel
        {
            get { return imageGridViewModel; }
            set { imageGridViewModel = value;
            NotifyPropertyChanged();     
            }
        }

        ImageViewModel imageViewModel;

        public ImageViewModel ImageViewModel
        {
            get { return imageViewModel; }
            set { imageViewModel = value;
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

        object currentViewModel;

        public object CurrentViewModel
        {
            get { return currentViewModel; }
            set { currentViewModel = value;
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

     
   
        public MediaFileBrowserViewModel() {

            ImageViewModel = new ImagePanel.ImageViewModel();
            ImageViewModel.SelectedScaleMode = ImagePanel.ImageViewModel.ScaleMode.RELATIVE;
            ImageViewModel.IsEffectsEnabled = false;

            VideoViewModel = new VideoPanel.VideoViewModel();

            ImageGridViewModel = new ImageGridViewModel(MediaFileWatcher.Instance.MediaState);     

            CurrentViewModel = ImageGridViewModel;

            mediaFileWatcher = MediaFileWatcher.Instance;
         
            DeleteSelectedItemsCommand = new Command(new Action(deleteSelectedItems));
      
            ImportSelectedItemsCommand = new Command(() =>
            {          
                ImportView import = new ImportView();
                import.ShowDialog();          
            });

            ExportSelectedItemsCommand = new Command(async () =>
            {
                List<MediaFileItem> selectedItems = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();
                if (selectedItems.Count == 0) return;

                CancellableOperationProgressView export = new CancellableOperationProgressView();
                ExportViewModel vm = new ExportViewModel();
                export.DataContext = vm;
                export.Show();            
                await vm.exportAsync(selectedItems);

            });

            ExpandCommand = new Command<MediaFileItem>((item) =>
            {
                String location;

                if (item == null)
                {
                    List<MediaFileItem> selectedItems = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();
                    if (selectedItems.Count == 0) return;

                    location = selectedItems[0].Location;
                }
                else
                {
                    location = item.Location;
                }

                if (Utils.MediaFormatConvert.isImageFile(location))
                {
                    CurrentViewModel = ImageViewModel;
                    ImageViewModel.LoadImageAsyncCommand.DoExecute(location);
                }
                else if (Utils.MediaFormatConvert.isVideoFile(location))
                {
                    CurrentViewModel = VideoViewModel;

                    //GlobalMessenger.Instance.NotifyColleagues("MediaFileBrowser_ShowVideo", location);
                    videoViewModel.OpenCommand.DoExecute(location);
                    videoViewModel.PlayCommand.DoExecute();
                }

                ContractCommand.CanExecute = true;
                ExpandCommand.CanExecute = false;
            });

            ContractCommand = new Command(() =>
                {                  
                    CurrentViewModel = ImageGridViewModel;
                  
                    ContractCommand.CanExecute = false;
                    ExpandCommand.CanExecute = true;
                });
            ContractCommand.CanExecute = false;

            CreateVideoPreviewImagesCommand = new Command(() =>
                {
                    List<MediaFileItem> media = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();
                    if (media.Count == 0) return;

                    VideoPreviewImageView preview = new VideoPreviewImageView();
                    
                    preview.ViewModel.Media = media;
                    preview.ShowDialog();
                });

            GeoTagCommand = new Command(() =>
            {
                PluginWindow pluginWindow = new PluginWindow();
                pluginWindow.Show();
            });

            CreateTorrentFileCommand = new Command(() =>
                {
                    List<MediaFileItem> media = MediaFileWatcher.Instance.MediaState.getSelectedItemsUIState();
                    if (media.Count == 0) return;

                    try
                    {
                        TorrentCreationView torrent = new TorrentCreationView();

                        torrent.ViewModel.Media = media;
                        torrent.ViewModel.PathRoot = MediaFileWatcher.Instance.Path;

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
     

        private void metaDataToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {
           
        }

        private void geoTagToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {
            
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

        Command geoTagCommand;

        public Command GeoTagCommand
        {
            get { return geoTagCommand; }
            set { geoTagCommand = value; }
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

        private void renameImageToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {


        }
        private void selectAllToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

           // imageGrid.setSelectedForAllImages(true);
        }
        private void deselectAllToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            //imageGrid.setSelectedForAllImages(false);
        }
        private void cutToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            
        }
        private void copyToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

           
        }


        private void moveButton_Click(System.Object sender, System.EventArgs e)
        {
            /*
                             List<MediaPreviewAsyncState > images = imageGrid.getSelectedImageData();	
                             if(images.Count == 0) return;

                             FolderBrowserDialog dialog = new FolderBrowserDialog();

                             dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                             dialog.SelectedPath = mediaFileWatcher.Path;
                             dialog.Description = L"Move " + Convert.ToString(images.Count) + L" image(s) to";

                             if(dialog.ShowDialog() == DialogResult.OK) {

                                 for(int i = 0; i < images.Count; i++) {

                                     string fileName = Path.GetFileName(images[i].ImageLocation);
                                     string destFileName = dialog.SelectedPath + "\\" + fileName;

                                     if(File.Exists(destFileName)) {

                                         DialogResult result = MessageBox.Show(destFileName + L"\n already exists, do you want to overwrite?", "Overwrite File?",
                                             MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                                         if(result == DialogResult.Yes) {

                                             File.Delete(destFileName);

                                         } else if(result == DialogResult.No) {

                                             continue;

                                         } else if(result == DialogResult.Cancel) {

                                             break;
                                         }

                                     }

                                     File.Move(images[i].ImageLocation, destFileName);

                                 }

                             }
             */
        }
    

        private void imageGrid_MouseEnter(System.Object sender, System.EventArgs e)
        {


        }
        private void directoryBrowser_MouseEnter(System.Object sender, System.EventArgs e)
        {


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
