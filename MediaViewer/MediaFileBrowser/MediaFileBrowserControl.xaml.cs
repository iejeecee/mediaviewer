using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
using MediaViewer.DirectoryBrowser;
using MediaViewer.ImageGrid;
using MediaViewer.Input;
using MediaViewer.MediaPreview;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using MediaViewer.Pager;

namespace MediaViewer.MediaFileBrowser
{
    /// <summary>
    /// Interaction logic for MediaFileBrowserControl.xaml
    /// </summary>
    public partial class MediaFileBrowserControl : UserControl
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler ViewImage;
        public event EventHandler BrowseDirectoryChanged;

        private MediaFileWatcher mediaFileWatcher;
        private delegate void imageFileWatcherEventDelegate(List<MediaPreviewAsyncState> imageData);
        private delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);

        public event EventHandler<FileSystemEventArgs> CurrentMediaChanged;

        PartialImageGridViewModel partialImageGridViewModel;
        DirectoryBrowserViewModel directoryBrowserViewModel;
        PagerViewModel pagerViewModel;

        public MediaFileBrowserControl()
        {
            InitializeComponent();
         
            mediaFileWatcher = new MediaFileWatcher();
            mediaFileWatcher.MediaDeleted += new FileSystemEventHandler(ImageFileWatcherThread_MediaDeleted);
            mediaFileWatcher.MediaChanged += new FileSystemEventHandler(ImageFileWatcherThread_MediaChanged);
            mediaFileWatcher.MediaCreated += new FileSystemEventHandler(ImageFileWatcherThread_MediaCreated);
            mediaFileWatcher.MediaRenamed += new RenamedEventHandler(ImageFileWatcherThread_MediaRenamed);
            mediaFileWatcher.CurrentMediaChanged += new EventHandler<FileSystemEventArgs>(imageFileWatcherThread_CurrentMediaChanged);
            
            directoryBrowserViewModel = new DirectoryBrowserViewModel();
            directoryBrowser.DataContext = directoryBrowserViewModel;

            // recieve message when a path node is selected          
            GlobalMessenger.Instance.Register<PathModel>("PathModel_IsSelected", new Action<PathModel>(browsePath_IsSelected));

            partialImageGridViewModel = new PartialImageGridViewModel();
            imageGrid.DataContext = partialImageGridViewModel;

            pagerViewModel = new PagerViewModel();
            pager.DataContext = pagerViewModel;
         
            pagerViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((s,e) => {

                if (e.PropertyName.Equals("CurrentPage"))
                {
                    partialImageGridViewModel.SetPageCommand.DoExecute(pagerViewModel.CurrentPage);
                }
            });
        }

        private MediaPreviewAsyncState fileInfoToMediaPreviewAsyncState(string location)
        {

            MediaPreviewAsyncState state = new MediaPreviewAsyncState(location);

            // item.ContextMenu = createContextMenu();
            state.InfoIconMode = MediaPreviewAsyncState.InfoIconModes.DEFAULT_ICONS_ONLY;

            return (state);
        }

        public void setBrowsePath(string path)
        {

            FileInfo fileInfo = new FileInfo(path);

            string pathWithoutFileName;

            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {

                pathWithoutFileName = path;

            }
            else
            {

                pathWithoutFileName = FileUtils.getPathWithoutFileName(path);
                mediaFileWatcher.CurrentMediaFile = path;
            }

            if (mediaFileWatcher.Path.Equals(pathWithoutFileName))
            {

                return;
            }

            mediaFileWatcher.Path = pathWithoutFileName;

            directoryBrowserViewModel.selectPath(pathWithoutFileName);

            List<ImageGridItem> items = new List<ImageGridItem>();

            foreach (String location in mediaFileWatcher.MediaFiles)
            {
                items.Add(new ImageGridItem(location));
            }

            partialImageGridViewModel.Items.Clear();
            partialImageGridViewModel.Items.AddRange(items);

            int totalPages = (int)Math.Ceiling(partialImageGridViewModel.Items.Count / (float)partialImageGridViewModel.MaxItems);
            if (totalPages == 0)
            {
                totalPages = 1;
            }
            pagerViewModel.TotalPages = totalPages;
            pagerViewModel.CurrentPage = 1;

            if (BrowseDirectoryChanged != null)
            {
                BrowseDirectoryChanged(this, EventArgs.Empty);
            }
        }
        /*
                public void displayMediaSearchResult(MediaSearchState state)
                {

                    List<MediaPreviewAsyncState> mediaData = new List<MediaPreviewAsyncState>();

                    foreach (FileInfo info in state.Matches)
                    {

                        MediaPreviewAsyncState item = fileInfoToImageGridItem(info.FullName);
                        mediaData.Add(item);
                    }

                    imageGrid.initializeImageData(mediaData);
                }
        */
        public string getBrowsePath()
        {

            return (mediaFileWatcher.Path);
        }
        public void setNextImageFile()
        {

            mediaFileWatcher.setNextMediaFile();
        }
        public void setPrevImageFile()
        {

            mediaFileWatcher.setPrevMediaFile();
        }

        private void imageFileWatcherThread_CurrentMediaChanged(Object sender, System.IO.FileSystemEventArgs e)
        {


            Dispatcher.BeginInvoke(new Action(() => CurrentMediaChanged(this, e)));
        }


        private void ImageFileWatcherThread_MediaDeleted(Object sender, System.IO.FileSystemEventArgs e)
        {

            List<MediaPreviewAsyncState> imageData = new List<MediaPreviewAsyncState>();

            imageData.Add(new MediaPreviewAsyncState(e.FullPath));

            //Dispatcher.BeginInvoke(new Action(() => imageGrid.removeImageData(imageData)));

        }


        private void ImageFileWatcherThread_MediaChanged(Object sender, System.IO.FileSystemEventArgs e)
        {

            List<MediaPreviewAsyncState> imageData = new List<MediaPreviewAsyncState>();

            imageData.Add(fileInfoToMediaPreviewAsyncState(e.FullPath));

            imageData.Add(new MediaPreviewAsyncState(e.FullPath));

            //Dispatcher.BeginInvoke(new Action(() => imageGrid.updateImageData(imageData)));

        }

        private void ImageFileWatcherThread_MediaCreated(Object sender, System.IO.FileSystemEventArgs e)
        {

            List<MediaPreviewAsyncState> imageData = new List<MediaPreviewAsyncState>();

            imageData.Add(fileInfoToMediaPreviewAsyncState(e.FullPath));

            imageData.Add(new MediaPreviewAsyncState(e.FullPath));

           // Dispatcher.BeginInvoke(new Action(() => imageGrid.addImageData(imageData)));

        }


        private void ImageFileWatcherThread_MediaRenamed(Object sender, System.IO.RenamedEventArgs e)
        {


            Dispatcher.BeginInvoke(new Action(() =>
            {
               // imageGrid.removeImageData(imageGrid.getImageData(e.OldFullPath));
                //imageGrid.addImageData(fileInfoToMediaPreviewAsyncState(e.FullPath));
            }));

        }

        private void viewToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            ViewImage(this, e);
        }

        private void metaDataToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {
            /*
                        List<MediaPreviewAsyncState> selected = imageGrid.getSelectedImageData();

                        if (selected.Count == 0)
                        {

                            selected.Add(e.Item);
                        }

                        List<string> fileNames = new List<string>();

                        foreach (MediaPreviewAsyncState item in selected)
                        {

                            fileNames.Add(item.ImageLocation);
                        }

                        foreach (Form form in Application.OpenForms)
                        {

                            if (form.GetType() == MediaInfoForm.typeid)
                            {

                                MediaInfoForm mediaInfo = MediaInfoForm(form);

                                if (Util.listSortAndCompare<string>(mediaInfo.FileNames, fileNames))
                                {

                                    mediaInfo.BringToFront();
                                    return;
                                }
                            }
                        }

                        try
                        {

                            MediaInfoForm mediaInfo = new MediaInfoForm();
                            mediaInfo.FileNames = fileNames;

                            mediaInfo.Show();

                        }
                        catch (Exception e)
                        {

                            log.Error("Error showing media info", e);
                            MessageBox.Show(e.Message, "Error");
                        }
            */
        }

        private void geoTagToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {
            /*
                        List<MediaPreviewAsyncState> selected = imageGrid.getSelectedImageData();

                        if (selected.Count == 0)
                        {

                            selected.Add(e.Item);
                        }

                        List<string> fileNames = new List<string>();

                        foreach (MediaPreviewAsyncState item in selected)
                        {

                            fileNames.Add(item.ImageLocation);
                        }

                        try
                        {

                            GeoTagForm geoTag = new GeoTagForm(fileNames);

                            geoTag.Show();

                        }
                        catch (Exception e)
                        {

                            log.Error("Error opening geotag window", e);
                            MessageBox.Show("Error opening geotag window: " + e.Message, "error");
                        }
            */
        }

        private void deleteImageToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            // e.Item.ContextMenu.Hide();

            List<MediaPreviewAsyncState> selected = null;// imageGrid.getSelectedImageData();

            if (selected.Count == 0)
            {

                //selected.Add(e.Item.AsyncState);
            }

            if (MessageBox.Show("Are you sure you want to permanently delete " + selected.Count.ToString() + " file(s)?",
                "Delete Images", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {

                try
                {

                    //imageGrid.removeImageData(selected);

                    for (int i = 0; i < selected.Count; i++)
                    {

                        System.IO.File.Delete(selected[i].MediaLocation);
                    }

                }
                catch (Exception ex)
                {

                    log.Error("Error deleting file", ex);
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void renameImageToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            InputWindow input = new InputWindow();

            List<MediaPreviewAsyncState> selected = null;// imageGrid.getSelectedImageData();

            string info;

            if (selected.Count == 0)
            {

                //selected.Add(e.Item.AsyncState);
                //info = "Rename Image: " + e.Item.AsyncState.MediaLocation;
                //input.InputText = System.IO.Path.GetFileNameWithoutExtension(e.Item.AsyncState.MediaLocation);

            }
            else
            {

                info = "Rename " + Convert.ToString(selected.Count) + " images";
            }

            //input.Title = info;

            if (input.ShowDialog() == true)
            {

                try
                {

                    for (int i = 0; i < selected.Count; i++)
                    {

                        string directory = System.IO.Path.GetDirectoryName(selected[i].MediaLocation);
                        string counter = selected.Count > 1 ? " (" + Convert.ToString(i + 1) + ")" : "";

                        string name = System.IO.Path.GetFileNameWithoutExtension(input.InputText);
                        string ext = System.IO.Path.GetExtension(selected[i].MediaLocation);

                        string newLocation = directory + "\\" + name + counter + ext;

                        File.Move(selected[i].MediaLocation, newLocation);
                    }

                }
                catch (Exception ex)
                {

                    log.Error("Error renaming file", ex);
                    MessageBox.Show(ex.Message);
                }

            }

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

            List<MediaPreviewAsyncState> selected = null;// imageGrid.getSelectedImageData();

            if (selected.Count == 0)
            {

                //selected.Add(e.Item.AsyncState);
            }

            StringCollection files = new StringCollection();

            foreach (MediaPreviewAsyncState item in selected)
            {

                files.Add(item.MediaLocation);
            }

            //DirectoryBrowserControl.clipboardAction = DirectoryBrowserControl.ClipboardAction.CUT;

            Clipboard.Clear();
            if (files.Count > 0)
            {

                Clipboard.SetFileDropList(files);
            }
        }
        private void copyToolStripMenuItem_MouseDown(System.Object sender, ImageGridMouseEventArgs e)
        {

            List<MediaPreviewAsyncState> selected = null;// imageGrid.getSelectedImageData();

            if (selected.Count == 0)
            {

                //selected.Add(e.Item.AsyncState);
            }

            StringCollection files = new StringCollection();

            foreach (MediaPreviewAsyncState item in selected)
            {

                files.Add(item.MediaLocation);
            }

            //DirectoryBrowserControl.clipboardAction = DirectoryBrowserControl.ClipboardAction.COPY;

            Clipboard.Clear();
            if (files.Count > 0)
            {

                Clipboard.SetFileDropList(files);
            }
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
        private void browsePath_IsSelected(PathModel path)
        {

            if (path.GetType() == typeof(DummyPathModel)) return;

            string fullPath = path.getFullPath();

            setBrowsePath(fullPath);
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

            setBrowsePath(e.FullPath);
            //}
        }
    }
}
