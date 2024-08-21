using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using System.Data;
using System.Xml;
namespace OOBToNames
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine();
            Console.WriteLine("[OOBToNames v1.1 - Convert OOB csv entris to XML]");

            string oobcsvpath = null;
            bool overwrite = false;

            if (args.Length > 0)
            {
                oobcsvpath = args[0];
            }


            if (oobcsvpath == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" Drag and Drop an OOB csv file here and press Enter");
                Console.ResetColor();
                string response = Console.ReadLine();
                Console.WriteLine("\"" + response + "\"");
                oobcsvpath = response;
            }

            if (oobcsvpath != null && oobcsvpath != String.Empty)
            {

                if (!File.Exists(oobcsvpath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(oobcsvpath);
                    Console.WriteLine("Error: First Argument must be valid csv file path");
                    Console.ResetColor();

                    Console.WriteLine(" Press any key to exit");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                DirectoryInfo modDirInfo = Directory.GetParent(Directory.GetParent(oobcsvpath).ToString());
                Console.WriteLine("Setting Mod Directory to " + modDirInfo.FullName);
                Directory.SetCurrentDirectory(modDirInfo.FullName);
            }



            if (!Directory.Exists("OOBs"))
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(Directory.GetCurrentDirectory() + " is not a valid OOB mod directory.");
                Console.ResetColor();
                Console.WriteLine(" Press any key to exit");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (oobcsvpath == null)
            {
                //Interactive 
                string[] files = Directory.GetFiles("OOBs", "*.csv");

                if (files.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: There are no csv files in OOBs directory");
                    Console.ResetColor();

                    Console.WriteLine(" Press any key to exit");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                Console.WriteLine("Enter the number of the OOB file to process:");

                for (int i = 0; i < files.Length; i++)
                {

                    Console.WriteLine("[" + (i + 1) + "] " + files[i]);
                }

                string response = Console.ReadLine();

                int choice;
                if (!int.TryParse(response.Trim(), out choice))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("I did not understand your input");
                    Console.WriteLine(" Press any key to exit");
                    Console.ResetColor();

                    Console.ReadKey();
                    Environment.Exit(1);
                }

                choice -= 1;

                if (choice < 0 || choice >= files.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Not a valid choice: " + choice);
                    Console.WriteLine("Please choose a number from 1 to " + files.Length);
                    Console.ResetColor();

                    Console.WriteLine(" Press any key to exit");
                    Console.ReadKey();
                    Environment.Exit(1);

                }

                oobcsvpath = files[choice];
                Console.WriteLine(oobcsvpath);

            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Overwrite names already defined in XML with those in the CSV?");
            Console.WriteLine("  Press 'Y' if you do, any other key for No.");
            Console.ResetColor();

            ConsoleKeyInfo info = Console.ReadKey();
            if (info.Key == ConsoleKey.Y)
            {
                overwrite = true;
            }
            Console.WriteLine();

            Console.WriteLine("overwrite = " + overwrite);

            // string oobcsvpath = Path.Combine(moddir, Path.Combine("OOBs","OOB_SB_ Friant vs Cooke _SR1.csv"));
            if (!File.Exists(oobcsvpath))
            {
                Console.WriteLine("OOB csv file does not exist:" + oobcsvpath);
                Environment.Exit(1);
            }

            XmlDocument xDoc = new XmlDocument();

            string LanguageEnglishMod_dir = @"Layout/Media/Language/";
            string LanguageEnglishMod_path = LanguageEnglishMod_dir+@"LanguageEnglishMod.xml";

            Dictionary<string, string> tagToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


            CreateBackup(LanguageEnglishMod_path);
            CreateBackup(oobcsvpath);

            if (File.Exists(LanguageEnglishMod_path))
            {


                Console.WriteLine("Reading " + LanguageEnglishMod_path);

                xDoc.Load(LanguageEnglishMod_path);
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "Tag")
                    {
                        string tagname = node.Attributes["name"].Value;
                        string value = node.InnerText;
                        tagToName[tagname] = value;
                    }
                }

                Console.WriteLine("Read " + tagToName.Count + " entries");
            }
   
            Encoding en = Encoding.GetEncoding(1252, new EncoderReplacementFallback(" "), new DecoderReplacementFallback(" "));
            string[] allLines = File.ReadAllLines(oobcsvpath, en);

            using (var sw = new StreamWriter(oobcsvpath, false, en))
            {

                int cnt = 0;

                foreach (string line in allLines)
                {
                    string[] data = line.Split(',');

                    //pass along
                    if (data.Length < 4)
                    {
                        sw.WriteLine(line);
                        continue;
                    }

                    string username = data[0];
                    string id = data[1];
                    string name1 = data[2];
                    string name2 = data[3];

                    if (id.ToUpper() == "ID")
                    {
                        sw.WriteLine(line);
                        continue;
                    }

                    string id_name1 = id;
                    string id_name2 = id + "_NAME_2";
                    string id_last = id + "_Last";

                    name1 = name1 == string.Empty ? null : name1;
                    name2 = name2 == string.Empty ? null : name2;

                    string last = name1 == null ? null : name1.Split(' ').Last();

                    if (name1 != null && (overwrite || !tagToName.ContainsKey(id_name1)))
                    {
                        // Console.WriteLine("Creating new entry for "+id_name1);
                        tagToName[id_name1] = name1;
                        cnt++;

                    }

                    if (last != null && (overwrite || !tagToName.ContainsKey(id_last)))
                    {
                        tagToName[id_last] = last;
                        cnt++;

                    }

                    if (name2 != null && (overwrite || !tagToName.ContainsKey(id_name2)))
                    {
                        tagToName[id_name2] = name2;
                        cnt++;
                    }

                    //update csv
                    name1 = tagToName.ContainsKey(id_name1) ? tagToName[id_name1] : string.Empty;
                    name2 = tagToName.ContainsKey(id_name2) ? tagToName[id_name2] : string.Empty;
                    last = tagToName.ContainsKey(last) ? tagToName[id_name1] : string.Empty;


                    data[2] = name1;
                    data[3] = name2;

                    sw.WriteLine(String.Join(",", data));

                }
                Console.WriteLine("Created " + cnt + " XML entries");
            }



            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.Encoding = Encoding.UTF8;


            // xmlWriterSettings.Encoding = new UTF8Encoding(false);
            // xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
            // xmlWriterSettings.Indent = true;


            
            if (!Directory.Exists(LanguageEnglishMod_dir)) {
                Console.WriteLine("Creating Directory "+LanguageEnglishMod_dir);
               System.IO.Directory.CreateDirectory(LanguageEnglishMod_dir);
            }

            using (XmlWriter writer = XmlWriter.Create(LanguageEnglishMod_path, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("MyGUI");

                foreach (KeyValuePair<string, string> kvp in tagToName)
                {
                    writer.WriteStartElement("Tag");

                    writer.WriteAttributeString("name", kvp.Key);
                    writer.WriteString(kvp.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Close();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Wrote " + oobcsvpath);


            Console.WriteLine("Wrote "+LanguageEnglishMod_path);
            Console.ResetColor();
            Console.WriteLine(" Press any key to exit");
            Console.ReadKey();

        }

        static string baklabel = DateTime.Now.ToString("yyMMddHmm");
        static void CreateBackup(string filepath)
        {
            if (File.Exists(filepath))
            {
                Console.WriteLine("Creating Backup " + filepath + "." + baklabel);
                File.Copy(filepath, filepath + "." + baklabel);
            }
        }

    }



}