using System;
using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;


public class LimitedSizeStack<T> : LinkedList<T>, INotifyPropertyChanged, INotifyCollectionChanged
{
    private readonly int _maxSize;

    public int nPushes
    {
        get { return _nPushes; }
        set
        {
            _nPushes = value;
            OnPropertyChanged("nPushes");

        }
    }
    int _nPushes;


    public LimitedSizeStack(int maxSize)
    {
        _maxSize = maxSize;
    }

    public void Push(T item)
    {
        this.AddFirst(item);

        if (this.Count > _maxSize)
            this.RemoveLast();

        nPushes++;
        if (CollectionChanged != null)  CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public T Pop()
    {
        var item = this.First.Value;
        this.RemoveFirst();
        if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));        
        return item;

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

    public event NotifyCollectionChangedEventHandler CollectionChanged; 
    #endregion
}

namespace NorbSoftDev.SOW
{

    public interface IUndoStack {
        bool active {get; set;}
        void scenario_SaveBulkState(string properyName);
        void oob_SaveBulkState(string properyName);

    }


    public class ScenarioRosterState : MemoryStream {}
    public class OOBState : MemoryStream {}
    public class EventsState : MemoryStream {}

    public class ScenarioUndoStack : IUndoStack, INotifyPropertyChanged
    {

        Scenario _scenario;
        //string undoDirPath;
        public bool active {get; set;}

        ScenarioRosterState currentScenarioRosterState, previousScenarioRosterState;
        OOBState currentOOBState, previousOOBState;
        EventsState currentEventsState, previousEventsState;

        Scenario scenario {
            get { return _scenario; }
            set {
                if (_scenario != null)
                {
                    _scenario.ItemPropertyChanged -= scenario_OnUnitChanged;
                    _scenario.CollectionChanged -= scenario_OnRosterChanged;

                    _scenario.battleScript.events.ItemPropertyChanged -= scenario_OnEventChanged;
                    _scenario.battleScript.events.CollectionChanged -= scenario_OnEventListChanged;

                    if (_scenario.orderOfBattle != null) 
                    {
                        _scenario.orderOfBattle.ItemPropertyChanged -= oob_OnItemChanged;
                        _scenario.orderOfBattle.CollectionChanged -= oob_OnRosterChanged;                        
                    }


                }

                _scenario = value;
                _scenario.ItemPropertyChanged += scenario_OnUnitChanged;
                _scenario.CollectionChanged += scenario_OnRosterChanged;
                
                _scenario.battleScript.events.ItemPropertyChanged += scenario_OnEventChanged;
                _scenario.battleScript.events.CollectionChanged += scenario_OnEventListChanged;

                _scenario.orderOfBattle.ItemPropertyChanged += oob_OnItemChanged;
                _scenario.orderOfBattle.CollectionChanged += oob_OnRosterChanged; 
            }
        }


        //Stack<IDoAction> undoActions = new Stack<IDoAction>();
        //Stack<IDoAction> redoActions = new Stack<IDoAction>();


        public LimitedSizeStack<IDoAction> undoActions;
        public LimitedSizeStack<IDoAction> redoActions;

        public int UndoCount
        {
            get { return undoActions.Count; }
        }

        public LimitedSizeStack<IDoAction> UndoStack
        {
            get { return undoActions; }
        }

        public void PrintUndoStack()
        {
            foreach (var i in undoActions)
            {
                Console.WriteLine(i);
            }
        }

        /// <summary>
        /// active is false by default
        /// </summary>
        /// <param name="scenario"></param>
        public ScenarioUndoStack(Scenario scenario, int maxSize) {

            //undoDirPath = System.IO.Path.Combine(
            //   Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            //    "SOWWL", "Editor", "Undo", Process.GetCurrentProcess().Id.ToString()
            //    );
            //if (!Directory.Exists(undoDirPath))
            //{
            //    Directory.CreateDirectory(undoDirPath);
            //}
            //Console.WriteLine("Setting undoDirPath "+undoDirPath);

            this.scenario = scenario;
            this.active = false;

            undoActions = new LimitedSizeStack<IDoAction>(maxSize);

            redoActions = new LimitedSizeStack<IDoAction>(maxSize);
        }





