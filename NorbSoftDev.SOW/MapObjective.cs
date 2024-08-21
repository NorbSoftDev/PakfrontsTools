using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;


namespace NorbSoftDev.SOW
{

 

    public class MapObjective : Position, IObjective
    {

        public string id {
            get { return _id; }
            set { _id = value; OnPropertyChanged("id"); }
        }
        string _id;


        public MapObjective()
            : base()
        {
            
        }

        public override string ToString()
        {
            return id;
        }


        public void FromCsvLine(CsvReader csv)
        {
            int i = 0;
            this._id = csv[i++];//columns["ID"]];
            float.TryParse(csv[i++], out _south);//columns["loc x"]], out this._locX);
            float.TryParse(csv[i++], out _east);//columns["loc z"]], out this._locZ);
        }
    }



}