using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {
	public class Map : ISOWFile {
		// @"Scourge of War - Gettysburg\Mods\GcmRandomMaps4\MapsRandomMaps4L_222.csv";
        public Config config { get; protected set; }
        //public string dirpath;
        public Mod mod { get; protected set; }
        public string name { get; set; }

        public ObservableDictionary<string, MapObjective> objectives = new ObservableDictionary<string, MapObjective>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Fort> forts = new Dictionary<string,Fort>(StringComparer.OrdinalIgnoreCase);

        public float extent = 4069 * 64;//in unit map location units;
        public float unitPerYard = 30;
        string csvFileName, grayscaleFileName, miniMapFileName;
        string csvFilePath;

        public Dictionary<string, Terrain> terrains = new Dictionary<string, Terrain>(StringComparer.OrdinalIgnoreCase);
        public Terrain[] grayscales = new Terrain[256];

        public string grayscaleFilePath
        {
            get;
            protected set;
        }
        public string niceName;

        public bool isDirty { get; protected set;}



        internal class ReadState {
            internal Fort currentFort;
        }

		delegate LineReadMode LineReadMode(CsvReader csv, ReadState readState);


        public Map(Config config, Mod mod, string name) {
            this.config = config;
            this.mod = mod;
            this.name = name;
        }

        public Terrain GetTerrainAtIndex(int index) {
            

            while (index > 0)
            {
                index--;
                if (grayscales[index] != null) return grayscales[index];
            }

            return null;

        }

        public void PrettyPrint() {
            Console.WriteLine("Map:"+name);
        }

        #region PreLoad
        public void PreLoad() {
            PreLoadIni(Path.Combine(mod.mapDir.FullName,name+".ini") );
        }

        public void PreLoadIni(string filepath) {
            IniReader iniReader = new IniReader(filepath);
            csvFileName = iniReader.GetValue("csvfile","files");
            grayscaleFileName = iniReader.GetValue("grayscale","files");
            miniMapFileName = iniReader.GetValue("minimap","files");
            niceName = iniReader.GetValue("mapname","about");
            unitPerYard = Convert.ToInt32( iniReader.GetValue("unitperyard", "settings") );
        }
        #endregion

        public void Load() {
            PreLoad();
            Log.Info(this,"Loading "+mod+" "+name);
            string csvSearchPath = Path.Combine("Maps",csvFileName);
            csvFilePath = config.FindFileInDir( mod.orderOfBattleDir.FullName, csvFileName );

            if (csvFilePath == null) csvFilePath = config.FindFileInMods(csvSearchPath);           
            // if (csvFilePath == null) csvFilePath = globals.FindFileInBase(csvSearchPath);           
            // if (csvFilePath == null) csvFilePath = globals.FindFileInSDK(csvSearchPath);

            if (csvFilePath == null) {
                Log.Info(this,"Unable to find "+csvFileName); 
                return;
            }

            Log.Info(this,"Loading " + csvFilePath);
            ReadCsv(csvFilePath);

            foreach (string ext in new [] {"bmp", "png", "jpg"}) {
                grayscaleFileName = Path.ChangeExtension(grayscaleFileName, ext);
                Console.WriteLine(grayscaleFileName);
                grayscaleFilePath = config.FindFileInMods(Path.Combine("Maps", grayscaleFileName));
                //if (grayscaleFilePath == null)
                //{
                //    grayscaleFilePath = Path.Combine( Path.Combine(config.baseMod.directory.FullName, "Maps"), grayscaleFileName);
                //    if (!File.Exists(grayscaleFilePath)) grayscaleFilePath = null;
                //}
                if (grayscaleFilePath != null) break;
            }

            if (grayscaleFilePath == null)
            {
                Log.Warn(this,"Unable to find terrain bitmap as bmp, png, jpg" + grayscaleFilePath);
            }   
        }


#region Save
        public bool CanSafelySave() {
            return false;
        }
#endregion
        // public static Map Load(Globals globals, string name) {
        //     // map.globals = globals;
        //     // map.name = name;
        //     // map.dirpath = dirpath;
        //     Map map  = PreLoad(globals, name) {
        //     map.Load();
        //     return map;
        // }

        // public static List<Map> Enumerate(Globals globals) {
        //     List<string> filepaths = globals.EnumerateFilesInMods("Maps", "*.ini" );
        //     filepaths.AddRange(globals.EnumerateFilesInSDK("Maps", "*.ini")); 

        //     List<Map> maps = new List<Map>();
        // 	foreach (string filepath in filepaths) {
        //         Map map = Map.PreLoad(globals, filepath);
        //         maps.Add(map);
        //     }
        // 	return maps;
        // }




		public void ReadCsv(string filepath)
		{
            LineReadMode lrm = ReadTerrainLine;
            ReadState readState = new ReadState();

			using (CsvReader csv = new CsvReader(new StreamReader(filepath, Config.TextFileEncoding), true)) {
				string[] headers = csv.GetFieldHeaders();
				while (csv.ReadNextRecord()) {

                    if (csv[0] == String.Empty) continue;

                    if ( String.Equals(csv[0],"TERRAIN TABLE BRUSH", StringComparison.OrdinalIgnoreCase) )
                    {
                        lrm = ReadTerrainLine;
                        continue;
                    }
                    else  if ( String.Equals(csv[0], "TERRAIN TABLE OBJECTIVES", StringComparison.OrdinalIgnoreCase))
                    {
                        lrm = ReadObjectiveLine;
                        continue;
                    }
                    else  if ( String.Equals(csv[0], "TERRAIN TABLE FORTS", StringComparison.OrdinalIgnoreCase))
                    {
                        lrm = ReadFortsLine;
                        continue;
                    }
                    else  if ( String.Equals(csv[0], "TERRAIN TABLE SOUNDS", StringComparison.OrdinalIgnoreCase))
                    {
                        // Don't care about sounds
                        lrm = null;
                        continue;
                    }                    
                    else if (csv[0].ToUpperInvariant().Contains("TERRAIN TABLE"))
                    {
                        //Something we dont's know how to handle yet
                        Log.Warn(this, "No rules to parse " + csv[0]);
                        lrm = null;
                        continue;
                    }

                    if (lrm == null) continue;
                    lrm = lrm(csv, readState);
                }
			}
            Log.Info(this,"Read "+objectives.Count+" objectives");
            Log.Info(this,"Read "+forts.Count+" forts");

            //compute roads
            float maxmod = Single.NegativeInfinity;
            foreach (Terrain t in grayscales) {
                if (t == null) continue;
                maxmod = t.movementFactor > maxmod ? t.movementFactor : maxmod;
            }

            foreach (Terrain t in grayscales) {
                if (t == null) continue;
                t.isRoad = t.movementFactor == maxmod;
            }            

            // foreach (MapObjective objective in objectives.Values) {
            //     Console.WriteLine("MapObjective: \""+objective.id+"\"");
            // }

            // foreach (Fort fort in forts.Values) {
            //     Console.WriteLine("fort: \""+fort.id+"\" firingLines:"+fort.firingLines.Count);
            // }

		}

        LineReadMode ReadTerrainLine(CsvReader csv, ReadState readState)
        {
            if (csv[1] == String.Empty) return ReadTerrainLine;
            int grayscale = 0;
            if (!Int32.TryParse(csv[1], out grayscale))
            {
                Log.Warn(this, "Unable to parse grayscale value \"" + csv[1] + "\"");
            }
           Terrain terrain = new Terrain();
           terrain.id = csv[0];
           terrain.grayscale = grayscale;

            if (!Single.TryParse(csv[2], out terrain.movementFactor))
            {
                Log.Warn(this, "Unable to parse move mod value \"" + csv[2] + "\"");
            }

           if (grayscale >= grayscales.Length)
           {
               Log.Error(this, "Unable to store terrain type with gray index greater than 256: \"" + terrain.id + "\" " + terrain.grayscale);
               return ReadTerrainLine;
           }

           terrains[terrain.id] = terrain;
           grayscales[terrain.grayscale] = terrain;

           return ReadTerrainLine;
        }

		LineReadMode ReadObjectiveLine(CsvReader csv, ReadState readState) {
            if (csv[0] == null || csv[0] == String.Empty) return ReadObjectiveLine;

			MapObjective objective = new MapObjective();
            objective.FromCsvLine(csv);
            if (objective.south == 0 && objective.east == 0 ) return ReadObjectiveLine;
            objectives[objective.id] = objective;

            return ReadObjectiveLine;
		}

        LineReadMode ReadFortsLine (CsvReader csv, ReadState readState) {

            if (csv[0] == null || csv[0] == String.Empty) return ReadFortsLine;

            if ( String.Compare(csv[0],"FIRING LINE", true) == 0 ) {
                if (readState.currentFort == null) {
                    Log.Error(this,"Encountered FIRING LINE with no parent fort");
                    return ReadFortsLine;
                }
                FiringLine firingLine = new FiringLine();
                firingLine.FromCsvLine(csv);
                readState.currentFort.firingLines.Add(firingLine);
                return ReadFortsLine;
            }

            Fort fort = new Fort();
            fort.FromCsvLine(csv);
            if (fort.grayscale == 0) return ReadFortsLine;
            forts[fort.id] = fort;
            readState.currentFort = fort;

            return ReadFortsLine;
        }


        #region Save
        public void Save() {
            throw new NotImplementedException();
        }
        #endregion
	}

	// public class Objective : IHasPosition {
	// 	public string id;
	// 	public float x,z;
	// }

 //    public class Fort {
 //        public string id;
 //        public float x,z;
 //    }

    public class Terrain {
        public string id;
        public int grayscale;
        public float movementFactor;
        public bool isRoad = false;

        public override string ToString()
        {
            return id;
        }
    }

}