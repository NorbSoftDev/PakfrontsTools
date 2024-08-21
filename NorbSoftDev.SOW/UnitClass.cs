using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace NorbSoftDev.SOW {

    public class UnitClass : LogisticsEntry, ICsvLine {

        public UnitType unitType;
        //public string lowResClass;
        public string altClass;
        public string capturedClass;
        public float walkSpeedMilesHr;
        public float midSpeedMilesHr;
        public float runSpeedMilesHr;
        public UnitModel uniform1;
        public UnitModel uniform2;
        public UnitModel uniform3;
        public UnitModel uniform4;
        public UnitModel uniform5;
        public UnitModel uniform6;
        public string walkSound;
        public string standSound;
        public string loadSound;
        public string ramSound;
        public string fireSound;
        public string runSound;
        public string chargeSound;
        public string meleeSound;
        public string proneSound;
        public string flagBearerSprite;
        public string menuDestination;
        public string menuStand;
        public string menuMarch;
        public string menuTargets;
        public Formation fightingFormation;
        public Formation walkFormation;
        public Formation squareFormation;
        public Formation assaultFormation;
        public Formation skirmishFormation;
        public Formation lineFormation;
        public Formation alternativeSkirmishFormation;
        public Formation columnHalfDistanceFormation;
        public Formation columnFullDistanceFormation;
        public Formation specialFormation;

    //     // Input
    //     public Globals globals;


        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            _csv = csv;
            _column = 0;
            try {
                this.id = _csv[_column++];
                definedIn = "["+this.id+"] "+definedIn;
                this.unitType = config.unitTypes[_csv[_column++]];
                //this.lowResClass = _csv[_column++];
                this.altClass = _csv[_column++];
                this.capturedClass = _csv[_column++];
                this.walkSpeedMilesHr = ToSingle(_csv[_column++]);
                this.midSpeedMilesHr = ToSingle(_csv[_column++]);
                this.runSpeedMilesHr = ToSingle(_csv[_column++]);
                this.uniform1 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.uniform2 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.uniform3 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.uniform4 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.uniform5 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.uniform6 = config.unitModels.GetValueWarnIfMissing(_csv[_column++],definedIn);
                this.walkSound = _csv[_column++];
                this.standSound = _csv[_column++];
                this.loadSound = _csv[_column++];
                this.ramSound = _csv[_column++];
                this.fireSound = _csv[_column++];
                this.runSound = _csv[_column++];
                this.chargeSound = _csv[_column++];
                this.meleeSound = _csv[_column++];
                this.proneSound = _csv[_column++];
                this.flagBearerSprite = _csv[_column++];

                this.menuDestination = _csv[_column++];
                this.menuStand = _csv[_column++];
                this.menuMarch = _csv[_column++];
                this.menuTargets = _csv[_column++];

                this.fightingFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.walkFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.squareFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.assaultFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.skirmishFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.lineFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.alternativeSkirmishFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.columnHalfDistanceFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.columnFullDistanceFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);
                this.specialFormation = config.formations.GetValueAllowEmpty<Formation>(_csv[_column++]);

            } catch (Exception e) {
                string[] headers = _csv.GetFieldHeaders();
                Log.Error(this, "Read failed for '"+_csv[0]+"' on column: "+(_column-1)+" '"+headers[_column-1]+"' value:'"+_csv[_column-1]+"'");
                //throw(e);
            }
         }
        // Output
        public string ToCsvLine() {
            return String.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},"
                , id, unitType, altClass, capturedClass, walkSpeedMilesHr, runSpeedMilesHr, uniform1, uniform2, uniform3, uniform4, uniform5, uniform6, walkSound, standSound, loadSound, ramSound, fireSound, runSound, chargeSound, meleeSound, proneSound, flagBearerSprite, menuDestination, menuStand, menuTargets, fightingFormation, walkFormation
            );
        }

    
    }
        
}