        public void UpdateScenarioRosterState() {
            previousScenarioRosterState = currentScenarioRosterState;
            currentScenarioRosterState =  new ScenarioRosterState();

            StreamWriter outfile = new StreamWriter(currentScenarioRosterState, Config.TextFileEncoding);
            outfile.Write(scenario.ScenarioCsv());
            outfile.Flush();
            //Log.Info(this,"UpdateScenarioRosterState "+currentScenarioRosterState.GetHashCode()+" Bytes:"+currentScenarioRosterState.Length);

            currentScenarioRosterState.Position = 0;

        }


        public void UpdateEventsState() {
            previousEventsState = currentEventsState;
            currentEventsState =  new EventsState();

            StreamWriter outfile = new StreamWriter(currentEventsState, Config.TextFileEncoding);
            outfile.Write(scenario.battleScript.Csv());
            outfile.Flush();
            //Log.Info(this,"UpdateEventsState "+currentEventsState.GetHashCode()+" Bytes:"+currentEventsState.Length);

            currentEventsState.Position = 0;

        }
        
        public string Summary() {
            string summary = "UndoStack "+undoActions.Count+":";
            foreach (IDoAction i in undoActions) {
                summary += " "+i.name;
            }

            return summary;
        }


        public void UpdateOOBState()
        {
            previousOOBState = currentOOBState;
            currentOOBState = new OOBState();
            StreamWriter outfile = new StreamWriter(currentOOBState, Config.TextFileEncoding);
            outfile.Write(scenario.orderOfBattle.OrderOfBattleCsv());
            outfile.Flush();
            //Log.Info(this,"to " + currentOOBState.GetHashCode()+" Bytes:"+currentOOBState.Length);
            currentOOBState.Position = 0;
        }

        public void Initialize()
        {
            UpdateScenarioRosterState();
            UpdateOOBState();
            UpdateEventsState();
            undoActions.Push(new BulkChangeAction(this, this, "Initialize"));
        }

        private void scenario_OnRosterChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Console.WriteLine("scenario_OnRosterChanged A "+sender+" "+e.OldItems[0]);

            if (!active) return;

            if (e.OldItems == null)
            {
                //Log.Error(this, "Unable to put roster changed on undo stack:  olditem is null");
                //return;
                UpdateScenarioRosterState();

                undoActions.Push(new ScenarioBulkChangeAction(this, sender, "RosterChangedNull"));
            } else  if (e.OldItems.Count < 0)
            {
                //Log.Error(this, "Unable to put roster changed on undo stack: no olditems");
                //return;
                UpdateScenarioRosterState();
                undoActions.Push(new ScenarioBulkChangeAction(this, sender, "RosterChangedInit0"));
            } else if (e.OldItems[0] == null)
                {
                    //Log.Error(this, "Unable to put roster changed on undo stack: first olditem is null");
                    //return;
                    UpdateScenarioRosterState();
                    undoActions.Push(new ScenarioBulkChangeAction(this, sender, "RosterChangedInit0IsNull"));
                }
                else
                {

                    UpdateScenarioRosterState();
                undoActions.Push(new ScenarioRosterChangedAction(this, ((ScenarioUnit)e.OldItems[0]).scenarioEchelon, "RosterChanged"));
                }
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");
            //undoActions.Push(new ScenarioBulkChangeAction(this, sender) );
            redoActions.Clear();
            //Console.WriteLine(Summary());

        }

        private void scenario_OnEventListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //Console.WriteLine("scenario_OnEventsChanged A "+sender+" "+e);


            if (!active) return;
            BattleScriptEvent bEvent =null;
            if (e.OldItems != null && e.OldItems.Count > 0) bEvent = (BattleScriptEvent)e.OldItems[0];
            UpdateEventsState();
            undoActions.Push(new EventListChangedAction(this, bEvent));
            OnPropertyChanged("UndoCount");
            OnPropertyChanged("UndoStack");

