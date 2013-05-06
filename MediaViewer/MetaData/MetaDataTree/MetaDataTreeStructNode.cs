using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeStructNode : MetaDataTreeDictionaryNode
    {

        protected override string getPath()
        {

            return (data + "/");
        }

        public MetaDataTreeStructNode(string data)
            : base(data, MetaDataTreeNode.Type.STRUCT)
        {

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }
    };
}
