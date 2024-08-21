using System;
using System.Collections;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;

namespace NorbSoftDev.SOW
{

    public interface ISOWFile
    {
        Config config { get; }
        //public string dirpath;
        Mod mod { get; }

        string name { get; set; }

        bool isDirty { get; }
        bool CanSafelySave();
        // void PreLoad();
        // void Load();
        void Save();

        void PrettyPrint();
    }

    // public interface IUnitCollection : IEnumerable<IUnit> {
    //     IUnit this[string i] {get;}
    //     Echelon this[int i] { get;}
    // }



    // public abstract class UnitRoster<T, R> : ObservableCollection<T>, ISOWFile
    //     where T : class, IUnit, INotifyPropertyChanged, IComparable<T>
    //     where R : EchelonGeneric<T>, new()
    public abstract class UnitRoster<T, R> : ObservableRoster<T, R>, ISOWFile, INotifyPropertyChanged
        where T : class, IUnit, INotifyPropertyChanged
        where R : EchelonGeneric<T>
    {

        public Dictionary<string, int> csvHeaderLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public List<Attribute> attributeNames = new List<Attribute>();
        //public List<Attribute> attributes
        //{
        //    get
        //    {
        //        return new List<Attribute>(config.attributes.Values);
        //    }
        //}

        //protected Dictionary<string, T> _unitsById = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        // R _root;
        // public R root { 
        //     get { return _root;}
        //     protected set
        //     {
        //        if (_root != null) _root.CollectionChanged -= HandleRootChanged;
        //         _root = value;
        //        _root.CollectionChanged += HandleRootChanged;
        //     }
        // }

        public Config config { get; protected set; }

        public Mod mod { 
            get { return _mod; }
            set { _mod = value;
            OnPropertyChanged("mod");
            isDirty = true;
            }
            }
        Mod _mod;

       
        public string name {         
            get { return _name; }
            set { 
                _name = value;
                OnPropertyChanged("name");
                isDirty = true;

            }
        }
        string _name;

        UnitRoster() : base() {}


        public UnitRoster(Config config, Mod mod, string name) : this()
        {
            this.config = config;
            this.mod = mod;
            this.name = name;
        }


        // public abstract void PreLoad();
        // public abstract void Load();
        public abstract void Save();



        //#region Dictlike
        //public bool Contains(string id)
        //{
        //    return _unitsById.ContainsKey(id);
        //}

        //public T this[string i]
        //{
        //    get
        //    {
        //        T iunit = default(T);
        //        if (_unitsById.TryGetValue(i, out iunit)) return iunit;
        //        Log.Info(this,mod + " " + name + "] Unable to find unit id '" + i + "'");
        //        foreach (string k in _unitsById.Keys.OrderBy(k => k))
        //        {
        //            Log.Info(k);
        //        }
        //        throw new System.Collections.Generic.KeyNotFoundException(i);
        //    }
        //}
        //#endregion

        #region Save

        public abstract bool CanSafelySave();

        public string UnitCsvLine(T unit, string [] headers)
        {

            string line = "";
            foreach (string header in headers)
            {
                AttributeLevel level = null;
                if (unit.TryGetAttributeLevel(header, out level))
                {
                    if (level == null)
                    {
                        line += ",";
                        continue;
                    }
                    line += level.ToCsvValue() + ",";
                    continue;
                }

    

                PropertyInfo propertyInfo = unit.GetType().GetProperty(header);
                Object obj = unit;
                if (propertyInfo == null)
                {
                    propertyInfo = unit.echelon.GetType().GetProperty(header);
                    obj = unit.echelon;
                }
                if (propertyInfo == null && unit.transform != null)
                {
                    propertyInfo = unit.transform.GetType().GetProperty(header);
                    obj = unit.transform;
                }
                if (propertyInfo == null)
                {
                    //Log.Error(this, "UnitCsvLine: Unable to find property \"" + header + "\"");
                    line += "MISSING:" + header + ",";
                    continue;
                }

                object primVal = propertyInfo.GetValue(obj, null);
                ICsvValue csvValue = primVal as ICsvValue;

                if (primVal == null) {
                    //Log.Warn(this,"Writing null value to "+unit.id+" "+header);
                    line += ",";                    
                }
                else if (csvValue != null)
                {
                    line += csvValue.ToCsvValue() + ",";
                }
                else
                {
                    line += primVal.ToString() + ",";
                }
            }
            line = line.Substring(0, line.Length - 1);
            return line;
        }

        #endregion
        public void PrettyPrint()
        {
            Console.WriteLine(this + " Units:" + _list.Count + " " + "Ids:" + _unitsById.Count + " Echelons:" + root.DescendantsCount());
            root.PrettyPrint();
        }

        public override string ToString()
        {
            return base.ToString() + ":" + mod + ":" + name;
        }

        // #region INotifyPropertyChanged
        // public event PropertyChangedEventHandler PropertyChanged;

        // // Create the OnPropertyChanged method to raise the event 
        // protected void OnPropertyChanged(string name)
        // {
        //     PropertyChangedEventHandler handler = PropertyChanged;
        //     if (handler != null)
        //     {
        //         handler(this, new PropertyChangedEventArgs(name));
        //     }
        // }


        // protected void OnPropertyChanged(PropertyChangedEventArgs e)
        // {
        //     PropertyChangedEventHandler handler = PropertyChanged;
        //     if (handler != null)
        //     {
        //         handler(this, e);
        //     }
        // }
        // #endregion

        public abstract void ExportAsOOB(string filename);

        #region StaticUtility
        public static string[] GetHeadersOfFile(string filepath)
        {
            using (CsvReader csv = new CsvReader(new StreamReader(filepath, Config.TextFileEncoding), true))
            {
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                int fieldCount = csv.FieldCount;
                return csv.GetFieldHeaders();
            }
        }
        #endregion

    }


}
