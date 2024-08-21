using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


using NorbSoftDev.SOW;
//using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW.Utils
{




  public abstract class DataEntry  {
    // public string id { get; set; }
    public string id;
    // public abstract void ParseCSV(CsvReader csv, Dictionary<string, int> headerLUT, Config config, string where);
    public abstract void ParseCSV(CsvReader csv, Config config);

  }





    public class CsvReader {

        public class Record {
            public int lineNo;
            public string raw;
            public string [] data;

            public Record(string [] data, string raw, int lineNo) {
                this.data = data;
                this.lineNo = lineNo;
                this.raw = raw;
            }
        }

        public string name = "MemoryStream";

        public string [] headers;
        Dictionary<string, int> headerLUT;
        
        List<Record> records = new List<Record>();
        List<NorbSoftDev.SOW.Utils.CsvReader.Record>.Enumerator enumerator;


        public CsvReader(Stream stream, string [] headers) {
            //build LUT for index by name
            name = stream.ToString() + ":" + stream.GetHashCode();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                name = fs.Name;
            }


            this.headers = headers;
            headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerLUT[headers[i]] = i;
            }

            Encoding en = Encoding.GetEncoding(1252, new EncoderReplacementFallback(" "), new DecoderReplacementFallback(" "));
            StreamReader streamReader = new StreamReader(stream, en);

            using (streamReader) {
                string raw;
                int lineNo = 0;
                int nRecords = 0;;

                //skip file headers
                streamReader.ReadLine(); 

                while ((raw = streamReader.ReadLine()) != null)
                {

                    if (raw.Length < 1 || raw[0] == ',') continue;
                    bool found = false;
                    for( int i = 0; i < raw.Length; i++) {
                        if (raw[i] == ',') {
                            found = true;
                            break;
                        }
                    }
                    if ( found) {

                        records.Add( new Record(raw.Split(','), raw, lineNo));
                        nRecords++;
                    }
                    lineNo++;
                }
                Log.Info(this,"Read "+nRecords+" records from "+lineNo+" lines in "+name);
            }

            enumerator = records.GetEnumerator();
        }

        public bool ReadNextRecord() {
            return enumerator.MoveNext();
        }

        // public  string AsString(string header)
        // {
        //     int i = headerLUT[header];
        //     if (i < 0 || i > enumerator.Current.data.Length) {
        //         if (name != null) Log.Warn(this, name);
        //         Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" no column found "+enumerator.Current.raw);
        //         return String.Empty;
        //     }
        //     string str = enumerator.Current.data[i].Trim();
            
        //     return str;
        // }

        public bool TryAsString(string header, ref string value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" no column found "+enumerator.Current.raw);
                return false;
            }
            value = enumerator.Current.data[i].Trim();
            return true;
        }

        public bool TryAsString(string header, Action<string> func)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" no column found "+enumerator.Current.raw);
                return false;
            }
            func(enumerator.Current.data[i].Trim());
            return true;
        }

        // public  string AsASCII(string header)
        // {
        //     int i = headerLUT[header];
        //     if (i < 0 || i > enumerator.Current.data.Length) {
        //         if (name != null) Log.Warn(this, name);
        //         Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" no column found "+enumerator.Current.raw);
        //         return String.Empty;
        //     }
        //     string str = enumerator.Current.data[i].Trim();
            
        //     byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
        //     char[] chars = System.Text.Encoding.ASCII.GetChars(bytes);
        //     str = new String(chars);
        //     str = str.Replace("?", "");

        //     return str;
        // }

        public bool TryAsASCII(string header, ref string value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" no column found "+enumerator.Current.raw);
                return false;
            }
            string str = enumerator.Current.data[i].Trim();
            
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            char[] chars = System.Text.Encoding.ASCII.GetChars(bytes);
            str = new String(chars);
            str = str.Replace("?", "");

            value = str;
            return true;
        }


        // public int AsInt32(string header)
        // {
        //     int i = headerLUT[header];
        //     if (i < 0 || i > enumerator.Current.data.Length) {
        //         if (name != null) Log.Warn(this, name);
        //         Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\"  "+enumerator.Current.raw);                
        //         return 0;
        //     }
        //     string str = enumerator.Current.data[i];

        //     if (str == String.Empty) return 0;
        //     try {
        //         return Convert.ToInt32(str);
        //     } catch (Exception e) {
        //         if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
        //         Log.Error(this, "[AsInt32] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
        //         throw;
        //     }
        // }

        public bool TryAsInt32(string header, ref int? value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\"  "+enumerator.Current.raw);                
                return false;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return false;
            try {
                value = Convert.ToInt32(str);
            } catch (Exception e) {
                if (name != null) Log.Error(this, "Error in "+name);
                Log.Error(this, "[AsInt32] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }
            return true;
        }

        public bool TryAsInt32(string header, ref int value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\"  "+enumerator.Current.raw);                
                return false;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return false;
            try {
                value = Convert.ToInt32(str);
            } catch (Exception e) {
                if (name != null) Log.Error(this, "Error in "+name);
                Log.Error(this, "[AsInt32] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }
            return true;
        }

        public bool TryAsInt32(string header, Action<int> func)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\"  "+enumerator.Current.raw);                
                return false;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return false;
            try {
                func(Convert.ToInt32(str));
            } catch (Exception e) {
                if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
                Log.Error(this, "[AsInt32] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }
            return true;
        }

        // public float AsSingle(string header)
        // {
        //     int i = headerLUT[header];
        //     if (i < 0 || i > enumerator.Current.data.Length) {
        //         if (name != null) Log.Warn(this, name);
        //         Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" "+enumerator.Current.raw);
        //         return 0;
        //     }
        //     string str = enumerator.Current.data[i];

        //     if (str == String.Empty) return 0f;
        //     try {
        //         return Convert.ToSingle(str);
        //     } catch (Exception e) {
        //         if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
        //         Log.Error(this, "[AsSingle] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
        //         throw;
        //     }
        // }

        public bool TryAsSingle(string header, ref float? value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" "+enumerator.Current.raw);
                return false;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return false;
            try {
                value = Convert.ToSingle(str);
            } catch (Exception e) {
                if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
                Log.Error(this, "[AsSingle] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }

            return true;
        }

        public bool TryAsSingle(string header, ref float value)
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsASCII] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" "+enumerator.Current.raw);
                return false;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return false;
            try {
                value = Convert.ToSingle(str);
            } catch (Exception e) {
                if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
                Log.Error(this, "[AsSingle] line "+enumerator.Current.lineNo+" missing column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }

            return true;
        }


        public AttributeLevel AsAttributeLevel(string header, Attribute attribute) 
        {
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" no column "+i+" \""+header+"\" "+enumerator.Current.raw);                
                return null;
            }
            string str = enumerator.Current.data[i];

            if (str == String.Empty) return null;
            try {
                int index = Convert.ToInt32(str);
                if (index >= attribute.Count) {
                    if (name != null) Log.Warn(this, name);
                    Log.Warn(this, "[AsAttributeLevel] index out of range "+index+" >= "+attribute.Count);
                    Log.Warn(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                    return attribute[attribute.Count - 1];
                }
                return attribute[index];

            } catch (Exception e) {
                if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
                Log.Error(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                throw;
            }
        }

        public bool TryAsAttributeLevel(string header, Attribute attribute, ref AttributeLevel value) 
        {
            Console.WriteLine("TryAsAttributeLevel "+header);
            int i = headerLUT[header];
            if (i < 0 || i > enumerator.Current.data.Length) {
                if (name != null) Log.Warn(this, name);
                Log.Error(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" no column "+i+" \""+header+"\" "+enumerator.Current.raw);                
                return false;
            }
            string str = enumerator.Current.data[i];
            Console.WriteLine("TryAsAttributeLevel "+header+":"+str);

            if (str == String.Empty) {
                Log.Error(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" empty "+header);
                return false;
            }

            try {
                int index = Convert.ToInt32(str);
                if (index >= attribute.Count) {
                    if (name != null) Log.Warn(this, name);
                    Log.Warn(this, "[AsAttributeLevel] index out of range "+index+" >= "+attribute.Count);
                    Log.Warn(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                    return false;
                }
                value = attribute[index];
                return true;

            } catch (Exception e) {
                if (name != null) Log.Error(this, "\""+e.Message+"\" in "+name);
                Log.Error(this, "[AsAttributeLevel] line "+enumerator.Current.lineNo+" column "+i+" \""+header+"\" value \""+str+"\" : "+enumerator.Current.raw);
                return false;
            }
        }      

    }

      public class DataTable<T> where T : DataEntry, new() {
        public string name;
        public Config config;

        public DataTable(Config config) {
          this.config = config;
        }

        public Dictionary<string,T> table = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase); 
  
        public void ReadFromCsv(string filepath, string [] headers)
        {
            name = filepath;
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ReadFromCsv(stream, headers);
            stream.Close();
        }


        public void ReadFromCsv(Stream stream, string[] headers)
        {

            int count = 0;

            CsvReader csv = new CsvReader(stream, headers);

            while (csv.ReadNextRecord())
            {

                string id = null;

                if ( !csv.TryAsString("id", ref id) ) {
                    continue;
                }



                T result;

                if (!table.TryGetValue(id, out result))
                {
                    result = new T();
                    table[id] = result;
                }

                result.ParseCSV(csv, this.config);
                count++;
                

            }

            Log.Info(this, "Read " + count + " entries from "+csv.name);
        }


        public void PrettyPrint() {
          foreach (KeyValuePair<string, T> kvp in table) {
            Console.WriteLine(kvp.Key+":"+kvp.ToString());
          }
        }
      }
}