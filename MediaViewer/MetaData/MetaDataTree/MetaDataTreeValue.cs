using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeValue : MetaDataTreeArray
    {

        protected override string getPath()
        {

            return ("");
        }

        public MetaDataTreeValue(string data)
            : base(data, MetaDataTreeNode.Type.VALUE)
        {

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }
    };

}
