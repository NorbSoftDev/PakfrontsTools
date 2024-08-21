using System;
using System.Collections;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Linq;

namespace NorbSoftDev.SOW {


    public class OrderOfBattle : UnitRoster<OOBUnit,OOBEchelon>, ILoad {
        public bool hasLoaded { get; internal set; }
        // these are here as backup if Resources/OOB/headers does not exist
        //static string[] defaultOOBHeaders = new string[] {
        //    "userName" , "id", "name1", "name2", 
        //    "sideIndex", "armyIndex", "corpsIndex", "divisionIndex", "brigadeIndex", "regimentIndex",
        //    "unitClass",
        //    "portrait", "weapon", "ammo", "flag", "flag2", "formation", "headCount",
        //    "initiative","leadership","loyalty","ability",
        //    "style", "experience",
        //    "fatigue", "morale",
        //    "close", "open", "edged", 
        //    "firearm", "marksmanship","horsemanship", "surgeon", "callisthenics"
        //    };

        public string [] oobHeaders;

        //public bool isSandbox
        //{
        //    get
        //    {
        //        return (name.ToUpper() == "SANDBOXOOB");
        //    }

        //}

        public OrderOfBattle(Config config, Mod mod, string name, string [] oobHeaders) : base(config, mod, name) {
            // root = new OOBEchelonRoot();
            ClearRoot();
            hasLoaded = false;

            if (oobHeaders == null) {
                oobHeaders = config.headers.oob;
                //string filepath = Path.Combine("OOB","headers.csv");
                //try
                //{

                //    StreamReader reader = config.GetResourceStreamReader(filepath);
                //    CsvReader csv = new CsvReader(reader, true);
                //    Log.Info(this,"Using Resource headers \"" + filepath + "\"");
                //    oobHeaders = csv.GetFieldHeaders(); 

                //}
                //catch (System.IO.FileNotFoundException e)
                //{
                //    Log.Warn(this,"Using default headers, did not find Resource \"" + filepath + "\"");
                //    oobHeaders = defaultOOBHeaders;
                //}
            }

            this.oobHeaders = oobHeaders;

            int i = 0;
            foreach (string header in oobHeaders)
            {
                csvHeaderLUT[header] = i;
                i++;
            }

            foreach (string attributeName in config.attributes.Keys)
            {
                if (!csvHeaderLUT.ContainsKey(attributeName)) continue;
                Attribute attribute = config.attributes[attributeName];
                attributeNames.Add(attribute);
            }
        }

        public OrderOfBattle(Config config, Mod mod, string name) : this(config, mod, name, null) {
            // root = new OOBEchelonRoot();
            // ClearRoot();
            // hasLoaded = false;
            // // define mapping of headers to properties and attributes
            // // this sets the column order in the csvs for reading and writing
            // string filepath = Path.Combine("OOB","headers.csv");
            // try
            // {

            //     StreamReader reader = config.GetResourceStreamReader(filepath);
            //     CsvReader csv = new CsvReader(reader, true);
            //     Log.Info(this,"Using Resource headers \"" + filepath + "\"");
            //     oobHeaders = csv.GetFieldHeaders(); 

            // }
            // catch (System.IO.FileNotFoundException e)
            // {
            //     Log.Warn(this,"Using default headers, did not find Resource \"" + filepath + "\"");
            //     oobHeaders = defaultOOBHeaders;
            // }


            // int i = 0;
            // foreach (string header in oobHeaders)
            // {
            //     csvHeaderLUT[header] = i;
            //     i++;
            // }

            // foreach (string attributeName in config.attributes.Keys)
            // {
            //     if (!csvHeaderLUT.ContainsKey(attributeName)) continue;
            //     Attribute attribute = config.attributes[attributeName];
            //     attributes.Add(attribute);
            // }
        }

        protected override void ClearRoot() {
            root = new OOBEchelonRoot();

        }

        //public void AddUnit(OOBEchelon echelon, OOBUnit unit) {
        //    Add(unit);
        //    echelon.Add(unit);
        //    unit.echelon = echelon;
        //}

        public string orderOfBattleFile { get { return System.IO.Path.Combine(mod.orderOfBattleDir.FullName,name+".csv"); }}

        public void PreLoad() {
            //no dependencies
        }

        public void Load() {
            if (hasLoaded)
            {
                Log.Error(this,"Already Loaded "+name+" "+this.GetHashCode());
                throw new Exception(this + " already Loaded");
            }
            ReadCsv(orderOfBattleFile);
            hasLoaded = true;
            isDirty = false;
            Log.Info(this,"Load "+name+" "+this.GetHashCode());
        }

        internal OOBUnit CreateUnit(long echelonId, TemporaryEchelonTable<OOBEchelon> tmpTable, string id)
        {
            OOBEchelon echelon = tmpTable.ConjureEchelon(echelonId);
            OOBUnit unit = new OOBUnit(id);
            echelon.Add(unit);
            this.Add(unit);
            return unit;
        }



