using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Collections.Cache
{
    namespace LRUCache
    {
        public class LRUCache<K, V>
        {
            int capacity;
            Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
            LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

            ReaderWriterLockSlim rwLock;

            public LRUCache(int capacity)
            {
                this.capacity = capacity;
                rwLock = new ReaderWriterLockSlim();
            }
           
            public V get(K key)
            {
                rwLock.EnterReadLock();
                try
                {
                    LinkedListNode<LRUCacheItem<K, V>> node;
                    if (cacheMap.TryGetValue(key, out node))
                    {
                        V value = node.Value.value;
                        lruList.Remove(node);
                        lruList.AddLast(node);
                        return value;
                    }
                    return default(V);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }

          
            public void add(K key, V val)
            {
                rwLock.EnterWriteLock();
                try
                {
                    if (cacheMap.Count >= capacity)
                    {
                        RemoveFirst();
                    }

                    LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
                    LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
                    lruList.AddLast(node);
                    cacheMap.Add(key, node);
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }

            private void RemoveFirst()
            {
                // Remove from LRUPriority
                LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
                lruList.RemoveFirst();

                // Remove from cache
                cacheMap.Remove(node.Value.key);
            }
        }

        class LRUCacheItem<K, V>
        {
            public LRUCacheItem(K k, V v)
            {
                key = k;
                value = v;
            }
            public K key;
            public V value;
        }
    }
}
