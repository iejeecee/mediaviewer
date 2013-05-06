using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    abstract class MetaDataTreeDictionaryNode : MetaDataTreeNode
    {

        protected Dictionary<string, MetaDataTreeNode> child;

        protected override MetaDataTreeNode getChild(string data)
        {

            MetaDataTreeNode node;
            bool exists = child.TryGetValue(data, out node);

            if (exists == false)
            {

                return (null);

            }
            else
            {

                return (node);
            }

        }

        protected MetaDataTreeDictionaryNode(string data, MetaDataTreeNode.Type type)
            : base(data, type)
        {

            child = new Dictionary<string, MetaDataTreeNode>();
        }



        public override void clear()
        {

            child.Clear();

        }

        public override int Count
        {

            get
            {

                return (child.Count);
            }
        }

        public override System.Collections.Generic.ICollection<MetaDataTreeNode> Child
        {

            get
            {

                return (child.Values);
            }
        }

        public MetaDataTreeNode this[string key]
        {

            get
            {

                return (child[key]);
            }

            set
            {

                child[key] = value;

            }
        }

        public override bool hasChild(MetaDataTreeNode node)
        {

            MetaDataTreeNode found = child[node.Data];

            if (found == null) return (false);
            else return (found.Equals(node));
        }

        public override void insertChild(MetaDataTreeNode node)
        {

            child[node.Data] = node;

        }

    }

}
