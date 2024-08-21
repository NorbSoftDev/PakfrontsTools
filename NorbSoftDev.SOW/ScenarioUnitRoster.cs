using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;

namespace NorbSoftDev.SOW
{

    public class ScenarioUnitRoster : UnitRoster<ScenarioUnit, ScenarioEchelon>
    {

        // these are here as backup if Resources/Scenario/headers does not exist


        //static string[] defaultScenarioHeaders = new string[] {
        //    "userName", "id",
        //    "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", "battalionIndex",
        //    "ammo",
        //    "dirSouth", "dirEast", "south", "east",
        //    "formation", "headCount",
        //    "fatigue", "morale"
        //    };

        //static string[] defaultSandboxScenarioHeaders = new string[] {
        //    "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", "battalionIndex",
        //    "id", "name1", "name2", 
        //    "unitClass",
        //    "portrait", "weapon", "ammo", 
        //    "dirSouth", "dirEast", "south", "east",
        //    "flag", "flag2", "formation", "headCount",
        //    "ability", "command", "control", "leadership", "style", "experience",
        //    "fatigue", "morale",
        //    "close order proficiency", "open order proficiency", "edged weapon proficiency", 
        //    "Firearm Proficiency", "Marksmanship","Horsemanship", "Surgeon Ability", "Calisthenics"
        //};

        //Name,Ammo,dir x,dir z,loc x,loc z,Formation,Head Count,Fatigue,Morale,10:03:30
        // dumped by l key in game
        //public static string[] defaultStartLocsHeaders = new string[] {
        //    "id", "ammo", "dirSouth", "dirEast", "south", "east",  "formation", "headCount", "fatigue", "morale"
        //    };

        // dumped by k key in game
        //public static string[] defaultGameDBHeaders = new string[] {
        //    "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex", // "battalionIndex",
        //    "id",
        //    "ammo", "status",
        //    "deserted", "killed", "wounded",
        //    "r1", "r2", "r3", "r4", "r5", "r6",
        //    "killerName", "casualties"
        //};

        public string[] scenarioHeaders;

        // Dictionary<string,int> csvHeaderLUT = new Dictionary<string,int>();


        OrderOfBattle _orderOfBattle;
        public OrderOfBattle orderOfBattle
        {
            get { return _orderOfBattle; }
            protected set
            {
                _orderOfBattle = value;
                _orderOfBattle.CollectionChanged += OnOOBChanged;
            }

        }

        protected long _preloadPlayerUnitEchelonId;
        public ScenarioEchelon playerEchelon
        {
            get
            {
                return _playerEchelon;
            }
            set
            {
                _playerEchelon = value;
                isDirty = true;
                OnPropertyChanged("playerEchelon");
            }
        }
        ScenarioEchelon _playerEchelon;

        public ScenarioUnitRoster(Config config, Mod mod, string name)
            : base(config, mod, name)
        {
            ClearRoot();

            scenarioHeaders = config.headers.scenario;

            int i = 0;
            foreach (string header in scenarioHeaders)
            {
                csvHeaderLUT[header] = i;
                i++;
            }

            // define attributes based on template in Resources
            foreach (string attributeName in config.attributes.Keys)
            {
                // Sandbox contains more, but real scenarios have less
                if (!csvHeaderLUT.ContainsKey(attributeName)) continue;

                Attribute attribute = config.attributes[attributeName];
                attributeNames.Add(attribute);
            }

        }

        /// <summary>
        /// An empty Scenario based on the given OrderOfBattle
        /// </summary>
        /// <param name="orderOfBattle"></param>
        public ScenarioUnitRoster(OrderOfBattle orderOfBattle, bool createFromOOB)
            : this(orderOfBattle.config, orderOfBattle.config.userMod, "UnnamedScenario")
        {
            this.orderOfBattle = orderOfBattle;
            if (createFromOOB)
            {
                Log.Info(this,"Creating from orderOfBattle " + this.orderOfBattle.GetHashCode());
                if (!this.orderOfBattle.hasLoaded) this.orderOfBattle.Load();

                PopulateUnitsFromOrderOfBattle();
            }
        }

        public ScenarioUnitRoster(OrderOfBattle orderOfBattle) : this(orderOfBattle, true) { }

        //public ScenarioUnitRoster(ScenarioUnitRoster other) : base(other.config, other.mod, other.name) {
        //    this.orderOfBattle = other.orderOfBattle;
        //}

