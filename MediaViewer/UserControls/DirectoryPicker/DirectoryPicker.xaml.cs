using ICSharpCode.TreeView;
using MediaViewer.Infrastructure.Logging;
using MediaViewer.Model.Collections.Sort;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.Model.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        protected 

        SharpTreeNode scrollToNodeOnTreeViewVisible;
        InfoGatherTask infoGatherTask;
        CancellationTokenSource cancelInfoGatherTaskTokenSource;

        public DirectoryPicker()
        {
            InitializeComponent();

            cancelInfoGatherTaskTokenSource = new CancellationTokenSource();

            infoGatherTask = new InfoGatherTask(cancelInfoGatherTaskTokenSource.Token);
            RootLocation root = new RootLocation(infoGatherTask, MediaFileWatcher.Instance.MediaFileState);
            treeView.Root = root;
            treeView.IsVisibleChanged += treeView_IsVisibleChanged;
            root.NodePropertyChanged += root_NodePropertyChanged;
           
            scrollToNodeOnTreeViewVisible = null;
            //SharpTreeView temp;
         
            
        }

        public void stopDirectoryPickerInfoGatherTask()
        {
            cancelInfoGatherTaskTokenSource.Cancel();
        }

        void root_NodePropertyChanged(object sender, Location e)
        {
            SelectedLocation = e.FullName;
            
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
            Location selectedNode = treeView.SelectedItem as Location;
            if (selectedNode == null)
            {
                return;
            }
                                                           
            try                       
            {
                String newFolder = FileUtils.getUniqueDirectoryName(selectedNode.FullName);

                selectedNode.IsExpanded = true;

                DirectoryInfo newFolderInfo = System.IO.Directory.CreateDirectory(newFolder);

                DirectoryLocation child = new DirectoryLocation(newFolderInfo, infoGatherTask, MediaFileWatcher.Instance.MediaFileState);                                         
                            
                CollectionsSort.insertIntoSortedCollection(selectedNode.Children, child);
                infoGatherTask.addLocation(child);
               
                //treeView.SelectedItem = child;
                //treeView.ScrollIntoView(child);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error creating directory", ex);
                MessageBox.Show("Error creating directory\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                        
        }

        private void refreshDirectory_Click(object sender, RoutedEventArgs e)
        {      
            Location selectedNode = treeView.SelectedItem as Location;
            if (selectedNode == null)
            {
                return;
            }

            Location parent = selectedNode.Parent as Location;
            Location newNode;
            string fullName = selectedNode.FullName;
            parent.Children.Remove(selectedNode);

            if (selectedNode is DriveLocation)
            {
                newNode = new DriveLocation(new DriveInfo(fullName), infoGatherTask, MediaFileWatcher.Instance.MediaFileState);                           
            }
            else
            {
                newNode = new DirectoryLocation(new DirectoryInfo(fullName), infoGatherTask, MediaFileWatcher.Instance.MediaFileState);
            }

            CollectionsSort.insertIntoSortedCollection(parent.Children, newNode);
            infoGatherTask.addLocation(newNode);
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

        public class DummyComparer : IComparer
        {
            public int Compare(object a, object b)
            {
                Location nodeA = (Location)a;
                Location nodeB = (Location)b;

                return(nodeA.NrImported.CompareTo(nodeB.NrImported));
              
            }
        }

        private void location_Click(object sender, RoutedEventArgs e)
        {
            /*GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
           
            var dataView =
                    (ListCollectionView)CollectionViewSource.GetDefaultView(treeView.ItemsSource);
            dataView.CustomSort = new DummyComparer();
            dataView.Refresh();*/
                                                          
        }
        
    }
}
