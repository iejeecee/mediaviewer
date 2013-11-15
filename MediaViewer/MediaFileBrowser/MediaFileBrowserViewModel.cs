using MediaViewer.DirectoryBrowser;
using MediaViewer.ImageGrid;
using MediaViewer.Input;
using MediaViewer.MediaFileModel;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.MediaPreview;
using MediaViewer.MoveRename;
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

namespace MediaViewer.MediaFileBrowser
{
    class MediaFileBrowserViewModel : ObservableObject
    {

        static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        private MediaFileWatcher mediaFileWatcher;
        private delegate void imageFileWatcherEventDelegate(List<MediaPreviewAsyncState> imageData);
        private delegate void imageFileWatcherRenamedEventDelegate(System.IO.RenamedEventArgs e);       

        PagedImageGridViewModel pagedImageGridViewModel;

        public PagedImageGridViewModel PagedImageGridViewModel
        {
            get { return pagedImageGridViewModel; }
            set { pagedImageGridViewModel = value;                         
            }
        }
      
        public MediaFileBrowserViewModel() {
         
            mediaFileWatcher = MediaFileWatcher.Instance;
         
            DeleteSelectedItemsCommand = new Command(new Action(deleteSelectedItems));        
  
            MoveRenameSelectedItemsCommand = new Command(new Action(() => {

                List<MediaFileItem> selected = mediaFileWatcher.MediaFiles.GetSelectedItems();
                if (selected.Count == 0) return;

                MoveRenameView moveRenameView = new MoveRenameView();
                MoveRenameViewModel moveRenameViewModel = (MoveRenameViewModel)moveRenameView.DataContext;

                moveRenameViewModel.SelectedItems = selected;
                moveRenameViewModel.MovePath = BrowsePath;
             
                moveRenameView.ShowDialog();

            }));

           
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
    
        Command deleteSelectedItemsCommand;

        public Command DeleteSelectedItemsCommand
        {
            get { return deleteSelectedItemsCommand; }
            set { deleteSelectedItemsCommand = value; }
        }

        private void deleteSelectedItems()
        {

            List<MediaFileItem> selected = MediaFileWatcher.Instance.MediaFiles.GetSelectedItems();

            if (selected.Count == 0) return;
           
            if (MessageBox.Show("Are you sure you want to permanently delete " + selected.Count.ToString() + " file(s)?",
                "Delete Media", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {

                try
                {     
                    for (int i = 0; i < selected.Count; i++)
                    {
                        System.IO.File.Delete(selected[i].Location);
                    }
                }
                catch (Exception ex)
                {

                    log.Error("Error deleting file", ex);
                    MessageBox.Show(ex.Message, "Error deleting file");
                }
            }

        }

        Command moveRenameSelectedItemsCommand;

        public Command MoveRenameSelectedItemsCommand
        {
            get { return moveRenameSelectedItemsCommand; }
            set { moveRenameSelectedItemsCommand = value; }
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
    }
}
