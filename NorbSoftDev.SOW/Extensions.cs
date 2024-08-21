using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using LumenWorks.Framework.IO.Csv;
using System.Windows;


namespace NorbSoftDev.SOW
{
    public static class Extensions
    {

        public static double east(this Point p)
        {
            return p.X;
        }

        public static double south(this Point p)
        {
            return p.Y;
        }

        public static string ToASCII(this string str)
        {
            
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            char[] chars = System.Text.Encoding.ASCII.GetChars(bytes);
            string line = new String(chars);
            line = line.Replace("?", "");

            return line;
        }

        public static int ToInt32(this string str)
        {
            if (str == String.Empty) return 0;
            try {
                return Convert.ToInt32(str);
            } catch (Exception e) {
                Log.Error(str,"[ToInt32] Unable to convert value \""+str+"\"");
                throw;
            }
        }

        public static float ToSingle(this string str)
        {
            if (str == String.Empty) return 0f;
            try {
                return Convert.ToSingle(str);
            } catch (Exception e) {
                Log.Error(str,"[ToSingle] Unable to convert value "+str);
                throw;
            }
        }

        public static int ToInt32(this CsvReader csv, Dictionary<string, int> lut, string header, string where) {
            try {
				return ToInt32 (csv[lut[header]]);//csv[lut[header]].ToInt32();
            } catch (System.Collections.Generic.KeyNotFoundException e) {
                Log.Error(csv,"[ToInt32] Unable find column with header \""+header+"\""); 
                throw;               
            } catch (Exception e) {
                Log.Error(csv,"[ToInt32] Unable to convert value "+csv[lut[header]]+" of header \""+header+"\" index:" + lut[header]+" "+where);
                throw;
            }
        }

        public static float ToSingle(this CsvReader csv, Dictionary<string, int> lut, string header, string where)
        {
            try {
				return ToSingle (csv[lut[header]]);
//                return csv[lut[header]].ToSingle();
            } catch (System.Collections.Generic.KeyNotFoundException e) {
                Log.Error(csv,"[ToSingle] Unable find column with header \""+header+"\""); 
                throw;               
            } catch (Exception e) {
                Log.Error(csv, "[ToSingle] Unable to convert value \"" + csv[lut[header]] + "\" of header \"" + header + "\" index:" + lut[header] + " " + where);
                throw;
            }
        }


        public static string ToString(this CsvReader csv, Dictionary<string, int> lut, string header, string where)
        {
            int index;
            if (!lut.TryGetValue(header, out index))
            {
                Log.Error(csv, "[ToString] Unable find column with header \"" + header + "\" " + where); 
                throw new Exception();
            }

            if (index >= csv.FieldCount)
            {
                Log.Error(csv, "[ToString] Header \"" + header + "\" has out of range index "+index +" "+where);
                throw new Exception();
            }

            try {
                return csv[index];           
            } catch (Exception e) {
                Log.Error(csv, "[ToString] Unable to convert value of header \"" + header + " " + index +" in " + where);
                throw;
            }
        }

        public static T GetValueAllowEmpty<T>(this Dictionary<string, T> dict, CsvReader csv, Dictionary<string,int> headerLUT, string header, string where)
        {

            string key = csv[headerLUT[header]];

            if (key == String.Empty || key == null)
            {
                return default(T);
            }

            T value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            Log.Error(dict, "[GetValue] Unable find \"" + typeof(T) + "\" \"" + key + "\" in " + where);
            return default(T);
        }

        public static T GetValueAllowEmpty<T>(this Dictionary<string, T> dict, string key)
        {
            if (key == String.Empty || key == null)
            {
                return default(T);
            }

            T value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }
            Log.Error(dict, "[GetValue] Unable find \"" + typeof(T) + "\" \"" + key + "\" in " + dict);
            return default(T);
            


            //try
            //{
            //    return dict[key];
            //}
            //catch (System.Collections.Generic.KeyNotFoundException e)
            //{
            //    Log.Error(dict, "[GetValue] Unable find \""+ typeof(T)+ "\" \"" + key + "\" in " +dict);
            //    throw e;
                
            //}
        }

        public static T GetValueWarnIfMissing<T>(this Dictionary<string, T> dict, string key, string definedIn)
        {
            if (key == String.Empty || key == null)
            {
                return default(T);
            }
            if (!dict.ContainsKey(key))
            {
                Log.Warn(typeof(T), "Missing key: '" + key + "' used by " + definedIn);
                return default(T);
            }
            return dict[key];
        }

