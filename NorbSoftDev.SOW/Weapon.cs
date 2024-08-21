using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public class Weapon : LogisticsEntry {
        //this should be abstract, but doesnt seem to work with generics
        public Weapon() : base() {}

        public int min, best, normal, lng, max;
        public float reliability;
        public int rof, mrof;
        public string userName;

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
        }

    }

    public class Rifle : Weapon, ICsvLine  {
        public Ammunition ammoClass; 

        public Rifle() : base() {}

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try { 
                userName = csv[i++];
                id = csv[i++];

                string ammoClassId = csv[i++];
                config.ammunitions.TryGetValue(ammoClassId, out ammoClass);
                if (ammoClass == null) {
                    config.ammunitions[ammoClassId] = new Ammunition(ammoClassId);
                }

                min = ToInt32(csv[i++]);
                best = ToInt32(csv[i++]);
                normal = ToInt32(csv[i++]);
                lng = ToInt32(csv[i++]);
                max = ToInt32(csv[i++]);
                reliability = ToSingle(csv[i++]);
                rof = ToInt32(csv[i++]);
                mrof = ToInt32(csv[i++]);
                ResetNiceName(config);

            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }

         }

        public string ToCsvLine() {
            return String.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}"
                , userName, id, ammoClass, min, best, normal, lng, max, reliability, rof, mrof
                );
        }

        // Output
        // public void ToCsv(string [] columns) {
        //     String.Format(
        //         "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},"
        //         , classId, type, altClass, capturedClass, walkSpeedMilesHr, runSpeedMilesHr, uniform1, uniform2, uniform3, uniform4, uniform5, uniform6, walkSound, standSound, loadSound, ramSound, fireSound, runSound, chargeSound, meleeSound, proneSound, flagBearerSprite, gui, friendlyGUI, enemyGUI, fightingFormation, walkFormation
        //     );
        //  }
    }

    public class Artillery : Weapon, ICsvLine  {


        string artilleryClass;
        int cannisterPct, shellPct, shrapnelPct, solidPct; 
        Bore boreClass;
        Munition cannisterClass, shellClass, shrapnelClass, solidClass;

        public Artillery() : base() {}

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try { 
                userName = csv[i++];
                id = csv[i++];
                artilleryClass = csv[i++];
                min = ToInt32(csv[i++]);
                best = ToInt32(csv[i++]);
                normal = ToInt32(csv[i++]);
                lng = ToInt32(csv[i++]);
                max = ToInt32(csv[i++]);
                reliability = ToSingle(csv[i++]);
                rof = ToInt32(csv[i++]);
                mrof = ToInt32(csv[i++]);

                cannisterPct = ToInt32(csv[i++]);
                shellPct = ToInt32(csv[i++]);
                shrapnelPct = ToInt32(csv[i++]);
                solidPct = ToInt32(csv[i++]);

                string boreClassId = csv[i++];
                config.bores.TryGetValue(boreClassId, out boreClass);
                if (boreClass == null) {
                    config.bores[boreClassId] = new Bore(boreClassId);
                }

                cannisterClass = config.munitions.GetValueAllowEmpty(csv[i++]);

                shellClass = config.munitions.GetValueAllowEmpty(csv[i++]);
                shrapnelClass  = config.munitions.GetValueAllowEmpty(csv[i++]);
                solidClass  = config.munitions.GetValueAllowEmpty(csv[i++]);
                ResetNiceName(config);

            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }
        }


        public string ToCsvLine() {
            return String.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}"
                , userName, id, artilleryClass, min, best, normal, lng, max, reliability, rof, mrof, cannisterPct, shellPct, shrapnelPct, solidPct, boreClass, cannisterClass, shellClass, shrapnelClass, solidClass
                );
        }
    }


    public class Ammunition {
        public string id { get; protected set;}

        public Ammunition(string id) {
            this.id = id;
        }
    }

    public class Bore {
        public string id { get; protected set;}
        public Bore(string id) {
            this.id = id;
        }
    }

    public class Munition: LogisticsEntry, ICsvLine  {
        string spriteGfx;
        bool displaySmokeEffect, displayBurstEffect;
        int min, max;
        float hitPct;
        int maxKill, missMoraleDecrease, gunKill, ammoKill;
        float canisterAngle;
        int canisterShot;
        string explosionSnd;


        public Munition() : base() {}

        public override void FromCsvLine( Config config, string definedIn, CsvReader csv) {
            this.definedIn = definedIn;
            int i = 0;
            try { 
                id = csv[i++];
                spriteGfx = csv[i++];
                displaySmokeEffect = ToBool(csv[i++]);
                displayBurstEffect = ToBool(csv[i++]);
                min = ToInt32(csv[i++]);
                max = ToInt32(csv[i++]);
                hitPct = ToSingle(csv[i++]);
                maxKill = ToInt32(csv[i++]);
                missMoraleDecrease = ToInt32(csv[i++]);
                gunKill = ToInt32(csv[i++]);
                ammoKill = ToInt32(csv[i++]);
                canisterAngle = ToSingle(csv[i++]);
                canisterShot = ToInt32(csv[i++]);
                explosionSnd = csv[i++];
                ResetNiceName(config);


            } catch (Exception e) {
                string[] headers = csv.GetFieldHeaders();
                Log.Info(this," read failed on '"+csv[0]+"'' column: ["+(i-1)+"] '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                throw(e);
            }
        }


        public string ToCsvLine() {
            return String.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}"
                , id, spriteGfx, displaySmokeEffect, displayBurstEffect, min, max, hitPct, maxKill, missMoraleDecrease, gunKill, ammoKill, canisterAngle, canisterShot, explosionSnd
                );
        }
    }


}