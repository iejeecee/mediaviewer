using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTreeLanguage : MetaDataTreeArray
    {

        protected override string getPath()
        {

            return ("/?xml:lang");
        }

        public MetaDataTreeLanguage(string data)
            : base(data, MetaDataTreeNode.Type.LANGUAGE)
        {

        }

        public override string ToString()
        {

            string info = data;

            return (info);
        }

    }
}