        protected override void ClearRoot()
        {
            root = new ScenarioEchelonRoot();

        }

        public Scenario ShallowCopy()
        {
            return (Scenario)this.MemberwiseClone();
        }

        void OnOOBChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Get the sender observable collection
            if (sender != orderOfBattle)
            {
                throw new Exception("Got CollectionChanged from other OOB");
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                List<ScenarioEchelon> toRemove = new List<ScenarioEchelon>();
                foreach (OOBUnit oldu in e.OldItems)
                {
                    foreach (ScenarioUnit su in this)
                    {
                        if (su.refUnit == oldu)
                        {
                            toRemove.Add(su.scenarioEchelon);
                        }
                    }
                }

                foreach (ScenarioEchelon se in toRemove)
                {
                    RemoveEchelon(se);
                }
            }
        }


        bool _isSandbox;
        public bool isSandbox
        {
            get { return _isSandbox; }
            set
            {
                _isSandbox = value;
                if (_isSandbox)
                {
                    //scenarioHeaders = defaultSandboxScenarioHeaders;
                    scenarioHeaders = config.headers.sandbox;
                }
                else
                {
                    scenarioHeaders = config.headers.scenario;
                    //string filepath = Path.Combine("Scenario", "scenario.csv");
                    //try
                    //{

                    //    StreamReader reader = config.GetResourceStreamReader(filepath);
                    //    CsvReader csv = new CsvReader(reader, true);
                    //    Log.Info(this,"Using Resource headers \"" + filepath + "\"");
                    //    scenarioHeaders = csv.GetFieldHeaders();

                    //}
                    //catch (System.IO.FileNotFoundException e)
                    //{
                    //    Log.Warn(this,"Using default headers, did not find Resource \"" + filepath + "\"");
                    //    scenarioHeaders = defaultScenarioHeaders;
                    //}
                }

                //crete index to header mappings
                int i = 0;
                foreach (string header in scenarioHeaders)
                {
                    csvHeaderLUT[header] = i;
                    i++;
                }

                // define attributes


                Dictionary<string, Attribute> definedattrs = new Dictionary<string, Attribute>();

                foreach (Attribute attribute in attributeNames)
                {
                    definedattrs[attribute.name] = attribute;
                }

                foreach (string attributeName in config.attributes.Keys)
                {
                    if (!csvHeaderLUT.ContainsKey(attributeName)) continue;
                    if (definedattrs.ContainsKey(attributeName)) continue;
                    Attribute attribute = config.attributes[attributeName];
                    attributeNames.Add(attribute);
                }

            }
        }


        public void ReadCsv(string filepath)
        {
            Log.Info(this,"ReadCsv \"" + filepath + "\"");

            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            TestScenarioType(stream);
            CreateOrgFromCsv(stream, scenarioHeaders);
            ReadUnitDataFromCsv(stream, scenarioHeaders);
            stream.Close();
        }


