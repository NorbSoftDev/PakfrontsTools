using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NorbSoftDev.SOW
{

    public enum ERank { None = -1, Battalion=0, Regiment = 2, Brigade = 4, Division = 6, Corps = 8, Army = 10, Side = 12, Root = 13 }

    public static class EchelonHelper
    {
        public static void DecomposeEchelonId(long echelonId, out long side, out long army, out long corps, out long div, out long bgde, out long reg)
        {
            side = echelonId / ERank.Side.Hash();
            echelonId -= side * ERank.Side.Hash();

            army = echelonId / ERank.Army.Hash();
            echelonId -= army * ERank.Army.Hash();

            corps = echelonId / ERank.Corps.Hash();
            echelonId -= corps * ERank.Corps.Hash();

            div = echelonId / ERank.Division.Hash();
            echelonId -= div * ERank.Division.Hash();

            bgde = echelonId / ERank.Brigade.Hash();
            echelonId -= bgde * ERank.Brigade.Hash();

            reg = echelonId / ERank.Regiment.Hash();
            echelonId -= reg * ERank.Regiment.Hash();
        }


        public static void DecomposeEchelonId(long echelonId, out long side, out long army, out long corps, out long div, out long bgde, out long reg, out long comp)
        {
            side = echelonId / ERank.Side.Hash();
            echelonId -= side * ERank.Side.Hash();

            army = echelonId / ERank.Army.Hash();
            echelonId -= army * ERank.Army.Hash();

            corps = echelonId / ERank.Corps.Hash();
            echelonId -= corps * ERank.Corps.Hash();

            div = echelonId / ERank.Division.Hash();
            echelonId -= div * ERank.Division.Hash();

            bgde = echelonId / ERank.Brigade.Hash();
            echelonId -= bgde * ERank.Brigade.Hash();

            reg = echelonId / ERank.Regiment.Hash();
            echelonId -= reg * ERank.Regiment.Hash();

            comp = echelonId / ERank.Battalion.Hash();
            echelonId -= comp * ERank.Battalion.Hash();
        }

        public static long ComposeEchelonId(long side, long army, long corps, long div, long bgde, long reg)
        {
            return side * ERank.Side.Hash() + army * ERank.Army.Hash() + corps * ERank.Corps.Hash() + div * ERank.Division.Hash()
                + bgde * ERank.Brigade.Hash() + reg * ERank.Regiment.Hash();
        }


        public static long ComposeEchelonId(long side, long army, long corps, long div, long bgde, long reg, long comp)
        {
            return side * ERank.Side.Hash() + army * ERank.Army.Hash() + corps * ERank.Corps.Hash() + div * ERank.Division.Hash()
                + bgde * ERank.Brigade.Hash() + reg * ERank.Regiment.Hash() + comp * ERank.Battalion.Hash();
        }

        public static long SideIndexOf(long echelonId) {
              return echelonId / ERank.Side.Hash();
        }

        public static ERank RankOf(long echelonId)
        {
            long side, army, corps, div, bgde, reg, comp;

            DecomposeEchelonId(echelonId, out side, out army, out corps, out div, out bgde, out reg, out comp);

            if (army < 1) return ERank.Side;
            if (corps < 1) return ERank.Army;
            if (div < 1) return ERank.Corps;
            if (bgde < 1) return ERank.Division;
            if (reg < 1) return ERank.Brigade;
            if (comp < 1) return ERank.Regiment;                      
            return ERank.Battalion;
        }

        public static long ParentEchelonId(long echelonId)
        {
            long side, army, corps, div, bgde, reg, comp;
            DecomposeEchelonId(echelonId, out side, out army, out corps, out div, out bgde, out reg, out comp);
            return ParentEchelonId(side, army, corps, div, bgde, reg, comp);
        }

        public static long ParentEchelonId(long side, long army, long corps, long div, long bgde, long reg)
        {
            long id = 0;
            id += side * ERank.Side.Hash();
            if (corps < 1) return id;

            id += army * ERank.Army.Hash();
            if (div < 1) return id;

            id += corps * ERank.Corps.Hash();
            if (bgde < 1) return id;

            id += div * ERank.Division.Hash();
            if (reg < 1) return id;

            id += bgde * ERank.Brigade.Hash();

            return id;
        }

        public static long ParentEchelonId(long side, long army, long corps, long div, long bgde, long reg, long comp)
        {
            long id = 0;
            id += side * ERank.Side.Hash();
            if (corps < 1) return id;

            id += army * ERank.Army.Hash();
            if (div < 1) return id;

            id += corps * ERank.Corps.Hash();
            if (bgde < 1) return id;

            id += div * ERank.Division.Hash();
            if (reg < 1) return id;

            id += bgde * ERank.Brigade.Hash();
            if (comp < 1) return id;

            id += reg * ERank.Regiment.Hash();

            return id;
        }

    }

    public interface IEchelon
    {
        // cache
        long sideIndex { get; }
        long armyIndex { get; }
        long corpsIndex { get; }
        long divisionIndex { get; }
        long brigadeIndex { get; }
        long regimentIndex { get; }
        long battalionIndex { get; }
        IEchelonRoot root { get; }
        //IEchelon sideEchelon { get;  }
        //IEchelon armyEchelon { get;  }
        //IEchelon corpsEchelon { get; }
        //IEchelon divisionEchelon { get;  }
        //IEchelon brigadeEchelon { get;  }

        ERank rank { get; }

        string symbol { get;  }

        bool isLeaf { get; }

        bool isRoot { get; }

        string AsCsv();

        long id { get; }

        bool CanBeChildOf(IEchelon other);

        bool CanBeSiblingOf(IEchelon other);

        int nInfantry { get; }
        int nCavalry
        {
            get;
        }

        int nArtillery
        {
            get;
        }

    }

    public interface IEchelonRoot
    {
        ObservableRoster roster { get; set;  }
    }

    public abstract class EchelonHolder<T> :
		ObservableCollection<T>,
		IComparable<T>, IEchelon
        where T : EchelonHolder<T>
    {
        // cache
        protected long _sideIndex, _armyIndex, _corpsIndex, _divisionIndex, _brigadeIndex, _regimentIndex, _battalionIndex;
        public long sideIndex { get { return _sideIndex; } }
        public long armyIndex { get { return _armyIndex; } }
        public long corpsIndex { get { return _corpsIndex; } }
        public long divisionIndex { get { return _divisionIndex; } }
        public long brigadeIndex { get { return _brigadeIndex; } }
        public long regimentIndex { get { return _regimentIndex; } }
        public long battalionIndex { get { return _battalionIndex; } }

        public T rootEchelon { get; protected set; }
        public T sideEchelon { get; protected set; }
        public T armyEchelon { get; protected set; }
        public T corpsEchelon { get; protected set; }
        public T divisionEchelon { get; protected set; }
        public T brigadeEchelon { get; protected set; }
        public T regimentEchelon { get; protected set; }


        public string sideName
        {
            get
            {
                switch (sideIndex)
                {
                    case 0:
                        return "None";
                    case 1:
                        return "Blue";
                    case 2:
                        return "Red";
                    case 3:
                        return "Green";
                    default:
                        return "Unknown";

                }
            }
        }

        public bool isDirty { get; protected set; }

        public EchelonHolder() : base()
        {
            CollectionChanged += HandleCollectionChanged;
        }

        public IEchelonRoot root
        {
            get {
                return (IEchelonRoot) rootEchelon;
            }
        }

        public virtual bool isLeaf
        {
            get
            {
                return (rank == ERank.Battalion);
            }
        }

        public virtual bool isRoot
        {
            get
            {
                return false;
            }

        }

        public virtual long id
        {
            get
            {
                return parent != null ?  parent.id + OffsetIndex() * rank.Hash() : 0;
            }
            // public static int ComposeEchelonId( int side, int army, int corps, int div, int bgde, int reg) {
            //     return side*Rank.Side.Hash() + army*Rank.Army.Hash() + corps*Rank.Corps.Hash() + div*Rank.Division.Hash()
            //         + bgde*Rank.Brigade.Hash()+ reg*Rank.Regiment.Hash();
            // }           

        }

        protected ERank _rank = ERank.None;
        public virtual ERank rank
        {
            get
            {
                if (_rank == ERank.None || _rank == ERank.Root) _rank = ComputeRank();
                return _rank;
            }
        }

        //public string rightSymbol
        //{
        //    get
        //    {
        //        switch (_rank)
        //        {
        //            case ERank.Side:
        //                return "XXXXX...";
        //            case ERank.Army:
        //                return " XXXX...";
        //            case ERank.Corps:
        //                return "  XXX...";
        //            case ERank.Division:
        //                return "   XX...";
        //            case ERank.Brigade:
        //                return "    X...";
        //            case ERank.Regiment:
        //                return "     III";
        //            case ERank.Battalion:
        //                return "     II";
        //            default:
        //                return "-------?";
        //        }
        //    }
        //}

        public string symbol
        {
            get
            {
                switch (_rank)
                {
                    case ERank.Side:
                        return "XXXXX";
                    case ERank.Army:
                        return "XXXX";
                    case ERank.Corps:
                        return "XXX";
                    case ERank.Division:
                        return "XX";
                    case ERank.Brigade:
                        return "X";
                    case ERank.Regiment:
                        return "III";
                    case ERank.Battalion:
                        return "II";
                    default:
                        return "?";
                }
            }
        }

        public abstract int nInfantry
        {
            get;
        }

        public abstract int nCavalry
        {
            get;
        }

        public abstract int nArtillery
        {
            get;
        }

        public T children
        {
            get { return (T)this; }

        }

        public int DescendantsCount()
        {
            int ret = 0;
            foreach (T child in children)
            {
                ret++;
                ret += child.DescendantsCount();
            }
            return ret;
        }

        // public void DescendantsList(IAttach attachedTo, ref List<T> list) {

        //     foreach (T child in children) {
        //         if (attachedTo != null && attachedTo != child.unit.attachedTo)
        //             continue;
        //         list.Add(child);
        //         child.DescendantsList(ref list);
        //     }
        // }

        // public void LeafDescendantsList(IAttach attachedTo, ref List<T> list) {
        //     foreach (T child in children) {
        //         if (attachedTo != null && attachedTo != child.unit.attachedTo)
        //             continue;                
        //         if (child.isLeaf) {
        //             list.Add(child);
        //         } else {
        //             child.LeafDescendantsList(ref list);
        //         }
        //     }
        // }

        public int CompareTo(T that)
        {
            return this.id.CompareTo(that.id);
        }

        public string AsCsv()
        {
            //DecomposeEchelonId(id, out _side, out _army, out _corps, out _division, out _brigade, out _regiment);

            return String.Format("{0},{1},{2},{3},{4},{5},{6}",
               _sideIndex, _armyIndex, _corpsIndex, _divisionIndex, _brigadeIndex, _regimentIndex, _battalionIndex);
        }

        protected int OffsetIndex()
        {
            return _parent.IndexOf((T)this) + 1;
        }

        T _parent = null;

        public T parent
        {
            get
            {
                return _parent;
            }

            set
            {
                if (value == parent) return;

                if (_parent != null)
                {
                    _parent.Remove((T)this);
                }

                _parent = value;

                if (value != null)
                {
                    value.Add((T)this);
                }

                UpdateCache();
                //OnPropertyChanged("parent");
                OnPropertyChanged(new PropertyChangedEventArgs("parent"));
            }
        }

        public virtual bool CanBeChildOf(IEchelon other)
        {
            //Console.WriteLine(this.sideIndex + " ?= " + other.sideIndex);
            //return this.sideIndex == other.sideIndex && this.rank.Up() == other.rank;
            if (this == other) return false;
            bool can = this.sideIndex == other.sideIndex && this.rank.Up() == other.rank;
            return can;
        }

        public virtual bool CanBeSiblingOf(IEchelon other)
        {
            return this.sideIndex == other.sideIndex && this.rank == other.rank;
        }


        public void InsertChild(int index, T child)
        {
            child.parent = null;
            Insert(index, child);
        }

        protected virtual ERank ComputeRank()
        {
            if (_parent == null)
            {
                _rank = ERank.None;
                return ERank.None;
            }
            
            if (_rank == ERank.None) _rank = _parent.ComputeRank().Down();
            return _rank;
        }


        public void UpdateCache()
        {
            EchelonHelper.DecomposeEchelonId(id, out _sideIndex, out _armyIndex, out _corpsIndex, out _divisionIndex, out _brigadeIndex, out _regimentIndex);
            _rank = ComputeRank();
            switch (_rank)
            {
                case ERank.Side:
                    rootEchelon = parent;
                    sideEchelon = (T)this;
                    armyEchelon = null;
                    corpsEchelon = null;
                    divisionEchelon = null;
                    brigadeEchelon = null;
                    break;
                case ERank.Army:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent;
                    armyEchelon = (T)this;
                    corpsEchelon = null;
                    divisionEchelon = null;
                    brigadeEchelon = null;
                    break;
                case ERank.Corps:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent.sideEchelon;
                    armyEchelon = parent.armyEchelon;
                    corpsEchelon = (T)this;
                    divisionEchelon = null;
                    brigadeEchelon = null;
                    break;
                case ERank.Division:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent.sideEchelon;
                    armyEchelon = parent.armyEchelon;
                    corpsEchelon = parent;
                    divisionEchelon = (T)this;
                    brigadeEchelon = null;
                    break;
                case ERank.Brigade:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent.sideEchelon;
                    armyEchelon = parent.armyEchelon;
                    corpsEchelon = parent.corpsEchelon;
                    divisionEchelon = parent;
                    brigadeEchelon = (T)this;
                    break;
                case ERank.Regiment:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent.sideEchelon;
                    armyEchelon = parent.armyEchelon;
                    corpsEchelon = parent.corpsEchelon;
                    divisionEchelon = parent.divisionEchelon;
                    brigadeEchelon = parent;
                    break;
                case ERank.Battalion:
                    rootEchelon = parent.rootEchelon;
                    sideEchelon = parent.sideEchelon;
                    armyEchelon = parent.armyEchelon;
                    corpsEchelon = parent.corpsEchelon;
                    divisionEchelon = parent.divisionEchelon;
                    brigadeEchelon = parent.brigadeEchelon;
                    regimentEchelon = parent;
                    break;
                default:
                    rootEchelon = null;
                    sideEchelon = null;
                    armyEchelon = null;
                    corpsEchelon = null;
                    divisionEchelon = null;
                    brigadeEchelon = null;
                    regimentEchelon = null;
                    break;
            }

            foreach (T child in this)
            {
                child.UpdateCache();
            }
        }


        ///
        /// updates caches when main collection changes

        void HandleCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                    foreach (T item in e.NewItems)
                    {
                        item._parent = (T)this;
                        //item.PropertyChanged += HandleElementChanged;
                    }
                    break;


                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        if (item._parent == this)
                        {
                            item._parent = null;
                            //item.UpdateCache();
                        }
                        //item.PropertyChanged -= HandleElementChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (T item in e.OldItems)
                    {
                        if (item._parent == this)
                        {
                            item._parent = null;
                            //item.UpdateCache();
                        }
                        //item.PropertyChanged -= HandleElementChanged;
                    }
                    foreach (T item in e.NewItems)
                    {
                        item._parent = (T)this;
                        //item.UpdateCache();
                        //item.PropertyChanged += HandleElementChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (T item in e.OldItems)
                    {
                        if (item._parent == this)
                        {
                            item._parent = null;
                            //item.UpdateCache();
                        }
                        //item.PropertyChanged -= HandleElementChanged;
                    }
                    break;
            }

            isDirty = true;
            foreach (T item in this)
            {
                item.UpdateCache();
            }

            if (e.NewItems != null)
                foreach (T item in e.NewItems)
                    item.PropertyChanged += element_PropertyChanged;

            if (e.OldItems != null)
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= element_PropertyChanged;

            OnPropertyChanged(new PropertyChangedEventArgs("nInfantry"));
            OnPropertyChanged(new PropertyChangedEventArgs("nCavalry"));
            OnPropertyChanged(new PropertyChangedEventArgs("nArtillery"));

        }

        private void element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "nInfantry")
                OnPropertyChanged(new PropertyChangedEventArgs("nInfantry"));
            
            if (e.PropertyName == "nCavalry")

                OnPropertyChanged(new PropertyChangedEventArgs("nCavalry"));

            if (e.PropertyName == "nArtillery")

                OnPropertyChanged(new PropertyChangedEventArgs("nArtillery"));
            
        }


        abstract public bool hasUnit { get; }
        public T FirstUsedEchelon()
        {
            if (hasUnit) return (T)this;
            foreach (T child in this)
            {
                T k = child.FirstUsedEchelon();
                if (k != null) return k;
            }
            return null;
        }



        #region INotifyPropertyChanged
        protected void UnitsUpdated() {
            OnPropertyChanged(new PropertyChangedEventArgs("units"));
            OnPropertyChanged(new PropertyChangedEventArgs("hasUnits"));
            OnPropertyChanged(new PropertyChangedEventArgs("niceName"));
        }

        //SHULD THIS BE NEW?
        //public new event PropertyChangedEventHandler PropertyChanged;
        //protected override event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged(string name)
        //{
            //PropertyChangedEventHandler handler = ((INotifyPropertyChanged)this).PropertyChanged;
            //if (handler != null)
            //{
                //handler(this, new PropertyChangedEventArgs(name));
            //}
        //}
        #endregion

    }

    abstract public class EchelonGeneric<T> : EchelonHolder<EchelonGeneric<T>> where T : IUnit
    {
        #region Units
        internal List<T> _units = new List<T>();

        public IEnumerable<T> units { 
            get {
                return _units;

            }
        }

        public T unit
        {
            get
            {
                if (_units.Count < 1) return default(T);
                return _units[0];
            }
        }

        public void Remove(T unit)
        {
            if (!_units.Contains(unit)) return;
            _units.Remove(unit);
            //OnPropertyChanged("units");
            UnitsUpdated();
        }

        public bool Contains(T iunit)
        {
            return _units.Contains(iunit);
        }

        abstract public void Add(T unit);

        abstract public T InsertUnitWithChildren(OOBUnit unit);
        abstract public T InsertUnit(OOBUnit unit);

        #endregion


        public void ListAllChildrenAndUnits(List<T> allunits, List<EchelonGeneric<T>> allechelons)
        {

            allunits.AddRange(_units);
            allechelons.Add(this);
            foreach (EchelonGeneric<T> child in children) {
                child.ListAllChildrenAndUnits(allunits, allechelons);
            }
            
        }


        public override string ToString()
        {
            if (unit == null) return rank.ToString() + ":null";
            return rank.ToString() + ": " + unit.name1;
        }

        public string niceName
        {
            get
            {
                if (unit == null) return (sideName+" "+rank.ToString());
                return unit.name1;
            }
        }
        override public bool hasUnit { get { return _units.Count > 0; } }

        protected void unit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "parent")
            //    OnPropertyChanged("parent");

            if (e.PropertyName == "headCount")
            {
                OnPropertyChanged(new PropertyChangedEventArgs("nInfantry"));
                OnPropertyChanged(new PropertyChangedEventArgs("nCavalry"));
                OnPropertyChanged(new PropertyChangedEventArgs("nArtillery"));
            }
        }


        public UnitType.EUnitType unitType {
            get {
                if (unit == null) return UnitType.EUnitType.None;
                return unit.unitClass.unitType.type;
            }
        }

        public override int nInfantry
        {
            get
            {
                int men = unit == null || unit.unitClass.unitType.type != UnitType.EUnitType.Infantry ? 0 : unit.headCount;
                foreach (IEchelon child in children) 
                {
                    men += child.nInfantry;
                }
                return men;
            }
        }

        public override int nCavalry
        {
            get
            {
                int men = unit == null || unit.unitClass.unitType.type != UnitType.EUnitType.Cavalry ? 0 : unit.headCount;
                foreach (IEchelon child in children)
                {
                    men += child.nCavalry;
                }
                return men;
            }
        }

        public override int nArtillery
        {
            get
            {
                int men = unit == null || unit.unitClass.unitType.type != UnitType.EUnitType.Artillery ? 0 : unit.headCount;
                foreach (IEchelon child in children)
                {
                    men += child.nArtillery;
                }
                return men;
            }
        }

        public abstract List<IEchelon> AllDescendantsGeneric();
       
        public void PrettyPrint()
        {

            Console.Write(
                AsCsv()+"".PadLeft((int)ERank.Root - (int)rank, '.')+'['+(unit == null ? "null" : unit.id)+']'+niceName
            );
            //Console.Write(
            //    //id.ToString() +
            //     " " + rank.ToString().Substring(0, 3) +
            //    "".PadLeft((int)ERank.Root - (int)rank, '.')
            //);
            //Console.WriteLine();

            //foreach (IUnit unit in _units)
            //{
            //    Console.Write(": \"" + unit.id + "\" \"" + unit.name1 + "\", \"" + unit.name2 + "\"");
            //    Console.WriteLine();
            //    Console.Write("Attributes:");
            //    foreach (KeyValuePair<string, AttributeLevel> kvp in unit.attributes)
            //    {
            //        if (kvp.Value == null) Console.Write(" " + kvp.Key + ":null");
            //        else Console.Write(" " + kvp.Value.attribute.name + ":" + kvp.Value);
            //    }
            //}
            Console.WriteLine();

            foreach (EchelonGeneric<T> echelon in this) echelon.PrettyPrint();
        }


    }

    public class OOBEchelon : EchelonGeneric<OOBUnit>
    {
        public override void Add(OOBUnit unit)
        {
            if (_units.Contains(unit)) return;
            _units.Add(unit);
            unit.oobEchelon = this;
            unit.PropertyChanged += unit_PropertyChanged;
            UnitsUpdated();


        }





        public override OOBUnit InsertUnitWithChildren(OOBUnit unit) {
            //TODO copy oobunit data 
            // make sure they use the same config (mods)
            // Shallow copy
            // reconnect
            throw new Exception("OOBUnit to OOBUnit not implmented yet");
        }

        public override OOBUnit InsertUnit(OOBUnit unit)
        {
            //TODO copy oobunit data 
            // make sure they use the same config (mods)
            // Shallow copy
            // reconnect
            throw new Exception("OOBUnit to OOBUnit not implmented yet");
        }



        public override List<IEchelon> AllDescendantsGeneric()
        {
            List<OOBEchelon> list = new List<OOBEchelon>();
            DescendantsList( ref list);
            //cast tastict
            List<IEchelon> echelons = new List<IEchelon>();
            echelons.AddRange(list);
            return echelons;
        }


        public  List<OOBEchelon> AllDescendants()
        {
            List<OOBEchelon> list = new List<OOBEchelon>();
            DescendantsList(ref list);
            return list;
        }

        public void DescendantsList(ref List<OOBEchelon> list) {
            foreach (object child in children) {
                OOBEchelon echelon = ((OOBEchelon)child);
                list.Add(echelon);
                echelon.DescendantsList(ref list);
            }
        }

        public void LeafDescendantsList(ref List<OOBEchelon> list) {
            foreach (object child in children) {
                OOBEchelon echelon = ((OOBEchelon)child);
                    if (echelon.isLeaf) {
                list.Add(echelon);
                } else {
                    echelon.DescendantsList(ref list);
                }
            }
        }

        //public bool CanBePlacedAsChildOf(ScenarioEchelon scenarioEchelon) 
        //{
        //    //TODO check for side
        //    return this.rank.Up() == scenarioEchelon.rank;

        //}

    }

    public class OOBEchelonRoot : OOBEchelon, IEchelonRoot
    {
        public override bool isRoot { get { return true; } }
        public override ERank rank
        {
            get
            {
                return ERank.Root;
            }
        }

        protected override ERank ComputeRank()
        {
            return ERank.Root;
        }

        public override long id
        {
            get
            {
                return 0; //ComposeEchelonId( 1, 0, 0, 0, 0, 0, 0);
            }
        }

        public ObservableRoster roster
        {
            get;
            set;
        }
    }

    public interface IScenarioEchelonRule {
        //void ApplyResults(ScenarioEchelon current);
    
    }

    public class ScenarioEchelon : EchelonGeneric<ScenarioUnit>
    {
        // internal ScenarioEchelon( int echelonId) : base(echelonId){
        // }
        public override void Add(ScenarioUnit unit)
        {
            if (_units.Contains(unit)) return;
            if (_units.Count > 0)
            {
                Log.Error(this, "["+id+"] already has a unit " + _units[0].id);
                return;
            }
            _units.Add(unit);
            unit.scenarioEchelon = this;
            UnitsUpdated();


            unit.PropertyChanged += unit_PropertyChanged;
        }


        public override ScenarioUnit InsertUnitWithChildren(OOBUnit unit) {

            if (root == null) {
                throw new Exception("[InsertUnitWithChildren] Scenario root is null "+root);
            }

            Scenario scenario = (Scenario) root.roster;
            if (scenario == null) {
                throw new Exception("[InsertUnitWithChildren] Scenario root roster is null");
            }

            return scenario.InsertUnitWithChildren(unit, this);
        }

        public override ScenarioUnit InsertUnit(OOBUnit unit)
        {

            if (root == null)
            {
                throw new Exception("[InsertUnit] Scenario root is null " + root);
            }

            Scenario scenario = (Scenario)root.roster;
            if (scenario == null)
            {
                throw new Exception("[InsertUnit] Scenario root roster is null");
            }

            return scenario.InsertUnit(unit, this);
        }

        public void RemoveFromRoster()
        {
            ((ScenarioUnitRoster)(root.roster)).RemoveEchelon(this);

        }

        internal IAttach _attachedTo;
        public IAttach attachedTo {
            get {
                return _attachedTo;
            }

            set {
                if (_attachedTo != null) {
                    _attachedTo.OnDetach(this);
                }

                _attachedTo = value;
            }
        }

        public  override List<IEchelon> AllDescendantsGeneric() {
            List<ScenarioEchelon> list = new  List<ScenarioEchelon>();
            DescendantsList(null, ref list);
            //cast tastict
            List<IEchelon> echelons = new List<IEchelon>();
            echelons.AddRange(list);
            return echelons;
        }

        public List<ScenarioEchelon> AllDescendants() {
            List<ScenarioEchelon> list = new  List<ScenarioEchelon>();
            DescendantsList(null, ref list);
            return list;
        }


        public void DescendantsList(IAttach onlyAttachedTo, ref List<ScenarioEchelon> list) {
            foreach (object child in children) {
                ScenarioEchelon echelon = ((ScenarioEchelon)child);
                if (onlyAttachedTo != null && echelon.attachedTo != null && echelon.attachedTo != onlyAttachedTo)
                    continue;

                list.Add(echelon);
                echelon.DescendantsList(onlyAttachedTo, ref list);
            }
        }


        IScenarioEchelonRule _rule;
        public IScenarioEchelonRule rule
        {

            
            get { return _rule; }

            set
            {

                if (_rule == value) return;
                _rule = value;
                OnPropertyChanged(new PropertyChangedEventArgs("rule"));
            }
        }

        // public void LeafDescendantsList(IAttach onlyAttachedTo, ref List<ScenarioEchelon> list) {
        //     foreach (object child in children) {
        //         ScenarioEchelon echelon = ((ScenarioEchelon)child);
        //         if (onlyAttachedTo != null && echelon.unit.attachedTo != onlyAttachedTo)
        //             continue;

        //         if (echelon.isLeaf) {
        //             list.Add(echelon);
        //         } else {
        //             echelon.DescendantsList(onlyAttachedTo, ref list);
        //         }
        //     }
        // }

        // public void LeafDescendantsList(ref List<ScenarioEchelon> list) {
        //     foreach (ScenarioEchelon child in children) {
        //         if (child.isLeaf) {
        //             list.Add(child);
        //         } else {
        //             child.LeafDescendantsList(ref list);
        //         }
        //     }
        // }
    }

    public class ScenarioEchelonRoot : ScenarioEchelon, IEchelonRoot
    {
        public override bool isRoot { get { return true; } }

        public override ERank rank
        {
            get
            {
                return ERank.Root;
            }
        }

        protected override ERank ComputeRank()
        {
            return ERank.Root;
        }

        public override long id
        {
            get
            {
                return 0; //ComposeEchelonId( 1, 0, 0, 0, 0, 0, 0);
            }
        }

        public ObservableRoster roster
        {
            get;
            set;
        }
    }


 

    public static class RankExtenstions
    {
        public static long Hash(this ERank rank)
        {
            long pow = (long)rank;
            long ret = 1;
            long x = 10;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        public static ERank Down(this ERank rank)
        {
            switch (rank)
            {
                case ERank.Root: return ERank.Side;
                case ERank.Side: return ERank.Army;
                case ERank.Army: return ERank.Corps;
                case ERank.Corps: return ERank.Division;
                case ERank.Division: return ERank.Brigade;
                case ERank.Brigade: return ERank.Regiment;
                case ERank.Regiment: return ERank.Battalion;
                default: return ERank.None;
            }
        }

        public static ERank Up(this ERank rank)
        {
            switch (rank)
            {
                case ERank.Side: return ERank.Side;
                case ERank.Army: return ERank.Side;
                case ERank.Corps: return ERank.Army;
                case ERank.Division: return ERank.Corps;
                case ERank.Brigade: return ERank.Division;
                case ERank.Regiment: return ERank.Brigade;
                case ERank.Battalion: return ERank.Regiment;
                default: return ERank.None;
            }
        }

    }

}