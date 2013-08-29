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
        public event EventHandler<PathModel> SelectedPathChanged;
       
        public DirectoryBrowserControl()
        {
            InitializeComponent();

            directoryTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>((o, t) =>
            {
                TreeViewItem item = ContainerFromItem(directoryTreeView, t.NewValue);
                if (item != null)
                {
                    item.BringIntoView();
                }                
            });

           // setPathAsync("C:\\game\\XMP-Toolkit-SDK-5.1.2\\samples\\testfiles\\geotag");
            
     
        }

        public static TreeViewItem ContainerFromItem(TreeView treeView, object item)
        {

            TreeViewItem containerThatMightContainItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);

            if (containerThatMightContainItem != null)

                return containerThatMightContainItem;

            else

                return ContainerFromItem(treeView.ItemContainerGenerator, treeView.Items, item);

        }



        private static TreeViewItem ContainerFromItem(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, object item)
        {

            foreach (object curChildItem in itemCollection)
            {

                TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);

                if (parentContainer == null) continue;

                TreeViewItem containerThatMightContainItem = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(item);

                if (containerThatMightContainItem != null)

                    return containerThatMightContainItem;

                TreeViewItem recursionResult = ContainerFromItem(parentContainer.ItemContainerGenerator, parentContainer.Items, item);

                if (recursionResult != null)

                    return recursionResult;

            }

            return null;

        }



        public static object ItemFromContainer(TreeView treeView, TreeViewItem container)
        {

            TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)treeView.ItemContainerGenerator.ItemFromContainer(container);

            if (itemThatMightBelongToContainer != null)

                return itemThatMightBelongToContainer;

            else

                return ItemFromContainer(treeView.ItemContainerGenerator, treeView.Items, container);

        }



        private static object ItemFromContainer(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, TreeViewItem container)
        {

            foreach (object curChildItem in itemCollection)
            {

                TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);

                TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)parentContainer.ItemContainerGenerator.ItemFromContainer(container);

                if (itemThatMightBelongToContainer != null)

                    return itemThatMightBelongToContainer;

                TreeViewItem recursionResult = ItemFromContainer(parentContainer.ItemContainerGenerator, parentContainer.Items, container) as TreeViewItem;

                if (recursionResult != null)

                    return recursionResult;

            }

            return null;

        }



        /*
             
        private void directoryTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {

            TreeViewItem item = (TreeViewItem)e.OriginalSource;

            if (item.DataContext != null && SelectedPathChanged != null)
            {
               
               SelectedPathChanged(this, (PathModel)item.DataContext);
               
            }

            item.BringIntoView();
        }

       
       */
    }
}
