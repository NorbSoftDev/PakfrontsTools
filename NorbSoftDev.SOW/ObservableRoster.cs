using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace NorbSoftDev.SOW
{

    // public interface IUnit : INotifyPropertyChanged {
    //     IEchelon echelon { get; set; }
    //     string id { get; }
    // }

    // public interface IEchelon : INotifyPropertyChanged {
    //     ICollection<IEchelon> children { get; }
    //     ICollection<IUnit> units { get; }
    //     IUnit unit { get; }
    //     ObservableRoster roster { get; }
    //     int id { get; }

    // }

    public class ObservableRoster
    {

    }

    /// <summary>
    ///     Implements an observable collection which maintains its items in sorted order. In particular, items remain sorted
    ///     when changes are made to their properties: they are reordered automatically when necessary to keep them sorted.</summary>
    /// <remarks>
    ///     <para>This class currently requires <typeparamref name="T" /> to be a reference type. This is because a couple of
    ///     methods operate on the basis of reference equality instead of the comparison used for sorting. As implemented,
    ///     their behaviour for value types would be somewhat unexpected.</para>
    ///     <para>The INotifyCollectionChange interface is fairly complicated and relatively poorly documented (see
    ///     http://stackoverflow.com/a/5883947/33080 for example), increasing the likelihood of bugs. And there are currently
    ///     no unit tests. There could well be bugs in this code.</para></remarks>
    public abstract class ObservableRoster<T, R> : ObservableRoster, 
		IList<T>,
		System.Collections.IList,
        INotifyPropertyChanged,
        INotifyCollectionChanged
        where T : class, IUnit, INotifyPropertyChanged
        //        where R : class, IEchelon, INotifyPropertyChanged, ICollection<T>
        where R : EchelonGeneric<T>
    {
        protected List<T> _list = new List<T>();
        protected IComparer<T> _comparer;
        protected Dictionary<string, T> _unitsById = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        protected Dictionary<string, T> _unitsByName1 = null;

        protected R _root;

        public R root
        {
            get { return _root; }
            protected set
            {
                if (_root != null)
                {
                    _root.CollectionChanged -= HandleRootChanged;
                    // _root.PropertyChanged += ItemPropertyChanged;
                }
                _root = value;
                ((IEchelonRoot)root).roster = this;
                _root.CollectionChanged += HandleRootChanged;
                // _root.PropertyChanged += ItemPropertyChanged;
            }
        }


        public string stats
        {
            get
            {
                return "_list:" + _list.Count + " _ids:" + _unitsById.Count;
            }
        }

        bool _isDirty;
        public bool isDirty
        {
            get
            {
                return _isDirty;
            }
            protected set
            {
                if (_isDirty == value) return;
                _isDirty = value;
                OnPropertyChanged("isDirty");
            }
        }


        /// <summary>Gets the number of items stored in this collection.</summary>
        public int Count { get { return _list.Count; } }
        /// <summary>Returns false.</summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        ///     Constructor.</summary>
        /// <remarks>
        ///     Certain serialization libraries require a parameterless constructor.</remarks>
        public ObservableRoster() : this(4,null) { }

        /// <summary>Constructor.</summary>
		/// 
        public ObservableRoster(int capacity, IComparer<T> comparer)
        {
            _list = new List<T>(capacity);
            _comparer = comparer ?? Comparer<T>.Default;
        }

        // /// <summary>Constructor.</summary>
        // public ObservableRoster(IEnumerable<T> items, IComparer<T> comparer = null)
        // {
        //     _list = new List<T>(items);
        //     _comparer = comparer ?? Comparer<T>.Default;
        //     _list.Sort(_comparer);
        //     foreach (var item in _list)
        //         item.PropertyChanged += ItemPropertyChanged;
        // }


        void HandleRootChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            isDirty = true;
            OnPropertyChanged("Count");
            // OnCollectionChanged(
            //     new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            //         );

        }

        /// <summary>Removes all items from this collection.</summary>
        public void Clear()
        {
            foreach (var item in _list)
                item.PropertyChanged -= unit_PropertyChanged;
            _list.Clear();
            _unitsById.Clear();
            _unitsById = null;
            ClearRoot();
            collectionChanged_Reset();
            isDirty = true;

            isDirty = true;
            OnPropertyChanged("Count");
        }

        protected abstract void ClearRoot();

        /// <summary>
        ///     Adds an item to this collection, ensuring that it ends up at the correct place according to the sort order.</summary>
        public int Add(T unit, R echelon)
        {
            if (echelon == null)
            {
                throw new RosterAddException("Cannot add unit " + unit + " with echelon null");
            }

            if (echelon.root == null)
            {
                throw new RosterAddException("Cannot add unit " + unit + " with echelon " + echelon + " with null root");
            }


            if (unit == null)
            {
                ((INotifyPropertyChanged)echelon).PropertyChanged += echelon_PropertyChanged;
                OnPropertyChanged("Count");
                isDirty = true;

                return -1;
            }

            if (unit.echelon.root != null && unit.echelon.root != this.root)
            {
                throw new RosterAddException("Cannot add unit " + unit + " with echelon " + echelon + " from another root " + echelon.root + ". Excpeted " + this.root);
            }


            if (_list.Contains(unit))
            {
                throw new RosterAddException("Cannot Add; already contains " + unit);
            }

            if (_unitsById.ContainsKey(unit.id) && _unitsById[unit.id] != unit)
            {
                Log.Error(this, "Already contains id " + unit.id + " " + unit.GetHashCode() + " != " + _unitsById[unit.id].GetHashCode());
                throw new RosterAddException("Cannot Add already contains id " + unit.id);
            }

            int i = _list.BinarySearch(unit, _comparer);
            if (i < 0)
                i = ~i;
            else
                do i++; while (i < _list.Count && _comparer.Compare(_list[i], unit) == 0);

            if (!echelon.Contains(unit)) echelon.Add(unit);

            unit.echelon = echelon;
            _list.Insert(i, unit);
            // Console.WriteLine("Adding "+item.id);
            _unitsById[unit.id] = unit;
            _unitsByName1 = null;

            unit.PropertyChanged += unit_PropertyChanged;

            // MS fs it up again
            // http://stackoverflow.com/questions/1003344/observablecollection-propertychanged-event
            ((INotifyPropertyChanged)echelon).PropertyChanged += echelon_PropertyChanged;

            collectionChanged_Added(unit, i);
            isDirty = true;

            OnPropertyChanged("Count");
            return i;
        }


        public void Add(T item)
        {
            Add(item, (R)item.echelon);
        }

        /// <summary>Not supported on a sorted collection.</summary>
        public void Insert(int index, T item)
        {
            throw new InvalidOperationException("Cannot insert an item at an arbitrary index into a ObservableRoster.");
        }


        //bool RemoveQuietly(T item)
        //{
        //    int i = IndexOf(item);
        //    if (i < 0) return false;

        //    _list.RemoveAt(i);
        //    ((R)(item.echelon)).Remove(item);
        //    _unitsById.Remove(item.id);
        //    _unitsByName1 = null;

        //    //collectionChanged_Removed(item, i);
        //    isDirty = true;
        //    //OnPropertyChanged("Count");

        //    return true;
        //}
        /// <summary>Removes the specified item, returning true if found or false otherwise.</summary>
        public bool Remove(T item)
        {
            int i = IndexOf(item);
            if (i < 0) return false;

            _list.RemoveAt(i);
            ((R)(item.echelon)).Remove(item);
            _unitsById.Remove(item.id);
            _unitsByName1 = null;

            collectionChanged_Removed(item, i);
            isDirty = true;

            return true;
        }

        /// <summary>Removes the specified item.</summary>
        public void RemoveAt(int index)
        {
            T item = _list[index];
            _list.RemoveAt(index);
            ((R)(item.echelon)).Remove(item);
            _unitsById.Remove(item.id);
            _unitsByName1 = null;

            collectionChanged_Removed(item, index);
            isDirty = true;

            OnPropertyChanged("Count");
        }

        /// <summary>Gets the item at the specified index. Does not support setting.</summary>
        public T this[int index]
        {
            get { return _list[index]; }
            set { throw new InvalidOperationException("Cannot set an item at an arbitrary index in a ObservableRoster."); }
        }

        /// <summary>
        ///     Gets the index of the specified item, or -1 if not found. Only reference equality matches are considered.</summary>
        /// <remarks>
        ///     Binary search is used to make the operation more efficient.</remarks>
        public int IndexOf(T item)
        {
            int i = _list.BinarySearch(item, _comparer);
            if (i < 0) return -1;
            if (object.ReferenceEquals(_list[i], item)) return i;
            // Search downwards
            for (int s = i - 1; s >= 0 && _comparer.Compare(_list[s], item) == 0; s--)
                if (object.ReferenceEquals(_list[s], item))
                    return s;
            // Search upwards
            for (int s = i + 1; s < _list.Count && _comparer.Compare(_list[s], item) == 0; s++)
                if (object.ReferenceEquals(_list[s], item))
                    return s;
            // Not found
            return -1;
        }

        /// <summary>
        ///     Returns a value indicating whether the specified item is contained in this collection.</summary>
        /// <remarks>
        ///     Uses binary search to make the operation more efficient.</remarks>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>Copies all items to the specified array.</summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }


        public T GetUnitByName1(string key) {
            if (_unitsByName1 == null) {
                    _unitsByName1 = new Dictionary<string, T>();
                    foreach (T unit in _unitsById.Values)
                    {
                        _unitsByName1[unit.name1] = unit;
                    }
            }

            return _unitsByName1[key];
        }

        public bool TryGetUnitByIdOrName(string key, out T value) {
            if ( _unitsById.TryGetValue(key, out value) ) {
                return true;
            }

            if (_unitsByName1 == null) {
                    _unitsByName1 = new Dictionary<string, T>();
                    foreach (T unit in _unitsById.Values)
                    {
                        _unitsByName1[unit.name1] = unit;
                    }
            }

            if ( _unitsByName1.TryGetValue(key, out value) ) {
                return true;
            }

            return false;
        }


        /// <summary>Enumerates all items in sorted order.</summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #region Dictlike
        public bool Contains(string id)
        {
            return _unitsById.ContainsKey(id);
        }

        public T this[string i]
        {
            get
            {
                T iunit = default(T);
                if (_unitsById.TryGetValue(i, out iunit)) return iunit;
                Log.Info(this,"Unable to find unit id '" + i + "'");
                // foreach (string k in _unitsById.Keys)
                // {
                //     Log.Info(this, k);
                // }
                throw new System.Collections.Generic.KeyNotFoundException(i);
            }
        }
        #endregion

        #region Echelon
        //remove all children and units of this echelon
        public void RemoveEchelon(R echelon)
        {
            List<T> unitsToRemove = new List<T>();
            List<EchelonGeneric<T>> echelonsToRemove = new List<EchelonGeneric<T>>();

            echelon.ListAllChildrenAndUnits(unitsToRemove, echelonsToRemove);
            foreach (T unit in unitsToRemove)
            {
                Remove(unit);
            }

            foreach (EchelonGeneric<T> ech in echelonsToRemove)
            {
                ech.parent = null;
            }
            isDirty = true;

            OnPropertyChanged("Count");

        }

        //remove all children and units of this echelon
        public void RemoveEchelons(R [] echelons)
        {
            List<T> unitsToRemove = new List<T>();
            List<EchelonGeneric<T>> echelonsToRemove = new List<EchelonGeneric<T>>();
            foreach (R echelon in echelons)
            {
                echelon.ListAllChildrenAndUnits(unitsToRemove, echelonsToRemove);
            }
            foreach (T unit in unitsToRemove)
            {
                Remove(unit);
            }

            foreach (EchelonGeneric<T> ech in echelonsToRemove)
            {
                ech.parent = null;
            }
            isDirty = true;

            OnPropertyChanged("Count");


        }

        #endregion

        protected string GenerateId(string hint)
        {
            if (hint == null)
            {
                hint = Path.GetRandomFileName();
                hint = hint.Replace(".", "");
            }
            string id = null;
            int cnt = 0;
            while (id == null && cnt < 400)
            {
                id = hint + (cnt++).ToString();
                if (_unitsById.ContainsKey(id)) id = null;
                else return id;
            }
            throw new Exception("Unable to generate Id");
        }

        public event PropertyChangedEventHandler ItemPropertyChanged;


        /// <summary>Triggered whenever the <see cref="Count" /> property changes as a result of adding/removing items.</summary>
        // public event PropertyChangedEventHandler PropertyChanged;

        // private void OnPropertyChanged(string name)
        // {
        //     if (PropertyChanged != null)
        //         PropertyChanged(this, new PropertyChangedEventArgs(name));
        // }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion


        /// <summary>
        ///     Triggered whenever items are added/removed, and also whenever they are reordered due to item property changes.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        private void collectionChanged_Reset()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void collectionChanged_Added(T item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        private void collectionChanged_Removed(T item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        private void collectionChanged_Moved(T item, int oldIndex, int newIndex)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        private void unit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {


            var item = (T)sender;
            int oldIndex = _list.IndexOf(item);

            // Console.WriteLine("unit_PropertyChanged");
            // ItemPropertyChanged(sender, e); 

            if (oldIndex == -1)
            {
                // Log.Debug(this + " ItemPropertyChanged -1 sender:" + item + " property:" + e.PropertyName);
                if (ItemPropertyChanged != null) 
                    ItemPropertyChanged(sender, e);
                isDirty = true;
                return;
            }
            // See if item should now be sorted to a different position
            if (Count <= 1 || (oldIndex == 0 || _comparer.Compare(_list[oldIndex - 1], item) <= 0)
                && (oldIndex == Count - 1 || _comparer.Compare(item, _list[oldIndex + 1]) <= 0))
            {
                // Log.Debug(this + " ItemPropertyChanged sender:" + item + " property:" + e.PropertyName);
                if (ItemPropertyChanged != null)
                    ItemPropertyChanged(sender, e);
                isDirty = true;
                return;
            }
            
            // Find where it should be inserted 
            _list.RemoveAt(oldIndex);
            int newIndex = _list.BinarySearch(item, _comparer);
            if (newIndex < 0)
                newIndex = ~newIndex;
            else
                do newIndex++; while (newIndex < _list.Count && _comparer.Compare(_list[newIndex], item) == 0);

            _list.Insert(newIndex, item);
            Console.WriteLine("collectionChanged_Moved");
            collectionChanged_Moved(item, oldIndex, newIndex);
            isDirty = true;

        }

        private void echelon_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            isDirty = true;
            if (e.PropertyName == "parent")
                unit_PropertyChanged(((R)sender).unit, e);
        }

        public int Add(object value)
        {
            T unit = value as T;
            return Add(unit, (R)unit.echelon);
            
        }

        public void Add(R echelon)
        {
            Add(null, echelon);
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        object System.Collections.IList.this[int index]
        {
            get
            {
                return _list[index];
            }

            set { throw new InvalidOperationException("Cannot set an item at an arbitrary index in a ObservableRoster."); }

        }

        public void CopyTo(Array array, int index)
        {
            //this will probably fail
            ((System.Collections.IList)_list).CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return ((System.Collections.IList)_list).SyncRoot; }
        }


        public void PrettyPrintUnits()
        {
            foreach (T unit in _list)
            {
                Console.WriteLine(unit + " : " + (_unitsById[unit.id] == unit).ToString());

            }
        }
    }

    public class RosterAddException : Exception
    {
        public RosterAddException()
        {
        }

        public RosterAddException(string message)
            : base(message)
        {
        }

        public RosterAddException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}