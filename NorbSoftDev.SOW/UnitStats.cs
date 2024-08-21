// using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;


using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW
{

    //public interface IUnit
    //{
    //    string id { get; }
    //    int ammo { get; }
    //    int headCount { get; }
    //}

    //public class ScenarioUnit : IUnit
    //{
    //    public string id { get; set; }
    //    public int ammo { get; set; }
    //    public int headCount { get; set; }

    //}

    public abstract class UnitStats : INotifyPropertyChanged
    {

        public abstract IUnit iunit { get; set; }
        public abstract void ApplyResults();


        private bool _active = true;
        public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged(""); //force everyting to update
            }
        }

        public UnitStats () : base() {}

        public abstract void ParseCSV(CsvReader csv, Dictionary<string, int> headerLUT, Config config,string where);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void all_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
        }
    }

    public abstract class UnitStatsGeneric<T, R, U> : UnitStats
        where T : IUnit
        where R : UnitStats
        where U : UnitStatsRoster
    {

        U _roster;
        public  U roster
        {
            get { return _roster; }

            set
            {

                if (_roster == value) return;
                if (_roster != null) _roster.PropertyChanged -= all_PropertyChanged;
                if (value != null) value.PropertyChanged += all_PropertyChanged;
                _roster = value;
                OnPropertyChanged("");
            }
        }

        public abstract R parent
        {
            get;
            set;
        }

        private T _unit;
        public T unit
        {
            get { return _unit; }
            set
            {
                _unit = value;
                OnPropertyChanged("unit");
            }
        }

        public override IUnit iunit
        {
            get { return this.unit; }
            set { this.unit = (T)value; }
        }

        public UnitStatsGeneric () : base() {}

    }

    public abstract class UnitStatsModifier {
        public UnitStatsModifier () : base() {}
    }

    public abstract class UnitStatsModifier<T> : UnitStatsModifier, INotifyPropertyChanged where T : UnitStats
    {
        public UnitStatsModifier () : base() {}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

    }

    public abstract class UnitStatsRoster : INotifyPropertyChanged
    {
        public string name;
        //public abstract UnitStatsModifier rootModifier
        //{
        //    get;
        //}

        public Config config
        {
            get;
            set;
        }

        public UnitStatsRoster(Config config)
        {
            this.config = config;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }

    public abstract class UnitStatsRoster<T, R, U> : UnitStatsRoster where T : UnitStats, new() where R : IUnit where U : IEchelon
    {
        protected Dictionary<R, T> unitStatsLUT;


        public List<UnitStats> unitStats;

        public UnitStatsRoster(Config config) : base(config)
        { 
            unitStats = new List<UnitStats>();
            unitStatsLUT = new Dictionary<R, T>();
        }

        public void ReadUnitDataFromCsv(string filepath, string [] headers, IList<R> units)
        {
            name = filepath;
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ReadUnitDataFromCsv(stream, headers, units);
            stream.Close();
        }


        public void ReadUnitDataFromCsv(Stream stream, string[] headers, IList<R> units)
        {
            string streamName = stream.ToString() + ":" + stream.GetHashCode();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }
            Log.Info(this, "ReadUnitDataFromCsv \"" + streamName + "\"");

            int count = 0;

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerLUT[headers[i]] = i;
            }


            Dictionary<string, R> unitTable = new Dictionary<string, R>();

            foreach (R unit in units)
            {
                unitTable[unit.id] = unit;
            }

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
            );

            while (csv.ReadNextRecord())
            {

                if (csv[0] == String.Empty)
                {
                    continue;
                }

                try
                {
                    if (csv[1] == null || csv[1] == string.Empty)
                    {
                        continue;
                    }
                }
                catch (System.Resources.MissingManifestResourceException e)
                {
                    //raised by null or bad fields
                    continue;
                }

                try
                {

                    R iunit;
                    string unitkey;

                    if (headerLUT.ContainsKey("id"))
                    {
                        unitkey = csv[headerLUT["id"]];
                    }
                    else
                    {
                        Log.Error(this, "CSV does not contain id field: " + streamName);
                        throw new Exception("CSV does not contain ids field: " + streamName);
                    }

                    if (!unitTable.TryGetValue(unitkey, out iunit))
                    {
                        Log.Warn(this, "Could not find unit \"" + unitkey + "\" in \"" + this.name + "\"");
                        continue;
                    }

                    T result;

                    if (!unitStatsLUT.TryGetValue(iunit, out result))
                    {
                        result = new T();
                        result.iunit = iunit;
                        unitStatsLUT[iunit] = result;
                        unitStats.Add(result);
                    }

                    result.ParseCSV(csv, headerLUT, this.config, this.name);
                    count++;
                }
                catch (Exception e)
                {
                    string summary = "headerLUT: ";
                    foreach (KeyValuePair<string, int> kvp in headerLUT)
                    {
                        summary += " " + kvp.Key + ":" + kvp.Value;
                    }
                    Log.Error(this, summary);
                    Log.Error(this, "ReadUnitDataFromCsv failed on entry " + count + " '" + streamName + "'");
                    // Log.Error(this,"OrderOfBattle CreateOrgFromCsv failed on entry "+count+" id:'"+sunit.id+"' '"+streamName+"'");
                    throw e;
                }
            }

            Reorg(units);

            Log.Info(this, "Read " + count + " units from CSV");
            stream.Position = 0;


        }

        protected abstract void Reorg(IList<R> units);
    }
}
