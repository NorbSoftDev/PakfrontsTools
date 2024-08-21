using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW {

    public abstract class LogisticsEntryBitmap : LogisticsEntry {
        //this should be abstract, but doesnt seem to work with generics

        //Config _config;

        public string file { 
            get { return _file;}
            set { 
                _file = value;
                #if !__MonoCS__
                //_bitmap = null;
                #endif
            }
        }
        protected string _file;


        // #if !__MonoCS__
        // public System.Drawing.Bitmap bitmap {
        //    get {
        //        if (_file == null) return null;
        //        if (_bitmap == null)
        //        {
        //            string filepath = _config.FindGraphic(_file);
        //            if (filepath != null)
        //            {
        //                Console.WriteLine("Loading Bitmap " + filepath);
        //                _bitmap = DevIL.DevIL.LoadBitmap(filepath);
        //            }
        //        }
                    
        //        return _bitmap;
        //    }
        //}
        //System.Drawing.Bitmap _bitmap;
        //#endif
    }    


}