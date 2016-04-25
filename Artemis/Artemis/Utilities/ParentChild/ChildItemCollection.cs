using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Utilities.ParentChild
{
    /// <summary>
    ///     Collection of child items. This collection automatically set the
    ///     Parent property of the child items when they are added or removed
    ///     Thomas Levesque - http://www.thomaslevesque.com/2009/06/12/c-parentchild-relationship-and-xml-serialization/
    /// </summary>
    /// <typeparam name="P">Type of the parent object</typeparam>
    /// <typeparam name="T">Type of the child items</typeparam>
    public class ChildItemCollection<P, T> : IList<T>
        where P : class
        where T : IChildItem<P>
    {
        private IList<T> _collection;
        private readonly P _parent;

        public ChildItemCollection(P parent)
        {
            _parent = parent;
            _collection = new List<T>();
        }

        public ChildItemCollection(P parent, IList<T> collection)
        {
            _parent = parent;
            _collection = collection;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_collection as IEnumerable).GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item != null)
                item.Parent = _parent;
            _collection.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            var oldItem = _collection[index];
            _collection.RemoveAt(index);
            if (oldItem != null)
                oldItem.Parent = null;
        }

        public T this[int index]
        {
            get { return _collection[index]; }
            set
            {
                var oldItem = _collection[index];
                if (value != null)
                    value.Parent = _parent;
                _collection[index] = value;
                if (oldItem != null)
                    oldItem.Parent = null;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (item != null)
                item.Parent = _parent;
            _collection.Add(item);
        }

        public void Clear()
        {
            foreach (var item in _collection)
            {
                if (item != null)
                    item.Parent = null;
            }
            _collection.Clear();
        }

        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public bool Remove(T item)
        {
            var b = _collection.Remove(item);
            if (item != null)
                item.Parent = null;
            return b;
        }

        #endregion

        public void Sort(Func<T, object> func)
        {
            _collection = _collection.OrderBy(func).ToList();
        }
    }
}