using ICSharpCode.TreeView;
using MediaViewer.MediaFileModel.Watcher;
using MediaViewer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace MediaViewer.UserControls.DirectoryPicker
{
    /// <summary>
    /// Interaction logic for DirectoryPicker.xaml
    /// </summary>
    public partial class DirectoryPicker : UserControl
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        SharpTreeNode scrollToNodeOnTreeViewVisible;

        public DirectoryPicker()
        {
            InitializeComponent();         
            treeView.Root = new RootLocation();
            treeView.IsVisibleChanged += treeView_IsVisibleChanged;

            scrollToNodeOnTreeViewVisible = null;
            //SharpTreeView temp;
        }

        private void treeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && scrollToNodeOnTreeViewVisible != null)
            {                         
                treeView.FocusNode(scrollToNodeOnTreeViewVisible);
                treeView.ScrollIntoView(scrollToNodeOnTreeViewVisible);
                scrollToNodeOnTreeViewVisible = null;
            }

        }

        private void treeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (treeView.SelectedItems.Count == 0)
            {
                return;
            }

            Location location = treeView.SelectedItems[0] as Location;

            SelectedLocation = location.FullName;
        }

        public String SelectedLocation
        {
            get { return (String)GetValue(SelectedLocationProperty); }
            set { SetValue(SelectedLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLocationProperty =
            DependencyProperty.Register("SelectedLocation", typeof(String), typeof(DirectoryPicker), new PropertyMetadata(null,selectedLocationChangedCallback));

        private static async void selectedLocationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DirectoryPicker dp = (DirectoryPicker)d;

            String newLocation = ((String)e.NewValue).TrimEnd(new char[]{'\\'});

            if (newLocation == null) return;

            String[] splitLocation = newLocation.Split('\\');

            Location node = dp.treeView.Root as Location;

            foreach (String name in splitLocation)
            {
                node.IsExpanded = true;
                await node.LoadingChildrenTask;
                node = node.Children.FirstOrDefault((n) => (n as Location).Name.Equals(name)) as Location;
                if (node == null) break;                
            }

            if (node != null)
            {
                dp.treeView.SelectedItem = node;    

                if (dp.treeView.IsVisible == false)
                {
                    dp.scrollToNodeOnTreeViewVisible = node;
                }
                else
                {        
                    dp.treeView.FocusNode(node);
                    dp.treeView.ScrollIntoView(node);
                }
            }
        }

        private void createDirectory_Click(object sender, RoutedEventArgs e)
        {
            /*if (directoryTreeList.SelectedNode == null)
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
            */
        }

        private void refreshDirectory_Click(object sender, RoutedEventArgs e)
        {      
            Location selectedNode = treeView.SelectedItem as Location;

            Location parent = selectedNode.Parent as Location;
            Location newNode;
            parent.Children.Remove(selectedNode);

            if (selectedNode is DriveLocation)
            {                
                newNode = new DriveLocation(new DriveInfo(selectedNode.FullName));                           
            }
            else
            {
                newNode = new DirectoryLocation(new DirectoryInfo(selectedNode.FullName));
            }

            Utils.Misc.insertIntoSortedCollection(parent.Children, newNode);
            newNode.IsExpanded = selectedNode.IsExpanded;
            treeView.SelectedItem = newNode;
                  
        }

        private void renameDirectory_Click(object sender, RoutedEventArgs e)
        {
            SharpTreeNode node = treeView.SelectedItem as SharpTreeNode;
            if (node != null)
            {
                node.IsEditing = true;
            }

        }

        
    }
}
