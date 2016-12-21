// Copyright 2012-2016 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Authority.Util
{
    using System.Collections;
    using System.Collections.Generic;


    public class OrderedHashSet<T> :
        ICollection<T>
    {
        readonly IDictionary<T, LinkedListNode<T>> _nodes;
        readonly LinkedList<T> _values;

        public OrderedHashSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public OrderedHashSet(IEqualityComparer<T> comparer)
        {
            _nodes = new Dictionary<T, LinkedListNode<T>>(comparer);
            _values = new LinkedList<T>();
        }

        public int Count => _nodes.Count;

        public virtual bool IsReadOnly => _nodes.IsReadOnly;

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            _values.Clear();
            _nodes.Clear();
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            var found = _nodes.TryGetValue(item, out node);
            if (!found)
                return false;

            _nodes.Remove(item);
            _values.Remove(node);

            return true;
        }

        public bool TryGetValue(T item, out T value)
        {
            LinkedListNode<T> node;
            if (_nodes.TryGetValue(item, out node))
            {
                value = node.Value;
                return true;
            }

            value = default(T);
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _nodes.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Add(T item)
        {
            if (_nodes.ContainsKey(item))
                return false;

            LinkedListNode<T> node = _values.AddLast(item);

            _nodes.Add(item, node);

            return true;
        }
    }
}