using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace NorbSoftDev.SOW {

    public class UnitType : LogisticsEntry {
        public enum EUnitType {None=0,Infantry=1,Cavalry=2,Artillery=3,Ordnance=4,Courier=5} 

        public EUnitType type;
        public string aiDll, aiFunc;


    //     // Input
    //     public Globals globals;

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try {
                this.id = csv[i++];
                this.type = (EUnitType) ToInt32(csv[i++]);
                this.aiDll = csv[i++];
                this.aiFunc = csv[i++];

            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this,"Scenario read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }
         }
    
    }
        
}