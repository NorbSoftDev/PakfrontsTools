using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Specialized;

namespace NorbSoftDev.SOW
{

    //TEMP
    // public class DataObject {

    //     object _data;

    //     public DataObject (object data)
    //     {
    //         this._data = data;
    //     }


    //     public object GetData (Type format)
    //     {
    //         return _data;
    //     }

    // }


    // public static class ObservableCollectionExtenstions {

    //     public static TCollection Get<TCollection,T> (this DataObject data) where TCollection :  class, ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged where T : class, INotifyPropertyChanged
    //     {
    //         return data.GetData(typeof(TCollection)) as TCollection;
    //     }

    // }

    public static class ObservableCollectionExtenstions {

        public static T GetData<T> (this DataObject data) where T :  class
        {
            return data.GetData(typeof(T)) as T;
        }

    }

    // public interface ISelectionSet<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    // where T : class,  INotifyPropertyChanged {
    //     // static SelectionSet<T> ExtractFromDataObject(DataObject data)
    // }


    //A class for containing selections of echelons, used in drag and drop
    internal class EchelonSelectionSet<TEchelon, TUnit> : ObservableSortedList<TEchelon> //, ISelectionSet<TEchelon>
        where TEchelon : EchelonGeneric<TUnit>
        where TUnit : IUnit
    {
        public IEchelonRoot root
        {
            get
            {
                if (Count < 1) return null;
                return this[0].root;
            }
        }

        internal static EchelonSelectionSet<TEchelon, TUnit> ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(EchelonSelectionSet<TEchelon, TUnit>)) as EchelonSelectionSet<TEchelon, TUnit>;
        }

        public void Add(TUnit unit) {
            this.Add((TEchelon) unit.echelon);
        }

        public Point? selectionWorldPoint;

        public SortedSet<TEchelon> WithChildren()
        {
            SortedSet<TEchelon> all = new SortedSet<TEchelon>(this);

            foreach (TEchelon ech in this)
            {
                AddChildren(ech, ref all);
            }

            return all;
        }

        private void AddChildren(TEchelon ech, ref SortedSet<TEchelon> all)
        {
            foreach (TEchelon child in ech.children)
            {
                all.Add(child);
                AddChildren(child, ref all);
            }
        }

        public IEnumerable<TUnit> units
        {
            get
            {
                List<TUnit> list = new List<TUnit>();
                foreach (TEchelon ech in this)
                {
                    list.Add(ech.unit);
                }
                return list;
            }
        }
    }

    ////A class for containing selections used in drag and drop
    public class EventSelectionSet : ObservableCollection<BattleScriptEvent>//, ISelectionSet<Event>
    {

        public EventSelectionSet() : base() { }

        public EventSelectionSet(ReadOnlyObservableCollection<object> collection) : base()
        {

            foreach (object o in collection)
            {
                BattleScriptEvent e = o as BattleScriptEvent;
                if (e != null) Add(e);
            }

        }

        internal static EventSelectionSet ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(EventSelectionSet)) as EventSelectionSet;
        }
    }

    public class ScenarioObjectiveSelectionSet : ObservableCollection<ScenarioObjective>
    {
        internal static ScenarioObjectiveSelectionSet ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(ScenarioObjectiveSelectionSet)) as ScenarioObjectiveSelectionSet;
        }
    }

    public class MapObjectiveSelectionSet : ObservableCollection<MapObjective>
    {
        internal static MapObjectiveSelectionSet ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(MapObjectiveSelectionSet)) as MapObjectiveSelectionSet;
        }
    }

    public class PositionSelectionSet : ObservableCollection<Position>
    {
        internal static PositionSelectionSet ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(PositionSelectionSet)) as PositionSelectionSet;
        }
    }
    public class ScreenMessageSelectionSet : ObservableCollection<ScreenMessage>
    {
        internal static ScreenMessageSelectionSet ExtractFromDataObject(DataObject data)
        {
            return data.GetData(typeof(ScreenMessageSelectionSet)) as ScreenMessageSelectionSet;
        }
    }
}
