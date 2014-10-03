using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Collections.Sort
{
    class CollectionsSort
    {
        public static void insertIntoSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc, int start, int end)
        {
            if (list.Count == 0)
            {
                list.Add(item);
                return;
            }

            int mid = start;
            int low = start;
            int high = end - 1;

            while (low <= high)
            {
                mid = (high + low) / 2;

                int val = compareFunc(item, list[mid]);

                if (val < 0)
                {
                    high = mid - 1;
                }
                else if (val > 0)
                {
                    low = mid + 1;
                }
                else
                {
                    break;
                }

            }

            if (compareFunc(item, list[mid]) >= 0)
            {
                list.Insert(mid + 1, item);
            }
            else
            {
                list.Insert(mid, item);
            }

        }

        public static void insertIntoSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc)
        {

            insertIntoSortedCollection<T>(list, item, compareFunc, 0, list.Count);

        }

        public static void insertIntoSortedCollection<T>(IList<T> list, T item)
        {
            insertIntoSortedCollection<T>(list, item, (a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            }, 0, list.Count);

        }
    }
}
