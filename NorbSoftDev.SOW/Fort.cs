using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;


namespace NorbSoftDev.SOW
{



    public class Fort : Position, IObjective
    {

        public List<FiringLine> firingLines;

        public string id {
            get { return _id; }
            set { _id = value; OnPropertyChanged("id"); }
        }
        string _id;

        public int grayscale {
            get { return _grayscale; }
            set { _grayscale = value; OnPropertyChanged("grayscale"); }
        }
        int _grayscale;
        

        public float defense {
            get { return _defense; }
            set { _defense = value; OnPropertyChanged("defense"); }
        }
        float _defense;

        public float offense {
            get { return _offense; }
            set { _offense = value; OnPropertyChanged("offense"); }
        }
        float _offense;


        public Fort()
            : base()
        {
            firingLines = new List<FiringLine>();
        }

        public override string ToString()
        {
            return id;
        }


        public void FromCsvLine(CsvReader csv)
        {
            int i = 0;
            this._id = csv[i++];
            int.TryParse(csv[i++], out _grayscale);
            float.TryParse(csv[i++], out _offense);
            float.TryParse(csv[i++], out _defense);
            float.TryParse(csv[i++], out _south);
            float.TryParse(csv[i++], out _east);
        }
    }

    public class FiringLine {
        public void FromCsvLine(CsvReader csv)
        {

        }        
    }


}