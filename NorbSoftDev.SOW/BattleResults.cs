using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorbSoftDev.SOW
{
    public class BattleResultsModifier : UnitStatsModifier<BattleResultStats>
    {



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


        private float _killed = 1;
        public float killed
        {
            get { return _killed; }
            set
            {
                _killed = value;
                OnPropertyChanged("killed");
                OnPropertyChanged("summary");
            }
        }

        private float _deserted = 1;
        public float deserted
        {
            get { return _deserted; }
            set
            {
                _deserted = value;
                OnPropertyChanged("deserted");
                OnPropertyChanged("summary");
            }
        }

        private float _wounded = 1;
        public float wounded
        {
            get { return _wounded; }
            set
            {
                _wounded = value;
                OnPropertyChanged("wounded");
                OnPropertyChanged("summary");
            }
        }

        public string summary
        {
            get { return ToString(); }
        }

        public BattleResultsModifier() { }

        public BattleResultsModifier(BattleResultsModifier other)
            : this()
        {
            if (other == null) return;
            _ammo = other._ammo;
            _killed = other._killed;
            _wounded = other._wounded;
            _deserted = other._deserted;
        }


        public int PreviewAmmo(BattleResultStats stats)
        {
            return (int)(stats.unit.ammo * (1 - _ammo) + stats.ammo * _ammo);

        }

        public int PreviewHeadCount(BattleResultStats stats)
        {

            return stats.unit.headCount - (int)(
                stats.deserted * _deserted
                + stats.wounded * _wounded
                + stats.killed * _killed
                );

        }

        public override string ToString()
        {
            return String.Format("Ammo:{0:P2} Killed:{1:P2} Wounded:{2:P2} Deserted:{2:P2}", ammo, killed, wounded, deserted);
        }

    }


    public interface IBattleResultModified {
         BattleResultsModifier modifier
        {
            get;
            set;
        }
    }

    public class BattleResultStats : UnitStatsGeneric<ScenarioUnit, BattleResultStats, BattleResultsRoster>, IBattleResultModified
    {

        // dumped by k key in game

        public BattleResultStats() : base() { }

        BattleResultStats _parent;
        public override BattleResultStats parent
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

        BattleResultsModifier _modifier;
        public BattleResultsModifier modifier
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
                        modifier = new BattleResultsModifier();

                    }
                    else
                    {
                        modifier = new BattleResultsModifier(parent.modifier);
                    }
                    return;
                }
                modifier = null;

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

        private int _deserted;
        public int deserted
        {
            get { return _deserted; }
            set
            {
                _deserted = value;
                OnPropertyChanged("deserted");
            }
        }

        private int _killed;
        public int killed
        {
            get { return _killed; }
            set
            {
                _killed = value;
                OnPropertyChanged("killed");
            }
        }

        private int _wounded;
        public int wounded
        {
            get { return _wounded; }
            set
            {
                _wounded = value;
                OnPropertyChanged("wounded");
            }
        }

        private string _status;
        public string status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("status");
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
                    unit.headCount - (deserted + wounded + killed)
                    : modifier.PreviewHeadCount(this);



            }
        }


        public override void ParseCSV(CsvReader csv, Dictionary<string, int> headerLUT, Config config, string where)
        {

            if (headerLUT.ContainsKey("ammo"))
                ammo = csv.ToInt32(headerLUT, "ammo", where);

            if (headerLUT.ContainsKey("deserted"))
                deserted = csv.ToInt32(headerLUT, "deserted", where);

            if (headerLUT.ContainsKey("killed"))
                killed = csv.ToInt32(headerLUT, "killed", where);

            if (headerLUT.ContainsKey("wounded"))
                wounded = csv.ToInt32(headerLUT, "wounded", where);

            if (headerLUT.ContainsKey("status"))
                status = csv.ToString(headerLUT, "status", where);
        }


        public override void ApplyResults()
        {
            if (unit == null) return;
            unit.ammo = PreviewAmmo;
            unit.headCount = PreviewHeadCount;
        }
    }

    public class BattleResultsRoster : UnitStatsRoster<BattleResultStats, ScenarioUnit, ScenarioEchelon>, IBattleResultModified
    {


        public BattleResultsRoster(Config config) : base(config)
        {
        }

        BattleResultsModifier _modifier;
        public BattleResultsModifier modifier
        {
            get { return _modifier; }

            set
            {
                if (_modifier == value) return;
                //if (_modifier != null) _modifier.PropertyChanged -= all_PropertyChanged;
                //if (value != null) value.PropertyChanged += all_PropertyChanged;
                _modifier = value;
                OnPropertyChanged("");
            }
        }

        //public override UnitStatsModifier rootModifier
        //{
        //    get { return _modifier; }
        //}

        protected override void Reorg(IList<ScenarioUnit> units)
        {

            this.modifier = new BattleResultsModifier();

            foreach (BattleResultStats stat in unitStats)
            {
                stat.roster = this;


                //usually you only want to apply stats to the regiments
                if (stat.unit.scenarioEchelon.children.Count != 0) stat.active = false;

                //figure out children for inheritance of modifiers

                BattleResultStats parentStat;
                ScenarioEchelon parentEchelon = stat.unit.scenarioEchelon.parent as ScenarioEchelon;
                if (parentEchelon == null) continue;
                ScenarioUnit parentUnit = parentEchelon.unit;
                if (parentUnit == null) continue;

                if (unitStatsLUT.TryGetValue(parentUnit, out parentStat))
                {
                    stat.parent = parentStat;
                }


            }

            unitStats = unitStats.OrderBy(o => ((BattleResultStats)o).unit.echelon.id).ToList();
        }
    }
}