        public OOBEchelon CreateChild(OOBEchelon echelon)
        {
            OOBUnit newUnit;

            int cnt = 0;
            if (echelon.children.Count > 0 && echelon.children[0].unit != null)
            {
                string newId = GenerateId(echelon.children[0].unit.id);
                newUnit = ((OOBUnit)echelon.children[0].unit).ShallowCopy(newId);
            }
            else
            {
                string idHint = echelon.unit == null ? echelon.unit.id + "_" : null;
                string newId = GenerateId(idHint);
                newUnit = new OOBUnit(newId);
            }

            OOBEchelon newEchelon = new OOBEchelon();

            newEchelon.parent = echelon;
            newEchelon.Add(newUnit);
            this.Add(newUnit);
            return newEchelon;

        }

        public void ReadCsv(string filepath)
        {
            Log.Info(this,"[OOB] ReadCsv \"" + filepath + "\"");
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            ReadCsv(stream);
            stream.Close();

        }

        public void ReadCsv(Stream stream)
        {
            CreateOrgFromCsv(stream, oobHeaders);
            ReadUnitDataFromCsv(stream, oobHeaders );
        }


        //Reads from a stream and rewinds. Up to caller to close.
        public void ReadUnitDataFromCsv(Stream stream, string [] headers)
        {
            ReadUnitDataFromCsv( stream, headers, null);
        }

        public void ReadUnitDataFromCsv(Stream stream, string [] headers, List<OOBUnit> unitFilter) {
            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null) {
                streamName = fs.Name;
            }
            Log.Info(this,"ReadUnitDataFromCsv \"" + streamName + "\"");
                    
            int count = 0, nReranks = 0;

            // StartLocs uses the name1, rather than ID, so we have to build a map
            // Dictionary<string, OOBUnit> unitsByName = new Dictionary<string, OOBUnit>();
            // foreach (OOBUnit unit in _unitsById.Values)
            // {
            //     unitsByName[unit.name1] = unit;
            // }

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
               headerLUT[headers[i]] = i;
            }


            TemporaryOOBEchelonTable tmpTable = new TemporaryOOBEchelonTable(root);

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



                OOBUnit unit = null;

                //try
                {


                    if ( headerLUT.ContainsKey("id") ) {
                        unit = this[ csv[headerLUT["id"]] ];
                    } else if ( headerLUT.ContainsKey("userName") ) {
                        string userName = csv[headerLUT["userName"]];
                        unit = this.GetUnitByName1(userName);
                    } else {
                        Log.Error(this,"CSV does not contain ids or userNames: "+streamName);
                        throw new Exception("CSV does not contain ids or userNames: "+streamName);
                    }

                    // Unit Filter
                    if (unitFilter != null) {
                        if (unitFilter.Count < 1) return;

                        if (! unitFilter.Contains(unit))
                            continue;

                        Console.WriteLine("Apply Filter to "+unit.id);
                    }

                    string where = unit.id + " in " + streamName;

                    if (csvHeaderLUT.ContainsKey("userName"))
                        unit.userName = csv.ToString(csvHeaderLUT, "userName", where);
                    else
                        unit.userName = unit.id;


                    unit.name1 = csv.ToString(csvHeaderLUT, "name1", where);
                    unit.name2 = csv.ToString(csvHeaderLUT, "name2", where);


                    unit.unitClass = csv.GetValue<UnitClass>(csvHeaderLUT, config.unitClasses, "unitClass");

                    //portrait is weird, it is the xy coord in a specially named file that matches the echelon level
                    unit.portrait = csv[csvHeaderLUT["portrait"]];
                    
                    // unit.portrait = config.graphics[csv[i++]];

                    unit.weapon = csv.GetValueAllowEmpty<Weapon>(csvHeaderLUT, config.weapons, "weapon");
                    unit.ammo = csv.ToInt32(csvHeaderLUT, "ammo", where);

                    //unit.flag = config.graphics.GetValueAllowEmpty(csv,csvHeaderLUT,"flag", where);
                    //unit.flag2 = config.graphics.GetValueAllowEmpty(csv, csvHeaderLUT, "flag2", where);

                    // Flags are no longer in gfx.csv, but are in mygui xml, so we have to put in placeholders or nulls
                    string flag1Key =  csv[csvHeaderLUT["flag"]];
                    if (flag1Key != string.Empty)
                    {
                        Graphic flag1Graphic;
                        if (!config.graphics.TryGetValue(flag1Key, out flag1Graphic))
                        {
                            flag1Graphic = new Graphic();
                            flag1Graphic.id = flag1Key;
                            config.graphics[flag1Key] = flag1Graphic;
                        }
                        unit.flag = flag1Graphic;
                    }


                    string flag2Key = csv[csvHeaderLUT["flag2"]];
                    if (flag2Key != string.Empty)
                    {
                        Graphic flag2Graphic;
                        if (!config.graphics.TryGetValue(flag2Key, out flag2Graphic))
                        {
                            flag2Graphic = new Graphic();
                            flag2Graphic.id = flag2Key;
                            config.graphics[flag2Key] = flag2Graphic;
                        }
                        unit.flag2 = flag2Graphic;
                    }

                    unit.formation = config.formations.GetValueAllowEmpty(csv, csvHeaderLUT, "formation", where);
                    unit.headCount = csv.ToInt32(csvHeaderLUT, "headCount", where);

                    foreach (Attribute attribute in attributeNames) {
                        int column = csvHeaderLUT[attribute.name];
                        if ( csv[column] == String.Empty || csv[column] == null  ) {
                            unit.attributes[attribute.name] = null;
                            continue;
                        }
                        int level = csv.ToInt32(csvHeaderLUT, attribute.name, where); 
                        //choose a level
                        try {
                            unit.attributes[attribute.name] = attribute[level];
                        } catch (System.ArgumentOutOfRangeException e) {
                            Log.Warn(this,"[OOB] " + unit.id + " Attribute out-of-range: " + attribute.name + " column:" + column + " level:" + level);
                            foreach (AttributeLevel alevel in attribute) {
                                Log.Info(this,"  "+alevel.index+" "+alevel);
                            }
                            unit.attributes[attribute.name] = null;
                        }
                    }

                    count++;

                }

