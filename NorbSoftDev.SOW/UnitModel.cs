using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace NorbSoftDev.SOW {

    public class UnitModel : LogisticsEntry {

        DeferredLogisticsReference<UnitModel> _lowRes;
        public UnitModel lowRes { 
            get {
                return _lowRes.entry;
            }
            set {
                _lowRes.entry = value;
            }
        }
        


        public Sprite walk, stand, load, ready, fire, run, charge, melee, prone, death, death_2;

    //     // Input
    //     public Globals globals;

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try {
                this.id = csv[i++];
                definedIn = "[" + this.id + "] " + definedIn;

                this._lowRes = new DeferredLogisticsReference<UnitModel>(config.unitModels,csv[i++]);

                walk = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                stand = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                load = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                ready = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                fire = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                run = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                charge = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                melee = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                prone = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                death = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
                death_2 = config.sprites.GetValueWarnIfMissing(csv[i++], definedIn);
            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this,"Scenario read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }
         }
    
    }
        
}