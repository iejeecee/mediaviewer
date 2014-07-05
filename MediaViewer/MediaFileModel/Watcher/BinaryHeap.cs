using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileModel.Watcher
{

    class BinaryHeap<T> where T : IComparable<T>
    {
        List<T> heap;

        public BinaryHeap() {

            heap = new List<T>();
        }

        private int ParentIndex(int node)
        {
            int index = (int)Math.Floor((node - 1) / 2.0);
            return (index);
        }

        private T Parent(int node)
        {
            System.Diagnostics.Debug.Assert(node > 0 && node < NrNodes);
            int index = ParentIndex(node);

            if (index < 0)
            {
                return(default(T));
            }

            return (heap[index]);
        }

        private T Child0(int node)
        {
            System.Diagnostics.Debug.Assert(node > 0 && node < NrNodes);

            int index = 2 * node + 1;

            if (index > NrNodes)
            {
                return (default(T));
            }

            return (heap[index]);
        }

        private T Child1(int node)
        {
            System.Diagnostics.Debug.Assert(node > 0 && node < NrNodes);

            int index = 2 * node + 2;

            if (index > NrNodes)
            {
                return (default(T));
            }

            return (heap[index]);
        }

        public int NrNodes
        {
            get
            {
                return (heap.Count);
            }
        }

        public void Add(T item)
        {
            heap.Add(item);

            int currentNode = NrNodes;

            while (!EqualityComparer<T>.Default.Equals(Parent(currentNode), default(T)) && 
                Parent(currentNode).CompareTo(heap[currentNode]) > 0) 
            {
                T temp = heap[currentNode];
                heap[currentNode] = Parent(currentNode);                
                heap[ParentIndex(currentNode)] = temp;
                currentNode = ParentIndex(currentNode);
            }
        }

        public void Delete(int nodeIndex)
        {
            int deleteIndex = nodeIndex;
          
            while (!EqualityComparer<T>.Default.Equals(Child0(nodeIndex), default(T)) && 
                !EqualityComparer<T>.Default.Equals(Child1(nodeIndex), default(T))) 
            {
                

            } 


            //heap[node];
        }


    }
}
