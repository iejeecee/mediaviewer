using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMPLib;

namespace MediaViewer.MetaData.MetaDataTree
{
    class MetaDataTree
    {


        public static MetaDataTreeNode create(XMPLib.MetaData data)
        {

            List<MetaDataProperty> propsList = new List<MetaDataProperty>();

            data.iterate(Consts.IterOptions.XMP_IterJustLeafNodes, ref propsList);

            MetaDataTreeNode root = new MetaDataTreeNameSpaceNode("root");

            foreach (MetaDataProperty p in propsList)
            {

                string path = p.path;

                //Debug.Print(p.path);
                root.insertNode(p.path, p.value);

            }

            return (root);
        }
    };
}
