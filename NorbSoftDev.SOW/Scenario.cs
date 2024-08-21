using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Linq;

namespace NorbSoftDev.SOW
{

    public class Scenario : ScenarioUnitRoster, ILoad
    {


        static public string defaultMapName = "Waterloo";

        string intro_txt;// screen_txt;
        BattleScript _battleScript;
        IniReader _ini;

        public BattleScript battleScript
        {
            get
            {
                return _battleScript;
            }
            set
            {
                this._battleScript = value;
            }
        }
        public Dictionary<string, ScreenMessage> screenMessages { get; internal set; }


        #region Map
        Map _map;
        public Map map
        {
            get { return _map; }
            protected set { _map = value;}
        }
        //public IdOrderedSet<ScenarioObjective> objectives = new IdOrderedSet<ScenarioObjective>(4);
        public IdDictionary<ScenarioObjective> objectives = new IdDictionary<ScenarioObjective>(StringComparer.OrdinalIgnoreCase);
        //public ObservableDictionary<string,ScenarioObjective> objectives = new IdDictionary<ScenarioObjective>(StringComparer.OrdinalIgnoreCase);
        #endregion

        bool _strategicAI = true;
        public bool strategicAI
        {
            get { return _strategicAI; }
            set
            {
                if (_strategicAI == value) return;
                _strategicAI = value;
                OnPropertyChanged("strategicAI");
            }
        }

        bool _carryover = false;
        public bool carryover
        {
            get { return _carryover; }
            set
            {
                if (_carryover == value) return;
                _carryover = value;
                OnPropertyChanged("carryover");
            }
        }

        public enum ESandbox { Hunt, Attack, Defend }
        ESandbox _sandbox;
        public ESandbox sandbox
        {
            get { return _sandbox; }
            set
            {
                if (_sandbox == value) return;
                _sandbox = value;
                OnPropertyChanged("sandbox");
            }
        }


        //int _failgrade = -20000;
        //public int failgrade
        //{
        //    get { return _failgrade; }
        //    set
        //    {
        //        if (_failgrade == value) return;
        //        _failgrade = value;
        //        OnPropertyChanged("failgrade");
        //    }
        //}

        //int _lvl1grade = -2000;
        //public int lvl1grade
        //{
        //    get { return _lvl1grade; }
        //    set
        //    {
        //        if (_lvl1grade == value) return;
        //        _lvl1grade = value;
        //        OnPropertyChanged("lvl1grade");
        //    }
        //}

        //int _lvl2grade = 1500;
        //public int lvl2grade
        //{
        //    get { return _lvl2grade; }
        //    set
        //    {
        //        if (_lvl2grade == value) return;
        //        _lvl2grade = value;
        //        OnPropertyChanged("lvl2grade");
        //    }
        //}

        //int _lvl3grade = 10000;
        //public int lvl3grade
        //{
        //    get { return _lvl3grade; }
        //    set
        //    {
        //        if (_lvl3grade == value) return;
        //        _lvl3grade = value;
        //        OnPropertyChanged("lvl3grade");
        //    }
        //}

        //int _lvl4grade = -20000;
        //public int lvl4grade
        //{
        //    get { return _lvl4grade; }
        //    set
        //    {
        //        if (_lvl4grade == value) return;
        //        _lvl4grade = value;
        //        OnPropertyChanged("lvl4grade");
        //    }
        //}

        //int _lvl5grade = -20000;
        //public int lvl5grade
        //{
        //    get { return _lvl5grade; }
        //    set
        //    {
        //        if (_lvl5grade == value) return;
        //        _lvl5grade = value;
        //        OnPropertyChanged("lvl5grade");
        //    }
        //}

        int _lvl6grade = -20000;
        public int lvl6grade
        {
            get { return _lvl6grade; }
            set
            {
                if (_lvl6grade == value) return;
                _lvl6grade = value;
                OnPropertyChanged("lvl6grade");
            }
        }