        public void TestScenarioType(Stream stream)
        {
            string streamName = stream.ToString() + ":" + stream.GetHashCode();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }
            Log.Info(this,"TestScenarioType \"" + streamName + "\"");

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
            );

            string firstHeader = csv.GetFieldHeaders()[0];
            bool misSandbox = firstHeader == "SANDBOXOOB";
            isSandbox = misSandbox;
            stream.Position = 0;
            return;
        }


        public void ReadStartLocsCsv(string filepath, IList<ScenarioUnit> unitFilter)
        {
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ReadUnitDataFromCsv(stream, config.headers.startLocs, unitFilter);
            stream.Close();
        }

        // this needs math to happen inside
        //public void ReadGameDBCsv(string filepath, List<ScenarioUnit> unitFilter)
        //{
        //    FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    ReadUnitDataFromCsv(stream, defaultGameDBHeaders, unitFilter);
        //    stream.Close();
        //}

        public void ReadUnitDataFromCsv(Stream stream, string[] headers)
        {
            ReadUnitDataFromCsv(stream, headers, null);
        }

        public void ReadUnitDataFromCsv(Stream stream, string[] headers, IList<ScenarioUnit> unitFilter)
        {
            string streamName = stream.ToString() + ":" + stream.GetHashCode();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }
            Log.Info(this,"ReadUnitDataFromCsv \"" + streamName + "\"");

            int count = 0;

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerLUT[headers[i]] = i;
            }


            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
            );

            while (csv.ReadNextRecord())
            {

                if (csv[0] == String.Empty)
                {
                    continue;
                }

                if (string.Equals( csv[0], "MASTER", StringComparison.OrdinalIgnoreCase) )
                {
                    if (orderOfBattle == null)
                    {
                        Log.Info(this,"ReadUnitDataFromCsv orderOfBattle is null, reading " + csv[1]);
                        orderOfBattle = config.GetOrderOfBattle(csv[1].Substring(0, csv[1].Length - 4));
                    }
                    continue;
                }

                try
                {
                    if (csv[1] == null || csv[1] == string.Empty)
                    {
                        continue;
                    }
                }
                catch (System.Resources.MissingManifestResourceException e)
                {
                    //raised by null or bad fields
                    continue;
                }

                try
                {

                    ScenarioUnit sunit;
                    string unitkey;

                    if (headerLUT.ContainsKey("id"))
                    {
                        unitkey = csv[headerLUT["id"]];
                    }
                    else if (headerLUT.ContainsKey("userName"))
                    {
                        unitkey = csv[headerLUT["userName"]];
                    }
                    else
                    {
                        Log.Error(this,"CSV does not contain ids or userNames: " + streamName);
                        throw new Exception("CSV does not contain ids or userNames: " + streamName);
                    }

                    if ( ! this.TryGetUnitByIdOrName( unitkey, out sunit) ) {
                        Log.Warn(this, "Could not find unit \"" + unitkey + "\" in \""+this.name+"\"");
                        continue;
                    }


                    // Unit Filter
                    if (unitFilter != null)
                    {
                        if (unitFilter.Count < 1) return;

                        if (!unitFilter.Contains(sunit))
                            continue;

                        Console.WriteLine("Apply Filter to " + sunit.id);

                    }
                    string where = sunit.id + " in " + streamName;


                    if (headerLUT.ContainsKey("ammo"))
                        sunit.ammo = csv.ToInt32(headerLUT, "ammo", where);

                    if (headerLUT.ContainsKey("dirSouth"))
                        sunit.transform.Set(
                            csv.ToSingle(headerLUT, "dirSouth", where),
                            csv.ToSingle(headerLUT, "dirEast", where),
                            csv.ToSingle(headerLUT, "south", where),
                            csv.ToSingle(headerLUT, "east", where)
                            );

                    if (headerLUT.ContainsKey("formation"))
                        sunit.formation = csv.GetValueAllowEmpty(headerLUT, config.formations, "formation");

                    if (headerLUT.ContainsKey("headCount"))
                        sunit.headCount = csv.ToInt32(headerLUT, "headCount", where);


                    foreach (Attribute attribute in attributeNames)
                    {
                        if (!headerLUT.ContainsKey(attribute.name))
                        {
                            continue;
                        }

                        int column;
                        try
                        {
                            column = headerLUT[attribute.name];
                        }
                        catch (System.Collections.Generic.KeyNotFoundException)
                        {
                            Log.Error(this, attribute.name+" not in header");
                            throw;
                        }

                        try
                        {
                            if (csv[column] == String.Empty)
                            {
                                sunit.attributes[attribute.name] = null;
                                continue;
                            }
                        }
                        catch (System.ArgumentOutOfRangeException e)
                        {
                            Log.Error(this,"\"" + sunit.id + "\" attribute \"" + attribute.name + "\" column " + column + " is out of csv range of " + csv.FieldCount);
                            throw;
                        }

                        int level = csv.ToInt32(headerLUT, attribute.name, where);

                        try
                        {
                            sunit.attributes[attribute.name] = attribute[level];
                        }
                        catch (System.ArgumentOutOfRangeException e)
                        {
                            Log.Warn(this,"" + sunit.id + " Attribute out-of-range: " + attribute.name + " column:" + column + " level:" + level);
                            foreach (AttributeLevel alevel in attribute)
                            {
                                Log.Info(this,"  " + alevel.index + " " + alevel);
                            }
                            sunit.attributes[attribute.name] = null;
                        }
                    }

                    count++;

                }
                catch (Exception e)
                {
                    string summary = "csvHeaderLUT: ";
                    foreach (KeyValuePair<string, int> kvp in csvHeaderLUT)
                    {
                        summary += " " + kvp.Key + ":" + kvp.Value;
                    }
                    Log.Error(this,summary);
                    Log.Error(this,"ReadUnitDataFromCsv failed on entry " + count + " '" + streamName + "'");
                    // Log.Error(this,"OrderOfBattle CreateOrgFromCsv failed on entry "+count+" id:'"+sunit.id+"' '"+streamName+"'");
                    throw e;
                }
            }
            Log.Info(this,"Read " + count + " units from CSV");
            stream.Position = 0;
           }


        public void CreateOrgFromCsv(Stream stream, string[] headers)
        {
            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }
            Log.Info(this,"CreateOrgFromCsv \"" + streamName + "\"");


            if (isSandbox)
            {
                orderOfBattle = new OrderOfBattle(config, mod, "SANDBOXOOB", scenarioHeaders);
                orderOfBattle.ReadCsv(stream);
                stream.Position = 0;
            }

            int count = 0, nReranks = 0;

            // StartLocs uses the name1, rather than ID, so we have to build a map
            // Dictionary<string, ScenarioUnit> unitsByName = new Dictionary<string, ScenarioUnit>();
            // foreach (ScenarioUnit unit in _unitsById.Values)
            // {
            //     unitsByName[unit.name1] = unit;
            // }

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerLUT[headers[i]] = i;
            }

            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
                );

            while (csv.ReadNextRecord())
            {

                if (csv[0] == String.Empty)
                {
                    continue;
                }


                if (csv[0].ToUpper() == "MASTER")
                {
                    // handle cases where trailing .csv is missing
                    string oobFilename = csv[1];
                    int idx = oobFilename.LastIndexOf('.');
                    string oobName = oobFilename;
                    if (idx > -1)
                    {
                        oobName = oobFilename.Substring(0, idx);
                    }

                    orderOfBattle = config.GetOrderOfBattle(oobFilename);
                    continue;
                }

                string id = csv[csvHeaderLUT["id"]];
                if (this.Contains(id))
                {
                    Log.Error(this, "Skipping duplicate unit \"" +id + "\" "+streamName);
                    continue;
                }

                string where = id + " in " + streamName;
                OOBUnit oobunit = orderOfBattle[id];

                long echelonId = EchelonHelper.ComposeEchelonId(
                    csv.ToInt32(headerLUT, "sideIndex", where),
                    csv.ToInt32(headerLUT, "armyIndex", where),
                    csv.ToInt32(headerLUT, "corpsIndex", where),
                    csv.ToInt32(headerLUT, "divisionIndex", where),
                    csv.ToInt32(headerLUT, "brigadeIndex", where),
                    csv.ToInt32(headerLUT, "regimentIndex", where),
                    csv.ToInt32(headerLUT, "battalionIndex", where)
                    );


                ScenarioUnit sunit = CreateUnit(echelonId, tmpTable, oobunit);
                if (echelonId == _preloadPlayerUnitEchelonId) playerEchelon = sunit.scenarioEchelon;


                //ERROR WARNING
                if (sunit.echelon.rank != oobunit.echelon.rank)
                {
                    Log.Warn(this,"" + oobunit.id + " has Rank of " + oobunit.echelon.rank + " in OOB but Rank " + sunit.echelon.rank + " in Scenario");
                    nReranks++;
                }

                // error checking to see if there is a parent in the scenario
                if (sunit.echelon.rank < ERank.Army)
                {
                    if (oobunit.oobEchelon.parent != null && sunit.scenarioEchelon.parent == null)
                    {
                        Log.Warn(this,"Odd null parent for " + sunit + ", expected " + oobunit.oobEchelon.parent);
                    }
                }

                count++;

                //} catch (Exception e) {
                //    Log.Error(this,"Scenario read failed on id:'"+csv[1]+"' '"+csv[0]);//+" column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
                //    throw;
                //}
            }



            Log.Info(this,"Read " + count + " units from " + streamName);
            stream.Position = 0;
        }

        public void ReorgFromCsv(Stream stream, string[] headers)
        {
            ReorgFromCsv(stream, headers, null);
        }

        public void ReorgFromCsv(Stream stream, string[] headers, List<ScenarioEchelon> echelonFilter)
        {

            string streamName = stream.ToString() + ":" + stream.GetHashCode();
            FileStream fs = stream as FileStream;
            if (fs != null)
            {
                streamName = fs.Name;
            }
            Log.Info(this,"ReorgFromCsv \"" + streamName + "\"");


            int count = 0, nReranks = 0;

            // StartLocs uses the name1, rather than ID, so we have to build a map
            // Dictionary<string, ScenarioUnit> unitsByName = new Dictionary<string, ScenarioUnit>();
            // foreach (ScenarioUnit unit in _unitsById.Values)
            // {
            //     unitsByName[unit.name1] = unit;
            // }

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerLUT[headers[i]] = i;
            }

            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            CsvReader csv = new CsvReader(
                new StreamReader(stream, Config.TextFileEncoding), true,
                CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape,
                '$', ValueTrimmingOptions.UnquotedOnly, CsvReader.DefaultBufferSize
                );

            //new CsvReader(new StreamReader(filepath), true)) {
            //string[] headers = csv.GetFieldHeaders();


            while (csv.ReadNextRecord())
            {

                if (csv[0] == String.Empty)
                {
                    continue;
                }


                if (csv[0].ToUpper() == "MASTER")
                {
                    if (orderOfBattle == null)
                    {
                        Log.Info(this,"ReorgFromCsv orderOfBattle is null, reading " + csv[1]);
                        orderOfBattle = config.GetOrderOfBattle(csv[1].Substring(0, csv[1].Length - 4));
                    }
                    continue;
                }

                try
                {

                    ScenarioUnit sunit;
                    if (headerLUT.ContainsKey("id"))
                    {
                        sunit = this[csv[headerLUT["id"]]];
                    }
                    else if (headerLUT.ContainsKey("userName"))
                    {
                        string userName = csv[headerLUT["userName"]];
                        sunit = this.GetUnitByName1(userName); ;
                    }
                    else
                    {
                        Log.Error(this,"CSV does not contain ids or userNames: " + streamName);
                        throw new Exception("CSV does not contain ids or userNames: " + streamName);
                    }

                    string where = sunit.id + " in " + streamName;

                    long echelonId = EchelonHelper.ComposeEchelonId(
                         csv.ToInt32(csvHeaderLUT, "sideIndex", where),
                         csv.ToInt32(csvHeaderLUT, "armyIndex", where),
                         csv.ToInt32(csvHeaderLUT, "corpsIndex", where),
                         csv.ToInt32(csvHeaderLUT, "divisionIndex", where),
                         csv.ToInt32(csvHeaderLUT, "brigadeIndex", where),
                         csv.ToInt32(csvHeaderLUT, "regimentIndex", where),
                         csv.ToInt32(csvHeaderLUT, "battalionIndex", where)
                         );

                    ScenarioEchelon echelon = tmpTable.ConjureEchelon(echelonId);

                    if (echelonFilter != null)
                    {
                        Log.Error(this,"ReorgeEchelonFilter does not work!");
                        throw new Exception("ReorgeEchelonFilter does not work!");
                        // if (! echelonFilter.Contains(echelon)) continue;
                        // Console.WriteLine("Apply Filter to Echelon "+echelon.id);

                    }

                    if (sunit.scenarioEchelon.parent != echelon.parent)
                    {
                        Console.WriteLine("Parent current:" + sunit.scenarioEchelon.parent + " correct:" + echelon.parent);
                        sunit.scenarioEchelon.parent = echelon.parent;
                    }
                    //echelon.Add(sunit);

                    count++;

                }
                catch (Exception e)
                {
                    Log.Error(this,"Scenario read failed on id:'" + csv[1] + "' '" + csv[0] + " '" + streamName + "'");
                    throw e;
                }
            }


            Log.Info(this,"Read " + count + " units from " + streamName);
            stream.Position = 0;
        }


        #region Populate
        protected virtual void FromDefaults() {
            //nothing
        }

        public void PopulateUnitsFromOrderOfBattle()
        {
            Log.Info(this,"Composing from " + orderOfBattle.mod + " OOB \"" + orderOfBattle.name + "\" " + orderOfBattle.GetHashCode());

            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            //Better to traverse root
            Console.WriteLine("PopulateFromOrderOfBattle");
            foreach (OOBUnit oobunit in orderOfBattle)
            {
                // always use the first on the list
                if (tmpTable.ContainsKey(oobunit.echelon.id))
                {
                    continue;
                }

                ScenarioUnit sunit = CreateUnit(oobunit.echelon.id, tmpTable, oobunit);

                if (sunit.attributes.Count < 1)
                {
                    throw new Exception("attributes not set");
                }

            }

            playerEchelon = (ScenarioEchelon)root.FirstUsedEchelon();
            Log.Info(this,"Found first leader " + mod + " \"" + playerEchelon.unit + "\"");

            this.FromDefaults();

            isDirty = true;
            Log.Info(this,"Composed " + mod + " " + name + "\"");

        }



        public void PopulateEchelonsFromOrderOfBattle()
        {
            Log.Info(this, "Composing from " + orderOfBattle.mod + " OOB \"" + orderOfBattle.name + "\" " + orderOfBattle.GetHashCode());

            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            //Better to traverse root
            Console.WriteLine("PopulateEchelonsFromOrderOfBattle");
            foreach (OOBUnit oobunit in orderOfBattle)
            {
                // always use the first on the list
                if (tmpTable.ContainsKey(oobunit.echelon.id))
                {
                    continue;
                }

                CreateEchelon(oobunit.echelon.id, tmpTable);

            }

            playerEchelon = (ScenarioEchelon)root.FirstUsedEchelon();

            this.FromDefaults();

            isDirty = true;
            Log.Info(this, "Composed " + mod + " " + name + "\"");

        }

        public void PopulateUnitsFromScenarioUnitRoster(ScenarioUnitRoster other)
        {
            Log.Info(this,"Composing from " + other.mod + " OOB \"" + other.name + "\" " + other.GetHashCode());

            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            //Better to traverse root
            Console.WriteLine("PopulateUnitsFromScenarioUnitRoster");
            foreach (ScenarioUnit otherunit in other)
            {
                // always use the first on the list
                if (tmpTable.ContainsKey(otherunit.echelon.id))
                {
                    continue;
                }

                ScenarioUnit sunit = CreateUnit(otherunit.echelon.id, tmpTable, otherunit);

                if (sunit.attributes.Count < 1)
                {
                    throw new Exception("attributes not set");
                }

            }

            playerEchelon = other.playerEchelon;

            //this.FromDefaults();

            isDirty = true;
            Log.Info(this,"Composed " + mod + " " + name + "\"");

        }

        // public ScenarioUnit MoveTo(ScenarioUnit origUnit) {

        //     ScenarioUnit newUnit = InsertUnitWithChildren(origUnit);
        //     ScenarioEchelon origEchelon = (ScenarioEchelon)origUnit.echelon;
        //     //origEchelon.parent.Remove(origEchelon);
        //     origEchelon.RemoveFromRoster();
        //     return newUnit;
        // }


        public ScenarioUnit InsertUnitWithChildren(OOBUnit oobUnit)
        {
            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            return InsertUnitWithChildren(oobUnit, tmpTable, null);
        }


        public ScenarioUnit InsertUnitWithChildren(OOBUnit oobUnit, ScenarioEchelon parent)
        {
            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);
            return InsertUnitWithChildren(oobUnit, tmpTable, parent);
        }

        /// <summary>
        /// Inserts a Unit into th scenario,creating empty parents if no existing parent, 
        /// and creating childreon if none exist in scenario yet. 
        /// If children exist, it will adopt them, and recursion will stop
        /// </summary>
        /// <param name="oobUnit"></param>
        /// <param name="tmpTable"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal ScenarioUnit InsertUnitWithChildren(OOBUnit oobUnit, TemporaryScenarioEchelonTable tmpTable, ScenarioEchelon parent)
        {


            OOBEchelon oobEchelon = oobUnit.oobEchelon;
            if (oobEchelon.root.roster != this.orderOfBattle)
            {
                throw new RosterAddException("Cannot add unit from other OOB:" + oobEchelon.root.roster);
            }

            ScenarioUnit unit;

            if (_unitsById.TryGetValue(oobUnit.id, out unit))
            {
                foreach (OOBEchelon child in oobEchelon.children)
                {
                    InsertUnitWithChildren(child.unit, tmpTable, unit.scenarioEchelon);
                }
                return unit;
            }


            //determine new parent
            //first see if parent exists
            //then conjure
            if (parent == null)
            {
                unit = CreateUnit(oobEchelon.id, tmpTable, oobUnit);
                foreach (OOBEchelon child in oobEchelon.children)
                {
                    InsertUnitWithChildren(child.unit, tmpTable, null);
                }
                return unit;
            }

            // ScenarioEchelon echelon = new ScenarioEchelon();
            // echelon.parent = parent;
            // ScenarioUnit unit = new ScenarioUnit(oobUnit);
            // echelon.Add(unit);
            // this.Add(unit);

            unit = CreateUnitInParent(parent, oobUnit);
            foreach (OOBEchelon child in oobEchelon.children)
            {
                InsertUnitWithChildren(child.unit, tmpTable, unit.scenarioEchelon);
            }

            return unit;
        }

        public ScenarioUnit InsertUnit(OOBUnit oobUnit) {
             return InsertUnit( oobUnit, null);
        }

        /// <summary>
        /// returns exising if needed
        /// </summary>
        /// <param name="oobUnit"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ScenarioUnit InsertUnit(OOBUnit oobUnit,  ScenarioEchelon parent)
        {
            OOBEchelon oobEchelon = oobUnit.oobEchelon;

            if (oobEchelon.root.roster != this.orderOfBattle)
            {
                throw new RosterAddException("Cannot add unit "+oobUnit.id+" from other OOB:" + oobEchelon.root.roster);
            }

            ScenarioUnit unit;
            if (_unitsById.TryGetValue(oobUnit.id, out unit))
            {
                //throw new RosterAddException("Unit " + oobUnit.id + " already exists in Scenario:" + this);
                return unit;
            }


            //determine new parent
            //first see if parent exists
            //then conjure
            if (parent == null)
            {
                TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);
                unit = CreateUnit(oobEchelon.id, tmpTable, oobUnit);
                return unit;
            }

            unit = CreateUnitInParent(parent, oobUnit);
            return unit;

        }

        internal ScenarioUnit CreateUnitInParent(ScenarioEchelon parent, OOBUnit oobUnit)
        {
            ScenarioEchelon echelon = new ScenarioEchelon();
            echelon.parent = parent;
            ScenarioUnit unit = new ScenarioUnit(oobUnit, attributeNames);
            echelon.Add(unit);
            this.Add(unit);
            return unit;
        }

        internal ScenarioUnit CreateUnitInParent(ScenarioEchelon parent, ScenarioUnit origUnit)
        {
            ScenarioEchelon echelon = new ScenarioEchelon();
            echelon.parent = parent;
            ScenarioUnit unit = new ScenarioUnit(origUnit, attributeNames);
            //ScenarioUnit unit = origUnit;

            echelon.Add(unit);
            this.Add(unit);
            return unit;
        }

        public ScenarioUnit InsertUnitWithChildren(ScenarioUnit scenarioUnit)
        {
            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);

            return InsertUnitWithChildren(scenarioUnit, tmpTable, null);
        }

        public ScenarioUnit InsertUnitWithChildren(ScenarioUnit scenarioUnit, ScenarioEchelon parent)
        {
            TemporaryScenarioEchelonTable tmpTable = new TemporaryScenarioEchelonTable(root);
            return InsertUnitWithChildren(scenarioUnit, tmpTable, parent);
        }



        internal ScenarioUnit InsertUnitWithChildren(ScenarioUnit origUnit, TemporaryScenarioEchelonTable tmpTable, ScenarioEchelon parent)
        {
            ScenarioEchelon origEchelon = origUnit.scenarioEchelon;

            if (((ScenarioUnitRoster)origEchelon.root.roster).orderOfBattle != this.orderOfBattle)
            {
                throw new RosterAddException("Cannot add unit from other OOB:" + origEchelon.root.roster);
            }

            if (_unitsById.ContainsKey(origUnit.id))
            {
                Log.Error(this, "Unit already exists in this Scenario:" + this);
                throw new RosterAddException("Unit already exists in this Scenario:" + this);
            }

            //determine new parent
            //first see if parent exists
            //then conjure
            if (parent == null)
            {
                ScenarioUnit newUnit = CreateUnit(origEchelon.id, tmpTable, origUnit);
                foreach (ScenarioEchelon child in origEchelon.children)
                {
                    InsertUnitWithChildren(child.unit, tmpTable, null);
                }
                return newUnit;
            }

            // ScenarioEchelon echelon = new ScenarioEchelon();
            // echelon.parent = parent;
            // ScenarioUnit unit = new ScenarioUnit(oobUnit);
            // echelon.Add(unit);
            // this.Add(unit);

            ScenarioUnit unit = CreateUnitInParent(parent, origUnit);
            foreach (ScenarioEchelon child in origEchelon.children)
            {
                InsertUnitWithChildren(child.unit, tmpTable, unit.scenarioEchelon);
            }

            return unit;
        }

        internal ScenarioEchelon CreateEchelon(long echelonId, TemporaryEchelonTable<ScenarioEchelon> tmpTable)
        {
            ScenarioEchelon echelon = tmpTable.ConjureEchelon(echelonId);

            long newId = echelon.id;

            this.Add(echelon);
            return echelon;
        }


        internal ScenarioUnit CreateUnit(long echelonId, TemporaryEchelonTable<ScenarioEchelon> tmpTable, OOBUnit oobUnit)
        {
            ScenarioEchelon echelon = tmpTable.ConjureEchelon(echelonId);
            ScenarioUnit unit = new ScenarioUnit(oobUnit, attributeNames);

            long newId = echelon.id;

            echelon.Add(unit);
            this.Add(unit);
            return unit;
        }

        internal ScenarioUnit CreateUnit(long echelonId, TemporaryEchelonTable<ScenarioEchelon> tmpTable, ScenarioUnit origUnit)
        {
            ScenarioEchelon echelon = tmpTable.ConjureEchelon(echelonId);
            ScenarioUnit unit = new ScenarioUnit(origUnit, attributeNames);

            echelon.Add(unit);
            this.Add(unit);
            return unit;
        }

        #endregion

       public void RecursiveRunFunc(ScenarioEchelon current, Func <ScenarioEchelon, bool> func)
       {

            if (func(current)) {
               return;
            }
           

           foreach (ScenarioEchelon child in current.children)
           {
               RecursiveRunFunc(child, func);
           }
       }


        #region Save

        public override bool CanSafelySave()
        {
            return true;
            //return !Directory.Exists(scenarioDir);
        }


        public override void Save()
        {

            string scenarioDir = Environment.CurrentDirectory;
            Log.Info(this,"Writing: \"" + Path.Combine(scenarioDir, "scenario.csv") + "\"");
            using (StreamWriter outfile = new StreamWriter(Path.Combine(scenarioDir, "scenario.csv"), false, Config.TextFileEncoding))
            {
                outfile.Write(ScenarioCsv());
                outfile.Flush();
            }
        }

        public override void ExportAsOOB(string filename)
        {

            Log.Info(this, "Exporting OOB \"" + filename + "\"");

            using (StreamWriter outfile = new StreamWriter(filename, false, Config.TextFileEncoding))
            {
                outfile.Write(OrderOfBattleCsv(orderOfBattle.oobHeaders));
                outfile.Flush();
            }

        }

        public string UnitCsvHeaderLine(string[] headers)
        {
            string line = "";

            int cnt = 0;
            foreach (string header in headers)
            {
                if (cnt == 0 && isSandbox)
                {
                    line += "SANDBOXOOB,";
                    cnt++;
                    continue;
                }
                line += header + ",";
                cnt++;
            }
            

            line = line.Substring(0, line.Length - 1);
            return line;
        }


        public string ScenarioCsv()
        {
            return ScenarioCsv(scenarioHeaders);
        }

        public string ScenarioCsv(string[] headers)
        {
            string lines = string.Empty;

            lines += UnitCsvHeaderLine(headers);
            lines += Config.NewLine;

            if ( ! isSandbox) {
                lines += "MASTER," + orderOfBattle.name + ".csv,,,,,,,,,,,,,,," + Config.NewLine;
            }

            foreach (ScenarioEchelon child in root)
            {
                lines += EchelonCsv(child, scenarioHeaders);
            }

            return lines;
        }

        public string OrderOfBattleCsv(string[] headers)
        {
            string lines = string.Empty;

            lines += UnitCsvHeaderLine(headers) + Config.NewLine;

            // "Name,ID,SIDE,ARMY,CORPS,DIV,BGDE,REG,AMMO ,dir x,dir z,loc x,loc z,Formation,Head Count,Fatigue,Morale" + Config.NewLine
            // + ",idstring,[0/1],[0/100],[0/100],[0/100],[0/100],[0/100],[0/100000],[-1/1],[-1/1],[mapmin/mapmax],[mapmin/mapmax],idstring,[1/1000],[0/6],[0/9]" + Config.NewLine

            foreach (ScenarioEchelon child in root)
            {
                lines += EchelonCsv(child, headers);
            }

            return lines;
        }

        /// <summary>
        /// recurisvely walk through and get csv formatted lines
        /// TODO a format string or delegate method
        /// </summary>
        /// <param name="echelon"></param>
        /// <returns></returns>
        public string EchelonCsv(ScenarioEchelon echelon, string [] headers)
        {
            string lines = String.Empty;

            if (echelon.unit != null)
            {
                lines += UnitCsvLine(echelon.unit, headers);
                lines += Config.NewLine;
            }

            foreach (ScenarioEchelon child in echelon)
            {
                lines += EchelonCsv(child, headers);
            }
            return lines;
        }
        #endregion
    }
}