            // //undoActions.Push(new ScenarioBulkChangeAction(this, sender) );
            // redoActions.Clear();
            // Console.WriteLine(Summary());

        }


        private void scenario_OnUnitChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!active) return;
            // Console.WriteLine("scenario_OnUnitChanged A "+sender+e.PropertyName);

            UpdateScenarioRosterState();
            undoActions.Push( new ScenarioUnitChangedAction(this, (ScenarioUnit) sender, e.PropertyName) );
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

            redoActions.Clear(); 
        }

        private void scenario_OnEventChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!active) return;
            // Console.WriteLine("scenario_OnUnitChanged A "+sender+e.PropertyName);
            UpdateEventsState();
            undoActions.Push(new EventChangedAction(this, (BattleScriptEvent)sender, e.PropertyName));
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

            redoActions.Clear();
        }

        public void scenario_SaveBulkState(string propertyName) {
            UpdateScenarioRosterState();
            UpdateEventsState();
            undoActions.Push( new ScenarioBulkChangeAction(this, this, propertyName) );
        }

        private void oob_OnRosterChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Console.WriteLine("oob_OnRosterChanged A");

            //if (!active) return;
            //UpdateOOBState();
            //// undoActions.Push(new OOBBulkChangeAction(this, sender));
            //undoActions.Push(new OOBRosterChangedAction(this, ((OOBUnit) e.OldItems[0]).oobEchelon));
            //OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

            //redoActions.Clear();


            if (!active) return;

            if (e.OldItems == null)
            {
                //Log.Error(this, "Unable to put roster changed on undo stack:  olditem is null");
                //return;
                UpdateOOBState();

                undoActions.Push(new OOBBulkChangeAction(this, sender, "OOBChangedNull"));
            }
            else if (e.OldItems.Count < 0)
            {
                //Log.Error(this, "Unable to put roster changed on undo stack: no olditems");
                //return;
                UpdateOOBState();
                undoActions.Push(new OOBBulkChangeAction(this, sender, "OOBChangedInit0"));
            }
            else if (e.OldItems[0] == null)
            {
                //Log.Error(this, "Unable to put roster changed on undo stack: first olditem is null");
                //return;
                UpdateOOBState();
                undoActions.Push(new OOBBulkChangeAction(this, sender, "OOBChangedInit0IsNull"));
            }
            else
            {

                UpdateOOBState();
                undoActions.Push(new OOBUnitChangedAction(this, ((OOBUnit)e.OldItems[0]), "OOBChanged"));
            }
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");
            //undoActions.Push(new ScenarioBulkChangeAction(this, sender) );
            redoActions.Clear();
        }
        private void oob_OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!active) return;

            UpdateOOBState();
            undoActions.Push( new OOBUnitChangedAction(this, (OOBUnit) sender, e.PropertyName) );
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

            redoActions.Clear(); 
        }

        public void oob_SaveBulkState(string propertyName) {
            UpdateOOBState();
            undoActions.Push( new OOBBulkChangeAction(this, this, propertyName) );
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

        }

        public void Undo()
        {

            if (!canUndo) {
                Log.Error(this,"Cannot undo." );
                Log.Error(this,Summary() );
            }

            active = false;

            IDoAction ido = undoActions.Pop();
            Log.Info(this, "Undo " + ido.name);
            redoActions.Push(ido);
            ido.Undo();
            active = true;
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

        }

        public void Redo()
        {
            active = false;
            IDoAction ido = redoActions.Pop();
            Log.Info(this,"Redo "+ido.name);
            ido.Redo();
            undoActions.Push( ido);
            active = true;
        }

        public void Clear() {
            redoActions.Clear(); 
            undoActions.Clear(); 
            UpdateScenarioRosterState();
            UpdateOOBState();
            OnPropertyChanged("UndoCount"); OnPropertyChanged("UndoStack");

        }

        public bool canUndo {
            get { return (undoActions.Count > 1); }
        }

        public bool canRedo {
            get { return (redoActions.Count > 0); }
        }

        public interface IDoAction {
            void Undo();
            void Redo();
            string name {get;}
        }

        public class BulkChangeAction : IDoAction, IDisposable
        {
            ScenarioUndoStack undoStack;
            ScenarioRosterState scenario_previousState, scenario_currentState;
            OOBState oob_previousState, oob_currentState;
            public string name { get; set; }
            //public string filepath { get; set;}

            public BulkChangeAction(ScenarioUndoStack undoStack, object sender, string propertyName)
            {
                this.undoStack = undoStack;
                this.scenario_previousState = undoStack.previousScenarioRosterState;
                this.scenario_currentState = undoStack.currentScenarioRosterState;
                this.oob_previousState = undoStack.previousOOBState;
                this.oob_currentState = undoStack.currentOOBState;
                this.name = this + "INIT:" + sender + ":" + propertyName + ":" + GetHashCode();
            }

            public void Undo()
            {
                Log.Info(this,"Undo Restoring " + oob_previousState.GetHashCode() + " & " + scenario_previousState.GetHashCode());
                oob_previousState.Position = 0;
                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv(oob_previousState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.scenario.orderOfBattle.ReorgFromCsv(oob_previousState, undoStack.scenario.orderOfBattle.oobHeaders);

                scenario_previousState.Position = 0;
                undoStack.scenario.ReadUnitDataFromCsv(scenario_previousState, undoStack.scenario.scenarioHeaders);
                undoStack.scenario.ReorgFromCsv(scenario_previousState, undoStack.scenario.scenarioHeaders);
            }

            public void Redo()
            {
                Log.Info(this,"Redo Restoring " + oob_currentState.GetHashCode() + " & " + scenario_currentState.GetHashCode());
                oob_currentState.Position = 0;
                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv(oob_currentState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.scenario.orderOfBattle.ReorgFromCsv(oob_currentState, undoStack.scenario.orderOfBattle.oobHeaders);

                scenario_currentState.Position = 0;
                undoStack.scenario.ReadUnitDataFromCsv(scenario_currentState, undoStack.scenario.scenarioHeaders);
                undoStack.scenario.ReorgFromCsv(scenario_currentState, undoStack.scenario.scenarioHeaders);
            }

            public void Dispose()
            {
                // Dispose of unmanaged resources.
                Dispose(true);
                // Suppress finalization.
                GC.SuppressFinalize(this);
            }

            bool disposed = false;
            // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    try
                    {
                        oob_previousState.Close();
                        scenario_previousState.Close();

                    }
                    finally
                    {
                        disposed = true;
                    }
                }

                // Free any unmanaged objects here. 
                //
                disposed = true;
            }

            public string ToString()
            {
                return name;
            }
        }

        public class ScenarioUnitChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            ScenarioRosterState previousState, currentState;
            List<ScenarioUnit> units;
            public string name { get; set;}
            //public string filepath { get; set;}

            public ScenarioUnitChangedAction(ScenarioUndoStack undoStack, ScenarioUnit unit, string propertyName ) {
                this.undoStack = undoStack;
                this.units = new List<ScenarioUnit>();
                this.units.Add(unit);
                this.previousState = undoStack.previousScenarioRosterState;
                this.currentState = undoStack.currentScenarioRosterState;
                if (unit == null)
                {
                    this.name = this + ":null." + propertyName + ":" + currentState.GetHashCode();
                }
                else {
                    this.name = this + ":" + unit.id + "." + propertyName + ":" + currentState.GetHashCode();
                }
            }

            public void Undo() {                    
                previousState.Position = 0;
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());

                undoStack.scenario.ReadUnitDataFromCsv( previousState, undoStack.scenario.scenarioHeaders, units);
                undoStack.currentScenarioRosterState = this.previousState;
            }

            public void Redo() {                    
                currentState.Position = 0;
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                undoStack.scenario.ReadUnitDataFromCsv(currentState, undoStack.scenario.scenarioHeaders, units);
                undoStack.currentScenarioRosterState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            disposed = true;
           }

            public string ToString()
            {
                return name;
            }
        }


        public class ScenarioRosterChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            ScenarioRosterState previousState, currentState;
            List<ScenarioEchelon> echelons;
            public string name { get; set;}
            //public string filepath { get; set;}

            public ScenarioRosterChangedAction(ScenarioUndoStack undoStack, ScenarioEchelon echelon, string propertyName) {
                this.undoStack = undoStack;
                this.echelons = new List<ScenarioEchelon>();
                this.echelons.Add(echelon);
                this.previousState = undoStack.previousScenarioRosterState;
                this.currentState = undoStack.currentScenarioRosterState;
                this.name = this+":"+echelon.id+"."+propertyName+":"+currentState.GetHashCode();
            }

            public void Undo() {
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode()+" "+echelons[0].unit.id);
    
                previousState.Position = 0;
                undoStack.scenario.ReorgFromCsv( previousState, undoStack.scenario.scenarioHeaders);
                undoStack.currentScenarioRosterState = this.previousState;
            }

            public void Redo() {
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                currentState.Position = 0;
                undoStack.scenario.ReorgFromCsv( currentState, undoStack.scenario.scenarioHeaders);
                undoStack.currentScenarioRosterState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
           }

            public override string ToString()
            {
                return name;
            }
        }

        
        public class EventChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            EventsState previousState, currentState;
            List<BattleScriptEvent> events;
            public string name { get; set; }
            //public string filepath { get; set;}

            public EventChangedAction(ScenarioUndoStack undoStack, BattleScriptEvent bEvent, string propertyName)
            {
                this.undoStack = undoStack;
                this.events = new List<BattleScriptEvent>();
                this.events.Add(bEvent);
                this.previousState = undoStack.previousEventsState;
                this.currentState = undoStack.currentEventsState;
                if (bEvent == null)
                {
                    this.name = this + ":null:" + propertyName + ":" + currentState.GetHashCode();
                }
                else {
                    this.name = this + ":" + bEvent.ToString() + ":" + propertyName + ":" + currentState.GetHashCode();
                }
            }

            public void Undo() {                    
                previousState.Position = 0;
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());
               //TODO read only changed event
                undoStack.scenario.ReadBattleScript(previousState);
                //undoStack.scenario.ReadUnitDataFromCsv( previousState, undoStack.scenario.scenarioHeaders, units);
                undoStack.currentEventsState = this.previousState;
            }

            public void Redo() {                    
                currentState.Position = 0;
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                //TODO read only changed event
                undoStack.scenario.ReadBattleScript(currentState);
                //undoStack.scenario.ReadUnitDataFromCsv(currentState, undoStack.scenario.scenarioHeaders, units);
                undoStack.currentEventsState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            disposed = true;
           }

            public override string ToString()
            {
                return name;
            }
        }


        public class EventListChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            EventsState previousState, currentState;
            List<BattleScriptEvent> events;
            public string name { get; set;}

            public EventListChangedAction(ScenarioUndoStack undoStack, BattleScriptEvent bEvent) {
                this.undoStack = undoStack;
                this.events = new List<BattleScriptEvent>();
                this.events.Add(bEvent);
                this.previousState = undoStack.previousEventsState;
                this.currentState = undoStack.currentEventsState;
                this.name = this+":"+bEvent+":"+currentState.GetHashCode();
            }

            public void Undo() {
                
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());
    
                previousState.Position = 0;
                undoStack.scenario.ReadBattleScript( previousState);
                undoStack.currentEventsState = this.previousState;
            }

            public void Redo() {
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                currentState.Position = 0;
                undoStack.scenario.ReadBattleScript( currentState);
                undoStack.currentEventsState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
           }

            public string ToString()
            {
                return name;
            }
        }

        public class ScenarioBulkChangeAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            ScenarioRosterState previousState, currentState;
            public string name { get; set;}
            //public string filepath { get; set;}

            public ScenarioBulkChangeAction(ScenarioUndoStack undoStack, object sender, string propertyName) {
                this.undoStack = undoStack;
                this.previousState = undoStack.previousScenarioRosterState;
                this.currentState = undoStack.currentScenarioRosterState;
                this.name = this+":"+sender+"."+propertyName+":"+GetHashCode();
            }

            public void Undo() {
                if (previousState == null) {
                    Log.Warn(this,"Undo Reached Beginning of Stack");
                    return;
                }

                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());
  
                previousState.Position = 0;
                undoStack.scenario.ReorgFromCsv(previousState, undoStack.scenario.scenarioHeaders);
                undoStack.scenario.ReadUnitDataFromCsv( previousState, undoStack.scenario.scenarioHeaders);
                undoStack.currentScenarioRosterState = this.previousState;
            }

            public void Redo() {
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                currentState.Position = 0;  
                undoStack.scenario.ReadUnitDataFromCsv( currentState, undoStack.scenario.scenarioHeaders);
                undoStack.scenario.ReorgFromCsv( currentState, undoStack.scenario.scenarioHeaders);
                undoStack.currentScenarioRosterState = this.currentState;
            }

            public void Dispose()
            {
               // Dispose of unmanaged resources.
               Dispose(true);
               // Suppress finalization.
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
           protected virtual void Dispose(bool disposing)
           {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
           }

           public override string ToString()
           {
               return name;
           }
        }


        public class OOBUnitChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            OOBState previousState, currentState;
            List<OOBUnit> units;
            public string name { get; set;}
            //public string filepath { get; set;}

            public OOBUnitChangedAction(ScenarioUndoStack undoStack, OOBUnit unit, string propertyName)
            {
                this.undoStack = undoStack;
                this.units = new List<OOBUnit>();
                this.units.Add(unit);
                this.previousState = undoStack.previousOOBState;
                this.currentState = undoStack.currentOOBState;
                if (unit == null)
                {
                    this.name = this + ":null:" + propertyName + ":" + currentState.GetHashCode();

                }
                else
                {
                    this.name = this + ":" + unit.id + ":" + propertyName + ":" + currentState.GetHashCode();
                }
            }

            public void Undo() {                    
                previousState.Position = 0;
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());

                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv( previousState, undoStack.scenario.orderOfBattle.oobHeaders, units);
                undoStack.currentOOBState = this.previousState;
            }

            public void Redo() {                    
                currentState.Position = 0;
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv(currentState, undoStack.scenario.orderOfBattle.oobHeaders, units);
                undoStack.currentOOBState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            disposed = true;
           }

            public string ToString()
            {
                return name;
            }
        }

        public class OOBRosterChangedAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            OOBState previousState, currentState;
            List<OOBEchelon> echelons;
            public string name { get; set;}
            //public string filepath { get; set;}

            public OOBRosterChangedAction(ScenarioUndoStack undoStack, OOBEchelon echelon ) {
                this.undoStack = undoStack;
                this.echelons = new List<OOBEchelon>();
                this.echelons.Add(echelon);
                this.previousState = undoStack.previousOOBState;
                this.currentState = undoStack.currentOOBState;
                this.name = this+":"+echelon.id+":"+currentState.GetHashCode();
            }

            public void Undo() {
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());

                // previousState.Position = 0;  
                // var sr = new StreamReader(previousState);
                // Console.WriteLine(sr.ReadToEnd());
    
                previousState.Position = 0;  
                undoStack.scenario.orderOfBattle.ReorgFromCsv( previousState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.currentOOBState = this.previousState;
            }

            public void Redo() {
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                currentState.Position = 0;
                // Console.WriteLine("Will apply to "+echelons[0].id);  
                undoStack.scenario.orderOfBattle.ReorgFromCsv( currentState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.currentOOBState = this.currentState;
            }

            public void Dispose()
            {
               Dispose(true);
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
            protected virtual void Dispose(bool disposing)
            {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
           }

            public string ToString()
            {
                return name;
            }
        }

        public class OOBBulkChangeAction : IDoAction, IDisposable {
            ScenarioUndoStack undoStack;
            OOBState previousState, currentState;
            public string name { get; set;}
            //public string filepath { get; set;}

            public OOBBulkChangeAction(ScenarioUndoStack undoStack, object sender, string properyName) {
                this.undoStack = undoStack;
                this.previousState = undoStack.previousOOBState;
                this.currentState = undoStack.currentOOBState;
                this.name = this+":"+sender+"."+properyName+":"+GetHashCode();
            }

            public void Undo() {
                Log.Info(this,"Undo Restoring " + previousState.GetHashCode());
  
                previousState.Position = 0;  
                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv( previousState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.scenario.orderOfBattle.ReorgFromCsv( previousState, undoStack.scenario.orderOfBattle.oobHeaders);
            }

            public void Redo() {
                Log.Info(this,"Redo Restoring " + currentState.GetHashCode());
                currentState.Position = 0;  
                undoStack.scenario.orderOfBattle.ReadUnitDataFromCsv( currentState, undoStack.scenario.orderOfBattle.oobHeaders);
                undoStack.scenario.orderOfBattle.ReorgFromCsv( currentState, undoStack.scenario.orderOfBattle.oobHeaders);
            }

            public void Dispose()
            {
               // Dispose of unmanaged resources.
               Dispose(true);
               // Suppress finalization.
               GC.SuppressFinalize(this);
            }   

            bool disposed = false;
           // Protected implementation of Dispose pattern. 
           protected virtual void Dispose(bool disposing)
           {
            if (disposed)
                return; 

            if (disposing) {
                try {
                    previousState.Close();

                } finally {
                    disposed = true;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
           }

           public string ToString()
           {
               return name;
           }
        }


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




    }

    
}