        #region Environment
        public enum EWeather { Day_Cloudy, Day_PartlyCloudy, Day_Clear, Dusk_Cloudy, Dawn_Cloudy, Dawn_Clear, Dusk_Clear, Night_PartlyCloudy, Night_PartlyCloudyFullMoon, Night_ClearFullMoon }
        EWeather _initialWeather;
        public EWeather initialWeather
        {
            get { return _initialWeather; }
            set
            {
                if (_initialWeather == value) return;
                _initialWeather = value;
                OnPropertyChanged("initialWeather");
            }
        }

        TimeSpan _startTime;
        public TimeSpan startTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                isDirty = true;
                OnPropertyChanged("startTime");
            }
        }

        DateTime _date;
        public DateTime date
        {
            get { return _date; }
            set
            {
                _date = value;
                isDirty = true;
                OnPropertyChanged("date");
            }
        }
        public TimeSpan endTime { get; set; }
        public double latitude = 39.8283; //N

        public TimeSpan sunrise
        {
            get { return Sky.SunCalc(date, latitude, Sky.CalcMode.Sunrise); }
        }
        public TimeSpan sunset
        {
            get { return Sky.SunCalc(date, latitude, Sky.CalcMode.Sunset); }
        }

        // public DateTime ParseDateTime(string str)
        // {
        //     return DateTime.ParseExact(str, Sky.formatDateTime, CultureInfo.InvariantCulture);
        // }

        // // reutns DateTiem of time in scnearios day

        // public DateTime ParseTime(string str)
        // {

        //     return startTime.Date + TimeSpan.ParseExact(str, Sky.formatTime);
        // }


        #endregion


        #region Constructor
        public Scenario(Config config, Mod mod, string name)
            : base(config, mod, name)
        {
            // root = new ScenarioEchelonRoot();

            date = Sky.ParseDate("06/18/1815");
            startTime = Sky.ParseTimeSpan("10:00:00");
            endTime = Sky.ParseTimeSpan("13:00:00");
            initialWeather = EWeather.Day_Clear;
            _battleScript = new BattleScript(this);
            screenMessages = new Dictionary<string, ScreenMessage>(StringComparer.OrdinalIgnoreCase);
            intro_txt = null;

        }

        /// <summary>
        /// An empty Scenario based on the given OrderOfBattle
        /// </summary>
        /// <param name="orderOfBattle"></param>
        public Scenario(OrderOfBattle orderOfBattle, bool createFromOOB)
            : this(orderOfBattle.config, orderOfBattle.config.userMod, "UnnamedScenario")
        {
            this.orderOfBattle = orderOfBattle;
            if (createFromOOB)
            {
                Log.Info(this, "Creating from orderOfBattle " + this.orderOfBattle.GetHashCode());
                if (!this.orderOfBattle.hasLoaded) this.orderOfBattle.Load();

                PopulateUnitsFromOrderOfBattle();
                isDirty = false;
            }
            else
            {
                FromDefaults();
                isDirty = true;
            }

        }

        //public Scenario(ScenarioUnitRoster other) : base(other) { }

        public Scenario(Scenario other) : this(other.orderOfBattle, false) {
            this.map = other.map;
            this.startTime = other.startTime;
            
        }


        //public Scenario(OrderOfBattle orderOfBattle) : this(orderOfBattle, true) { }
        #endregion

        /// <summary>
        /// Tries load load text from scenario directory, otherwise uses default in template, otherwise empty
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        string ReadTextFile(string filename)
        {

            // Log.Info(this,"ReadTextFile from dir \"" + scenarioDir + "\"");

            if (scenarioDir != null)
            {
                string filepath = Path.Combine(scenarioDir, filename);
                if (File.Exists(filepath))
                {
                    Log.Info(this, "ReadTextFile \"" + filepath + "\"");
                    return (new StreamReader(filepath, Config.TextFileEncoding)).ReadToEnd();
                }
            }
            return ReadDefaultTextFile(filename);
        }

        string ReadDefaultTextFile(string filename)
        {
            string resourcepath = Path.Combine("Scenario", filename);
            try
            {

                Log.Info(this, "ReadDefaultTextFile \"" + resourcepath + "\"");
                return config.GetResourceStreamReader(resourcepath).ReadToEnd();
            }
            catch (Exception e)
            {
                Log.Error(this, "Unable to Read Default \"" + resourcepath + "\"");
                return String.Empty;
            }


        }

        public string scenarioDir { get { return Path.Combine(mod.scenarioDir.ToString(), name); } }
        public string scenarioCsvFile { get { return Path.Combine(scenarioDir, "scenario.csv"); } }


        protected override void FromDefaults()
        {
            map = config.GetMap(defaultMapName);
            map.PreLoad();
            map.Load();

        }


        #region PreLoad
        /// <summary>
        /// A lightweight read of the ini to establish dependencies
        /// </summary>
        public void PreLoad()
        {
            if (mod == null) Log.Error(this, "No mod set for preload");
            Log.Info(this, "PreLoad " + mod.scenarioDir);
            string inipath = System.IO.Path.Combine(mod.scenarioDir.FullName, Path.Combine(name, "scenario.ini"));
            PreLoadIni(inipath);
        }

        protected void PreLoadIni(string filepath)
        {
            _ini = new IniReader(filepath);

            //load start time
            string starttimestr = _ini.GetValue("starttime", "init");
            if (starttimestr != null && starttimestr != String.Empty) {
                try {
                    startTime = Sky.ParseTimeSpan(starttimestr);
                } catch {
                    Log.Warn(this,"Unable to determine scenario start time. Using default "+startTime);
                }
            }

            string weatherstr = _ini.GetValue("weather", "init");
            EWeather tryWeather;
            if (weatherstr != null && weatherstr != String.Empty && Enum.TryParse(weatherstr, out tryWeather)) {
                initialWeather = tryWeather;
            } else {
                Log.Warn(this, "Unable to determine scenario weather. Using default " + initialWeather);
            }

            string sandboxstr = _ini.GetValue("sandbox", "init");
            ESandbox trysandbox;
            if (sandboxstr != null && sandboxstr != String.Empty && Enum.TryParse(sandboxstr, out trysandbox))
            {
                sandbox = trysandbox;
            }
            else
            {
                Log.Warn(this, "Unable to determine scenario sandbox. Using default " + sandbox);
            }

            string strategicaistr = _ini.GetValue("strategicai", "init");
            bool tryStrategicai;
            if (strategicaistr != null && strategicaistr != String.Empty && bool.TryParse(strategicaistr, out tryStrategicai))
            {
                strategicAI = tryStrategicai;
            }
            else
            {
                Log.Warn(this, "Unable to determine scenario strategicAI. Using default " + strategicAI);
            }

            string carryoverstr = _ini.GetValue("carryover", "init");
            bool trycarryover;
            if (carryoverstr != null && carryoverstr != String.Empty && bool.TryParse(carryoverstr, out trycarryover))
            {
                carryover = trycarryover;
            }
            else
            {
                Log.Warn(this, "Unable to determine scenario carryover. Using default " + carryover);
            }

            // load map info
            string mapname = _ini.GetValue("map", "init");
            try
            {
                map = config.GetMap(mapname);
            }
            catch
            {
                Log.Error(this, "Unable to determine map from scenario.ini "+mapname);
                throw;
            }
            map.PreLoad();


            // find player character
            try
            {
                int side = Convert.ToInt32(_ini.GetValue("cmdlvl1", "rank"));
                int army = Convert.ToInt32(_ini.GetValue("cmdlvl2", "rank"));
                int corps = Convert.ToInt32(_ini.GetValue("cmdlvl3", "rank"));
                int div = Convert.ToInt32(_ini.GetValue("cmdlvl4", "rank"));
                int bgde = Convert.ToInt32(_ini.GetValue("cmdlvl5", "rank"));
                int reg = Convert.ToInt32(_ini.GetValue("cmdlvl6", "rank"));
                _preloadPlayerUnitEchelonId = EchelonHelper.ComposeEchelonId(side, army, corps, div, bgde, reg);
            }
            catch (Exception e)
            {
                Log.Error(this, "Unable to determine player unit from scenario.ini");
                Log.Exception(this, e);
            }


        }
        #endregion

        #region Load
        public void Load()
        {
            PreLoad();
            Log.Flush();

            map.Load();
            Log.Flush();

            ReadCsv(Path.Combine(mod.scenarioDir.FullName, System.IO.Path.Combine(name, "scenario.csv")));
            intro_txt = ReadTextFile("EnglishScenIntro.txt");

            string mapLocations_csv = Path.Combine(mod.scenarioDir.FullName, System.IO.Path.Combine(name, "maplocations.csv"));
            if (File.Exists(mapLocations_csv))
            {
                ReadMapLocations(mapLocations_csv);
            }
            else
            {
                Log.Info(this, "No maplocations.csv " + mapLocations_csv);
            }


            string screen_txt = Path.Combine(mod.scenarioDir.FullName, System.IO.Path.Combine(name, "EnglishScenScreen.txt"));
            if (File.Exists(screen_txt))
            {
                ReadScreen(screen_txt);
            }
            else
            {
               Log.Info(this,"No EnglishScenScreen.txt " + screen_txt);
            }

            string battlescript_csv = Path.Combine(mod.scenarioDir.FullName, System.IO.Path.Combine(name, "battlescript.csv"));
            if (File.Exists(battlescript_csv))
            {
                ReadBattleScript(battlescript_csv);
            }
            else
            {
                Log.Info(this, "Creating Default BattleScript " + screen_txt);

                _battleScript.CreateDefault();
            }
            isDirty = false;

            Log.Flush();
        }

        public void ReadMapLocations(string filepath)
        {
            if (filepath == null || !File.Exists(filepath))
            {
                Log.Error(this, "Unable to Read MapLocations " + filepath);
                return;
            }

            Log.Info(this, "Read " + filepath);

            using (CsvReader csv = new CsvReader(new StreamReader(filepath, Config.TextFileEncoding), true))
            {
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                int fieldCount = csv.FieldCount;
                string[] headers = csv.GetFieldHeaders();
                Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < fieldCount; i++)
                {
                    columns[headers[i]] = i; // track the index of each column name
                }

                while (csv.ReadNextRecord())
                {
                    if (csv[0].Trim() == String.Empty) continue;

                    ScenarioObjective objective = new ScenarioObjective();
                    objective.FromCsvLine(columns, csv, this);
                    try
                    {
                        objectives.Add(objective);
                    } catch (System.ArgumentException e) {
                        Log.Error(this, "[MapLocations] "+ e.Message);
                    }
                }
            }
        }

        #endregion


        public void ReadScreen(string filepath)
        {
            screenMessages.Clear();
            if (filepath == null || !File.Exists(filepath))
            {
                Log.Error(this, "Unable to Read Screen " + filepath);
                return;
            }
            ScreenReader.ReadScreen(screenMessages, filepath);
            Log.Info(this, "ScreenReader read " + screenMessages.Count + " messages");

            // foreach (KeyValuePair<string, ScreenMessage> kv in screenMessages)
            // {
            //     Console.WriteLine("Msg: " + kv.Key + " " + kv.Value.contents.Length);
            // }
        }




        public void ReadBattleScript(string filepath)
        {
            if (filepath == null || !File.Exists(filepath))
            {
                Log.Error(this, "Unable to Read BattleScript " + filepath);
                return;
            }
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            ReadBattleScript(stream);
            stream.Close();
        }


        public void ReadBattleScript(Stream stream)
        {
            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }

            Log.Info(this, "ReadBattleScript \"" + streamName + "\"");


            _battleScript.Clear();

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
                );
            csv.MissingFieldAction = MissingFieldAction.ReplaceByEmpty;

            Dictionary<string, int> headerLUT;

            if (csv.GetFieldHeaders().Length == 8) {
                Log.Info(this, "Using new battlescript headers");
                headerLUT = new Dictionary<string, int>();
                headerLUT["trigger"] = 0;
                headerLUT["id"] = 1;
                headerLUT["command"] = 2;
                headerLUT["fromid"] = 3;
                headerLUT["x"] = 4;
                headerLUT["y"] = 5;
                headerLUT["timevar"] = 6;
                headerLUT["description"] = 7;                
            } else {
                Log.Error(this, "Using old battlescript headers");
                headerLUT = new Dictionary<string, int>();
                headerLUT["trigger"] = 0;
                headerLUT["id"] = 1;
                headerLUT["command"] = 2;
                headerLUT["x"] = 3;
                headerLUT["y"] = 4;
                headerLUT["timevar"] = 5;
                headerLUT["description"] = 6;
            }

            BattleScriptEvent blockStartEvent = null;
            while (csv.ReadNextRecord())
            {
                if (csv[0] == String.Empty) continue;

                string trigger = csv.ToString(headerLUT, "trigger", streamName);
                string description = String.Empty;

                // sometimes lazy people forget to add the description column to the headers
                // this is ignored by the game, so we need to be able to ignore it too
                if (csv.FieldCount > headerLUT["description"])
                {
                    description = csv.ToString(headerLUT, "description", streamName);
                }
                string commandString = csv.ToString(headerLUT, "command", streamName);

                float x = csv[headerLUT["x"]] == String.Empty ? float.NegativeInfinity :  csv.ToSingle(headerLUT, "x", streamName);
                float y = csv[headerLUT["y"]] == String.Empty ? float.NegativeInfinity : csv.ToSingle(headerLUT, "y", streamName);

                int timeVar = csv.ToInt32(headerLUT, "timevar", streamName);

                object thing = null;
                string thingId = csv.ToString(headerLUT, "id", streamName);

                if (thingId != String.Empty)
                {
                    // Console.WriteLine("Thing ID:"+thingId);

                    ScenarioUnit unit;
                    if ( TryGetUnitByIdOrName(thingId, out unit) ) 
                    {
                        // Console.WriteLine("Thing is unit:"+unit);
                        thing = unit;
                    } else {
                        ScenarioObjective objective;
                        if (objectives.TryGetValue(thingId,out objective)) {
                            thing = objective;
                        }
                    }

                    if (thing == null) {
                        Log.Error(this, "Battlescript event \""+trigger+"\": Unable to find game object with ID \""+thingId+"\"");
                        foreach (ScenarioObjective objective in objectives.Values) {
                            Console.WriteLine("ScenarioObjective: "+objective);
                        }
                    }

                }
                else
                {
                    thing = null;
                    // unit = null;
                }

                ScenarioUnit fromUnit = null;
                if (headerLUT.ContainsKey("fromid")) {
                    string fromid = csv.ToString(headerLUT, "fromid", streamName);
                    if (fromid != String.Empty && !TryGetUnitByIdOrName(fromid, out fromUnit) ) {
                        Log.Error(_battleScript, "Could not find fromUnit from id "+fromid);
                    }
                }

                // Generic Events
                string commandName = String.Empty;
                Stack<string> leftoverArgs = new Stack<string>();

                CommandTemplate commandTemplate;
                Command command = null;
                List<string> commandParts = new List<string>(commandString.Split(new char[] { ':', '=' }));
                if (commandString != String.Empty && commandParts.Count > 0)
                {

                    // some args may be part of the command name, so walk backwards to build the command name
                    while (commandParts.Count > 0)
                    {

                        //string bit = commandParts.Dequeue();
                        //if (commandName != String.Empty){
                        //    commandName += ":";
                        //}
                        //commandName += bit;
                        //Console.WriteLine("commandName? "+commandName);
                        commandName = String.Join(":", commandParts);
                        if (config.commandTemplates.ContainsKey(commandName)) break;

                        string bit = commandParts[commandParts.Count - 1]; 
                        commandParts.RemoveAt(commandParts.Count - 1);
                        leftoverArgs.Push(bit);
  
                    }

                    //leftoverArgs = commandParts.ToArray();

                    //if (commandName == String.Empty) break;
                    if (config.commandTemplates.ContainsKey(commandName))
                    {
                        try
                        {
                            commandTemplate = config.commandTemplates[commandName];
                            Log.Info(this, "CommandTemplate calling: " + trigger + " " + command + " " + commandName + " " + thing);
                            command = commandTemplate.Create(thing, leftoverArgs.ToArray(), fromUnit, x, y, this);
                        }
                        catch (Exception e)
                        {
                            Log.Error(this, "ReadBattleScript bad command \"" + commandString + "\" "+e.Message+" .Will use Command Type \"BlindCommand\"");
                            command = new BlindCommand(commandString, thing, fromUnit, x, y, this);
                        }
                    }
                    else
                    {
                        Log.Warn(this, "ReadBattleScript unknown command \"" + commandString + "\" using Command Type \"BlindCommand\"");
                        command = new BlindCommand(commandString, thing, fromUnit, x, y, this);
                    }

                }

                // Special Events
                TimeSpan timeSpan;
                BattleScriptEvent bEvent;

                if (trigger.StartsWith("evtran"))
                {
                    string randomEventName = trigger.Substring(6, trigger.Length - 7);

                    NamedEvent namedEvent;
                    if (! battleScript.namedEvents.TryGetValue(randomEventName, out namedEvent)) {
                        namedEvent = new NamedEvent(randomEventName);
                        battleScript.namedEvents[randomEventName] = namedEvent;
                        Log.Warn(this, "ReadBattleScript Found Unknown NamedEvent '" + randomEventName + "'");

                    }

                        bEvent = new RandomEvent(
                            timeVar, description,
                            namedEvent,
                            command, thing as ScenarioUnit
                            );

                }
                else if (Sky.TryParseTimeSpan(trigger, out timeSpan))
                {
                    bEvent = new TimeEvent(timeVar, description, timeSpan, command, thing as ScenarioUnit);
                    Console.WriteLine("bEvent:"+bEvent);
                }
                else
                {
                    EventTemplate eventTemplate;

                    if (config.eventTemplates.ContainsKey(trigger))
                    {
                        eventTemplate = config.eventTemplates[trigger];
                    } else {
                        //Make a guess
                        
                        Type type = null;

                        if (thing is ScenarioUnit) type = typeof(UnitEvent);
                        else if (thing is ScenarioObjective) type = typeof(ScenarioObjectiveEvent);
                        

                        eventTemplate = new EventTemplate(type, trigger, "Unknown Event type");
                        Log.Warn(this, "ReadBattleScript unknown event \"" + trigger + "\" using Event Type \"" + eventTemplate+"\"");
                        config.eventTemplates[name] = eventTemplate;
                        
                    }

                        bEvent = eventTemplate.Create(timeVar, description, trigger, thing, command, fromUnit, x, y);

                        if (bEvent.GetType() == typeof(ContinueEvent) )
                        {
                            ((ContinueEvent)bEvent).parent = blockStartEvent;
                        }
                    //}

                    //else
                    //{
                    //    Log.Warn(this, "ReadBattleScript Using Unknown Event Type '" + trigger);


                    //    //Log.Error(this, "ReadBattleScript Skipping Unknown Event Type '" + trigger);
                    //    //continue;
                    //}

                }

                if (bEvent.GetType() != typeof(ContinueEvent))
                {
                    blockStartEvent = bEvent;
                }
                _battleScript.events.Add(bEvent);

            }


        }



        #region Save

        public override bool CanSafelySave()
        {
            return !Directory.Exists(scenarioDir);
        }


        public void AutoSave()
        {
            Mod realMod = mod;
            string realName = name;

            mod = config.userMod;
            name = "EditorAutoSave";

            Save();

            mod = realMod;
            name = realName;
        }

        public override void Save()
        {
            if (!Directory.Exists(scenarioDir))
            {
                Directory.CreateDirectory(scenarioDir);
            }


            Log.Info(this, "Saving " + scenarioDir);

            Log.Info(this, "Writing: \"" + Path.Combine(scenarioDir, "scenario.csv") + "\"");
            using (StreamWriter outfile = new StreamWriter(Path.Combine(scenarioDir, "scenario.csv"), false, Config.TextFileEncoding))
            {
                outfile.Write(ScenarioCsv());
                outfile.Flush();
            }


            if (_ini == null)
            {
                WriteFilteredFileFromTemplate(@"scenario.ini");
            }
            else
            {

                _ini.SetValue("map", "init", map == null ? "" : map.name);

                if (playerEchelon != null) {
                    _ini.SetValue("cmdlvl1", "rank", playerEchelon.sideIndex);
                    _ini.SetValue("cmdlvl2", "rank", playerEchelon.armyIndex);
                    _ini.SetValue("cmdlvl3", "rank", playerEchelon.corpsIndex);
                    _ini.SetValue("cmdlvl4", "rank", playerEchelon.divisionIndex);
                    _ini.SetValue("cmdlvl5", "rank", playerEchelon.brigadeIndex);
                    _ini.SetValue("cmdlvl6", "rank", playerEchelon.regimentIndex);
                } else {
                    _ini.SetValue("cmdlvl1", "rank", 0);
                    _ini.SetValue("cmdlvl2", "rank", 0);
                    _ini.SetValue("cmdlvl3", "rank", 0);
                    _ini.SetValue("cmdlvl4", "rank", 0);
                    _ini.SetValue("cmdlvl5", "rank", 0);
                    _ini.SetValue("cmdlvl6", "rank", 0);
                }

                _ini.SetValue("weather", "init", (int)initialWeather);
                _ini.SetValue("starttime", "init", startTime == null ? "" : startTime.ToString());

                _ini.SetValue("sandbox", "init", (int)sandbox);
                _ini.SetValue("strategicai", "init", strategicAI ? 1 : 0);
                _ini.SetValue("carryover", "init", carryover ? 1 : 0);

                WriteTextFile(_ini.Pretty(), @"scenario.ini");
            }


            if (intro_txt == null)
            {
                try
                {
                    WriteFilteredFileFromTemplate(@"EnglishScenIntro.txt");
                }
                catch (Exception e)
                {
                    Log.Error(this, e.Message);
                    string tmp = ReadDefaultTextFile(@"EnglishScenIntro.txt");
                    WriteTextFile(intro_txt, "EnglishScenIntro.txt");
                }
            }
            else
            {
                WriteTextFile(intro_txt, "EnglishScenIntro.txt");
            }
            WriteScreens();
            WriteBattleScript();
            WriteMapLocatons();

            isDirty = false;
        }

        void WriteFilteredFileFromTemplate(string resourceName )
        {
            string filepath = Path.Combine("Scenario", resourceName);
            IniFilter scenarioFilter = new IniFilter(this);

            Log.Info(this, "Writing Filtered File: \"" + filepath + "\"");

            StreamReader reader = null;
            StreamWriter writer = null;
            if (playerEchelon == null) playerEchelon = (ScenarioEchelon)root.FirstUsedEchelon();
            try
            {
                reader = config.GetResourceStreamReader(filepath);
                writer = new StreamWriter(Path.Combine(scenarioDir, resourceName), false, Config.TextFileEncoding);

                writer.Write(
                    ObjectFormatter.TokenFormat(reader.ReadToEnd(), new { map, playerEchelon, scenario = scenarioFilter })
                );
            }
            finally
            {
                if (reader != null) reader.Close();
                if (writer != null) writer.Close();
            }
        }

        void WriteTextFile(string text, string filename)
        {

            string filepath = Path.Combine(scenarioDir, filename);
            if (File.Exists(filepath))
            {
                Log.Info(this, "Overwriting existing: \"" + filepath + "\"");
            }
            else
            {
                Log.Info(this, "Writing: \"" + filepath + "\"");
            }

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(filepath, false, Config.TextFileEncoding);

                writer.Write(text);
            }
            finally
            {
                if (writer != null) writer.Close();
            }

        }


        void WriteScreens()
        {

            string filepath = Path.Combine(scenarioDir, "EnglishScenScreen.txt");
            if (File.Exists(filepath))
            {
                Log.Info(this, "Overwriting existing: \"" + filepath + "\"");
            }
            else
            {
                Log.Info(this, "Writing: \"" + filepath + "\"");
            }

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(filepath, false, Config.TextFileEncoding);
                foreach (ScreenMessage screenMessage in screenMessages.Values)
                {
                    writer.Write(String.Format("${0} {1}{2}", screenMessage.id, screenMessage.contents, Config.NewLine));
                }

            }
            finally
            {
                if (writer != null) writer.Close();
            }

        }

        void WriteBattleScript()
        {

            string filepath = Path.Combine(scenarioDir, "battlescript.csv");
            if (File.Exists(filepath))
            {
                Log.Info(this, "Overwriting existing: \"" + filepath + "\"");
            }
            else
            {
                Log.Info(this, "Writing: \"" + filepath + "\"");
            }

            StreamWriter writer = null;
 

            try
            {

                writer = new StreamWriter(filepath, false, Config.TextFileEncoding);
                writer.Write(battleScript.Csv());

            }
            finally
            {
                if (writer != null) writer.Close();
            }

        }

        void WriteMapLocatons()
        {

            string filepath = Path.Combine(scenarioDir, "maplocations.csv");
            if (File.Exists(filepath))
            {
                Log.Info(this, "Overwriting existing: \"" + filepath + "\"");
            }
            else
            {
                Log.Info(this, "Writing: \"" + filepath + "\"");
            }

            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(filepath, false, Config.TextFileEncoding);
                writer.WriteLine("Objective Name,Objective ID,Major/Minor,Type,AI 1=valid for AI 0=Player only,loc x,loc z,radius (yds),# of Men needed,Points,Fatigue Bonus,Morale Bonus,Ammo Bonus,Occupied Modifier,Start Time H:M,End Time H:M,Interval,Sprite,Army1,Army2,Army3");

                
                //foreach (ScenarioObjective objective in objectives)
                List<string> keys = new List<string>(objectives.Keys);
                keys.Sort();
                foreach (string key in keys)
                {
                    writer.WriteLine( objectives[key].ToCsv());
                }
            }
            finally
            {
                if (writer != null) writer.Close();
            }

        }

        #endregion



        class IniFilter
        {
            /// remaps values to correct string format
            Scenario scenario;
            public IniFilter(Scenario scenario)
            {
                // for processing a scenario to output strings
                this.scenario = scenario;
            }

            public string name
            {
                get { return scenario.name.ToASCII(); }
            }

            public string startTime
            {
                get
                {
                    return Sky.ToCsv(scenario.startTime);
                }
            }

            public string endTime
            {
                get
                {
                    return Sky.ToCsv(scenario.endTime);
                }
            }

            public string sunrise
            {
                get
                {
                    return Sky.ToCsv(scenario.sunrise);
                }
            }

            public string sunset
            {
                get
                {
                    return Sky.ToCsv(scenario.sunset);
                }
            }

            public string duration
            {
                get { return String.Format("{0} minutes", (scenario.endTime - scenario.startTime).TotalMinutes); }
            }

            public int initialWeather
            {
                get { return (int)scenario.initialWeather; }
            }

            public string prettyDateTime
            {
                get { return scenario.date.ToString(Sky.formatDateTimePretty); }

            }

            public string playerCommand
            {
                get
                {
                    if (scenario.playerEchelon == null) return "None";
                    return
                        scenario.playerEchelon.unit.name2 == string.Empty ? scenario.playerEchelon.unit.name1.ToASCII() : scenario.playerEchelon.unit.name2.ToASCII();
                        

                }
            }

            public string map
            {
                get { return scenario.map == null ? "None" : scenario.map.niceName; }
            }
        }

    }

}
