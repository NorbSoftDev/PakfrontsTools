using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NorbSoftDev.SOW
{
    /// <summary>
    /// Must implement System.Collections.IList, to work in DataGrid
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IdOrderedSet<TValue> :
            IList<TValue>,
            System.Collections.IList,
        //IDictionary<string, TValue>,
            INotifyCollectionChanged,
            INotifyPropertyChanged
            where TValue : class, INotifyPropertyChanged, IHasId
    {
        private List<TValue> _list;
        readonly IDictionary<string, TValue> _dictionary; // = new Dictionary<TKey, TValue>();

        /// <summary>Gets the number of items stored in this collection.</summary>
        public int Count { get { return _list.Count; } }
        /// <summary>Returns false.</summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        ///     Constructor.</summary>
        /// <remarks>
        ///     Certain serialization libraries require a parameterless constructor.</remarks>
        public IdOrderedSet() : this(4) { }


        /// <summary>Constructor.</summary>
        public IdOrderedSet(int capacity = 4)
        {
            _list = new List<TValue>(capacity);
            _dictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Constructor.</summary>
        public IdOrderedSet(IEnumerable<TValue> items)
            : this(4)
        {
            foreach (var item in _list)
            {
                item.PropertyChanged += ItemPropertyChanged;
                _dictionary[item.id] = item;
            }
        }

        /// <summary>Removes all items from this collection.</summary>
        public void Clear()
        {
            foreach (var item in _list)
                item.PropertyChanged -= ItemPropertyChanged;
            _list.Clear();
            _dictionary.Clear();
            collectionChanged_Reset();
            propertyChanged("Count");
        }

        /// <summary>
        ///     Adds an item to this collection, ensuring that it ends up at the correct place according to the sort order.</summary>
        public void Add(TValue item)
        {

            if (_dictionary.ContainsKey(item.id))
            {
                Remove(item.id);
            }

            _list.Add(item);
            _dictionary[item.id] = item;
            item.PropertyChanged += ItemPropertyChanged;
            collectionChanged_Added(item, _list.Count - 1);
            propertyChanged("Count");
        }

        /// <summary>
        ///     Adds an item to this collection, ensuring that it ends up at the correct place according to the sort order.</summary>
        public void AddRange(IList<TValue> items)
        {
            foreach (TValue item in items)
            {
                if (_dictionary.ContainsKey(item.id))
                {
                    Remove(item.id);
                }

                _list.Add(item);
                _dictionary[item.id] = item;
                item.PropertyChanged += ItemPropertyChanged;
            }
            collectionChanged_Reset();
            propertyChanged("Count");
        }

        /// <summary>Not supported on a sorted collection.</summary>
        public void Insert(int index, TValue item)
        {

            //int? oldIndex = null;

            if (_dictionary.ContainsKey(item.id))
            {
                Remove(item.id);
            }

            _list.Insert(index, item);
            _dictionary[item.id] = item;
            item.PropertyChanged += ItemPropertyChanged;

            //collectionChanged_Moved(item, oldIndex, newIndex);

        }

        /// <summary>Removes the specified item, returning true if found or false otherwise.</summary>
        public bool Remove(TValue item)
        {
            int i = IndexOf(item);
            if (i < 0) return false;
            _list.RemoveAt(i);
            _dictionary.Remove(item.id);
            item.PropertyChanged -= ItemPropertyChanged;
            collectionChanged_Removed(item, i);
            propertyChanged("Count");
            return true;
        }

        /// <summary>
        ///     Gets the index of the specified item, or -1 if not found. Only reference equality matches are considered.</summary>
        /// <remarks>
        ///     Binary search is used to make the operation more efficient.</remarks>
        public int IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>Removes the specified item.</summary>
        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            _dictionary.Remove(item.id);
            collectionChanged_Removed(item, index);
            propertyChanged("Count");
        }

        /// <summary>Gets the item at the specified index. Does not support setting.</summary>
        public TValue this[int index]
        {
            get { return _list[index]; }
            set
            {
                TValue oldItem = null;
                if (index < _list.Count)
                {
                    oldItem = _list[index];
                    _dictionary.Remove(oldItem.id);
                }
                _list[index] = value;
                _dictionary[value.id] = value;
                collectionChanged_Removed(value, index);
                collectionChanged_Added(value, index);
            }
        }

        /// <summary>
        ///     Returns a value indicating whether the specified item is contained in this collection.</summary>
        /// <remarks>
        ///     Uses binary search to make the operation more efficient.</remarks>
        public bool Contains(TValue item)
        {
            return _dictionary.ContainsKey(item.id);
        }

        /// <summary>Copies all items to the specified array.</summary>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>Enumerates all items in sorted order.</summary>
        public IEnumerator<TValue> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }



        #region IDictionary
        public void Remove(string id)
        {
            TValue item = _dictionary[id];
            _dictionary.Remove(id);
            _list.Remove(item);
            item.PropertyChanged -= ItemPropertyChanged;
        }

        public TValue this[string key]
        {

            get { return _dictionary[key]; }

            set
            {

                if (_dictionary.ContainsKey(key))
                {
                    TValue old = _dictionary[key];
                    if (old == null)
                    {
                        if (value == null) return;
                    }
                    else if (old.Equals(value)) return;

                    //if (!changed) return; //if there are no changes then we don’t need to update the value or trigger changed events.
                }

                _dictionary[key] = value;
                _list.Add(value);

                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Keys"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Values"));
                }
            }

        }

        public bool TryGetValue(string key, out TValue value) {
            return _dictionary.TryGetValue(key, out value);
        }



        public ICollection<string> Keys
        {
            get { return _dictionary.Keys; }
        }



        //public ICollection<TValue> Values
        //{
        //   get { return _dictionary.Values; }
        //}        
        #endregion

        #region INotifyCollectionChanged
        /// <summary>Triggered whenever the <see cref="Count" /> property changes as a result of adding/removing items.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void propertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void collectionChanged_Reset()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void collectionChanged_Added(TValue item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        private void collectionChanged_Removed(TValue item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        private void collectionChanged_Moved(TValue item, int oldIndex, int newIndex)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (TValue)sender;

            if (e.PropertyName == "id")
            {
                foreach (KeyValuePair<string, TValue> kvp in _dictionary)
                {
                    if (kvp.Value == item)
                    {
                        _dictionary.Remove(kvp.Key);
                        _dictionary[item.id] = item;
                    }
                }

            }
        }
        #endregion

        #region IList
        int IList.Add(object value)
        {
            this.Add((TValue)value);
            return Count - 1;
        }

        //void IList.Clear()
        //{
        //    this
        //}

        bool IList.Contains(object value)
        {
            TValue item = value as TValue;
            if (item == null) return false;
            return this.Contains(item);
        }

        int IList.IndexOf(object value)
        {
            TValue item = value as TValue;
            if (item == null) return -1;
            return this.IndexOf(item);
        }

        void IList.Insert(int index, object value)
        {
            TValue item = (TValue) value;

            this.Insert(index, item);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        //bool IList.IsReadOnly
        //{
        //    get { throw new NotImplementedException(); }
        //}

        void IList.Remove(object value)
        {
            TValue item = (TValue)value;

            this.Remove(item);
        }

        //void IList.RemoveAt(int index)
        //{
        //    throw new NotImplementedException();
        //}

        object IList.this[int index]
        {
            get
            {
                return _list[index];
            }

            set { throw new InvalidOperationException("Cannot set an item at an arbitrary index in a IDCollection."); }

        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((System.Collections.IList)_list).CopyTo(array, index);
        }

        //int ICollection.Count
        //{
        //    get { throw new NotImplementedException(); }
        //}

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return ((System.Collections.IList)_list).SyncRoot; }
        }
        #endregion

        public string GetUniqueId(string requested)
        {
            //if (!_dictionary.ContainsKey(requested))
            //    return requested;
            
            string id = requested;
            int cnt = 1;
            while (_dictionary.ContainsKey(id))
            {
                id = requested + cnt;
            }
            return id;
        }


    }

   // public class IdOrderedSet<TValue> : ObservableCollection<TValue>
   //      where TValue : class, INotifyPropertyChanged, IHasId
   // {
   //     public TValue this[string key]
   //     {

   //         get {
   //             foreach (TValue item in this)
   //             {
   //                 if (String.Equals(key, item.id))
   //                 {
   //                     return item;
   //                 }
   //             }
   //             return null;
   //         }

   //         set
   //         {
   //             Add(value);
   //         }

   //     }
   //} 
}