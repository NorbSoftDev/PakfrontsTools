using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using System.Windows;

using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW
{

    public class Formation : LogisticsEntry, INotifyPropertyChanged
    {

        //conversion from Formation orientations (E,S) to World orientations (S,E)
        private const int FACING_OFFSET = 90;

        Config config;
        public string userName { get; set; }

        public Slot[] slots;
        public Slot[,] grid;

        int _rows, _columns;
        public int rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                Rebuild();
            }
        }

        public int columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
                //TODO should shift rathern than new
                Rebuild();
            }
        }

        void Rebuild()
        {
            slots = new Formation.Slot[_rows * _columns];
            grid = new Slot[_columns, _rows];
            OnPropertyChanged("");

        }

        /// <summary>
        /// man is one indexed, row and col are 0
        /// </summary>
        /// <param name="slot"></param>
        internal void SetSlot(Slot slot)
        {
            if (slot.man > slots.Length)
            {
                Log.Error(this, this.id + " attempting to set slot man " + slot.man + " greater than " + this.slots.Length);
            }

            if (slot.column >= grid.GetLength(0))
                Log.Error(this, this.id + " attempting to set man  " + slot.man + " column " + slot.column + " greater than " + this.grid.GetLength(0));

            if (slot.row >= grid.GetLength(1))
                Log.Error(this, this.id + " attempting to set man  " + slot.man + " row " + slot.row + " greater than " + this.grid.GetLength(1));

            slots[slot.man - 1] = slot;

            grid[slot.column, slot.row] = slot;

        }

        //TODO OnPropertyChanged
        public Distance rowDistYds { get; set; }
        public Distance colDistYds { get; set; }
        //public string rowDistYds {get; set;}
        //public string colDistYds {get; set;}
        public Formation subformation { get; set; }
        public bool keepFormation { get; set; }
        public bool canWheel { get; set; }
        public bool canFight { get; set; }
        public float moveRateModifier { get; set; }
        public bool canAboutFace { get; set; }
        public Formation artilleryFormation { get; set; }
        //public string artilleryFormation { get;set;}

        public float minEnemyYds { get; set; }
        public float fireModifier { get; set; }
        public float meleeModifier { get; set; }
        public bool cantMove { get; set; }
        public bool cantCounterCharge { get; set; }

        public int level { get; set; }

        public Formation(Config config)
            : base()
        {
            this.config = config;
        }

        public static int LevelFromRank(ERank rank)
        {
            // { None = -1, Battalion = 0, Regiment = 2, Brigade = 4, Division = 6, Corps = 8, Army = 10, Side = 12, Root = 13 }

            switch (rank)
            {
                case ERank.Battalion:
                case ERank.Regiment:
                    return 0;
                case ERank.Brigade:
                    return 1;
                case ERank.Division:
                    return 2;
                case ERank.Corps:
                    return 3;
                case ERank.Army:
                    return 4;
                default:
                    return -1;
            }
        }

        public Rect ComputeBounds(ScenarioEchelon echelon)
        {
            Location [] positionYds;

            if (echelon.rank > ERank.Regiment)
            {
                return ComputeSpritePositionsYds(echelon, out positionYds);
            }

            return ComputeSpritePositionsYds(echelon, out positionYds);
        }

        /// <summary>
        /// Given a Unit, return the space that it's would take up if using this formation, in yards
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public Rect ComputeSpritePositionsYds(ScenarioEchelon echelon, out Location[] positionsYds)
        {

            ScenarioUnit unit = echelon.unit;

            if (unit == null)
            {
                Log.Error(this, "ComputeSpritePositionsYds: echelon \"" + echelon + "\" has no unit");
                positionsYds = new Location[0];
                return new Rect();
            }

            positionsYds = new Location[this.slots.Length];

            // positionsYds = new Point[this.slots.Length];

            float rowBase = 0;
            for (int r = 0; r < this.grid.GetLength(1); r++)
            {
                float maxRowOffsetThisRow = this.rowDistYds.yards;

                float colBase = 0;
                for (int c = 0; c < this.grid.GetLength(0); c++)
                {

                        Formation.Slot slot = this.grid[c, r];

 

                    if (slot == null)
                    {
                        // use default
                        colBase += this.colDistYds.yards;
                    }

                    else
                    {
                        try
                        {
                        float colOffset = slot.colDistYds.yards;
                        float rowOffset = slot.rowDistYds.yards;

                        int positionIndex = slot.man - 1;

                        if (rowOffset > maxRowOffsetThisRow ) maxRowOffsetThisRow = rowOffset;

                        colBase += colOffset;

                        // positionsYds[positionIndex] = new Point(colBase, rowBase+rowOffset);

                        if (positionIndex >= positionsYds.Length)
                        {
                            Log.Error(this,"Position Index "+positionIndex+" excedded aloted array of "+positionsYds.Length+" Skipping" );
                            continue;
                        }

                            positionsYds[positionIndex] = new Location(
                                new Point(colBase, rowBase + rowOffset),
                                slot.facing,
                                null
                                );
                        }
                        catch (System.IndexOutOfRangeException e)
                        {
                            Log.Error(this,"Failed to find slot " + c + "," + r);
                            throw e;
                        }
                    }
                    

                }

                rowBase += maxRowOffsetThisRow;
            }


            // Rect maxBounds = new Rect(-centerRowYds, -centerColYds, maxRowYds, maxColYds);
            // Console.WriteLine("Max Bounds: "+this.id+" " + maxBounds);
            // if (false)
            // {

            //     return maxBounds;
            // }



            int nsprites = unit.headCount / config.spriteRatio;
            if (this.level > 0)
            {
                nsprites = 2;
            }
            //this should be a warning at least
            if (nsprites > slots.Length)
            {
                Log.Warn(this,"[Formation] " + this.id + " has only " + slots.Length + " positions, but unit " + unit.id + " has headCount " + unit.headCount);
                nsprites = slots.Length;
            }

            //now correct for positions
            //Set at origin, as pos 0 is always at origin
            double maxX = 0;
            double maxY = 0;
            double minX = 0;
            double minY = 0;

            for (int i = 1; i < nsprites; i++)
            {
                if (positionsYds[i] == null)
                {
                    Log.Error(this, echelon.id+" "+this.id+" Null Formation Positions starting at "+i);
                    break;
                }
                positionsYds[i].position.X -= positionsYds[0].position.X;
                positionsYds[i].position.Y -= positionsYds[0].position.Y;
                if (positionsYds[i].position.Y > maxY) maxY = positionsYds[i].position.Y;
                if (positionsYds[i].position.X > maxX) maxX = positionsYds[i].position.X;
                if (positionsYds[i].position.Y < minY) minY = positionsYds[i].position.Y;
                if (positionsYds[i].position.X < minX) minX = positionsYds[i].position.X;
            }
            //positionsYds[0].Offset(-positionsYds[0].X, -positionsYds[0].Y);
            if (positionsYds.Length > 0 && positionsYds[0] != null)
            {
                positionsYds[0].position.X = 0;
                positionsYds[0].position.Y = 0;
            }

            double lengthY = maxY - minY;
            double lengthX = maxX - minX;

            Rect bounds = new Rect(
                minX, minY,
                lengthX, lengthY);

            return bounds;

        }

        /// <summary>
        /// Given a Echelon, return the space that it would take up if using this formation
        /// and modify a array of positions for children
        /// 0/1 is always a flagbearer
        /// 1/2 is always an officer
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public Rect BridageComputeChildPositionsYds(ScenarioEchelon echelon, out Location[] positionsYds)
        {

            positionsYds = new Location[this.slots.Length];

            //Calculate row depths and column widths
            float [] colMaxOffsets = new float[this.grid.GetLength(0)];
            float [] rowMaxOffsets = new float[this.grid.GetLength(1)];
            for (int r = 0; r < this.grid.GetLength(1); r++)
            {
                float maxRowOffsetThisRow = this.rowDistYds.yards;

                for (int c = 0; c < this.grid.GetLength(0); c++)
                {
                    Formation.Slot slot = this.grid[c, r];
                    float colOffset = this.colDistYds.yards;
                    float rowOffset = this.rowDistYds.yards;

                    if (slot != null) {

                        colOffset = slot.colDistYds.yards;
                        rowOffset = slot.rowDistYds.yards;

                        int childIndex = slot.man - 3;

                        if (slot.man > 2 && childIndex < echelon.children.Count && (slot.rowDistYds.dependent || slot.colDistYds.dependent))
                        {
                            // it is dependent and occupied
                            ScenarioEchelon child = (ScenarioEchelon)echelon.children[childIndex];

                            Location[] childPositions;

                            Rect childBounds = slot.subformation.level == 0 ? slot.subformation.ComputeSpritePositionsYds(child, out childPositions) : slot.subformation.BridageComputeChildPositionsYds(child, out childPositions);
                            //if (setSubFormations)
                            //{
                            //    child.unit.formation = slot.subformation;
                            //}

                            if (slot.colDistYds.dependent)
                            {
                                colOffset += (float)childBounds.Width;
                            }

                            if (slot.rowDistYds.dependent)
                            {
                                rowOffset += (float)childBounds.Height;
                            }
                        }
                    }

                    if (colOffset > colMaxOffsets[c]) colMaxOffsets[c] = colOffset;
                    if (rowOffset > rowMaxOffsets[r]) rowMaxOffsets[r] = rowOffset;

                }
            }

            //Populate Positions grid
            float rowBase = 0;
            for (int r = 0; r < this.grid.GetLength(1); r++)
            {
                float colBase = 0;
                for (int c = 0; c < this.grid.GetLength(0); c++)
                {
                    Formation.Slot slot = this.grid[c, r];

                    if (slot == null)
                    {
                        colBase += colMaxOffsets[c];
                    }

                    else
                    {
                        int positionIndex = slot.man - 1;
                        float colOffset = colMaxOffsets[c];
                        //place in center of column

                        positionsYds[positionIndex] = new Location(
                            new Point(colBase + colOffset/2, rowBase),
                            slot.facing,
                            slot.subformation
                            );
                        colBase += colOffset;
                    }
                }
                //used by next row
                rowBase += rowMaxOffsets[r];
            }



            ScenarioUnit unit = echelon.unit;

            int nslots = 2 + echelon.children.Count;

            //this should be a warning at least
            if (nslots > slots.Length)
            {
                Log.Warn(this,"[Formation] " + this.id + " has only " + slots.Length + " positions, but unit " + unit.id + " has headCount " + unit.headCount);
                nslots = slots.Length;
            }

            // Move positions grid so that slot 1 is at origins
            // and calculate bounds
            // Set at origin, as pos 0 is always at origin
            double maxX = 0;
            double maxY = 0;
            double minX = 0;
            double minY = 0;

            for (int i = 1; i < nslots; i++)
            {
                positionsYds[i].position.X -= positionsYds[0].position.X;
                positionsYds[i].position.Y -= positionsYds[0].position.Y;
                if (positionsYds[i].position.Y > maxY) maxY = positionsYds[i].position.Y;
                if (positionsYds[i].position.X > maxX) maxX = positionsYds[i].position.X;
                if (positionsYds[i].position.Y < minY) minY = positionsYds[i].position.Y;
                if (positionsYds[i].position.X < minX) minX = positionsYds[i].position.X;
            }

            positionsYds[0].position.X = 0;
            positionsYds[0].position.Y = 0;

            double lengthY = maxY - minY;
            double lengthX = maxX - minX;

            Rect bounds = new Rect(
                minX, minY,
                lengthX, lengthY);

            return bounds;
        }

        /// <summary>
        /// Given a Unit, return the space that it would take up if using this formation
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        //public Rect ComputeBounds(ScenarioEchelon echelon) {

        //    ScenarioUnit unit = echelon.unit;

        //    Rect rect = new Rect(0,0,10,0);
        //    float x = 0;
        //    float y = 0;
        //    bool rowDep = false;
        //    bool colDep = false;
        //    int nsprites = unit.headCount/config.spriteRatio;

        //    //this should be a warning at least
        //    if (nslots > slots.Length)
        //    {
        //        Log.Warn(this,"[Formation] " + this.id + " has only " + slots.Length + " positions, but unit " + unit.id + " has headCount " + unit.headCount);
        //        nslots = slots.Length;
        //    }

        //    float maxRowYds = 0;
        //    float maxColYds = 0;
        //    float centerColYds = 0;
        //    float centerRowYds = 0;

        //    Console.WriteLine(this.id+" "+this._columns+"x"+this._rows+" Cols:"+grid.GetLength(0)+" Rows:"+grid.GetLength(1));

        //    float[] colYds = new float[_columns];
        //    float[] rowYds = new float[_rows]; 

        //    for (int r = 0; r < grid.GetLength(1); r++)
        //    {
        //        for (int c = 0; c < grid.GetLength(0); c++)
        //        {
        //            Slot slot = grid[c, r];
        //            if (slot == null) continue;

        //            //if (slot.subformation != null) {
        //            //    // FIXME allow recursion here
        //            //    continue;
        //            //}

        //            //if (slot.colDistYds.dependent || slot.rowDistYds.dependent)
        //            //{
        //            //    //FIXME compute or get bounds of subunit
        //            //    continue;
        //            //}

        //            if (slot.man == 1) {
        //                //this be + only half width and height for this slot, if flag is measured at center, not top left
        //                centerColYds = colYds[c] + slot.rowDistYds.yards/2.0f;
        //                centerRowYds = rowYds[r] + slot.colDistYds.yards/2.0f;

        //                Console.WriteLine(this.id+" Man 1: "+c+","+r);
        //            }                    

        //           // length of column uses space between row
        //           colYds[c] += slot.rowDistYds.yards;
        //           slot.y = colYds[c];
        //           if (colYds[c] > maxColYds) maxColYds = colYds[c];

        //           // length of row has uses space between col
        //           rowYds[r] += slot.colDistYds.yards;
        //           slot.x = rowYds[r];
        //           if (rowYds[r] > maxRowYds) maxRowYds = rowYds[r];

        //        }
        //    }
        //    Rect bounds = new Rect( -centerRowYds, -centerColYds, maxRowYds, maxColYds);

        //    if (unit.headCount > 0)
        //        Console.WriteLine("[Rect] "+id +" "+unit.id+ ": " + bounds.Width + "x" + bounds.Height+" "+bounds.X+","+bounds.Y);

        //     //int maxRow = 0, maxCol = 0;
        //    //for (int i = 0; i < nslots; i++)
        //    //{
        //    //    Slot slot = slots[i];
        //    //    if (slot == null) continue;
        //    //    if (slot.row > maxRow) maxRow = slot.row;
        //    //    if (slot.column > maxCol) maxCol = slot.column;
        //    //}
        //    //Bounds bounds = new Bounds(0, 0, maxCol * colDistYds.yards, maxRow * rowDistYds.yards);

        //    //if (rowDep ) Console.WriteLine(id + " row dependent "+rowDistYds.dependent);
        //    //if (colDep ) Console.WriteLine(id + " col dependent "+colDistYds.dependent);
        //    return bounds;
        //}

        public override void FromCsvLine(Config config, string definedIn, CsvReader csv)
        {
            this.definedIn = definedIn;
            // int i = 0;
            // try { 
            //     userName = csv[i++];
            //     id = csv[i++];

            // } catch (Exception e) {
            //     string[] headers = csv.GetFieldHeaders();
            //     Log.Info(this," read failed on '"+csv[0]+"'' column: "+(i-1)+" '"+headers[i-1]+"' value:'"+csv[i-1]+"'");
            //     throw(e);
            // }

        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }
        #endregion

        public class Distance
        {
            public bool dependent { get; set; }
            public float yards { get; set; }
            public Distance(string str)
            {
                str = str.Trim();
                if (str[str.Length - 1] == '+')
                {
                    str = str.Substring(0, str.Length - 1);
                    dependent = true;
                }
                else dependent = false;
                yards = Convert.ToSingle(str);
            }
        }

        public class Slot
        {
            //special situations (row distance - column distance - sprite - facing - subform - subtype), must precede man number    }    

            Distance _rowDistYds, _colDistYds;
            public Distance rowDistYds
            {
                get
                {
                    return _rowDistYds == null ? formation.rowDistYds : _rowDistYds;
                }
                set { _rowDistYds = value; }
            }

            public Distance colDistYds
            {
                get { return _colDistYds == null ? formation.colDistYds : _colDistYds; }
                set { _colDistYds = value; }
            }

            Formation _subformation;
            public Formation subformation
            {
                get { return _subformation == null ? formation.subformation : _subformation; }
                set { _subformation = value; }
            }

            public int column { get; set; }
            public int row { get; set; }

            public int man { get; set; }
            int _sprite = 0;
            public int sprite { get { return _sprite; } set { _sprite = value; } }
            Vector _facing = new Vector(0,-1);
            public Vector facing { get { return _facing; } set { _facing = value; } }
            public string subtype { get; set; }

            Formation formation;

            public float x, y;


            public Slot(string str, int col, int row, Config config, Formation formation)
            {
                this.formation = formation;
                int man;
                bool parse = false;

                this.column = col;
                this.row = row;

                // ,"special situations (row distance - column distance - sprite - facing - subform - subtype), must precede man number",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"Place a '+' after row or column distance to have the distance dependent on reg size, the distance that you add will be between reg's",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"sprite is an int and indexes into the unitglobal.csv file as to what specific sprite to use, leave 0 for default, valid values are currently 1-6",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,facing is an integer specifying the number of degrees that the unit should face off of the flag bearer,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"subform is the id of the sub formation to use for this slot, if it's not set, it uses the default for the formation",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"subtype is a number of the unit type to fill in for this slot (really only useful for div level formations), 1-Inf,2-Cav,3-Art, leave blank or zero for any",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"you only have to specify these if that specific slot uses a value, otherwise it is not necessary",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
                // ,"you only have to include up to the value you need, so if you need a 10 row distance and all else is default you just need to do (10)",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,


                if (str.Contains(")"))
                {
                    // Console.WriteLine("Parsing SLot "+str);
                    string[] a = str.Split(new Char[] { '(', ')' });

                    string overridePart = a[1];
                    string manPart = a[2];

                    if (manPart.Length < 1)
                    {
                        //typo in the drills.csv, but we will handle it
                        if (a[0].Length < 1)
                        {
                            Log.Error(this, "[Formation] " + formation.id + " Unable to parse () slot " + str);
                            return;
                        }
                        manPart = a[0];
                    }
                    


                    //if (manPart.StartsWith("("))
                    //{
                    //    //type in the drills.csv, but we will handle it
                    //    manPart = a[0];
                    //    overridePart = a[1];
                    //}

                    string[] overrideInfo = overridePart.Substring(1, overridePart.Length - 1).Split('-');


                    try
                    {
                        if (overrideInfo.Length > 0 && overrideInfo[0] != String.Empty && overrideInfo[0] != "0") _rowDistYds = new Distance(overrideInfo[0]);
                        if (overrideInfo.Length > 1 && overrideInfo[1] != String.Empty && overrideInfo[1] != "0") _colDistYds = new Distance(overrideInfo[1]);
                        if (overrideInfo.Length > 2 && overrideInfo[2] != String.Empty && overrideInfo[2] != "0") _sprite = Convert.ToInt32(overrideInfo[2]);
                        if (overrideInfo.Length > 3 && overrideInfo[3] != String.Empty && overrideInfo[3] != "0") _facing = WorldTransform.FacingToVector(Convert.ToInt32(overrideInfo[3]) + FACING_OFFSET);
                        if (overrideInfo.Length > 4 && overrideInfo[4] != String.Empty && overrideInfo[4] != "0") _subformation = config.formations[overrideInfo[4]];
                    }
                    catch
                    {
                       Log.Error(this,"[Formation] " + formation.id + " Unable to parse () slot " + str);
                       return;
                    }
                        // if (info.Length > 5 && info[5] != "0") _subtype = info[4];
                    
                    //if (a.Length < 2)
                    //{
                    //    Log.Error(this,"[Formation] " + formation.id + " Unable to parse () slot " + str);
                    //    return;
                    //}

                    parse = Int32.TryParse(manPart, out man);
                }
                else
                {
                    //man = Convert.ToInt32(str);
                    parse = Int32.TryParse(str, out man);
                }
                if (!parse)
                {
                    Log.Error(this, formation.id + " Unable to parse slot " + str);
                    return;
                    //throw new System.FormatException(formation.id + " Unable to parse slot " + str);
                }

                //man is 1 indexed
                if (man > formation.slots.Length)
                {
                    Log.Info(this,formation.id + " man " + str + " [" + man + "] outside of formation size " + formation.slots.Length);
                }
                this.man = man;

                // if (row*col >= formation.slots.Length)
                // {
                //     Log.Info(this,"[Formation] " + formation.id + " Skipping Slot Data " + str + " [" + col +","+row+"] outside of formation size " + formation.slots.Length);
                // }

                // slot index is 1 based
                formation.SetSlot(this);
            }

            
        }

        public class Location {
            public Point position;
            public Vector direction;
            public Formation formation;

            public Location(Point position,Vector direction,Formation formation) {
                this.position = position;
                this.direction = direction;
                this.formation = formation;
            }

        }

    }
}