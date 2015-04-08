using ICSharpCode.TreeView;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Media.State;
using MediaViewer.Model.Utils;
using MediaViewer.Progress;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MediaViewer.UserControls.DirectoryPicker
{
    class DirectoryLocation : Location, INonCancellableOperationProgress
    {

        public DirectoryLocation(DirectoryInfo info, InfoGatherTask infoGatherTask, MediaFileState mediaFileState)
            : base(infoGatherTask, mediaFileState)
        {        
            Name = info.Name;
            CreationDate = info.CreationTime;
                     
            VolumeLabel = "";

            //ImageUrl = "pack://application:,,,/Resources/Icons/mediafolder.ico";
            NrImported = 0;                    
                                           
            LazyLoading = true;
           
            PropertyChanged += propertyChanged;
        }

        private void propertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Text"))
            {
                SharpTreeNode parent = Parent;

                if (Parent != null && Parent.Children.Count > 1)
                {
                    parent.Children.Remove(this);
                    CollectionsSort.insertIntoSortedCollection(parent.Children, this);
                }

            }
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
                    Logger.Log.Error("Cannot read directories", e);
                    return (false);
                }
            }
        }

        public override int NrImported
        {
            get { return (base.NrImported); }
            set
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
            if (Name.Equals(newName)) return(false);

            List<MediaFileItem> mediaFilesToMove = new List<MediaFileItem>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
                   
            TotalProgress = 0;
            TotalProgressMax = 1;
            WindowTitle = "Renaming directory " + Name + " to " + newName;
            WindowIcon = "pack://application:,,,/Resources/Icons/folder_open.ico";
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
              
                    Name = newName;                    
                    return (true); 

                }
                catch (Exception e)
                {
                    Logger.Log.Error("Error renaming directory: " + FullName, e);
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

            if (task.Result == true)
            {
                nodePropertyChanged(this);
            }
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

                    MediaFileState.delete(mediaFilesToDelete, tokenSource.Token);

                    FileUtils fileUtils = new FileUtils();
                    fileUtils.deleteDirectory(location.FullName);
                   
                    location.Parent.Children.Remove(location);
                }
                catch (Exception e)
                {
                    Logger.Log.Error("Error deleting directory: " + location.FullName, e);
                    MessageBox.Show("Error deleting directory: " + location.FullName + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
               
            }
        }

        private bool getFiles(FileInfo info, object state)
        {
            List<MediaFileItem> items = (List<MediaFileItem>)state;

            if (MediaFormatConvert.isMediaFile(info.FullName))
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

        string windowIcon;

        public string WindowIcon
        {
            get
            {
                return (windowIcon);
            }
            set
            {
                windowIcon = value;
                RaisePropertyChanged("WindowIcon");
            }
        }
    }
}
