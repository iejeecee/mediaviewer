using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeNodeFactory
    {



        public static MetaDataTreeNode create(string data, MetaDataTreeNode.Type type)
        {

            switch (type)
            {

                case MetaDataTreeNode.Type.ARRAY:
                    {
                        return (new MetaDataTreeArray(data));

                    }
                case MetaDataTreeNode.Type.NAMESPACE:
                    {

                        return (new MetaDataTreeNameSpaceNode(data));

                    }
                case MetaDataTreeNode.Type.STRUCT:
                    {

                        return (new MetaDataTreeStructNode(data));

                    }
                case MetaDataTreeNode.Type.PROPERTY:
                    {

                        return (new MetaDataTreeProperty(data));

                    }
                case MetaDataTreeNode.Type.VALUE:
                    {

                        return (new MetaDataTreeValue(data));

                    }
                default:
                    {

                        Debug.Assert(false);
                        break;
                    }
            }

            return (null);
        }

        public static MetaDataTreeNode copy(MetaDataTreeNode node)
        {

            switch (node.NodeType)
            {

                case MetaDataTreeNode.Type.ARRAY:
                    {
                        return (new MetaDataTreeArray(node.Data));

                    }
                case MetaDataTreeNode.Type.NAMESPACE:
                    {

                        return (new MetaDataTreeNameSpaceNode(node.Data));

                    }
                case MetaDataTreeNode.Type.STRUCT:
                    {

                        return (new MetaDataTreeStructNode(node.Data));

                    }
                case MetaDataTreeNode.Type.PROPERTY:
                    {

                        return (new MetaDataTreeProperty(node.Data));

                    }
                case MetaDataTreeNode.Type.VALUE:
                    {

                        return (new MetaDataTreeValue(node.Data));

                    }
                case MetaDataTreeNode.Type.LANGUAGE:
                    {

                        return (new MetaDataTreeLanguage(node.Data));

                    }
                default:
                    {

                        Debug.Assert(false);
                        break;
                    }
            }

            return (null);
        }
    }
}