        public static T GetValue<T>(this CsvReader csv, Dictionary<string, int> lut, Dictionary<string, T> dict, string header)       
        {


            string key;
            try {
                key = csv[lut[header]];
             } catch (System.Collections.Generic.KeyNotFoundException e) {
                Log.Error(csv,"[GetValue] Unable find header \""+header+"\""); 
                throw;      
            }
            try {
                return dict[key];
            } catch (System.Collections.Generic.KeyNotFoundException e) {
                Log.Error(csv,"[GetValue] Unable find key \""+key+"\" in "+dict); 
                throw;               
            } catch (Exception e) {
                Log.Error(csv,"[GetValue] Unable to convert value of header \""+header+"\" index:" + lut[header]);
                throw;
            }
        }

        public static T GetValueAllowEmpty<T>(this CsvReader csv, Dictionary<string, int> lut, Dictionary<string, T> dict, string header)       
        {
            string key;
            try {
                key = csv[lut[header]];
             } catch (System.Collections.Generic.KeyNotFoundException e) {
                 Log.Error(csv,"[GetValueAllowEmpty] Unable find header \"" + header + "\""); 
                throw;      
            }
            
           try {
                return GetValueAllowEmpty<T>(dict, key);
            } catch (Exception e) {
                Log.Error(csv,"[GetValueAllowEmpty] Unable to convert "+header+" key \""+key+"\" in "+dict);
                throw;
            }
        }

        public static T GetValueWarnIfMissing<T>(this CsvReader csv, Dictionary<string, int> lut, Dictionary<string, T> dict, string header)       
        {
            string key;
            try {
                key = csv[lut[header]];
             } catch (System.Collections.Generic.KeyNotFoundException e) {
                Log.Error(csv,"[GetValueWarnIfMissing] Unable find header \""+header+"\""); 
                throw;      
            }

            if (!dict.ContainsKey(key))
            {
                Log.Warn(csv,"[GetValueWarnIfMissing] Using default; missing header '" + header + " with '" + key + "' in " + dict);
                return default(T);
            }
            return dict[key];
        }

        public static string CamelCase(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            if (s.Count() == 1) return s.ToUpper();
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        //public class MissingLogisticsEntryException<T> : Exception where T : LogisticsEntry
        //{
        //    //public MissingDataException()
        //    //{
        //    //    // Add implementation.
        //    //}
        //    //public MissingDataException(string message)
        //    //{
        //    //    // Add implementation.
        //    //}
        //    //public MissingDataException(string message, Exception inner)
        //    //{
        //    //    // Add implementation.
        //    //}

        //    public MissingLogisticsEntryException(Dictionary<string, T> dict, string header, string key, Exception inner) 
        //    {
        //        base();
        //    }


        //}

        // http://stackoverflow.com/a/16173000
        public static bool IsSame(this DirectoryInfo that, DirectoryInfo other)
        {
            // zip extension wouldn't work here because it truncates the longer 
            // enumerable, resulting in false positive

            var e1 = EnumeratePathDirectories(that).GetEnumerator();
            var e2 = EnumeratePathDirectories(other).GetEnumerator();

            while (true)
            {
                var m1 = e1.MoveNext();
                var m2 = e2.MoveNext();
                if (m1 != m2) return false; // not same length
                if (!m1) return true; // finished enumerating with no differences found

                if (!e1.Current.Name.Trim().Equals(e2.Current.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    return false; // current folder in paths differ
            }
        }

        public static IEnumerable<DirectoryInfo> EnumeratePathDirectories(this DirectoryInfo di)
        {
            var stack = new Stack<DirectoryInfo>();

            DirectoryInfo current = di;

            while (current != null)
            {
                stack.Push(current);
                current = current.Parent;
            }

            return stack;
        }

        public static bool IsSame(this FileInfo that, FileInfo other)
        {
            return that.Name.Trim().Equals(other.Name.Trim(), StringComparison.InvariantCultureIgnoreCase) &&
                   IsSame(that.Directory, other.Directory);
        }

        public static IEnumerable<DirectoryInfo> EnumeratePathDirectories(this FileInfo fi)
        {
			return EnumeratePathDirectories(fi.Directory);
        }


    }
}