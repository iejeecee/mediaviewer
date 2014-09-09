using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaViewer.ImageGrid
{
    class LockableListCollectionView : ListCollectionView
    {
        Object LockObject { get; set; }

        public LockableListCollectionView(System.Collections.IList list, Object lockObject)
            : base(list)
        {
            LockObject = lockObject;
        }
/*
        override IList PrepareLocalArray(IList list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            lock (LockObject)
            {

                // filter the collection's array into the local array
                ArrayList al;

                if (ActiveFilter == null)
                {
                    al = new ArrayList(list);
                }
                else
                {
                    al = new ArrayList(list.Count);       //
                    for (int k = 0; k < list.Count; ++k)
                    {
                        if (ActiveFilter(list[k]))
                            al.Add(list[k]);
                    }
                }

                // sort the local array
                if (ActiveComparer != null)
                {
                    al.Sort(ActiveComparer);
                }

                return al;
            }
           
        }
 */
    }
}