                //catch (Exception)
                //{
                //    string summary = "csvHeaderLUT: ";
                //    foreach (KeyValuePair<string, int> kvp in csvHeaderLUT)
                //    {
                //        summary += " " + kvp.Key + ":" + kvp.Value;
                //    }
                //    Log.Error(this, summary);

                //    string id = unit == null ? "INVALID" : unit.id;
                //    Log.Error(this, "ReadUnitDataFromCsv failed on entry " + count + " id:'" + id + "' '" + csv[0] + " '" + streamName + "'");
                //    throw;
                //}
            }
            Log.Info(this,"Read " + count + " units from CSV");
            stream.Position = 0;
        }

        public void CreateOrgFromCsv(Stream stream, string [] headers)
        {
            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null) {
                streamName = fs.Name;
            }
            Log.Info(this,"CreateOrgFromCsv \"" + streamName + "\"");


            int count = 0, nReranks = 0;

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
               headerLUT[headers[i]] = i;
            }

            TemporaryOOBEchelonTable tmpTable = new TemporaryOOBEchelonTable(root);

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

                string id = "INVALID";
                try {
                    id = csv[csvHeaderLUT["id"]];

                    string where = id + " " + streamName;

                    long echelonId = EchelonHelper.ComposeEchelonId(
                        csv.ToInt32(csvHeaderLUT, "sideIndex", where),
                        csv.ToInt32(csvHeaderLUT, "armyIndex", where),
                        csv.ToInt32(csvHeaderLUT, "corpsIndex", where),
                        csv.ToInt32(csvHeaderLUT, "divisionIndex", where),
                        csv.ToInt32(csvHeaderLUT, "brigadeIndex", where),
                        csv.ToInt32(csvHeaderLUT, "regimentIndex", where) 
                        );

                    OOBUnit unit = CreateUnit(echelonId, tmpTable, id );

                    if (csvHeaderLUT.ContainsKey("name1")) {
                        // Sometimes things are indexd by name rather than id, so good to have if possible
                        unit.name1 = csv.ToString(csvHeaderLUT, "name1", where);
                    }

                    count++;

                } catch (Exception e) {
                    string summary = "csvHeaderLUT: ";
                    foreach (KeyValuePair<string,int>  kvp in csvHeaderLUT) {
                        summary += " "+kvp.Key+":"+kvp.Value;
                    }
                    Log.Error(this,summary);
                    Log.Error(this,"CreateOrgFromCsv failed on entry "+count+" id:'"+id+"' '"+streamName+"'");
                    throw;
                }                
            }        
                
            Log.Info(this,"Read " + count + " units from "+streamName);
            stream.Position = 0;
        }

        public void ReorgFromCsv(Stream stream, string [] headers)
        {
            ReorgFromCsv(stream, headers, null);
        }

        public void ReorgFromCsv(Stream stream, string [] headers, List<OOBEchelon> echelonFilter)
        {    

            string streamName = stream.ToString();
            FileStream fs = stream as FileStream;
            if (fs != null) {
                streamName = fs.Name;
            }
            Log.Info(this,"Org from Unit Data \"" + streamName + "\"");


            int count = 0, nReranks = 0;

            // StartLocs uses the name1, rather than ID, so we have to build a map
            // Dictionary<string, OOBUnit> unitsByName = new Dictionary<string, OOBUnit>();
            // foreach (OOBUnit unit in _unitsById.Values)
            // {
            //     unitsByName[unit.name1] = unit;
            // }

            //build LUT for index by name
            Dictionary<string, int> headerLUT = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
               headerLUT[headers[i]] = i;
            }

            TemporaryOOBEchelonTable tmpTable = new TemporaryOOBEchelonTable(root);

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

                try
                {

                    OOBUnit ounit;
                    if ( headerLUT.ContainsKey("id") ) {
                        ounit = this[ csv[headerLUT["id"]] ];
                    } else if ( headerLUT.ContainsKey("userName") ) {
                        string userName = csv[headerLUT["userName"]];
                        ounit = this.GetUnitByName1(userName);
                    } else {
                        Log.Error(this,"CSV does not contain ids or userNames: "+streamName);
                        throw new Exception("CSV does not contain ids or userNames: "+streamName);
                    }

                    string where = ounit.id + " in " + streamName;

                   long echelonId = EchelonHelper.ComposeEchelonId(
                        csv.ToInt32(csvHeaderLUT, "sideIndex", where),
                        csv.ToInt32(csvHeaderLUT, "armyIndex", where),
                        csv.ToInt32(csvHeaderLUT, "corpsIndex", where),
                        csv.ToInt32(csvHeaderLUT, "divisionIndex", where),
                        csv.ToInt32(csvHeaderLUT, "brigadeIndex", where),
                        csv.ToInt32(csvHeaderLUT, "regimentIndex", where)
                        );

                    OOBEchelon echelon = tmpTable.ConjureEchelon(echelonId);

                    if (echelonFilter != null) {
                        if (! echelonFilter.Contains(echelon)) continue;
                        Console.WriteLine("Apply Filter to Echelon "+echelon.id);
                    }

                    if (ounit.oobEchelon.parent != echelon.parent) {
                        Console.WriteLine("Parent current:"+ounit.oobEchelon.parent+" correct:"+echelon.parent );
                        ounit.oobEchelon.parent = echelon.parent;
                    }

                    count++;

                } catch (Exception e) {
                   Log.Error(this,"Read failed on id:'"+csv[1]+"' '"+csv[0]+" '"+streamName+"'");
                   throw;
                }
            }
        
                
            Log.Info(this,"Read " + count + " units from "+streamName);
            stream.Position = 0;
        }


        #region Save

        public override bool CanSafelySave()
        {
            return !File.Exists(orderOfBattleFile);
        }

        public override void Save() {

            Log.Info(this,"Saving \""+orderOfBattleFile+"\"");

            DirectoryInfo dir = (new FileInfo(orderOfBattleFile)).Directory;

            if (! dir.Exists)
            {
                dir.Create();
            }

            using (StreamWriter outfile = new StreamWriter(orderOfBattleFile, false, Config.TextFileEncoding) )
            {
                outfile.Write( OrderOfBattleCsv());
                outfile.Flush();
            }

            isDirty = false;

        }

        public override void ExportAsOOB(string filename)
        {

            Log.Info(this, "Exporting OOB \"" + filename + "\"");

            using (StreamWriter outfile = new StreamWriter(filename, false, Config.TextFileEncoding))
            {
                outfile.Write(OrderOfBattleCsv(oobHeaders));
                outfile.Flush();
            }

        }

        public string UnitCsvHeaderLine(string[] headers)
        {
            string line = "";


            foreach (string header in headers)
            {
                line += header + ",";
            }


            line = line.Substring(0, line.Length - 1);
            return line;
        }

        public string OrderOfBattleCsv()
        {
            return OrderOfBattleCsv(oobHeaders);
        }

        public string OrderOfBattleCsv(string[] headers)
        {
            string lines = string.Empty;

            lines += UnitCsvHeaderLine(headers) + Config.NewLine;

            // "Name,ID,SIDE,ARMY,CORPS,DIV,BGDE,REG,AMMO ,dir x,dir z,loc x,loc z,Formation,Head Count,Fatigue,Morale" + Config.NewLine
            // + ",idstring,[0/1],[0/100],[0/100],[0/100],[0/100],[0/100],[0/100000],[-1/1],[-1/1],[mapmin/mapmax],[mapmin/mapmax],idstring,[1/1000],[0/6],[0/9]" + Config.NewLine
            
            foreach (OOBEchelon child in root)
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
        public string EchelonCsv(OOBEchelon echelon, string [] headers)
        {
            string lines = String.Empty;

            foreach (OOBUnit unit in echelon.units)
            {
                lines += UnitCsvLine(unit, headers);
                lines += Config.NewLine;
            }

            foreach (OOBEchelon child in echelon)
            {
                lines += EchelonCsv(child, headers);
            }
            return lines;
        }
        #endregion
      
    }



}