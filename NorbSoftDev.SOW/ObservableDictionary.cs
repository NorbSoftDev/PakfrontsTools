using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NorbSoftDev.SOW
{
    /// <summary>
    /// http://richardwilburn.wordpress.com/2009/07/13/observable-dictionary/
    /// with modifications to handle null values
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ObservableDictionary<TKey, TValue> :

            IDictionary<TKey, TValue>,

            INotifyCollectionChanged,

            INotifyPropertyChanged
    {

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;



        readonly IDictionary<TKey, TValue> _dictionary; // = new Dictionary<TKey, TValue>();

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IEqualityComparer<TKey> iEqualityComparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(iEqualityComparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {

            return _dictionary.GetEnumerator();

        }



        IEnumerator IEnumerable.GetEnumerator()
        {

            return GetEnumerator();

        }



        public void Add(KeyValuePair<TKey, TValue> item)
        {

            _dictionary.Add(item);



            if (CollectionChanged != null)

                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));



            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                PropertyChanged(this, new PropertyChangedEventArgs("Values"));

            }

        }



        public void Clear()
        {

            int keysCount = _dictionary.Keys.Count;



            _dictionary.Clear();



            if (keysCount == 0) return; //dont trigger changed event if there was no change.



            if (CollectionChanged != null)

                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));



            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                PropertyChanged(this, new PropertyChangedEventArgs("Values"));

            }

        }



        public bool Contains(KeyValuePair<TKey, TValue> item)
        {

            return _dictionary.Contains(item);

        }



        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {

            _dictionary.CopyTo(array, arrayIndex);

        }



        public bool Remove(KeyValuePair<TKey, TValue> item)
        {

            bool remove = _dictionary.Remove(item);



            if (!remove) return false; //don’t trigger change events if there was no change.



            if (CollectionChanged != null)

                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));



            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                PropertyChanged(this, new PropertyChangedEventArgs("Values"));

            }



            return true;

        }



        public int Count
        {

            get { return _dictionary.Count; }

        }



        public bool IsReadOnly
        {

            get { return _dictionary.IsReadOnly; }

        }



        public bool ContainsKey(TKey key)
        {

            return _dictionary.ContainsKey(key);

        }



        public void Add(TKey key, TValue value)
        {

            try
            {
                _dictionary.Add(key, value);
            }
            catch
            {
                Log.Error(this, "Unable to add over existing key " + key + " : " + value);
                throw;
            }


            if (CollectionChanged != null)
            {
                //using Add causes an exception, only likes Reset
                //CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            }



            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                PropertyChanged(this, new PropertyChangedEventArgs("Values"));

            }

        }



        public bool Remove(TKey key)
        {

            bool remove = _dictionary.Remove(key);



            if (!remove) return false;



            if (CollectionChanged != null)

                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));



            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                PropertyChanged(this, new PropertyChangedEventArgs("Values"));

            }



            return true;

        }



        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }



        public TValue this[TKey key]
        {

            get { 
                return _dictionary[key];
            }

            set
            {

                if (_dictionary.ContainsKey(key)) { 
//                bool changed = _dictionary.ContainsKey(key) ? !_dictionary[key].Equals(value) : true;
                //bool changed = _dictionary.ContainsKey(key);
                    TValue old = _dictionary[key];
                    if (old == null) { 
                        if ( value == null) return;
                    }
                    else if (old.Equals(value)) return;
                
                //if (!changed) return; //if there are no changes then we don’t need to update the value or trigger changed events.
                }


                _dictionary[key] = value;



                if (CollectionChanged != null)

                    //CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));



                if (PropertyChanged != null)
                {

                    PropertyChanged(this, new PropertyChangedEventArgs("Keys"));

                    PropertyChanged(this, new PropertyChangedEventArgs("Values"));

                }

            }

        }



        public ICollection<TKey> Keys
        {

            get { return _dictionary.Keys; }

        }



        public ICollection<TValue> Values
        {

            get { return _dictionary.Values; }

        }

    }


}
