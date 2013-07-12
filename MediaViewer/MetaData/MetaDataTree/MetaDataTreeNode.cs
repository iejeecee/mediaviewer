using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    public abstract class MetaDataTreeNode
    {

        public enum Type
        {
            NAMESPACE,
            ARRAY,
            STRUCT,
            PROPERTY,
            LANGUAGE,
            VALUE
        };

        protected string data;
        protected Type type;

        protected MetaDataTreeNode parent;

        protected abstract MetaDataTreeNode getChild(string data);
        protected abstract string getPath();

        protected MetaDataTreeNode(string data, Type type)
        {

            this.data = data;
            this.type = type;

            parent = null;
        }



        public virtual bool hasChild(MetaDataTreeNode node)
        {

            foreach (MetaDataTreeNode c in Child)
            {

                if (c.Equals(node))
                {

                    return (true);
                }
            }

            return (false);
        }

        public abstract void insertChild(MetaDataTreeNode node);


        public virtual bool hasNode(string path)
        {

            MetaDataTreeNode node = getNode(path);

            return (node == null ? false : true);
        }

        public virtual MetaDataTreeNode getNode(string path)
        {

            string head = "";
            string tail = "";

            for (int i = 0; i < path.Length; i++)
            {

                if (path[i] == '[' || path[i] == ':' || path[i] == '/')
                {

                    if (i != 0)
                    {

                        tail = path.Substring(i);
                        break;

                    }
                    else if (path[i] == '[')
                    {

                        head += path[i];
                    }

                }
                else
                {

                    head += path[i];
                }

            }

            MetaDataTreeNode node = getChild(head);

            if (node == null)
            {

                return (null);

            }
            else
            {

                if (string.IsNullOrEmpty(tail))
                {

                    return (node);

                }
                else
                {

                    return (node = node.getNode(tail));
                }
            }
        }

        public void insertNode(string name, string value)
        {

            string head = "";
            string tail = "";

            MetaDataTreeNode.Type type = MetaDataTreeNode.Type.ARRAY;

            for (int i = 0; i < name.Length; i++)
            {

                if (name[i] == '[' || name[i] == ':' || name[i] == '/')
                {

                    if (i != 0)
                    {

                        tail = name.Substring(i);

                        if (name[i] == '[')
                        {

                            type = MetaDataTreeNode.Type.ARRAY;

                        }
                        else if (name[i] == ':')
                        {

                            type = MetaDataTreeNode.Type.NAMESPACE;

                        }
                        else if (name[i] == '/')
                        {

                            type = MetaDataTreeNode.Type.STRUCT;
                        }
                        break;

                    }
                    else if (name[i] == '[')
                    {

                        head += name[i];
                    }

                }
                else
                {

                    head += name[i];
                }

            }

            MetaDataTreeNode node = getChild(head);

            if (node == null)
            {

                if (string.IsNullOrEmpty(tail))
                {

                    node = new MetaDataTreeValue(value);

                    if (this.NodeType == Type.ARRAY)
                    {

                        node.parent = this;
                        insertChild(node);

                    }
                    else
                    {

                        MetaDataTreeProperty prop = new MetaDataTreeProperty(head);
                        prop.parent = this;
                        node.parent = prop;
                        prop.insertChild(node);

                        insertChild(prop);
                    }

                }
                else
                {

                    if (this.NodeType == Type.ARRAY)
                    {

                        node = MetaDataTreeNodeFactory.create("", type);

                    }
                    else
                    {

                        node = MetaDataTreeNodeFactory.create(head, type);
                    }
                    node.parent = this;

                    insertChild(node);
                    node.insertNode(tail, value);
                }

            }
            else
            {

                if (string.IsNullOrEmpty(tail))
                {

                    if (node.type == Type.VALUE)
                    {

                        node.Data = value;

                    }
                    else
                    {

                        MetaDataTreeProperty prop = (MetaDataTreeProperty)(node);
                        prop.Value = value;

                    }

                }
                else
                {

                    if (node.type == type)
                    {

                        node.insertNode(tail, value);

                    }
                    else
                    {

                        MetaDataTreeLanguage lang = new MetaDataTreeLanguage(value);
                        MetaDataTreeArray arr = (MetaDataTreeArray)(node);
                        arr.insertChild("[1]", lang);
                    }
                }
            }

        }

        MetaDataTreeNode Parent
        {

            get
            {

                return (parent);
            }

            set
            {

                this.parent = value;
            }
        }

        public abstract void clear();

        public abstract int Count
        {

            get;
        }

        public abstract System.Collections.Generic.ICollection<MetaDataTreeNode> Child
        {

            get;
        }

        public Type NodeType
        {

            get
            {

                return (type);
            }
        }

        public string Data
        {

            get
            {

                return (data);
            }

            set
            {

                this.data = value;
            }
        }

        public string Path
        {

            get
            {

                string path = getPath();

                MetaDataTreeNode p = this;

                while (p.parent != null && p.parent.parent != null)
                {

                    path = p.parent.getPath() + path;

                    p = p.parent;
                }

                return (path);
            }

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }

        public virtual void print(int tabs)
        {

            string output = "";

            for (int i = 0; i < tabs; i++)
            {

                output += "\t";
            }

            output += ToString() + "\n";

            System.Diagnostics.Debug.Write(output);

            foreach (MetaDataTreeNode node in Child)
            {

                node.print(tabs + 1);

            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {

            MetaDataTreeNode node = (MetaDataTreeNode)(obj);

            if (node == null) return (false);

            if (node.NodeType != NodeType) return (false);

            if (node.Data.Equals(Data)) return (true);

            return (false);
        }

        MetaDataTreeNode intersection(MetaDataTreeNode tree)
        {

            MetaDataTreeNode result = null;

            if (this.Equals(tree))
            {

                result = MetaDataTreeNodeFactory.copy(tree);

                foreach (MetaDataTreeNode a in Child)
                {

                    foreach (MetaDataTreeNode b in tree.Child)
                    {

                        MetaDataTreeNode intersection = a.intersection(b);
                        if (intersection != null)
                        {

                            intersection.Parent = result;
                            result.insertChild(intersection);
                            break;
                        }
                    }
                }
            }

            return (result);
        }
    }

}
