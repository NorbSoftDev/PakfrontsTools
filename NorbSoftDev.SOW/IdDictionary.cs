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
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class IdDictionary<TValue> :

            ObservableDictionary<string, TValue>

         where TValue : class, INotifyPropertyChanged, IHasId
    {

        public IdDictionary() : base ()
        {
        }

        public IdDictionary(IEqualityComparer<string> iEqualityComparer) : base(iEqualityComparer)
        {
        }

        public void Add(TValue item)
        {
            Add(item.id, item);
        }

        public string GetUniqueId(string requested)
        {
            //if (!_dictionary.ContainsKey(requested))
            //    return requested;

            string id = requested;
            int cnt = 1;
            while (this.ContainsKey(id))
            {
                id = requested + cnt;
                cnt++;
            }
            return id;
        }
    }


}
