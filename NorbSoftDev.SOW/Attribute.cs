using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class Attribute : List<AttributeLevel> {

        public string definedIn;
        public string name { get; set;}
        public string label { get; set;}
        public string help { get; set;}

        public Attribute() : base() {       
        }

        public void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            // int i = 0;
            // try { 
            //     userName = csv[i++];
            //     id = csv[i++];

            // } catch (Exception e) {
            //     string[] headers = csv.GetFieldHeaders();
            //     Log.Info(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
            //     throw(e);
            // }

         }

        // public override string ToCsvLine() {
        //     return "FORMATION NOT IMPLEMENTED";
        // }
    }

    public class AttributeLevel : ICsvValue
    {

        public AttributeLevel(Attribute attribute) {
            this.attribute = attribute;
        }

        public void FromCsvLine( Config config,  string definedIn, CsvReader csv) {
            index = Convert.ToInt32(csv[0]);
            value = Convert.ToInt32(csv[1]);
            id = csv[2];
            this.definedIn = definedIn; 
            ResetNiceName(config);
        }

        public string definedIn { get; protected set;}


        public Attribute attribute {get; protected set;}
        public int index {get;  protected set;} 
        public int value {get;  protected set;}
        public string id { get; protected set;}

        SowStr _niceName;
        //public string niceName { get { return _niceName == null ? id : _niceName.value;}}
        /// <summary>
        /// Lookup nice name in locale based string tables
        /// </summary>
        /// <param name="config"></param>
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
            return index.ToString();
        }

    }    


}