using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeProperty : MetaDataTreeArray
    {

        protected override string getPath()
        {

            return (data);
        }

        public MetaDataTreeProperty(string data)
            : base(data, MetaDataTreeNode.Type.PROPERTY)
        {

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }

        public string Value
        {

            get
            {
                if (Count == 0) return (null);
                else return (child[0].Data);
            }

            set
            {
                Debug.Assert(Count > 0);
                child[0].Data = value;
            }
        }
    };
}
