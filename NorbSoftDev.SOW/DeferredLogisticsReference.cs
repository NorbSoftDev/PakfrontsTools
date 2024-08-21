using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class DeferredLogisticsReference<T> where T : LogisticsEntry  {
        string _id;
        T _entry;
        Dictionary<string,T> _dictionary;

        public DeferredLogisticsReference(Dictionary<string,T> dictionary, string id ) {
            _id = id;
            _dictionary = dictionary;
        }

        public T entry {
            get {
                if (_entry == null) {
                    _entry = _dictionary[_id];
                }
                return _entry;
            }

            set {
                _id = value.id;
                //Check if in dict?
            }
        }

        public bool isValid {
            get {
                return _dictionary.ContainsKey(_id);
            }
        }

    }    


}