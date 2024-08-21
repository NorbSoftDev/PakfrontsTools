using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;


namespace NorbSoftDev.SOW
{

    public enum EObjectiveType { Hold, Waypoint };
    public enum EObjectivePriority { Major, Minor };
    public enum EObjectiveAI { Player, French, British, Prussian, Army4, Army5, Everyone };

    public class ScenarioObjective : Position, IObjective
    {
        // Declarations
        public string name {
            get { return _name; }
            set { _name = value; OnPropertyChanged("name"); }
        }
        string _name;

        public string id {
            get { return _id; }
            set { _id = value; OnPropertyChanged("id"); }
        }
        string _id;


        public EObjectivePriority priority
        {
            get { return _priority; }
            set { 
                _priority = value; 
                OnPropertyChanged("priority");

            }
        }
        EObjectivePriority _priority = EObjectivePriority.Minor;


        public EObjectiveType type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("type"); }
        }
        EObjectiveType _type = EObjectiveType.Hold;


        public EObjectiveAI ai
        {
            get { return _ai; }
            set { _ai = value; OnPropertyChanged("ai"); }
        }
        EObjectiveAI _ai = EObjectiveAI.Everyone;

        //public Position position {
        //    get { return _position; }
        //    set {
        //        _position = value; 
        //        OnPropertyChanged("position");
        //    }
        //}
        //Position _position;        

        // public float x;
        // public float z;

        public int radius {
            get { return _radius; }
            set { _radius = value; OnPropertyChanged("radius"); }
        }
        int _radius = 50;


        public int men {
            get { return _men; }
            set { _men = value; OnPropertyChanged("men"); }
        }
        int _men = 100;


        public int points {
            get { return _points; }
            set { _points = value; OnPropertyChanged("points"); }
        }
        int _points = 100;


        public int fatigue {
            get { return _fatigue; }
            set { _fatigue = value; OnPropertyChanged("fatigue"); }
        }
        int _fatigue;


        public int morale {
            get { return _morale; }
            set { _morale = value; OnPropertyChanged("morale"); }
        }
        int _morale;


        public int ammo {
            get { return _ammo; }
            set { _ammo = value; OnPropertyChanged("ammo"); }
        }
        int _ammo;


        public int occMod {
            get { return _occMod; }
            set { _occMod = value; OnPropertyChanged("occMod"); }
        }
        int _occMod = 1;


        public TimeSpan beg {
            get { return _beg; }
            set { _beg = value; OnPropertyChanged("beg"); }
        }
        TimeSpan _beg;


        public TimeSpan end
        {
            get { return _end; }
            set { _end = value; OnPropertyChanged("end"); }
        }
        TimeSpan _end;


        public string interval {
            get { return _interval; }
            set { _interval = value; OnPropertyChanged("interval"); }
        }
        string _interval = "0:01";

        public string sprite
        {
            get
            {
                if (_priority == EObjectivePriority.Major) return "GFX_Obj_Major";
                return "GFX_Obj_Minor";
            }
        }



        public string army1
        {
            get
            {
                if (_priority == EObjectivePriority.Major) return "GFX_Obj_UMajor";
                return "GFX_Obj_UMinor";
            }
        }



        public string army2
        {
            get
            {
                if (_priority == EObjectivePriority.Major) return "GFX_Obj_CMajor";
                return "GFX_Obj_CMinor";
            }
        }

        public string army3
        {
            get
            {
                if (_priority == EObjectivePriority.Major) return "GFX_Obj_Major";
                return "GFX_Obj_Minor";
            }
        }


        //public string sprite
        //{
        //    get { return _sprite; }
        //    set { _sprite = value; OnPropertyChanged("sprite"); }
        //}
        //string _sprite = "GFX_Obj_Minor";


        //public string army1
        //{
        //    get { return _army1; }
        //    set { _army1 = value; OnPropertyChanged("army1"); }
        //}
        //string _army1 = "GFX_Obj_UMinor";


        //public string army2
        //{
        //    get { return _army2; }
        //    set { _army2 = value; OnPropertyChanged("army2"); }
        //}
        //string _army2 = "GFX_Obj_CMinor";


        //public string army3 {
        //    get { return _army3; }
        //    set { _army3 = value; OnPropertyChanged("army3"); }
        //}
        //string _army3;

        public ScenarioObjective()
            : base()
        {
            
        }

        public ScenarioObjective(string id)
            : base()
        {
            _id = id;
            name = id.CamelCase();
        }

        // Input
        public void FromCsvLine(Dictionary<string, int> columns, CsvReader csv, Scenario scenario)
        {
            int i = 0;
            this._name = csv[i++];//columns["Name"]];
            this._id = csv[i++];//columns["ID"]];
            this._priority = String.Compare(csv[i++].Trim(), "major", true) == 0 ? EObjectivePriority.Major : EObjectivePriority.Minor; ;//columns["Priority"]];
            this._type = String.Compare(csv[i++].Trim(), "waypoint", true) == 0 ? EObjectiveType.Waypoint : EObjectiveType.Hold; //columns["Type"]];

            switch (csv[i++].ToLower().Trim())
            {
                case "0":
                    this._ai = EObjectiveAI.Player;
                    break;
                case "1":
                    this._ai = EObjectiveAI.French;
                    break;
                case "2":
                    this._ai = EObjectiveAI.British;
                    break;
                case "3":
                    this._ai = EObjectiveAI.Prussian;
                    break;
                default:
                    this._ai = EObjectiveAI.Everyone;
                    break;
            }

            //int.TryParse(csv[i++], out this._ai);//columns["AI"]], out this._ai);
            

            //float south,east;
            float.TryParse(csv[i++], out _south);//columns["loc x"]], out this._locX);
            float.TryParse(csv[i++], out _east);//columns["loc z"]], out this._locZ);
            //_position = new Position(south, east);
            


            int.TryParse(csv[i++], out this._radius);//columns["radius"]], out this._radius);
            int.TryParse(csv[i++], out this._men);//columns["Men"]], out this._men);
            int.TryParse(csv[i++], out this._points);//columns["Points"]], out this._points);;//columns["Points"]], out this._points);
            int.TryParse(csv[i++], out this._fatigue);//columns["Fatigue"]], out this._fatigue);
            int.TryParse(csv[i++], out this._morale);//columns["Morale"]], out this._morale);
            int.TryParse(csv[i++], out this._ammo);//columns["Ammo"]], out this._ammo);
            int.TryParse(csv[i++], out this._occMod);//columns["OccMod"]], out this._occMod);

            this._beg = Sky.ParseTimeSpan(csv[i++]);//columns["Beg"]];
            this._end = Sky.ParseTimeSpan(csv[i++]);;//columns["End"]];
            this._interval = csv[i++];//columns["Interval"]];

            //this._sprite = csv[i++];//columns["Sprite"]];
            //this._army1 = csv[i++];//columns["Army1"]];
            //this._army2 = csv[i++];//columns["Army2"]];
            //this._army3 = csv[i++];//columns["Army3"]];
        }
        // Output
        public string ToCsv()
        {
            return String.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},"
                , this.name, this.id, this.priority, this.type, AIAsInt(this.ai), this.AsCsv2(), this.radius, this.men, this.points, this.fatigue, this.morale, this.ammo, this.occMod,
                Sky.ToCsv(this.beg),  Sky.ToCsv(this.end), this.interval,
                this.sprite, this.army1, this.army2, this.army3
            );
        }

        public override string ToString()
        {
            return id;
        }

        public int AIAsInt(EObjectiveAI ai) {
            switch (ai)
            {
                case EObjectiveAI.Everyone:
                    return 100;
                default:
                    return (int)ai;
                    
            }

        }

        //#region INotifyPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;

        //// Create the OnPropertyChanged method to raise the event 
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}


        //protected void OnPropertyChanged(PropertyChangedEventArgs e)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, e);
        //    }
        //}
        //#endregion
    }



}