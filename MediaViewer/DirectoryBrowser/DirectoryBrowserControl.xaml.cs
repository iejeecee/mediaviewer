using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//http://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaViewer.DirectoryBrowser
{
    /// <summary>
    /// Interaction logic for DirectoryBrowserControl.xaml
    /// </summary>
    public partial class DirectoryBrowserControl : UserControl
    {
        public event EventHandler<PathObject> SelectedPathChanged;

        private class AsyncState
        {
            Dispatcher uiDispatcher;

            public Dispatcher UiDispatcher
            {
                get { return uiDispatcher; }
                set { uiDispatcher = value; }
            }
            PathObject parent;

            public PathObject Parent
            {
                get { return parent; }
                set { parent = value; }
            }

            public AsyncState(Dispatcher uiDispatcher, PathObject parent)
            {
                UiDispatcher = uiDispatcher;
                Parent = parent;
            }

        }

        public DirectoryBrowserControl()
        {
            InitializeComponent();
    

            setPathAsync("C:\\game\\XMP-Toolkit-SDK-5.1.2\\samples\\testfiles");
    
        }

        private void directoryTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
       
            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            if (item.DataContext != null)
            {
                updateSubDirectoriesAsync(this.Dispatcher, (PathObject)item.DataContext);               

            }

          
          
        }

        private static Task updateSubDirectoriesAsync(Dispatcher uiDispatcher, PathObject parent)
        {
            Action<Object> action = new Action<Object>(updateSubDirectories);
            Task task = Task.Factory.StartNew(action, new AsyncState(uiDispatcher, parent));

            return (task);

        }

        private static void updateSubDirectories(Object args)
        {

            AsyncState state = (AsyncState)args;
            string fullPath = state.Parent.getFullPath();

            DirectoryInfo[] subDirsInfo = DirectoryLister.getSubDirectories(fullPath);
            List<DirectoryObject> subDirs = new List<DirectoryObject>();

            foreach (DirectoryInfo subDirInfo in subDirsInfo)
            {
                if ((subDirInfo.Attributes & FileAttributes.System) != 0)
                {
                    // skip system directories
                    continue;
                }

                DirectoryObject subDir = new DirectoryObject(state.Parent, subDirInfo);

                string subDirPath = fullPath + "\\" + subDir.Name;

                subDirsInfo = DirectoryLister.getSubDirectories(subDirPath);
                if (subDirsInfo.Length > 0)
                {
                    subDir.Directories.Add(new LoadingObject(subDir));
                }

                subDirs.Add(subDir);

            }

            Action updateTree = new Action(() =>
            {
                //state.Parent.Directories.Clear();

                for (int i = state.Parent.Directories.Count - 1; i >= 0; i--)
                {
                    if(!subDirs.Any(q => q.Name.Equals(state.Parent.Directories[i].Name))) {

                        state.Parent.Directories.RemoveAt(i);
                    }
                   
                }

                foreach (DirectoryObject subDir in subDirs)
                {

                    if (!state.Parent.Directories.Any(q => q.Name.Equals(subDir.Name)))
                    {
                        state.Parent.Directories.Add(subDir);
                    }
                }

               
            });

            state.UiDispatcher.Invoke(updateTree);
        }

        private void directoryTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {

            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            if (item.DataContext != null && SelectedPathChanged != null)
            {
               
               SelectedPathChanged(this, (PathObject)item.DataContext);
               
            }

            item.BringIntoView();
        }

        public Task setPathAsync(string path)
        {

            setPath(path);
            return (null);

     /*
            Action<Object> action = new Action<Object>(setPath);
            Task task = Task.Factory.StartNew(action, path);

            return (task);
     */
        }

        private void setPath(Object state) {

            string path = (string)state;
           
            path = path.Replace('/', '\\');

            string root = System.IO.Path.GetPathRoot(path);
            PathObject parent = null;          

            foreach (PathObject drive in directoryTreeView.Items)
            {
   
                if (drive.Name.Equals(root))
                {
                    parent = drive;
                    break;
                }

            }

            if (parent == null)
            {
                return;
            }

            string seperator = "\\";

            string[] splitDirs = path.Split(seperator.ToCharArray());

            for (int i = 1; i < splitDirs.Length; i++)
            {
                
                Action<Object> action = new Action<Object>(updateSubDirectories);
                Task task = new Task(action, new AsyncState(Dispatcher, parent));

                task.RunSynchronously();

                parent.IsExpanded = true;

                foreach (PathObject directory in parent.Directories)
                {
                    if (directory.Name.Equals(splitDirs[i]))
                    {
                        parent = directory;
                        //curItem.IsExpanded = true;
                        break;
                    }

                }

            }

            parent.IsSelected = true;
        }
       
    }
}
