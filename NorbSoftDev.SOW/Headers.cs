using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NorbSoftDev.SOW
{


    public class Headers : Dictionary<string, string[]>
    {


        //public static void Main()
        //{
        //    Headers h = new Headers();
        //    Console.WriteLine(String.Join(":", h["scenario"]));

        //    h.SetHeadersFromDir("Headers/");

        //    foreach (KeyValuePair<string, string[]> n in h)
        //    {
        //        Console.WriteLine(n.Key + " " + String.Join(",", n.Value));
        //    }

        //}

        public string[] scenario
        {
            get { return this["scenario"]; }
        }

        public string[] sandbox
        {
            get { return this["sandbox"]; }
        }

        public string[] oob
        {
            get { return this["oob"]; }
        }

        public string[] startLocs
        {
            get { return this["startLocs"]; }
        }

        public string[] gameDB
        {
            get { return this["gameDB"]; }
        }

        public string[] battlescript { get { return this["battlescript"]; } }

        public Headers()
            : base(StringComparer.OrdinalIgnoreCase)
        {


            this["oob"] = new string[] {
                "userName", "id", "name1", "name2", 
                "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex",
                "unitClass", "portrait", "weapon", "ammo", "flag", "flag2", "formation", "headCount",
                "initiative", "leadership", "loyalty", "ability", "style", "experience",
                "fatigue", "morale", 
                "IDS_Close_Order_Proficiency", "IDS_Open_Order_Proficiency", "IDS_Edged_Weapon_Proficiency", 
                "IDS_Firearm_Proficiency", "IDS_Marksmanship", "IDS_Horsemanship", "IDS_Surgeon_Ability", "IDS_Calisthenics"
            };

            this["scenario"] = new string[] {
            "userName", "id",
            "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", "battalionIndex",
            "ammo",
            "dirSouth", "dirEast", "south", "east",
            "formation", "headCount",
            "fatigue", "morale"
            };

            this["sandbox"] = new string[] {
            "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", "battalionIndex",
            "id", "name1", "name2", 
            "unitClass",
            "portrait", "weapon", "ammo", 
            "dirSouth", "dirEast", "south", "east",
            "flag", "flag2", "formation", "headCount",
               
            "ability", "initiative", "loyalty", "leadership",   "style", "experience", // Ability,Command,Control,Leadership,Style,Experience,
            "fatigue", "morale",
            "IDS_Close_Order_Proficiency", "IDS_Open_Order_Proficiency", "IDS_Edged_Weapon_Proficiency", 
            "IDS_Firearm_Proficiency", "IDS_Marksmanship", "IDS_Horsemanship", "IDS_Surgeon_Ability", "IDS_Calisthenics"
            };

            // dumped by l key in game
            this["startLocs"] = new string[] {
            "id", "ammo", "dirSouth", "dirEast", "south", "east",  "formation", "headCount", "fatigue", "morale"
            };

            // dumped by k key in game
            this["gameDB"] = new string[] {
            "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", // "battalionIndex",
            "id",
            "ammo", "status",
            "deserted", "killed", "wounded",
            "r1", "r2", "r3", "r4", "r5", "r6",
            "killerName", "casualties"
            };


            this["battlescript"] = new string[] {
                "trigger", "id", "command", "fromid", "x", "y", "timevar", "description"
            };

            this["maplocations"] = new string[] {
                "name", "id", "priority", "type", "ai", "AsCsv2()", "radius", "men", "points", "fatigue", "morale", "ammo", "occMod", "beg", "end", "interval", "sprite", "army1", "army2", "army3"
            };

        }

        public string[] HeadersInFile(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string firstLine = reader.ReadLine() ?? "";
                if (firstLine == null || firstLine == string.Empty) return null;

                string[] headers = firstLine.Split(',').Select(n => n.Trim()).ToArray();
                return headers;
            }
        }

        public void SetHeadersFromFile(string key, string file)
        {
            if (this.ContainsKey(key))
            {
                Log.Info(this, "Overwriting Headers " + key + " from " + file);
            }
            else
            {
                Log.Info(this, "Creating Headers " + key + " from " + file);
            }
            this[key] = HeadersInFile(file);
        }

        public bool TrySetHeadersFromDir(string dirpath)
        {
            if (!Directory.Exists(dirpath))
            {
                Log.Info(this, "No headers found in " + dirpath);
                return false;
            }
            foreach (string f in Directory.GetFiles(dirpath, "*.csv"))
            {
                string key = Path.GetFileNameWithoutExtension(f);
                SetHeadersFromFile(key, f);
            }
            return true;
        }

        public void WriteHeadersToDir(string dirpath)
        {
            foreach (KeyValuePair<string,string[]> kvp in this)
            {
                string filepath = Path.Combine(dirpath,kvp.Key+".csv");
                Log.Info(this, "Writing "+filepath);
                System.IO.File.WriteAllText(filepath, string.Join(",",kvp.Value)+Environment.NewLine, Config.TextFileEncoding );
            }
        }




    }
}
