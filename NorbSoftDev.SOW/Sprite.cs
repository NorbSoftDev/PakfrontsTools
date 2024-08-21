using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class Sprite : LogisticsEntryBitmap {
        //this should be abstract, but doesnt seem to work with generics
        public Sprite() : base() {}

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
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
    }    



}