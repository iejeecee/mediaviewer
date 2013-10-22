using Aga.Controls.Tree;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.MetaData.Tree;
using System.Threading;


namespace MediaViewer.MetaData
{
    class MetaDataTreeModel : ObservableObject, ITreeModel
    {
        MetaDataTreeNode root;

        public MetaDataTreeModel(List<MetaDataTreeNode> metaData)
        {
            if (metaData.Count == 0)
            {
                root = null;
                return;
            }

            root = metaData[0];

            for (int i = 1; i < metaData.Count; i++)
            {
                if (root == null) break;
                root = root.intersection(metaData[i]);
            }
        }

        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {
                if (root == null) return new List<MetaDataTreeNode>();
                else return (nodeChildrenToPathValue(root));
            }
            else
            {
                MetaDataNameValue item = (MetaDataNameValue)parent;
                return (nodeChildrenToPathValue(item.Node));
            }
        }

        public bool HasChildren(object parent)
        {
            MetaDataNameValue item = (MetaDataNameValue)parent;
            if (item.Node.Count == 0 || item.Node.NodeType == MetaDataTreeNode.Type.PROPERTY) return (false);
            else return (true);
        }


        List<MetaDataNameValue> nodeChildrenToPathValue(MetaDataTreeNode node)
        {
                   
            List<MetaDataNameValue> items = new List<MetaDataNameValue>();  

            foreach(MetaDataTreeNode n in node.Child) {

                MetaDataNameValue item = new MetaDataNameValue();
                item.Name = n.ToString();
                item.Node = n;
                item.IconPath = "pack://application:,,,/Resources/Icons/prop.ico";

                if (n.NodeType == MetaDataTreeNode.Type.NAMESPACE)
                {
                    item.IconPath = "pack://application:,,,/Resources/Icons/namespace.ico";
                    item.NodeType = "Namespace";
                }
                else if (n.NodeType == MetaDataTreeNode.Type.ARRAY)
                {
                    item.IconPath = "pack://application:,,,/Resources/Icons/array.ico";
                    item.NodeType = "Array";
                }
                else if (n.NodeType == MetaDataTreeNode.Type.PROPERTY)
                {
                    MetaDataTreeProperty prop = (MetaDataTreeProperty)n;

                    if (String.IsNullOrEmpty(prop.Value)) continue;

                    item.Name = FormatMetaData.formatPropertyName(prop.ToString());
                    item.Value = FormatMetaData.formatPropertyValue(prop.Path, prop.Value);
                    item.NodeType = "Property";

                }
                else if (n.NodeType == MetaDataTreeNode.Type.VALUE)
                {

                    if (n.Parent.NodeType == MetaDataTreeNode.Type.PROPERTY) continue;

                    item.Name = "";
                    item.Value = FormatMetaData.formatPropertyValue(n.Path, n.Data);
                    item.IconPath = "pack://application:,,,/Resources/Icons/constprop.ico";
                    item.NodeType = "Value";

                }
                else if (n.NodeType == MetaDataTreeNode.Type.LANGUAGE)
                {
                    item.Name = "Language";
                    item.Value = n.ToString();
                    item.NodeType = "Language";
                    item.IconPath = "pack://application:,,,/Resources/Icons/language.ico";
                }

                if(n.Parent != null && n.Parent.NodeType == MetaDataTreeNode.Type.ARRAY) {

		            MetaDataTreeArray arr = (MetaDataTreeArray)(n.Parent);

                    item.Value = FormatMetaData.formatPropertyValue(n.Path, n.Data);
                    item.IconPath = "pack://application:,,,/Resources/Icons/constprop.ico";
                    item.NodeType = "Value";

		            int i = arr.getChildIndex(n);

		            item.Name = "[" + Convert.ToString(i) + "] " + item.Name;
	            }

                items.Add(item);
            }

            return(items);

        }

           
    }
    
    class MetaDataNameValue : ObservableObject
    {
        public MetaDataNameValue()
        {
            node = null;
            Name = "";
            Value = "";
            IconPath = "";
        }

        MetaDataTreeNode node;

        public MetaDataTreeNode Node
        {
            get { return node; }
            set { node = value; }
        }

        String name;

        public String Name
        {
            get { return name; }
            set { name = value;
            NotifyPropertyChanged();
            }
        }
        String value;

        public String Value
        {
            get { return this.value; }
            set { this.value = value;
            NotifyPropertyChanged();
            }
        }

        String iconPath;

        public String IconPath
        {
            get { return iconPath; }
            set { iconPath = value;
            NotifyPropertyChanged();
            }
        }

        String nodeType;

        public String NodeType
        {
            get { return nodeType; }
            set { nodeType = value;
            NotifyPropertyChanged();
            }
        }

    }
    
}
