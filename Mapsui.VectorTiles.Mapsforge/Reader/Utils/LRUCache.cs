/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016, 2017 Dirk Weltz
 * Copyright 2016 Michael Oed
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Mapsui.VectorTiles.Mapsforge.Reader.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// An LRUCache with a fixed size and an access-order policy. Old mappings are automatically removed from the cache when
    /// new mappings are added. This implementation uses an <seealso cref="LinkedHashMap"/> internally.
    /// </summary>
    /// @param <K>
    ///            the type of the map key, see <seealso cref="Map"/>. </param>
    /// @param <V>
    ///            the type of the map value, see <seealso cref="Map"/>. </param>
    public class LRUCache<K, V>
    {
        private int capacity;
        private object lockObj = new object();
        private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
        private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

        public LRUCache(int capacity)
        {
            if (capacity < 0)
            {
                throw new System.ArgumentException("capacity must not below 0");
            }

            this.capacity = capacity;
        }

        public int Capacity
        {
            get
            {
                return this.capacity;
            }
        }

        public V Get(K key)
        {
            lock(lockObj)
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
        }

        public V Add(K key, V val)
        {
            LinkedListNode<LRUCacheItem<K, V>> old;

            cacheMap.TryGetValue(key, out old);

            lock(lockObj)
            {
                if (RemoveEldestEntry(new KeyValuePair<K, V>(key, val)))
                {
                    RemoveFirst();
                }

                if (lruList.Count < capacity)
                {
                    LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
                    LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
                    lruList.AddLast(node);
                    cacheMap.Add(key, node);
                }
            }

            if (old != null)
            {
                return old.Value.value;
            }
            else
            {
                return default(V);
            }
        }

        public bool ContainsKey(K key)
        {
            return cacheMap.ContainsKey(key);
        }

        public int Size()
        {
            return cacheMap.Count;
        }

        public void Clear()
        {
            while (cacheMap.Count > 0)
            {
                RemoveFirst();
            }
        }

        public void Remove(K key)
        {
            if (lruList == null || lruList.Count == 0)
            {
                return;
            }

            LinkedListNode<LRUCacheItem<K, V>> node;

            if (cacheMap.TryGetValue(key, out node))
            {
                // Remove from cache
                cacheMap.Remove(key);
                // Remove from LRUPriority
                lruList.Remove(node);
            }
        }

        private void RemoveFirst()
        {
            if (lruList == null || lruList.Count == 0)
            {
                return;
            }

            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
            lruList.RemoveFirst();

            // Remove from cache
            cacheMap.Remove(node.Value.key);
        }

        protected virtual bool RemoveEldestEntry(KeyValuePair<K, V> eldest)
        {
            return cacheMap.Count >= capacity;
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