using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NorbSoftDev.SOW
{
    class FormationReader
    {

        delegate FormationMethodDelegate FormationMethodDelegate();
        Config config;
        Formation currentFormation;
        Formation previousFormation;
        int currentRow;
        string definedIn;
        CsvReader csv;
        bool inError = false;
        Dictionary<string,bool> hasBeenRead = new Dictionary<string,bool>();


        public static void ReadFormations(Config config, string filepath)
        {
            FormationReader reader = new FormationReader();
            reader.StartReadFormations(config, filepath);

        }

        void StartReadFormations(Config config, string filepath) {
          
            if (filepath == null)
            {
                return;
            }
            this.config = config;

            config.logisticsFiles.Add(filepath);
            definedIn = filepath;

            Stream instream = null;
            FormationMethodDelegate formationMethod = null;

            Log.Info(this,"Reading Pass 1 \"" + filepath + "\"");
            instream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 

            // Stream outstream = instream;

            // hack the instream so the first line contains a lot of commas, so we get a big header list from csv reader
            Stream outstream = new MemoryStream();
            StreamReader sr = new StreamReader(instream, Config.TextFileEncoding);
            StreamWriter sw = new StreamWriter(outstream);

            bool isHeader = true;
            string test = "";
            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (isHeader)
                {
                    line += ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";
                    isHeader = false;
                }
                test += line + "\n";
            }

            //horrible, but works more reliably than copying from stream to stream
            byte[] byteArray = Encoding.ASCII.GetBytes( test );
            outstream = new MemoryStream( byteArray ); 

            //First pass to get all in dict
            {
                csv = new CsvReader(new StreamReader(outstream, Config.TextFileEncoding), true);
                Log.Info(this, "Max columns = "+csv.FieldCount); 
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                currentFormation = null; //new Formation();
                previousFormation = null;
                currentRow = 0;
                inError = false;


                while (csv.ReadNextRecord())
                {
                    if (formationMethod == null && csv[0] != String.Empty)
                    {
                        //past the comments
                        formationMethod = ReadFormationInitPass1;
                    }

                    if (formationMethod == null) continue;

                    //try {  
                    formationMethod = formationMethod();
                    //} catch (Exception e) {
                    //    if (formation == null ) Log.Error(this,"Read failed on null formation '"+csv[0]+"'' #"+formations.Count);
                    //    else Log.Error(this,"Read failed on '"+formation.id+"'' '"+csv[0]+"'' #"+formations.Count);
                    //    foreach (Delegate d in formationMethod.GetInvocationList()) {
                    //        Log.Error(this," Invoked: "+d.Method.Name);
                    //    }
                    //    throw;
                    //}

                }
            }


            Log.Info(this,"Completed Pass 1, "+hasBeenRead.Count+" formations");
            // Environment.Exit(0);


            outstream.Position = 0;

            Log.Info(this,"Reading Pass 2 \"" + filepath + "\"");
            formationMethod = null;
            //Second pass to get all defined
            {
                // csv.MoveTo(0);
                csv = new CsvReader(new StreamReader(outstream, Config.TextFileEncoding), true);
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;


                currentFormation = null; //new Formation();
                previousFormation = null;
                currentRow = 0;
                inError = false;

                while (csv.ReadNextRecord())
                {

                // Console.WriteLine("foo");
                    if (formationMethod == null && csv[0] != String.Empty)
                    {
                        //past the comments
                        formationMethod = ReadFormationInitPass2;
                    }

                    if (formationMethod == null) continue;

                    //try {  
                    formationMethod = formationMethod();
                    //} catch (Exception e) {


                    //    if (formation == null ) 
                    //        Log.Error(this,"Read failed on null formation '"+csv[0]+"'' #"+formations.Count);
                    //    else 
                    //        Log.Error(this,"Read failed on Formation: '"+formation.id+"'(#"+formations.Count+") row:"+row+" '"+csv[0]+"'");
                    //    foreach (Delegate d in formationMethod.GetInvocationList()) {
                    //        Log.Error(this,"Invoked: " + d.Method.Name);
                    //    }
                    //    throw;
                    //}
                }

            }

            Log.Info(this,"count " + config.formations.Count);
            outstream.Close();

            //} finally {
            //    if (stream != null)
            //      ((IDisposable)stream).Dispose();
            //}

        }
        /// <summary>
        /// Just inits id and size and adds to dict
        /// </summary>
        /// <param name="definedIn"></param>
        /// <param name="csv"></param>
        /// <param name="formation"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        FormationMethodDelegate ReadFormationInitPass1()
        {
            int i = 0;
            if (csv[0] == String.Empty)
            {
                return ReadFormationInitPass1;
            }

            Formation nformation = new Formation(config);
            // Log.Info(this,"Creating Formation "+csv[0]);
            nformation.definedIn = definedIn;

            string[] properties = new string[] {
              "userName", "id", "rows", "columns"
            };

            config.PopulateFromCsvLine(nformation, csv, properties, definedIn);

            if (nformation.id == null)
            {
                string csvstr = "";
                for (int c = 0; c < csv.FieldCount; c++)
                {
                    csvstr += "," + csv[c];
                }
                Log.Error(this, "ReadFormationInitPass1 failed to parse formation id "+csvstr);
                return ReadFormationInitPass1;
            }

            bool isDigitsOnly = true;
            foreach (char c in nformation.id)
            {
                if (c < '0' || c > '9')
                {
                    isDigitsOnly = false;
                    break;
                }
            }

            if (isDigitsOnly)
            {
                if (! inError) {

                    Log.Error(this,previousFormation.id+" expected "+previousFormation.rows+" rows, but got more. Skipping rows starting with "+csv[0]+","+csv[1]+" ... "+definedIn);
                }
                inError = true;
                return ReadFormationInitPass1;
            }

            if (nformation.rows < 1 || nformation.columns < 1)
            {
                if (! inError) Log.Error(this,"Illegal Formation col,rows" + nformation.id + " after "+previousFormation.id+". "+definedIn);
                inError = true;
                return ReadFormationInitPass1;
            }

            inError = false;

            currentFormation = nformation;
            currentFormation.ResetNiceName(config);

            // Console.WriteLine("F:"+currentFormation.id+" "+currentFormation.rows+"x"+currentFormation.columns);

            if (hasBeenRead.ContainsKey(currentFormation.id)) {
                Log.Error(this, currentFormation.id+" defined multiple times");
            }

            config.formations[currentFormation.id] = currentFormation;
            hasBeenRead[currentFormation.id] = false;
            //read row
            currentRow = 1;

            return ReadFormationBodyPass1;
        }

        /// <summary>
        /// skips rows til next formation
        /// </summary>
        /// <param name="definedIn"></param>
        /// <param name="csv"></param>
        /// <param name="formation"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        FormationMethodDelegate ReadFormationBodyPass1()
        {
            //read row
            currentRow++;
            if (currentRow > currentFormation.rows)
            {
                previousFormation = currentFormation;
                currentFormation = null;
                return ReadFormationInitPass1;
            }
            return ReadFormationBodyPass1;
        }

        /// <summary>
        /// fills in data for existing formation
        /// </summary>
        /// <param name="definedIn"></param>
        /// <param name="csv"></param>
        /// <param name="formation"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        FormationMethodDelegate ReadFormationInitPass2()
        {
            int i = 0;

            if (csv[0] == String.Empty)
            {
                return ReadFormationInitPass2;
            }

            string id = csv[1];
            if (!config.formations.ContainsKey(id))
            {
                if (! inError) Log.Debug(this,"Skipping unknown Formation id:" + id);
                inError = true;
                return ReadFormationInitPass2;
            }

            if (hasBeenRead[id]) {
                Log.Error(this, id+"read a Second time");
            }
            hasBeenRead[id] = true;

            inError = false;
            currentFormation = config.formations[id];

            string[] properties = new string[] {
              null, null, null, null, "rowDistYds", "colDistYds", "subformation",
              "keepFormation", "canWheel", "canFight", "moveRateModifier", "canAboutFace",
              "artilleryFormation", "minEnemyYds", "fireModifier", "meleeModifier",
              "cantMove", "cantCounterCharge"
            };

            config.PopulateFromCsvLine(currentFormation, csv, properties, definedIn);

            //formation.ResetNiceName(this);
            // Log.Info(this,"Created Formation "+formation.id);
            //formations[formation.id] = formation;
            //Console.WriteLine(formation.id+" sub:"+formation.subformation);




            //read row

            currentRow = 1;

            return ReadFormationBodyPass2;
        }


        /// <summary>
        /// populates formation slots
        /// </summary>
        /// <param name="definedIn"></param>
        /// <param name="csv"></param>
        /// <param name="formation"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        FormationMethodDelegate ReadFormationBodyPass2()
        {
            //for (int i = 0; i < csv.FieldCount; i++ )
            int ncols = currentFormation.columns;

            if (currentFormation == null)
            {
                Log.Error(this,"Got null currentFormation in ReadFormationBodyPass2");
                currentRow++;
                return ReadFormationInitPass2;
            }

            if (ncols > csv.FieldCount)
            {
                Log.Warn(this,"" + currentFormation.id + " row " + currentRow + " not enough columns. Expected " + currentFormation.columns + " got " + csv.FieldCount+" "+definedIn);
                ncols = csv.FieldCount;
            }

            for (int i = 0; i < ncols; i++)
            {
                if (csv[i] == null) continue;
                string str = csv[i].Trim();

                if (str == null || str == String.Empty) continue;
                //Console.WriteLine("'" + str + "'");
                // row minus one since fisrt row is init data
                // Console.WriteLine("Have "+i+" "+str);

                Formation.Slot slot = new Formation.Slot(csv[i], i, currentRow - 1, config, currentFormation);
                //Console.WriteLine("slot:"+str+":"+slot.man);
            }

            currentRow++;

            //done with currentFormation
            if (currentRow > currentFormation.rows)
            {
                //clean the array
                List<Formation.Slot> tmp = new List<Formation.Slot>();
                int firstEmpty = -1;
                int lastEntry = 0;
                for (int i = 0; i < currentFormation.slots.Length; i++)
                {

                    if (currentFormation.slots[i] == null)
                    {
                        if (firstEmpty < 0) firstEmpty = i;
                        continue;
                    }

                    tmp.Add(currentFormation.slots[i]);
                    lastEntry = i;
                }

                if (firstEmpty > -1 && firstEmpty < lastEntry)
                {
                    string missing = "";
                    for (int i = 0; i < lastEntry; i++)
                    {
                        if (currentFormation.slots[i] == null)
                            missing += " "+(i + 1).ToString();
                    }
                    Log.Error(this,currentFormation.id + " is missing men:"+missing+" "+definedIn);

                    // Console.WriteLine("Exiting test");
                    // Environment.Exit(0);
                }

                currentFormation.slots = tmp.ToArray();
                //Log.Debug(this,"Completed " + currentFormation.id);
                previousFormation = currentFormation;
                currentFormation = null;
                return ReadFormationInitPass2;
            }
            return ReadFormationBodyPass2;
        }
    }
}
