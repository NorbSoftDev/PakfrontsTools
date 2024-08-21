
using System;
using System.ComponentModel;
using System.Windows;


namespace NorbSoftDev.SOW {

    public class Position : IPosition, INotifyPropertyChanged
    {
        protected float _south, _east;

        public float south
        {
            get { return _south; }
            set
            {
                if (value == _south) return;
                _south = value;
                OnPropertyChanged("south");
            }
        }

        public float east
        {
            get { return _east; }
            set
            {
                if (value == _east) return;
                _east = value;
                OnPropertyChanged("east");
            }
        }

        public Point point
        {
            get
            {
                return new Point(_east, _south);
            }

            set
            {
                this._east = (float)value.X;
                this._south = (float)value.Y;
                OnPropertyChanged("");

            }
        }


        public Position()
        {

        }

        public Position(Position position)
        {
            this._south = position.south;
            this._east = position.east;
        }

        public Position(string south, string east)
        {
            this._south = Convert.ToSingle(south);
            this._east = Convert.ToSingle(east);
        }


        public Position(float south, float east) {
            this.south = south;
            this.east = east;
        }

        public void SetPosition(float south, float east)
        {
            _south = south;
            _east = east;
            OnPropertyChanged("");
        }

        public void SetPosition(Position position)
        {
            _south = position.south;
            _east = position.east;
            OnPropertyChanged("");
        }

        public override string ToString()
        {
            return _south+","+_east;
        }


        public string AsCsv2()
        {


            return String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1}",
               _south, _east
                );
        }


        public string AsIntCsv2()
        {


            return String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1}",
               (int)_south, (int)_east
                );
        }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

    }


    public class WorldTransform : Position
    {

        //float _south, _east;

        //public float south
        //{
        //    get { return _south; }
        //    set
        //    {
        //        if (value == _south) return;
        //        _south = value;
        //        OnPropertyChanged("south");
        //    }
        //}

        //public float east
        //{
        //    get { return _east; }
        //    set
        //    {
        //        if (value == _east) return;
        //        _east = value;
        //        OnPropertyChanged("east");
        //    }
        //}

        //public Point point
        //{
        //    get
        //    {
        //        return new Point(_east, _south);
        //    }
        //}



        float _facing;
        public float facing
        {
            get { return _facing; }
            set
            {
                _facing = value;
                OnPropertyChanged("facing");
            }
        }

        public float dirEast
        {
            get { return WorldTransform.FacingToEast(facing); }
        }

        public float dirSouth
        {
            get { return WorldTransform.FacingToSouth(facing); }
        }

        public void Face(Vector d)
        {
            d.Normalize();
            this.facing = WorldTransform.DirToFacing((float)d.Y, (float)d.X);
        }

        public void AimAt(Position p)
        {
            Vector d = new Vector( p.south - this.south, p.east - this.east);
            Face(d);
        }

        public void MoveBy(Vector d)
        {
            _east += (float)d.X;
            _south += (float)d.Y;
            OnPropertyChanged("");
        }


        public Vector direction
        {
            get { return FacingToVector(facing); }
            set { Face(value); }
        }

        public WorldTransform() : base() { }

        public WorldTransform(float dirSouth, float dirEast, float south, float east)
            : base(south, east)
        {
            this.south = south;
            this.east = east;

            this.facing = WorldTransform.DirToFacing(dirSouth, dirEast);

        }

        public WorldTransform(float south, float east) : this(0, 0, south, east) { }

        public WorldTransform(string dirSouth, string dirEast, string south, string east)
            : base(south, east)
        {
            this._south = Convert.ToSingle(south);
            this._east = Convert.ToSingle(east);

            this.facing = WorldTransform.DirToFacing(
                 Convert.ToSingle(dirSouth),
                  Convert.ToSingle(dirEast)
                  );

        }


        public WorldTransform(string south, string east):  base(south, east)
        {
            this._south = Convert.ToSingle(south);
            this._east = Convert.ToSingle(east);
        }



        public void Set(float dirSouth, float dirEast, float south, float east)
        {
            _south = south;
            _east = east;
            this.facing = WorldTransform.DirToFacing(dirSouth, dirEast);
            OnPropertyChanged("");
        }

        public void Set(string dirSouth, string dirEast, string south, string east)
        {
            this._south = Convert.ToSingle(south);
            this._east = Convert.ToSingle(east);
            this.facing = WorldTransform.DirToFacing(
                 Convert.ToSingle(dirSouth),
                  Convert.ToSingle(dirEast)
                  );
            OnPropertyChanged("");
        }

        public void Set(WorldTransform other)
        {
            this._south = other._south;
            this._east = other._east;
            this._facing = other._facing;
            OnPropertyChanged("");
        }
             
        public string AsCsv4()
        {
            float dirEast;
            float dirSouth;

            WorldTransform.FacingToDir(this.facing, out dirSouth, out dirEast);
            return String.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2},{3}",
                dirSouth, dirEast, south, east
                );
        }
        //TODO add, subtract, aim at, etc.

        public bool isAtOrigin
        {
            get
            {
                return (east == 0 && south == 0);
            }
        }


        /// <summary>
        /// in degrees
        /// first coord dirSouth
        /// second coord dirEast
        /// North: -1, 0
        /// East:   0, 1
        /// South:  1, 0
        /// West:   0,-1
        /// </summary>
        /// <param name="dirSouth"></param>
        /// <param name="dirEast"></param>
        /// <returns></returns>
        public static float DirToFacing(float dirSouth, float dirEast)
        {
            return (float)((Math.Atan2(-dirEast, dirSouth) + Math.PI) * 57.2957795);
        }

        public static void FacingToDir(float facing, out float dirSouth, out float dirEast)
        {
            dirSouth = FacingToSouth(facing);
            dirEast = FacingToEast(facing);
        }

        static double ALMOST_ZERO = 2.0E-16;
        internal static float FacingToSouth(float facing)
        {
            double val = -Math.Cos(facing / 180.0 * Math.PI);
            if (val > -ALMOST_ZERO && val < ALMOST_ZERO) return 0;
            return (float)val;
        }

        internal static float FacingToEast(float facing)
        {
            double val = Math.Sin(facing / 180.0 * Math.PI);
            if (val > -ALMOST_ZERO && val < ALMOST_ZERO) return 0;
            return (float)val;
        }

        internal static Vector FacingToVector(float facing)
        {
            return new Vector(
                FacingToSouth(facing),
                FacingToEast(facing)
              );
        }

        public override string ToString()
        {
            return _east + "," + _south + ":" + facing + "°";
        }
    }

}