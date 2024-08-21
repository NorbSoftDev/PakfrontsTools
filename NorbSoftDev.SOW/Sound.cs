using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class Sound : LogisticsEntry {
        //this should be abstract, but doesnt seem to work with generics
        public Sound() : base() {}

        public string file;

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 1;
            try { 
                id = csv[i++];
                file = csv[i++];
                ResetNiceName(config);
            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }

         }

        // public override string ToCsvLine() {
        // }
    }    


}