using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public abstract class Mod
    {
        public DirectoryInfo directory;
        public string name { get; set; }
        public abstract bool active { get; set; }
        public int index;

        public Mod(DirectoryInfo directory)
        {
            this.directory = directory;
            this.name = directory.Name;
        }

        public Mod(DirectoryInfo directory, string name) : this(directory)
        {
            if (directory.Exists)
            {
                Log.Info(this, "Found Mod \"" + name + "\"  \"" + directory.FullName+"\"");

            } else 
            {
                Log.Warn(this, "Cannot Find Mod \"" + name + "\"  \"" + directory.FullName + "\"");
            }
            this.name = name;
        }

        public Mod(string dirpath)
            : this(DirectoryInfoWithCheck(dirpath))
        {
  
        }

        public Mod(string dirpath, string name)
            : this(DirectoryInfoWithCheck(dirpath), name)
        {

        }

        public static DirectoryInfo DirectoryInfoWithCheck(string dirpath) {
            try
            {
                return new DirectoryInfo(dirpath);
            }
            catch (Exception e)
            {
                Log.Error(null, "Unable to parse Directory Path \"" + dirpath + "\" " + e.Message);
                e.Data.Add("UserMessage", "Unable to parse Directory Path \"" + dirpath + "\"");
                throw;
            }
        }

        public override string ToString()
        {
            return name;
        }

        public bool Contains(DirectoryInfo file)
        {
            if (!directory.Exists) return false;

            foreach (DirectoryInfo fi in directory.GetDirectories(file.Name, SearchOption.AllDirectories))
            {
                if (fi.IsSame(file)) return true;
            }
            return false;
        }

        public bool Contains(FileInfo file)
        {
            if (!directory.Exists) {
                //Log.Warn(this,"Searching missing mod "+directory.FullName);
                return false;
            }

            Console.WriteLine("Searching mod "+directory.FullName);
            Console.WriteLine("          for "+file.FullName);
            Console.WriteLine("              "+file.Name);

            int cnt = 0;
            foreach (FileInfo fi in directory.GetFiles(file.Name, SearchOption.AllDirectories))
            {
                Console.WriteLine("Test:"+fi.FullName);
                if (fi.IsSame(file)) return true;
                cnt++;
            }
            Console.WriteLine("          searched "+cnt);
            return false;
        }

        public string FindFileInText(string file) {
            return FindFileInMod(Path.Combine("Text", file));
        }

        public string FindFileInLogistics(string file) {
            return FindFileInMod(Path.Combine("Logistics", file));
        }

        public string FindFileInMod(string shortpath) {
            if ( ! this.active ) return null;
            string path = Path.Combine(directory.ToString(), shortpath);
            if ( File.Exists( path ) ) {
                return path;
            }
            // Log.Info(this,"Did not find file: '"+path+"'");
            return null;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subdir"></param>
        /// <param name="filespec"></param>
        /// <returns></returns>
        public string[] GetFilesInMod(string subdir, string filespec) {
            try {
                return Directory.GetFiles(Path.Combine(directory.ToString(), subdir), filespec, SearchOption.AllDirectories);
            } catch (System.IO.DirectoryNotFoundException e){
                return null;
            }
            
        }

        public string[] GetScenarios()
        {
            DirectoryInfo dir = new DirectoryInfo( Path.Combine(directory.ToString(), "Scenarios"));
            if (! dir.Exists) return new string[0];


            return Directory.GetFiles( dir.FullName, "scenario.csv", SearchOption.AllDirectories);
        }

        public string[] GetOOBs()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(directory.ToString(), "OOBs"));
            if (!dir.Exists) return new string[0];


            return Directory.GetFiles(dir.FullName, "*.csv", SearchOption.AllDirectories);
        }

        public string[] GetMaps()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(directory.ToString(), "Maps"));
            if (!dir.Exists) return new string[0];


            return Directory.GetFiles(dir.FullName, "*.ini", SearchOption.AllDirectories);
        }

        public DirectoryInfo scenarioDir
        {
            get { return new DirectoryInfo(Path.Combine(directory.ToString(), "Scenarios")); }
        }

        public DirectoryInfo orderOfBattleDir
        {
            get { return new DirectoryInfo(Path.Combine(directory.ToString(), "OOBs")); }
        }

        public DirectoryInfo logisticsDir
        {
            get { return new DirectoryInfo(Path.Combine(directory.ToString(), "Logistics")); }
        }

        public DirectoryInfo mapDir
        {
            get { return new DirectoryInfo(Path.Combine(directory.ToString(), "Maps")); }
        }



        // public string FindFileInDir(string dirpath, string file) {
        //     string path = Path.Combine(dirpath,file);
        //     string history = (" \""+path+"\""+System.Environment.NewLine);
        //     if ( File.Exists( path ) ) {
        //         // Log.Info(this,"Found \""+path+"\"");
        //         return path;
        //     }
        //     // Log.Info(this,"Searched "+history);
        //     return null;
        // }     

 
    }

    public class UserMod : Mod
    {

        public override bool active {get; set;}

        public UserMod(DirectoryInfo directory) : base(directory)
        {
        }

        public UserMod(string dirpath)
            : base(dirpath)
        {  
        }

        public UserMod(string dirpath, int index, bool active)
            : this(dirpath)
        {
            this.active = active;
        }
    }

    public class DefaultMod : Mod
    {

        public override bool active { get { return true; } set { } }

        public DefaultMod(DirectoryInfo directory)
            : base(directory)
        {
        }

        public DefaultMod(string dirpath)
            : base(dirpath)
        {
        }



        public DefaultMod(string dirpath, int index)
            : base(dirpath)
        {
            this.index = index;
        }
        public DefaultMod(string dirpath, string name, int index)
            : base(dirpath, name)
        {
            this.index = index;
        }

    }
}