using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NorbSoftDev.SOW;
using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;


namespace NorbSoftDev.SOW.Utils
{

    //public abstract class ScenarioEchelonRule : INotifyPropertyChanged
    //{

    //    public abstract void ApplyResults();

    //    // ScenarioEchelon _echelon;
    //    // public  ScenarioEchelon echelon
    //    // {
    //    //     get { return _echelon; }

    //    //     set
    //    //     {

    //    //         if (_echelon == value) return;
    //    //         _echelon = value;
    //    //         OnPropertyChanged("");
    //    //     }
    //    // }

    //    private bool _active = true;
    //    public bool active
    //    {
    //        get { return _active; }
    //        set
    //        {
    //            _active = value;
    //            OnPropertyChanged(""); //force everyting to update
    //        }
    //    }

    //    public ScenarioEchelonRule() : base() { }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //            handler(this, e);
    //    }

    //    protected void OnPropertyChanged(string propertyName)
    //    {
    //        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    //    }

    //    protected void all_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        OnPropertyChanged("");
    //    }
    //}

    //public class LocationEchelonRandomizer : ScenarioEchelonRule
    //{

    //    public LocationEchelonRandomizer() : base() { }

    //    //TODO a list of areas
    //    private Map _map;
    //    public Map map
    //    {
    //        get { return _map; }
    //        set
    //        {
    //            if (_map == value) return;
    //            _map = value;
    //            OnPropertyChanged("map");
    //        }
    //    }

    //    //TODO a list of areas
    //    private MapArea _mapArea;
    //    public MapArea mapArea
    //    {
    //        get { return _mapArea; }
    //        set
    //        {
    //            if (_mapArea == value) return;
    //            _mapArea = value;
    //            OnPropertyChanged("mapArea");
    //        }
    //    }

    //    private ERank _applyAt = ERank.Division;
    //    public ERank applyAt
    //    {
    //        get { return _applyAt; }
    //        set
    //        {
    //            if (_applyAt == value) return;
    //            _applyAt = value;
    //            OnPropertyChanged("applyAt");
    //        }
    //    }


    //    public override void ApplyResults()
    //    {
    //        if (echelon == null) return;
    //        Place(echelon);

    //    }

    //    protected virtual void Place(ScenarioEchelon current)
    //    {
    //        if (current.rank <= applyAt && echelon.unit != null)
    //        {
    //            echelon.unit.transform.SetPosition( mapArea.GetRandomPosition() );
    //            map.ApplyFormation(echelon);
    //        }
    //        else
    //        {
    //            foreach (ScenarioEchelon child in current.children)
    //            {
    //                Place(child);
    //            }
    //        }
    //    }
    //}

    //public class ScenarioRuleSet {
    //    public ObservableCollectionWithItemNotify<ScenarioEchelonRule> rules = new ObservableCollectionWithItemNotify<ScenarioEchelonRule>();
        
    //}
}