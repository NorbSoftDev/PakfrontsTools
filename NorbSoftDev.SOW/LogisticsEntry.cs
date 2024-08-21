using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {
    public abstract class LogisticsEntry : ICsvValue
    {

        public string id {get; set;}
        
        public string definedIn;

        protected CsvReader _csv;
        protected int _column;
        abstract public void FromCsvLine( Config config,  string definedIn, CsvReader csv);

        public int ToInt32(string str) {
            if (str == String.Empty) return 0;
            int i;
            if (Int32.TryParse(str, out i))
            {
                return i;
            }

            float f;
            if (Single.TryParse(str, out f))
            {
                Log.Error(this, id + " Truncating float '" + str + "' to integer "+definedIn);
                return (int)f;
            }

            Log.Error(this, id + " Unable to cast " + str + " to integer " + definedIn);
            return 0;

        }

        public float ToSingle(string str) {
            if (str == String.Empty) return 0f;
            float f;
            if (Single.TryParse(str, out f))
            {
                return f;
            }

            Log.Error(this, id + " Unable to cast " + str + " to float " + definedIn);
            return 0;
        }


        public bool ToBool(string str) {
            if (str == String.Empty) return false;
            bool b;
            if (Boolean.TryParse(str, out b))
            {
                return b;
            }

            float f;
            if (Single.TryParse(str, out f))
            {
                
                b = f != 0;
                if (f != 1 && f != 0) Log.Error(this, id + " Casting float '" + str + "' to "+b.ToString()+ " " + definedIn);
                return b;
            }

            Log.Error(this, id + " Unable to cast " + str + " to boolean " + definedIn);
            return false;
        }

        SowStr _niceName;
        //public string niceName { get { return _niceName == null ? id : _niceName.value;}}

        public void ResetNiceName(Config config) {
            config.sowstrs.TryGetValue(id, out _niceName);
        }

        public override string ToString()
        {
            return _niceName == null ? id : _niceName.value;
            //return id.ToString()+":"+niceName;
        }

        public string ToCsvValue()
        {
            return id;
        }

        // public T GetValueAllowEmpty<T>(Dictionary<string,T> dict, string key) {
        //     if (key == String.Empty || key == null) {
        //         return default(T);
        //     }
        //     return dict[key]; 
        // }

        // public T GetValueWarnIfMissing<T>(Dictionary<string,T> dict, string key) {
        //     if (key == String.Empty || key == null) {
        //         return default(T);
        //     }
        //     if (! dict.ContainsKey(key)) {
        //         Log.Info(this,"Using default; missing '"+key+"' in "+dict);
        //         return default(T);
        //     }
        //     return dict[key]; 
        // }

    }
}