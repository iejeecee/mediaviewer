using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeArray : MetaDataTreeNode
    {



        protected List<MetaDataTreeNode> child;

        protected int nameToIndex(string input)
        {

            if (string.IsNullOrEmpty(input)) return (-1);

            string temp = input.Substring(1, input.Length - 2);
            int index = Convert.ToInt32(temp) - 1;

            return (index);
        }

        protected string indexToName(int index)
        {

            string temp = Convert.ToString(index + 1);
            string output = "[" + temp + "]";

            return (output);
        }

        protected override MetaDataTreeNode getChild(string data)
        {

            int index = nameToIndex(data);

            if (index >= Count)
            {

                return (null);

            }
            else
            {

                return (child[index]);
            }
        }

        protected MetaDataTreeArray(string data, MetaDataTreeNode.Type type)
            : base(data, type)
        {

            child = new List<MetaDataTreeNode>();
        }

        protected override string getPath()
        {

            return (data + "[]");
        }



        public MetaDataTreeArray(string data)
            : base(data, MetaDataTreeNode.Type.ARRAY)
        {

            child = new List<MetaDataTreeNode>();
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

                return (child);
            }
        }

        public MetaDataTreeNode this[int index]
        {

            get
            {

                Debug.Assert(index < Count);

                return (child[index]);
            }

            set
            {

                Debug.Assert(index < Count);

                child[index] = value;

            }
        }

        public override bool hasChild(MetaDataTreeNode node)
        {

            foreach (MetaDataTreeNode c in child)
            {

                return (c.Equals(node));
            }

            return (false);
        }

        public override void insertChild(MetaDataTreeNode node)
        {

            child.Add(node);

        }

        public virtual int getChildIndex(MetaDataTreeNode node)
        {

            for (int i = 0; i < Count; i++)
            {

                if (node == child[i]) return (i);
            }

            return (-1);
        }

        public virtual void insertChild(string indexStr, MetaDataTreeNode node)
        {

            int index = nameToIndex(indexStr);

            if (index < Count)
            {

                child[index] = node;

            }
            else if (index == Count)
            {

                child.Add(node);

            }
            else if (index > Count)
            {

                Debug.Assert(false);
            }

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }

    }
}
