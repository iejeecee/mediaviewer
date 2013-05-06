using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeNameSpaceNode : MetaDataTreeDictionaryNode
    {
        protected override string getPath()
        {

            return (data + ":");
        }

        public MetaDataTreeNameSpaceNode(string data)
            : base(data, MetaDataTreeNode.Type.NAMESPACE)
        {

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }
    };
}
