using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Collections.Sort
{
    class CollectionsSort
    {
        /// <summary>
        ///    Insert item into a sorted list.
        ///    Returns index of the sorted item after insertion.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List to insert the item into</param>
        /// <param name="item">Item to be inserted</param>
        /// <param name="compareFunc">Compare function used for sorting</param>
        /// <param name="start">Start of subset of the list to insert the item into</param>
        /// <param name="end">End of subset of the list to insert item into</param>
        /// <returns>new index of sorted item</returns>
        public static int insertIntoSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc, int start, int end)
        {
            int newIndex = 0;

            if (list.Count == 0)
            {
                list.Add(item);
                return(newIndex);
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
                newIndex = mid + 1;               
            }
            else
            {
                newIndex = mid;              
            }

            list.Insert(newIndex, item);

            return (newIndex);
        }

        public static int insertIntoSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc)
        {
            int newIndex = insertIntoSortedCollection<T>(list, item, compareFunc, 0, list.Count);

            return (newIndex);
        }

        public static int insertIntoSortedCollection<T>(IList<T> list, T item)
        {
            int newIndex = insertIntoSortedCollection<T>(list, item, (a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            }, 0, list.Count);


            return (newIndex);
        }

        public static int itemIndexSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc, int start, int end)
        {            
            if (list.Count == 0)
            {                
                return (-1);
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
                    return(mid);
                }

            }
           
            return (-1);
        }

        public static int itemIndexSortedCollection<T>(IList<T> list, T item, Func<T, T, int> compareFunc)
        {
            int index = itemIndexSortedCollection(list, item, compareFunc, 0, list.Count);

            return (index);
        }

        public static int itemIndexSortedCollection<T>(IList<T> list, T item)
        {
            int index = itemIndexSortedCollection(list, item, (a, b) =>
            {
                return (a.ToString().CompareTo(b.ToString()));
            }, 0, list.Count);

            return (index);
        }
    }
}
