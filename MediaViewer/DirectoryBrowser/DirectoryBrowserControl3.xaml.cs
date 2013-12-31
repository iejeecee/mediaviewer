using Aga.Controls.Tree;
using MediaViewer.Input;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MediaViewer.DirectoryBrowser
{
    /// <summary>
    /// Interaction logic for DirectoryBrowserControl3.xaml
    /// </summary>
    public partial class DirectoryBrowserControl3 : UserControl
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        DirectoryBrowserViewModel directoryBrowserViewModel;
        bool bringSelectedNodeIntoViewOnVisibilityChange;

        public DirectoryBrowserControl3()
        {
            InitializeComponent();
            DataContext = directoryTreeList.Model = directoryBrowserViewModel = new DirectoryBrowserViewModel();
            directoryBrowserViewModel.SelectPathEvent += new EventHandler<string>((o, path) =>
            {
                selectPath(path);
            });

            bringSelectedNodeIntoViewOnVisibilityChange = false;

            directoryTreeList.IsVisibleChanged += new DependencyPropertyChangedEventHandler((o, e) =>
            {
                if ((bool)e.NewValue == true && bringSelectedNodeIntoViewOnVisibilityChange && directoryTreeList.SelectedItem != null)
                {
                    directoryTreeList.ScrollIntoView(directoryTreeList.SelectedItem);
                    ListViewItem item = directoryTreeList.ItemContainerGenerator.ContainerFromItem(directoryTreeList.SelectedItem) as ListViewItem;
                    if (item != null)
                    {
                        item.Focus();
                    }

                    bringSelectedNodeIntoViewOnVisibilityChange = false;
                }
            });

        }

        private void directoryTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectoryBrowserViewModel.PathSelectedDelegate func = directoryBrowserViewModel.PathSelectedCallback;

            if (func != null && directoryTreeList.SelectedNode != null)
            {
                func((directoryTreeList.SelectedNode as TreeNode).Tag as PathModel);
            }
        }

        public void selectPath(string path)
        {

            path = path.Replace('/', '\\');

            string root = System.IO.Path.GetPathRoot(path).ToUpper();
            TreeNode node = null;

            foreach (TreeNode treeNode in directoryTreeList.Nodes)
            {
                DrivePathModel drive = treeNode.Tag as DrivePathModel;

                if (drive.Name.Equals(root))
                {
                    node = treeNode;
                    break;
                }

            }

            if (node == null)
            {
                return;
            }

            string seperator = "\\";

            string[] splitDirs = path.Split(seperator.ToCharArray());

            for (int i = 1; i < splitDirs.Length; i++)
            {
                node.IsExpanded = true;

                foreach (TreeNode treeNode in node.Nodes)
                {
                    if ((treeNode.Tag as PathModel).Name.ToLower().Equals(splitDirs[i].ToLower()))
                    {
                        node = treeNode;
                        break;
                    }
                }
            }

            //ListView test = (ListView)directoryTreeList;
            directoryTreeList.SelectedItem = node;

            if (directoryTreeList.IsVisible == true)
            {
                bringSelectedNodeIntoView();
                bringSelectedNodeIntoViewOnVisibilityChange = false;
            }
            else
            {
                bringSelectedNodeIntoViewOnVisibilityChange = true;
            }

            node.IsSelected = true;

        }

        void bringSelectedNodeIntoView()
        {
            directoryTreeList.ScrollIntoView(directoryTreeList.SelectedItem);
            ListViewItem item = directoryTreeList.ItemContainerGenerator.ContainerFromItem(directoryTreeList.SelectedItem) as ListViewItem;
            item.Focus();
        }

        private void createDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (directoryTreeList.SelectedNode == null)
            {
                return;
            }

            PathModel parent = directoryTreeList.SelectedNode.Tag as PathModel;

            InputView input = new InputView();
            InputViewModel vm = (InputViewModel)input.DataContext;
            vm.Title = "Create new subdirectory in: " + parent.getFullPath();
            vm.InputHistory = MediaViewer.Settings.AppSettings.Instance.CreateDirectoryHistory;

            if (input.ShowDialog() == true)
            {
                if (String.IsNullOrEmpty(vm.InputText) || string.IsNullOrWhiteSpace(vm.InputText)) return;

                String newDirPath = parent.getFullPath() + "/" + vm.InputText;

                try
                {
                    DirectoryInfo newDirectory = System.IO.Directory.CreateDirectory(newDirPath);

                    DirectoryPathModel child = new DirectoryPathModel(parent, newDirectory);
                    if (parent.Directories == null)
                    {
                        parent.Directories = new System.Collections.ObjectModel.ObservableCollection<PathModel>();
                    }

                    insertIntoSortedCollection(parent.Directories, child);
                    directoryTreeList.SelectedNode.IsExpanded = true;

                    Utils.Misc.insertIntoHistoryCollection(MediaViewer.Settings.AppSettings.Instance.CreateDirectoryHistory, vm.InputText);
                }
                catch (Exception ex)
                {
                    log.Error("Error creating directory: " + newDirPath, ex);
                    MessageBox.Show("Error creating directory: " + newDirPath + "\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }

        }

        private void refreshDirectory_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeNode selectedNode in directoryTreeList.SelectedNodes)
            {

                TreeNode parentNode = selectedNode.Parent;
                PathModel parent = selectedNode.Parent.Tag as PathModel;
                PathModel child = selectedNode.Tag as PathModel;

                if (parent == null)
                {
                    directoryBrowserViewModel.Drives.Remove(child);

                    child = new DrivePathModel(new DriveInfo(child.Name));

                    insertIntoSortedCollection(directoryBrowserViewModel.Drives, child);
                }
                else
                {
                    String childPath = child.getFullPath();
                    parent.Directories.Remove(child);

                    DirectoryInfo info = new DirectoryInfo(childPath);

                    child = new DirectoryPathModel(parent, info);

                    insertIntoSortedCollection(parent.Directories, child);
                }

                foreach (TreeNode node in parentNode.Nodes)
                {
                    if ((node.Tag as PathModel).Name.Equals(child.Name))
                    {
                        node.IsExpanded = true;
                        node.IsSelected = true;
                    }
                }

                return;
            }

        }

        private void deleteDirectory_Click(object sender, RoutedEventArgs e)
        {

            if (directoryTreeList.SelectedNodes == null) return;

            String infoText = "";
            String directoryPath = "";

            foreach (TreeNode selectedNode in directoryTreeList.SelectedNodes)
            {
                if (selectedNode.Tag is DrivePathModel) return;

                PathModel directory = selectedNode.Tag as PathModel;
                directoryPath = directory.getFullPath();

                infoText += directoryPath + "\n";
            }

            if (MessageBox.Show("Delete:\n\n" + infoText + "\nand any subdirectories and files within the directory?",
                   "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
            {
                return;
            }
           
            foreach (TreeNode selectedNode in directoryTreeList.SelectedNodes)
            {

                PathModel parent = selectedNode.Parent.Tag as PathModel;
                PathModel directory = selectedNode.Tag as PathModel;
                directoryPath = directory.getFullPath();

                if(deleteDirectory(directory.getFullPath()) == false) return;              
                parent.Directories.Remove(directory);
            }

          
        }

        bool deleteDirectory(string path)
        {

            MediaFileWatcher.Instance.MediaFiles.EnterReaderLock();
            List<MediaFileItem> mediaFilesInUse = new List<MediaFileItem>();

            foreach (MediaFileItem item in MediaFileWatcher.Instance.MediaFiles.Items)
            {

                if (FileUtils.getPathWithoutFileName(item.Location).Equals(path))
                {
                    mediaFilesInUse.Add(item);
                }
            }

            MediaFileWatcher.Instance.MediaFiles.ExitReaderLock();
          
            try
            {
                bool result = MediaFileWatcher.Instance.MediaFilesInUseByOperation.AddRange(mediaFilesInUse);
                if (result == false)
                {
                    MessageBox.Show("Error", "Cannot delete file(s) in use by another operation");
                    return (false);
                }

                System.IO.Directory.Delete(path, true);
                return(true);
            }
            catch (Exception e)
            {
                log.Error("Error deleting directory: " + path, e);
                MessageBox.Show("Error deleting directory: " + path + "\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return (false);
            }
            finally
            {
                MediaFileWatcher.Instance.MediaFilesInUseByOperation.RemoveAll(mediaFilesInUse);
            }

        }

        void insertIntoSortedCollection(ObservableCollection<PathModel> collection, PathModel item)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Name.CompareTo(item.Name) > 0)
                {
                    collection.Insert(i, item);
                    return;
                }
            }

            collection.Add(item);
        }
    }
}
