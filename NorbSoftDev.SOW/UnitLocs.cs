using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorbSoftDev.SOW
{
    public class UnitLocsModifier : UnitStatsModifier<UnitLocStats>
    {



        private bool _transform = true;
        public bool transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                OnPropertyChanged("transform");
                OnPropertyChanged("summary");

            }
        }

        private float _ammo = 1;
        public float ammo
        {
            get { return _ammo; }
            set
            {
                _ammo = value;
                OnPropertyChanged("ammo");
                OnPropertyChanged("summary");

            }
        }

        private bool _formation;
        public bool formation
        {
            get { return _formation; }
            set
            {
                _formation = value;
                OnPropertyChanged("formation");
            }
        }

        private float _headCount = 1;
        public float headCount
        {
            get { return _headCount; }
            set
            {
                _headCount = value;
                OnPropertyChanged("headCount");
                OnPropertyChanged("summary");
            }
        }


        public string summary
        {
            get { return ToString(); }
        }

        public UnitLocsModifier() { }

        public UnitLocsModifier(UnitLocsModifier other)
            : this()
        {
            if (other == null) return;
            _transform = other._transform;
            _headCount = other._headCount;
        }

        public int PreviewAmmo(UnitLocStats stats)
        {
            return (int)(stats.unit.ammo * (1 - _ammo) + stats.ammo * _ammo);

        }



        public int PreviewHeadCount(UnitLocStats stats)
        {
            return (int)(stats.unit.headCount * (1 - _headCount) + stats.headCount * _headCount);

        }

        public WorldTransform PreviewTransform(UnitLocStats stats)
        {

            return _transform ? stats.transform : stats.unit.transform;

        }

        public Formation PreviewFormation(UnitLocStats stats)
        {
            return _formation ? stats.formation : stats.unit.formation;

        }
        public override string ToString()
        {
            return String.Format("Ammo:{0:P2} HeadCount:{1:P2} Ammo:{2:P2}", _ammo, _headCount, _transform);
        }

    }



    public class UnitLocStats : UnitStatsGeneric<ScenarioUnit, UnitLocStats, UnitLocsRoster>
    {
        // dumped by k key in game

        public UnitLocStats() : base() { }

        UnitLocStats _parent;
        public override UnitLocStats parent
        {
            get { return _parent; }

            set
            {

                if (_parent == value) return;
                if (_parent != null) _parent.PropertyChanged -= all_PropertyChanged;
                if (value != null) value.PropertyChanged += all_PropertyChanged;
                _parent = value;
                OnPropertyChanged("");
            }
        }

        UnitLocsModifier _modifier;
        public UnitLocsModifier modifier
        {
            get { return _modifier == null ? parent == null ? roster == null ? null : roster.modifier : parent.modifier : _modifier; }

            set
            {
                if (_modifier == value) return;
                if (_modifier != null) _modifier.PropertyChanged -= all_PropertyChanged;
                if (value != null) value.PropertyChanged += all_PropertyChanged;
                _modifier = value;
                OnPropertyChanged("");
            }
        }
        public bool IsLocallyModified
        {
            get
            {
                return _modifier != null;
            }
            set
            {
                //leave alone if unchanged, otherwise create a modifier if true, or remove otherwise
                if (value == (_modifier != null)) return;
                if (value)
                {
                    if (parent == null)
                    {
                        modifier = new UnitLocsModifier();
                    }
                    else
                    {
                        modifier = new UnitLocsModifier(parent.modifier);
                    }
                    return;
                }
                modifier = null;

            }
        }

        private WorldTransform _transform;
        public WorldTransform transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                OnPropertyChanged("transform");
            }
        }

        private Formation _formation;
        public Formation formation
        {
            get { return _formation; }
            set
            {
                _formation = value;
                OnPropertyChanged("formation");
            }
        }

        private int _ammo;
        public int ammo
        {
            get { return _ammo; }
            set
            {
                _ammo = value;
                OnPropertyChanged("ammo");
            }
        }

        private int _headCount;
        public int headCount
        {
            get { return _headCount; }
            set
            {
                _headCount = value;
                OnPropertyChanged("headCount");
            }
        }

        public WorldTransform PreviewTransform
        {
            get
            {
                if (_transform == null) return unit.transform;
                return active == false ? unit.transform : modifier == null ? _transform : modifier.PreviewTransform(this);
            }
        }

        public Formation PreviewFormation
        {
            get
            {
                if (_formation == null) return unit.formation;
                return active == false ? unit.formation : modifier == null ? _formation : modifier.PreviewFormation(this);
            }
        }

        public int PreviewAmmo
        {
            get
            {
                return active == false ? unit.ammo : modifier == null ? _ammo : modifier.PreviewAmmo(this);
            }
        }

        public int PreviewHeadCount
        {
            get
            {
                return active == false ? unit.headCount : modifier == null ?
                    this.headCount
                    : modifier.PreviewHeadCount(this);
            }
        }




        public override void ParseCSV(CsvReader csv, Dictionary<string, int> headerLUT, Config config, string where)
        {

            if (headerLUT.ContainsKey("dirSouth"))
            { 
                float dirSouth = csv.ToSingle(headerLUT, "dirSouth", where);
                float dirEast = csv.ToSingle(headerLUT, "dirEast", where);
                float south = csv.ToSingle(headerLUT, "south", where);
                float east = csv.ToSingle(headerLUT, "east", where);

                this.transform = new WorldTransform(dirSouth, dirEast, south, east);
            }

            if (headerLUT.ContainsKey("formation"))
            {
                formation = csv.GetValueAllowEmpty(headerLUT, config.formations, "formation");
            }



            if (headerLUT.ContainsKey("headCount"))
                headCount = csv.ToInt32(headerLUT, "headCount", where);


            // foreach (Attribute attribute in attributeNames)
            //     {
            //         if (!headerLUT.ContainsKey(attribute.name))
            //         {
            //             continue;
            //         }

            //         int column;
            //         try
            //         {
            //             column = headerLUT[attribute.name];
            //         }
            //         catch (System.Collections.Generic.KeyNotFoundException)
            //         {
            //             Log.Error(this, attribute.name+" not in header");
            //             throw;
            //         }

            //         try
            //         {
            //             if (csv[column] == String.Empty)
            //             {
            //                 sunit.attributes[attribute.name] = null;
            //                 continue;
            //             }
            //         }
            //         catch (System.ArgumentOutOfRangeException e)
            //         {
            //             Log.Error(this,"\"" + sunit.id + "\" attribute \"" + attribute.name + "\" column " + column + " is out of csv range of " + csv.FieldCount);
            //             throw;
            //         }

            //         int level = csv.ToInt32(headerLUT, attribute.name, where);

            //         try
            //         {
            //             sunit.attributes[attribute.name] = attribute[level];
            //         }
            //         catch (System.ArgumentOutOfRangeException e)
            //         {
            //             Log.Warn(this,"" + sunit.id + " Attribute out-of-range: " + attribute.name + " column:" + column + " level:" + level);
            //             foreach (AttributeLevel alevel in attribute)
            //             {
            //                 Log.Info(this,"  " + alevel.index + " " + alevel);
            //             }
            //             sunit.attributes[attribute.name] = null;
            //         }
            //     }


        }


        public override void ApplyResults()
        {
            if (unit == null) return;
            unit.transform.Set(PreviewTransform);
            unit.headCount = PreviewHeadCount;
            unit.formation = PreviewFormation;
            unit.ammo = PreviewAmmo;
        }
    }

    public class UnitLocsRoster : UnitStatsRoster<UnitLocStats, ScenarioUnit, ScenarioEchelon>
    {


        public UnitLocsRoster(Config config) : base(config)
        {
        }


        private UnitLocsModifier _modifier;
        public UnitLocsModifier modifier
        {
            get { return _modifier; }
            set
            {
                _modifier = value;
                OnPropertyChanged("unitLocsModifier");
                OnPropertyChanged("rootModifier");
            }
        }

        //public override UnitStatsModifier rootModifier
        //{
        //    get { return _modifier; }
        //}

        protected override void Reorg(IList<ScenarioUnit> units)
        {
            this.modifier = new UnitLocsModifier();


            foreach (UnitLocStats stat in unitStats)
            {

                stat.roster = this;

                //figure out children for inheritance of modifiers

                UnitLocStats parentStat;
                ScenarioEchelon parentEchelon = stat.unit.scenarioEchelon.parent as ScenarioEchelon;
                if (parentEchelon == null) continue;
                ScenarioUnit parentUnit = parentEchelon.unit;
                if (parentUnit == null) continue;

                if (unitStatsLUT.TryGetValue(parentUnit, out parentStat))
                {
                    stat.parent = parentStat;
                }


            }

            unitStats = unitStats.OrderBy(o => ((UnitLocStats)o).unit.echelon.id).ToList();
        }
    }
}
