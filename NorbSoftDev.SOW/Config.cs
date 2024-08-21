using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using System.Reflection;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW
{
    /// <summary>
    ///  Container for all config loaded files, most found in Logistics folder
    /// as well as handling search paths for the config
    /// </summary>
    public class Config
    {

        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx
        public static System.Text.Encoding TextFileEncoding = System.Text.Encoding.GetEncoding(28591); // 28591 iso-8859-1  ISO 8859-1 Latin 1; Western European (ISO)


        public const string NewLine = "\r\n";

        public const string GAME_SDK = "SDK";
        public const string GAME_BASE = "Base";
        public const string GAME_DLC = "DLC";
        public const string GAME_INI = "SowWL.ini";
        public static string DEFAULT_SOW_DIR = "SteamLibrary/steamapps/common/Scourge Of War - Remastered";
        public static string USER_SOW_DOCUMENTS_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SOWx64");
        public static string USER_CONFIG_DIR = Path.Combine(USER_SOW_DOCUMENTS_DIR, "ScenarioEditor");

        public string SOWDir;

        List<Mod> _mods = new List<Mod>();

        public List<Mod> mods
        {
            get
            {
                return _mods;
            }
            protected set
            {
                _mods = value;
            }
        }

        // Files and Assets
        public List<string> logisticsFiles = new List<string>();

        public Dictionary<string, SowStr> sowstrs = new Dictionary<string, SowStr>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, OrderOfBattle> orderOfBattles = new Dictionary<string, OrderOfBattle>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Map> maps = new Dictionary<string, Map>(StringComparer.OrdinalIgnoreCase);


        public Headers headers;
        // Values
        public Dictionary<string, Graphic> graphics = new Dictionary<string, Graphic>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Sound> sounds = new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, UnitModel> unitModels = new Dictionary<string, UnitModel>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Formation> formations = new Dictionary<string, Formation>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, UnitState> states = new Dictionary<string, UnitState>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, UnitType> unitTypes = new Dictionary<string, UnitType>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, UnitClass> unitClasses = new Dictionary<string, UnitClass>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Munition> munitions = new Dictionary<string, Munition>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Ammunition> ammunitions = new Dictionary<string, Ammunition>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Bore> bores = new Dictionary<string, Bore>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, CommandTemplate> commandTemplates = new Dictionary<string, CommandTemplate>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, EventTemplate> eventTemplates = new Dictionary<string, EventTemplate>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Order> orders = new Dictionary<string, Order>(StringComparer.OrdinalIgnoreCase);

        public int spriteRatio = 4;
        //sounds, sprites, etc

        public static Config AutoFind()
        {
            return Config.AutoFind(Directory.GetCurrentDirectory(), GAME_INI);
        }


        //public static Config AutoFind(string configiniName)
        //{
        //    return Config.AutoFind(Directory.GetCurrentDirectory(), configiniName);
        //}


        /// <summary>
        /// Create a Config object from a location on disk
        /// Assumes game dir is parent of SDK dir
        /// </summary>
        public static Config AutoFind(string path, string configiniName)
        {

            DirectoryInfo parentDir = new DirectoryInfo(path);

            //traverse upwards
            while (parentDir.FullName != parentDir.Root.FullName)
            {
                if (parentDir.GetDirectories(GAME_SDK).Length > 0)
                {
                    string SOWDir = parentDir.FullName;

                    Log.Info("Config", "Found Game Dir: " + SOWDir);

                    return new Config(SOWDir, Path.Combine(USER_SOW_DOCUMENTS_DIR, configiniName));
                }
                Log.Info("Config", "Tested " + parentDir);

                parentDir = parentDir.Parent;
            }


            foreach (var drive in DriveInfo.GetDrives())
            {
                var SOWDir = Path.Combine(drive.Name, Config.DEFAULT_SOW_DIR);
                if (Directory.Exists(SOWDir))
                {
                    return new Config(SOWDir, Path.Combine(USER_SOW_DOCUMENTS_DIR, configiniName));
                }
            }


            Log.Error(path, "Cannot find " + GAME_SDK + " from " + path);

            throw new Exception("Cannot find " + GAME_SDK + " from " + path);
        }


        Config()
        {
            //Make sure we use use . as decimal seperator and other computer friendly english-like standards in formatting
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SOWDir"></param>
        /// <param name="sowgameiniPath"></param>
        /// 
        public Config(string SOWDir, string sowgameiniPath) : this(SOWDir, sowgameiniPath, null) { }


        public Config(string SOWDir, string sowgameiniPath, string resourcesDir) : this()
        {

            if (SOWDir == null) Log.Error(this, "SOWDir is null");
            if (sowgameiniPath == null) Log.Error(this, "sowgbiniPath is null");
            if (resourcesDir == null)
            {
                FindResourcesDir();
            }
            else
            {
                this.resourcesDir = resourcesDir;
            }

            this.SOWDir = Path.GetFullPath(SOWDir);
            Log.Info(this, "SOWDir: " + this.SOWDir);

            headers = new Headers();
            headers.TrySetHeadersFromDir(Path.Combine(this.resourcesDir, "Headers"));
            headers.TrySetHeadersFromDir(Path.Combine(Config.USER_CONFIG_DIR, "Headers"));


            // default mods, in added in order of least to most authoratative
            Log.Info(this, "Documents: " + Environment.GetFolderPath(Environment.SpecialFolder.Personal));

            mods.Add(new DefaultMod(Path.Combine(Path.Combine(SOWDir, GAME_SDK), GAME_BASE), "SDK", -4));
            mods.Add(new DefaultMod(Path.Combine(SOWDir, GAME_BASE), "Game", -3));
            var dlcDir = Path.Combine(SOWDir, GAME_DLC);
            if (Directory.Exists(dlcDir))
                foreach (string dir in Directory.GetDirectories(dlcDir))
                {
                    mods.Add(new DefaultMod(dir, Path.GetFileName(dir), -2));
                }
            mods.Add(new DefaultMod(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), USER_SOW_DOCUMENTS_DIR),
                "User", -1));

            ReadGameIni(Path.GetFullPath(sowgameiniPath));

            // read Base, then mods in order, then local scenario data (rare)
            // for each set of data, so that later reads contain correct reference

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<SowStr, SowStr>(mod.FindFileInText("SoWstr.db"), sowstrs);

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Graphic, Graphic>(mod.FindFileInLogistics("gfx.csv"), graphics);

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Sound, Sound>(mod.FindFileInLogistics("sfx.csv"), sounds);

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadAttributes(mod.FindFileInLogistics("unitattributes.csv"));



            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Sprite, Sprite>(mod.FindFileInLogistics("unitpack.csv"), sprites);

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<UnitModel, UnitModel>(mod.FindFileInLogistics("unitmodel.csv"), unitModels);

            foreach (Mod mod in mods.Where(mod => mod.active))
                FormationReader.ReadFormations(this, mod.FindFileInLogistics("drills.csv"));

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<UnitType, UnitType>(mod.FindFileInLogistics("unittype.csv"), unitTypes);
            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<UnitClass, UnitClass>(mod.FindFileInLogistics("unitglobal.csv"), unitClasses);

            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Weapon, Rifle>(mod.FindFileInLogistics("rifles.csv"), weapons);
            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Munition, Munition>(mod.FindFileInLogistics("munitions.csv"), munitions);
            foreach (Mod mod in mods.Where(mod => mod.active))
                ReadCSVIntoDictionary<Weapon, Artillery>(mod.FindFileInLogistics("artillery.csv"), weapons);

            this.ComputeFormationLevels(formations);

            ReadCommandTemplates();
            ReadEventTemplates();
            Log.Info(this, "Config Loaded");

            Log.Flush();
        }

        public Mod sdkMod { get { return mods[0]; } }
        public Mod baseMod { get { return mods[1]; } }
        public Mod userMod { get { return mods[2]; } }

        public string resourcesDir = null;

        public void FindResourcesDir()
        {
            if (resourcesDir == null)
            {
                DirectoryInfo parentDir = null;
                if (AppDomain.CurrentDomain.BaseDirectory != null)
                {
                    parentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                }
                else
                {
                    Log.Warn(this, " FindResourcesDir Unable to find AppDomain.CurrentDomain.BaseDirectory ");
                }

                while (parentDir.FullName != parentDir.Root.FullName)
                {
                    DirectoryInfo[] subdirs = parentDir.GetDirectories("Resources");
                    if (subdirs.Length > 0)
                    {
                        resourcesDir = subdirs[0].FullName;
                        Log.Info(this, "Found Resources Dir: " + resourcesDir);
                        break;
                    }
                    Log.Info(this, "Searching for Resource dir: Resources tested " + parentDir);

                    parentDir = parentDir.Parent;
                }
            }
            //string filepath = Path.Combine(resourcesDir, name);
            //return filepath;
        }

        public string GetResourceFile(string name)
        {
            if (resourcesDir == null) FindResourcesDir();
            string filepath = Path.Combine(resourcesDir, name);
            return filepath;
        }

        public string GetResourceFile(string subdir, string name)
        {
            if (resourcesDir == null) FindResourcesDir();
            string filepath = Path.Combine(resourcesDir, Path.Combine(subdir, name));
            return filepath;
        }


        public StreamReader GetResourceStreamReader(string name)
        {
            if (resourcesDir == null) FindResourcesDir();
            // {
            //     if (AppDomain.CurrentDomain.BaseDirectory == null) Log.Error(this," GetResourceStreamReader Unable to find AppDomain.CurrentDomain.BaseDirectory "+name);

            //     DirectoryInfo parentDir = (new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            //     if (parentDir == null) Log.Error(this," GetResourceStreamReader Unable to find  parentDir with name "+name);

            //     //Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location ),

            //     while (parentDir.FullName != parentDir.Root.FullName)
            //     {
            //         DirectoryInfo [] subdirs = parentDir.GetDirectories("Resources");
            //         if (subdirs.Length > 0)
            //         {
            //             resourcesDir = subdirs[0].FullName;
            //             Log.Info(this,"Found Resources Dir: " + resourcesDir);
            //             break;
            //         }
            //         Log.Info(this,"Searching for Resource dir: Resources tested " + parentDir);

            //         parentDir = parentDir.Parent;
            //     }
            // }

            if (resourcesDir == null) Log.Error(this, " GetResourceStreamReader unable to find resourcesDir using name " + name);

            string filepath = Path.Combine(resourcesDir, name);
            Log.Info(this, "Opening Resource Stream " + filepath);
            return new StreamReader(filepath, Config.TextFileEncoding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OrderOfBattle GetOrderOfBattle(string nameOrPath)
        {
            OrderOfBattle orderOfBattle = null;

            //Check previously created
            if (orderOfBattles.TryGetValue(Path.GetFileNameWithoutExtension(nameOrPath), out orderOfBattle))
            {
                Log.Info(this, "Already Loaded OOB " + Path.GetFileNameWithoutExtension(nameOrPath));
                return orderOfBattle;
            }

            string filepath;
            string name;
            if (File.Exists(nameOrPath))
            {
                filepath = nameOrPath;
                name = Path.GetFileNameWithoutExtension(Path.GetFileName(filepath));
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(nameOrPath);
                string shortpath = Path.Combine("OOBs", name + ".csv");
                filepath = this.FindFileInMods(shortpath);
                if (filepath == null) filepath = this.FindFileInSOW("", shortpath);
                // if (filepath == null) filepath = this.FindFileInSDK(shortpath);

                if (filepath == null)
                {
                    Log.Error(this, "Unable to find OrderOfBattle " + name + " : " + shortpath);
                    throw new Exception("Unable to find OrderOfBattle " + name + " : " + shortpath);
                }
            }

            string dirpath = System.IO.Path.GetDirectoryName(filepath);
            Mod mod = GetModFromFile(new DirectoryInfo(dirpath));

            orderOfBattle = new OrderOfBattle(this, mod, name);
            orderOfBattle.Load();
            orderOfBattles[name] = orderOfBattle;
            return orderOfBattle;
        }

        public Map GetMap(string name)
        {
            Map map = null;

            //Check previously created
            if (maps.TryGetValue(Path.GetFileNameWithoutExtension(name), out map))
            {
                Log.Info(this, "Already Loaded Map " + Path.GetFileNameWithoutExtension(name));
                return map;
            }
            string filepath;
            if (File.Exists(name))
            {
                filepath = name;
                name = Path.GetFileNameWithoutExtension(Path.GetFileName(filepath));
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(name);
                string shortpath = Path.Combine("Maps", name + ".ini");
                filepath = this.FindFileInMods(shortpath);
                if (filepath == null) filepath = this.FindFileInSOW("", shortpath);

                if (filepath == null)
                {
                    // return null;
                    throw new Exception("Unable to find Map " + name + " " + shortpath);
                }
            }

            string dirpath = System.IO.Path.GetDirectoryName(filepath);
            Mod mod = GetModFromFile(new DirectoryInfo(dirpath));

            map = new Map(this, mod, name);
            map.PreLoad();
            maps[name] = map;
            return map;
        }

        /// <summary>
        /// Load active Mods
        /// </summary>
        /// <param name="filepath"></param>
        public void ReadGameIni(string filepath)
        {

            if (!File.Exists(filepath))
            {
                Log.Info(this, "[GameIni] Missing \"" + filepath + "\"");
                Log.Info(this, "[GameIni] No Mods will be loaded");
                return;
            }

            Log.Info(this, "[GameIni] Reading \"" + filepath + "\"");

            IniReader iniReader = new IniReader(filepath);
            int ignored = 0;


            foreach (string key in iniReader.GetKeys("mods"))
            {
                if (!key.StartsWith("modname")) continue;
                string modname = iniReader.GetValue(key, "mods");

                modname = modname.Replace('\\', Path.DirectorySeparatorChar);
                string n = key.Substring(7);
                int index = int.Parse(n);

                string fullpath = Path.Combine(SOWDir, modname);
                UserMod mod = new UserMod(fullpath);
                mod.index = index;
                mods.Add(mod);

                if (iniReader.GetValue("modactive" + n, "mods") != "1")
                {
                    ignored++;
                    continue;
                }

                Log.Info(this, "[GameIni] Using Mod \"" + fullpath + "\"");
                mod.active = true;
            }

            mods = mods.OrderBy(o => o.index).ToList();

            Log.Info(this, "[GameIni] " + ignored + " deactivated mods");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetScenarioFiles()
        {
            return GetFilesInMods("Scenarios", "scenario.csv");

            // List<string> filepaths = GetFilesInMods("Scenarios", "scenario.csv" );
            // filepaths.AddRange( GetFilesInWork("Scenarios", "scenario.csv"));             
            // filepaths.AddRange( GetFilesInSDK("Scenarios", "scenario.csv")); 
            // return filepaths;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetOrderOfBattleFiles()
        {
            return GetFilesInMods("OOBs", "*.csv");

            // List<string> filepaths = GetFilesInMods("OOBs", "*.csv" );
            // filepaths.AddRange( GetFilesInWork("OOBs", "*.csv"));             
            // filepaths.AddRange( GetFilesInSDK("OOBs", "*.csv")); 
            // return filepaths;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subdir"></param>
        /// <param name="filespec"></param>
        /// <returns></returns>
        public List<string> GetFilesInMods(string subdir, string filespec)
        {
            // TODO use ini to determine which mods are active
            List<string> files = new List<string>();
            foreach (Mod mod in mods)
            {
                if (!mod.active) continue;
                files.AddRange(mod.GetFilesInMod(subdir, filespec));

                // string fulldir = Path.Combine(mod.dirpath, subdir);
                // try {
                //     files.AddRange(Directory.GetFiles(fulldir, filespec, SearchOption.AllDirectories));
                // } catch (System.IO.DirectoryNotFoundException e){
                // }
            }
            return files;
        }

        // public List<string> GetFilesInSDK(string subdir, string filespec) {
        //     return GetFilesIn(Path.Combine("Work","SDK",subdir), filespec);
        // }

        // public List<string> GetFilesInWork(string subdir, string filespec) {
        //     return GetFilesIn(Path.Combine("Work", subdir), filespec);
        // }

        // public List<string> GetFilesIn(string dirname, string filespec) {
        //     string dir = Path.Combine(SOWDir,dirname);
        //     List<string> files = new List<string>();
        //     try {
        //     files.AddRange(Directory.GetFiles(dir, filespec, SearchOption.AllDirectories));
        //     } catch (System.IO.DirectoryNotFoundException e){
        //     }
        //     return files;
        // }        


        public Mod GetModByName(string name)
        {
            // reverse order so last gets returned first
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                Mod mod = mods[i];
                if (!mod.active) continue;
                //TODO this will break if subdir of another mod (SDK inside Work)
                if (mod.name == name)
                    return mod;
            }

            Log.Error(this, "Unable to GetModByName " + name);
            throw new Exception("Unable to GetModByName " + name);
        }

        // public Mod GetModFromFile(string filepath)
        // {
        //     return GetModFromFile(new FileInfo(filepath));
        // }

        /// <summary>
        /// given a FileInfo, return the Mod tha contains is. The Mod is not guarenteed to be active. Returns null if none is found.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Mod</returns>
        public Mod GetModFromFile(FileInfo file)
        {

            if ((file.Attributes & FileAttributes.Directory) == FileAttributes.Directory) return GetModFromFile(new DirectoryInfo(file.FullName));
            // reverse order so last gets returned first
            string modsSearched = "Searching for FileInfo " + file.ToString() + Environment.NewLine;

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                Mod mod = mods[i];

                modsSearched += "Searched Mod: " + mod.directory.FullName + Environment.NewLine;

                if (mod.Contains(file))
                    return mod;
            }
            Log.Error(this, modsSearched);
            Log.Error(this, "Unable to GetMod From FileInfo " + file);

            return null;
        }

        /// <summary>
        ///  given a DirectoryInfo, return the Mod that contains is
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Mod</returns>
        public Mod GetModFromFile(DirectoryInfo file)
        {
            // reverse order so last gets returned first
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                Mod mod = mods[i];
                if (!mod.active) continue;
                //TODO this will break if subdir of another mod (SDK inside Work)
                if (mod.Contains(file))
                    return mod;
            }

            Log.Error(this, "Unable to GetMod From DirectoryInfo " + file);
            throw new Exception("Unable to GetMod From DirectoryInfo " + file);
        }

        public string FindFileInMods(string file)
        {
            // reverse order so last gets returned first
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                string filepath = mods[i].FindFileInMod(file);
                if (filepath != null) return filepath;
            }
            return null;
        }

        public string FindFileInMods(string file, out Mod mod)
        {
            // reverse order so last gets returned first
            for (int i = mods.Count - 1; i >= 0; i--)
            {
                string filepath = mods[i].FindFileInMod(file);
                if (filepath != null)
                {
                    mod = mods[i];
                    return filepath;
                }
            }
            mod = null;
            return null;
        }
        // public string FindFileInBase(string file) {
        //     return FindFileInSOW("Base", file);
        // }

        // public string FindFileInWork(string file) {
        //     return FindFileInSOW("Work", file);
        // }

        public string FindFileInLogistics(string file)
        {

            string shortpath = Path.Combine("Logistics", file);
            string filepath = null;

            if (filepath == null) filepath = FindFileInMods(shortpath);
            // if (filepath == null) filepath = FindFileInWork(shortpath);
            // if (filepath == null) filepath = FindFileInBase(shortpath);
            // if (filepath == null) filepath = FindFileInSDK(shortpath);
            // if (filepath == null) filepath = FindFileInDefaults(shortpath);


            if (filepath == null) throw new Exception("Unable to find " + file + " " + shortpath);
            return filepath;
        }


        static string[] _graphicExtensions = new string[] { "", ".dds", ".png", ".jpg", ".bmp", ".tga" };
        static string[] _graphicSubfolders = new string[] { "Effects", "Flags", "Fonts", "Misc", "Packed", "Screens", "Misc", "Terrain" };
        public string FindGraphic(string file)
        {

            string filepath = null;

            string filename = filepath;
            string extension = null;

            int idx = file.LastIndexOf('.');
            if (idx > -1)
            {
                filename = file.Substring(0, idx);
                extension = file.Substring(idx + 1);
            }

            foreach (string subfolder in _graphicSubfolders)
            {


                string graphicsDir = Path.Combine(this.baseMod.directory.FullName, "Graphics");
                //try as is
                filepath = Path.Combine(
                    graphicsDir,
                    Path.Combine(subfolder, file)
                    );
                if (filepath != null && File.Exists(filepath))
                {
                    return filepath;
                }

                //try with various extensions
                foreach (string ext in _graphicExtensions)
                {
                    //filepath = FindFileInMods(shortpath+ext);
                    filepath = Path.Combine(
                        graphicsDir,
                        Path.Combine(subfolder, filename + ext)
                        );
                    if (filepath != null && File.Exists(filepath))
                    {
                        return filepath;
                    }
                }
            }

            if (filepath == null)
            {
                Log.Error(this, "Missing graphic " + file);
            }
            return null;
        }

        // public string FindFileInSDK(string file) {
        //     return FindFileInSOW(Path.Combine("Work","SDK"), file);
        // }

        // public string FindFileInDefaults(string file) {
        //     System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
        //     assembly.GetManifestResourceStream(resourceName))
        //     // string path = Path.Combine(dataDir,file);
        //     // Log.Info(this,"Seaching Defaults \""+path+"\"");

        //     // string history = (" \""+path+"\""+System.Environment.NewLine);
        //     // if ( File.Exists( path ) ) {
        //     //     return path;
        //     // }
        //     return null;            
        // }        

        public string FindFileInSOW(string dirname, string file)
        {
            string dirpath = Path.Combine(SOWDir, dirname);
            return FindFileInDir(dirpath, file);

        }

        public string FindFileInDir(string dirpath, string file)
        {
            string path = Path.Combine(dirpath, file);
            string history = (" \"" + path + "\"" + System.Environment.NewLine);
            if (File.Exists(path))
            {
                // Log.Info(this,"Found \""+path+"\"");
                return path;
            }
            // Log.Info(this,"Searched "+history);
            return null;
        }


        public void ReadCSVIntoDictionary<T, S>(string filepath, Dictionary<string, T> dict)
        //			where T : LogisticsEntry, new() where S : T, new() {
        where T : LogisticsEntry, new() where S : T, new()
        {
            string typeName = dict.GetType().GetGenericArguments()[1].ToString();

            if (filepath == null)
            {
                return;
            }

            logisticsFiles.Add(filepath);

            Stream stream = null;

            try
            {
                Log.Info(this, "[" + typeName + "] Reading \"" + filepath + "\"");
                stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (CsvReader csv = new CsvReader(new StreamReader(stream, Config.TextFileEncoding), true))
                {
                    csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    while (csv.ReadNextRecord())
                    {
                        if (csv[0] == String.Empty) continue;
                        try
                        {
                            T thing = new S();
                            thing.FromCsvLine(this, filepath, csv);
                            dict[thing.id] = (T)thing;

                        }
                        catch (Exception e)
                        {
                            Log.Error(this, "[" + typeName + "] Read failed on \"" + csv[0] + "\"");
                            throw (e);
                        }
                    }
                    Log.Info(this, "[" + typeName + "] count " + dict.Count);
                }

            }
            finally
            {
                if (stream != null)
                    ((IDisposable)stream).Dispose();
            }

        }


        public void ReadCommandTemplates()
        {
            string filepath = Path.Combine("Templates", "commands.csv");
            StreamReader stream = this.GetResourceStreamReader(filepath);
            if (stream == null) Log.Error(this, "ReadCommandTemplates cannot open stream to " + filepath);
            CsvReader csv = new CsvReader(stream, true);

            // Use reflection to build a table of command types to name
            Dictionary<string, Type> commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            commandTypes[typeof(Command).Name] = typeof(Command);

            foreach (
                Type atype in typeof(Command).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)) && !t.IsAbstract)
                )
            {

                commandTypes[atype.Name] = atype;
            }

            // Environment.Exit(1);

            while (csv.ReadNextRecord())
            {

                string name = csv[0].ToLower().Trim();
                string Atag = csv[1].ToUpper().Trim(); //prepend for group commmand
                string category = csv[2].ToLower().Trim();
                string typeName = csv[3].Trim();
                string help = csv[4].Trim();
                Type type;

                if (!commandTypes.TryGetValue(typeName, out type))
                {
                    Log.Error(this, "Unknown Command type " + typeName);
                    continue;
                }


                commandTemplates[name] = new CommandTemplate(type, name, category, help);

                if (!string.IsNullOrEmpty(Atag))
                {
                    string Aname = Atag + name;
                    commandTemplates[Aname] = new CommandTemplate(type, Aname, category, help);
                }
            }

            // foreach (CommandTemplate template in commandTemplates.Values) {
            //     Console.WriteLine("CommandTemplate: "+template.name+" "+template.type+" "+template.help);

            // }

        }

        public void ReadEventTemplates()
        {
            string filepath = Path.Combine("Templates", "events.csv");
            StreamReader stream = this.GetResourceStreamReader(filepath);
            if (stream == null) Log.Error(this, "ReadEventTemplates cannot open stream to " + filepath);

            CsvReader csv = new CsvReader(stream, true);


            // Use reflection to build a table of event types to name
            Dictionary<string, Type> eventTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (
                Type atype in typeof(BattleScriptEvent).Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BattleScriptEvent)) && !t.IsAbstract)
                )
            {

                eventTypes[atype.Name] = atype;
            }

            while (csv.ReadNextRecord())
            {

                string name = csv[0].ToLower();
                string typeName = csv[1];
                string help = csv[2];
                Type type;

                if (!eventTypes.TryGetValue(typeName, out type))
                {
                    Log.Error(this, "Unknown Event type " + typeName);
                    continue;
                }

                eventTemplates[name] = new EventTemplate(type, name, help);
            }

            foreach (EventTemplate template in eventTemplates.Values)
            {
                Console.WriteLine("EventTemplate: " + template.name + " " + template.type);

            }

        }

        // public static void ComputeFormationLevels(Dictionary<string, Formation> formations)
        public void ComputeFormationLevels(Dictionary<string, Formation> formations)
        {
            List<Formation> used = new List<Formation>();

            foreach (Formation formation in formations.Values)
            {
                if (formation.subformation == null)
                {
                    formation.level = 0;
                    used.Add(formation);
                }
            }
            //Log.Info(this,"[Formation] 0: Assigned "+used.Count+" Formations out of "+formations.Count);


            for (int i = 1; i < 9; i++)
            {
                List<Formation> newused = new List<Formation>();
                foreach (Formation formation in formations.Values)
                {
                    if (used.Contains(formation)) continue;
                    if (used.Contains(formation.subformation))
                    {
                        formation.level = i;
                        newused.Add(formation);
                    }
                }
                used.AddRange(newused);
                //Log.Info(this,"[Formation] Level "+i+": Assigned "+used.Count+" Formations out of "+formations.Count);
                if (used.Count == formations.Count) break;
            }

            //Print info
            // var query = from f in formations.Values orderby  f.level ascending select f;
            // foreach (Formation formation in query) {
            //    Log.Info(
            //        String.Format("{0,-2} {1,-32} {2,-32} {3,-32}", formation.level,formation.id,formation.subformation,formation.artilleryFormation)
            //        );
            // }

            Log.Info(this, "Assigned " + used.Count + " Formation Levels out of " + formations.Count);
        }

        #region Attributes
        delegate AttributeMethodDelegate AttributeMethodDelegate(string definedIn, CsvReader csv, ref Attribute attribute);

        public void ReadAttributes(string filepath)
        {

            if (filepath == null)
            {
                return;
            }

            logisticsFiles.Add(filepath);

            Stream stream = null;
            AttributeMethodDelegate attributeMethod = null;

            try
            {
                Log.Info(this, "[Attribute] Reading \"" + filepath + "\"");
                stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (CsvReader csv = new CsvReader(new StreamReader(stream, Config.TextFileEncoding), true))
                {
                    csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    Attribute attribute = null;
                    int row = 0;

                    while (csv.ReadNextRecord())
                    {

                        if (csv[0] == "NEW") attributeMethod = ReadAttributeInit;

                        if (attributeMethod == null) continue;

                        try
                        {
                            // Log.Info(this,"Invoking "+attributeMethod.Method.Name);

                            attributeMethod = attributeMethod(filepath, csv, ref attribute);
                        }
                        catch (Exception e)
                        {
                            if (attribute == null) Log.Error(this, "[Attribute] Read failed on null attribute '" + csv[0] + "'' #" + attributes.Count);
                            else Log.Error(this, "[Attribute] Read failed on '" + attribute.name + "'' '" + csv[0] + "'' #" + attributes.Count);
                            foreach (Delegate d in attributeMethod.GetInvocationList())
                            {
                                Log.Info(this, " Invoked: " + d.Method.Name);
                            }
                            throw (e);
                        }
                    }

                    Log.Info(this, "[Attribute] count " + attributes.Count);
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                if (stream != null)
                    ((IDisposable)stream).Dispose();
            }
        }

        AttributeMethodDelegate ReadAttributeInit(string definedIn, CsvReader csv, ref Attribute attribute)
        {
            attribute = new Attribute();
            // Log.Info(this,"Creating Attribute "+csv[1]);
            attribute.name = csv[1];
            attributes[attribute.name] = attribute;

            return ReadAttributeComment;
        }


        AttributeMethodDelegate ReadAttributeComment(string definedIn, CsvReader csv, ref Attribute attribute)
        {
            attribute.label = csv[1];
            attribute.help = csv[33];
            // Log.Info(this," Attribute "+attribute.label+" "+attribute.help);

            return ReadAttributeBody;
        }

        AttributeMethodDelegate ReadAttributeBody(string definedIn, CsvReader csv, ref Attribute attribute)
        {
            if (csv[0] == string.Empty) return ReadAttributeBody;
            AttributeLevel level = new AttributeLevel(attribute);
            level.FromCsvLine(this, definedIn, csv);

            // level.index = Convert.ToInt32(csv[0]);
            // level.value = Convert.ToInt32(csv[1]);
            // try {
            //     level.name = sowstrs[csv[2]];
            // } catch (Exception e) {
            //     Log.Error(this,"[Attribute] Unable to find string "+csv[2]);
            //     throw;

            // }
            attribute.Add(level);

            return ReadAttributeBody;

            //error check
        }
        #endregion  


        internal void PopulateFromCsvLine(object obj, CsvReader csv, string[] properties, string definedIn)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                string propertyName = properties[col];
                if (propertyName == null) continue;

                string propertyValue = csv[col];
                if (propertyValue == null || propertyValue.Length < 1) continue;

                //Console.WriteLine(propertyName+"="+propertyValue);

                PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    Log.Error(this, "[Formation] " + obj + " can't find col #" + col + " property '" + propertyName + "'  value:'" + propertyValue);
                }

                Type propertyType = propertyInfo.PropertyType;
                try
                {

                    if (propertyType == typeof(Formation))
                        propertyInfo.SetValue(obj, formations[propertyValue], null);
                    else if (propertyType == typeof(Formation.Distance))
                        propertyInfo.SetValue(obj, new Formation.Distance(propertyValue), null);
                    else if (propertyType == typeof(bool))
                        propertyInfo.SetValue(obj, propertyValue != "0", null);
                    else
                    {
                        propertyInfo.SetValue(obj, Convert.ChangeType(propertyValue, propertyType), null);
                    }


                }
                catch (Exception e)
                {
                    Log.Warn(this, "[Formation] " + obj + " can't convert col #" + col + " '" + propertyName + "'  value:'" + propertyValue + "' to " + propertyType + " " + definedIn);
                    //throw e;
                }

            }
        }

    }


    /// <summary>
    /// Can be written out as a csv line
    /// </summary>
    public interface ICsvLine
    {
        string ToCsvLine();
    }

    /// <summary>
    /// needs ToCsv called on it whe writing a reference to a csv file
    /// </summary>
    public interface ICsvValue
    {
        string ToCsvValue();
    }



    // results and campaign
    public class UnitStatus { }
    public class XP { }

}
