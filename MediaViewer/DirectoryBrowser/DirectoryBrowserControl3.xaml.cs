using Aga.Controls.Tree;
using System;
using System.Collections.Generic;
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
        DirectoryBrowserViewModel2 directoryBrowserViewModel;

        public DirectoryBrowserControl3()
        {
            InitializeComponent();
            DataContext = directoryTreeList.Model = directoryBrowserViewModel = new DirectoryBrowserViewModel2();
            directoryBrowserViewModel.SelectPathEvent += new EventHandler<string>((o, path) =>
            {
                selectPath(path);
            });
         
        }

        private void directoryTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectoryBrowserViewModel2.PathSelectedDelegate func = directoryBrowserViewModel.PathSelectedCallback;

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
            directoryTreeList.ScrollIntoView(directoryTreeList.SelectedItem);
            ListViewItem item = directoryTreeList.ItemContainerGenerator.ContainerFromItem(directoryTreeList.SelectedItem) as ListViewItem;
            item.Focus();
           
            node.IsSelected = true;
           
        }

    }
}
