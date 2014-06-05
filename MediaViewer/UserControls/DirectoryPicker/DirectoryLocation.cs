using ICSharpCode.TreeView;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Progress;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class DirectoryLocation : Location, INonCancellableOperationProgress
    {
        public DirectoryLocation(DirectoryInfo info)
        {
            
            Name = info.Name;
           
            FullName = info.FullName;
            VolumeLabel = "";

            using (MediaDbCommands mediaCommand = new MediaDbCommands())
            {
                NrImported = mediaCommand.getNrMediaInLocation(FullName);
            }
                     
            LazyLoading = true;                     
        }

        public override bool ShowExpander
        {
            get
            {               
                try
                {
                    IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(FullName).EnumerateDirectories();

                    if (dirInfos.Count() > 0)
                    {
                        return (true);
                    }
                    else
                    {
                        return (false);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Cannot read directories", e);
                    return (false);
                }
            }
        }

        public override int NrImported
        {           
            protected set
            {              
                if (value > 0)
                {
                    ImageUrl = "pack://application:,,,/Resources/Icons/mediafolder.ico";
                }
                else
                {
                    ImageUrl = "pack://application:,,,/Resources/Icons/Folder_Open.ico";
                }

                base.NrImported = value;
            }
        }

        public override bool IsEditable
        {
            get
            {            
                return true;
            }
        }
        public override bool SaveEditText(string newName)
        {           
            List<MediaFileItem> mediaFilesToMove = new List<MediaFileItem>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
                   
            TotalProgress = 0;
            TotalProgressMax = 1;
            WindowTitle = "Renaming directory " + Name + " to " + newName;
            NonCancellableOperationProgressView progress = new NonCancellableOperationProgressView();
            progress.DataContext = this;

            Task<bool> task = Task<bool>.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progress.ShowDialog();
                }));

                try
                {
                    if (String.IsNullOrEmpty(newName) || String.IsNullOrWhiteSpace(newName))
                    {
                        throw new ArgumentException("Illegal directory name");
                    }

                    String newFullName = FullName.Remove(FullName.LastIndexOf('\\')) + "\\" + newName;

                    FileUtils.walkDirectoryTree(new DirectoryInfo(FullName), getFiles, mediaFilesToMove, true);

                    TotalProgressMax = mediaFilesToMove.Count;
                    
                    System.IO.Directory.Move(FullName, newFullName);

                    foreach (MediaFileItem mediaFile in mediaFilesToMove)
                    {
                        String newMediaLocation = mediaFile.Location.Replace(FullName, newFullName);

                        mediaFile.Location = newMediaLocation;

                        TotalProgress++;
                    }

                    FullName = newFullName;
                    Name = newName;
                    return (true); 

                }
                catch (Exception e)
                {
                    log.Error("Error renaming directory: " + FullName, e);
                    MessageBox.Show("Error renaming directory: " + FullName + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return (false); 
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            });

            WaitWithPumping(task);
                                               
            return (task.Result);
           
        }

        public static void WaitWithPumping(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            var nestedFrame = new DispatcherFrame();
            task.ContinueWith(_ => nestedFrame.Continue = false);
            Dispatcher.PushFrame(nestedFrame);
            task.Wait();
        }

        public override string LoadEditText()
        {
            return Name;
        }

        public override bool CanDelete(SharpTreeNode[] nodes)
        {
            return true;
        }

        public override void Delete(SharpTreeNode[] nodes)
        {
            String infoText = "";

            foreach (Location selectedNode in nodes)
            {              
                infoText += selectedNode.FullName + "\n";               
            }
       
            if (MessageBox.Show("Delete:\n\n" + infoText + "\nand any subdirectories and files within the directory?",
               "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                DeleteWithoutConfirmation(nodes);
            }
        }

        public override void DeleteWithoutConfirmation(SharpTreeNode[] nodes)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            List<MediaFileItem> mediaFilesToDelete = new List<MediaFileItem>();

            foreach (Location location in nodes)
            {                
                try
                {
                    FileUtils.walkDirectoryTree(new DirectoryInfo(location.FullName), getFiles, mediaFilesToDelete, true);

                    MediaFileWatcher.Instance.MediaState.delete(mediaFilesToDelete, tokenSource.Token);

                    FileUtils fileUtils = new FileUtils();
                    fileUtils.deleteDirectory(location.FullName);
                   
                    location.Parent.Children.Remove(location);
                }
                catch (Exception e)
                {
                    log.Error("Error deleting directory: " + location.FullName, e);
                    MessageBox.Show("Error deleting directory: " + location.FullName + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
               
            }
        }

        private bool getFiles(FileInfo info, object state)
        {
            List<MediaFileItem> items = (List<MediaFileItem>)state;

            if (Utils.MediaFormatConvert.isMediaFile(info.FullName))
            {
                items.Add(MediaFileItem.Factory.create(info.FullName));
            }

            return (true);
        }

        /*
        ContextMenu menu;

        public override ContextMenu GetContextMenu()
        {
            if (menu == null)
            {
                menu = new ContextMenu();
                menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Cut });
                menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Copy });
                menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Paste });
                menu.Items.Add(new Separator());
                menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Delete });
            }
            return menu;
        }
         */


        int totalProgress;

        public int TotalProgress
        {
            get
            {
                return (totalProgress);
            }
            set
            {
                totalProgress = value;
                RaisePropertyChanged("TotalProgress");
                
            }
        }

        int totalProgressMax;

        public int TotalProgressMax
        {
            get
            {
                return (totalProgressMax);
            }
            set
            {
                totalProgressMax = value;
                RaisePropertyChanged("TotalProgressMax");
            }
        }

        string windowTitle;

        public string WindowTitle
        {
            get
            {
                return (windowTitle);
            }
            set
            {
                windowTitle = value;
                RaisePropertyChanged("WindowTitle");
            }
        }
    }
}
