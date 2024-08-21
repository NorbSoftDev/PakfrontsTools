using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;

namespace NorbSoftDev.SOW
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/ff407126%28v=vs.110%29.aspx
    /// </summary>
    public class EventCollection : ObservableCollectionWithItemNotify<BattleScriptEvent>
    {
        //
        // Creating the EventCollection collection in this way enables data binding from XAML.

        public class Block
        {

            public string key;
            public int originalIndex;
            public List<BattleScriptEvent> list = new List<BattleScriptEvent>();

            internal int CompareTo(Block block)
            {
                    return key.CompareTo(block.key);
            } 
        }

         //class BlockComparer : IComparer<Block>
         //{
         //    public int Compare(Block a, Block b)
         //    {

         //        TimeEvent at = a as TimeEvent;
         //        TimeEvent bt = b as TimeEvent;

         //        //this will break evtcont!!!!!
         //        if (at == null && bt == null) return string.Compare(a.ToString(), b.ToString());

         //        if (at == null ) return -1;
         //        if (bt == null ) return 1;

         //        return  0; //TimeSpan.Compare(at.time, bt.time);
         //    }
         //}

        public void Sort()
        {

            List<BattleScriptEvent> sorted = this.Sorted();
            this.Clear();
            foreach (BattleScriptEvent be in sorted)
            {
                this.Add(be);
            }
            
        }


        public List<BattleScriptEvent> Sorted()
        {
            Dictionary<string, Block> timeEventDict = new Dictionary<string, Block>(StringComparer.OrdinalIgnoreCase);
            List<BattleScriptEvent> otherEvents = new List<BattleScriptEvent>();
            Log.Info(this, "Sorted BattleEvents");

            int i = 0;
            foreach (BattleScriptEvent be in this)
            {
                TimeEvent te = be as TimeEvent;

                if (te == null)
                {
                    otherEvents.Add(be);
                    continue;
                }

                Block block;

                if ( ! timeEventDict.TryGetValue(be.block, out block) ) {
                    block = new Block();
                    block.key = be.block;
                    timeEventDict[block.key] = block;
                    block.originalIndex = i;
                    i++;
                }

                block.list.Add(be);

            }

            List<KeyValuePair<string, Block>> timeEventList = timeEventDict.ToList();

            timeEventList.Sort((firstPair,nextPair) =>
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            }
            );

            List<BattleScriptEvent> sorted = new List<BattleScriptEvent>();

            foreach (KeyValuePair<string, Block> kvp in timeEventList)
            {
                sorted.AddRange(kvp.Value.list);
            }

            sorted.AddRange(otherEvents);

            foreach (BattleScriptEvent be in sorted)
            {
                Debug.WriteLine(be.block + " " + be);
            }

            return sorted;
        }
    }

    public class BattleScript
    {
        //public interface IHasUnit { ScenarioUnit unit { get; set; } }
        public Scenario scenario;

        //static string[] defaultHeaders = new string[] {
        //    "trigger","id","command","x","y","timevar","description"
        //    };
        public string[] headers;
        public Dictionary<string, int> csvHeaderLUT = new Dictionary<string, int>();


        public EventCollection events { get; set; }

        public Dictionary<string, NamedEvent> namedEvents = new Dictionary<string, NamedEvent>(StringComparer.OrdinalIgnoreCase);

        // public List<Event> orderedEvents
        // {
        //     get { return events.OrderBy(x => x, new EventComparer() ).ToList(); }
        //     // get { return events.OrderBy(x => x.time.TimeOfDay).ToList(); }
        // }


        public BattleScript(Scenario scenario)
        {
            events = new EventCollection();
            this.scenario = scenario;
            if (this.scenario == null) throw new Exception("no scenario on init");

            if (headers == null)
            {
                headers = scenario.config.headers.battlescript;
                //string filepath = Path.Combine("Scenario", "battlescript.csv");
                //try
                //{

                //    StreamReader reader = scenario.config.GetResourceStreamReader(filepath);
                //    CsvReader csv = new CsvReader(reader, true);
                //    Log.Info(this, "Using Resource headers \"" + filepath + "\"");
                //    headers = csv.GetFieldHeaders();

                //}
                //catch (System.IO.FileNotFoundException e)
                //{
                //    Log.Warn(this, "Using default headers, did not find Resource \"" + filepath + "\"");
                //    headers = defaultHeaders;
                //}
            }

            int i = 0;
            foreach (string header in headers)
            {
                csvHeaderLUT[header] = i;
                i++;
            }

        }

        public NamedEvent GetOrAddNamedEvent(string name)
        {
            NamedEvent namedEvent;
            if (!namedEvents.TryGetValue(name, out namedEvent))
            {
                namedEvent = new NamedEvent(name);
                namedEvents.Add(namedEvent.tag, namedEvent);
            }
            return namedEvent;
        }

        public void Clear()
        {
            events.Clear();
        }

        public List<BattleScriptEvent> Sorted()
        {
            return events.Sorted();
        }

        public void CreateDefault()
        {

            if (scenario == null) throw new Exception("no scenario");

            TimeSpan time = scenario.startTime;
            // while (time < scenario.endTime)
            // {
            //     events.Add(new TimeEvent(time, null, "dumpgamedb", null, null, null));
            //     events.Add(new TimeEvent(time.AddSeconds(15), null, "dumplocs", null, null, null));

            //     time = time.AddMinutes(30);
            // }

            // events.Add(new TimeEvent(scenario.endTime.AddSeconds(-30), null, "dumpgamedb", null, null, null));
            // events.Add(new TimeEvent(scenario.endTime.AddSeconds(-15), null, "dumplocs", null, null, null));


            events.Add(new TimeEvent(0, "End of Game", scenario.endTime, scenario.config.commandTemplates["endscenario"].Create(), null));

            // if (scenario.startTime < scenario.sunrise)
            //     events.Add(new TimeEvent(scenario.sunrise, null, "sunrise", null, null, null));
            // events.Add(new TimeEvent(scenario.sunset, null, "sunset", null, null, null));

        }

        //public Event AddNew(int timeVar, string description, string trigger, ScenarioUnit unit, string commandName, ScenarioUnit fromUnit, float south, float east)
        //{
        //    CommandTemplate commandTemplate;
        //    Command command = null;
        //    if (commandName != String.Empty)
        //    {

        //        try
        //        {
        //            commandTemplate = scenario.config.commandTemplates[commandName];

        //        }
        //        catch
        //        {
        //            Log.Error(this, "ReadBattleScript No known command '" + commandName);
        //            return null;
        //        }

        //        command = commandTemplate.Create(unit, new object[] { }, fromUnit, south, east, scenario);

        //        //Console.WriteLine("Command: " + trigger + " " + command + " " + commandName + " " + thing);

        //    }

        //    return AddNew(timeVar, description, trigger, unit, command, fromUnit, south, east);

        //}

        //public Event AddNew(int timeVar, string description, string trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
        //{
        //    TimeSpan timeSpan;
        //    Event bEvent;

        //    if (trigger.StartsWith("evtran"))
        //    {
        //        string randomEventName = trigger.Substring(6, trigger.Length - 7);
        //        if (this.randomEventCommands.ContainsKey(randomEventName))
        //        {
        //            bEvent = new RandomEvent(

        //                timeVar, description,
        //                this.randomEventCommands[randomEventName],
        //                command, unit
        //                );
        //        }
        //        else
        //        {
        //            Log.Error(this, "ReadBattleScript Skipping Unknown RandomEvent "+trigger+"'" + randomEventName + "'");
        //            return null;
        //        }
        //    } else if (Sky.TryParseTimeSpan(trigger, out timeSpan))
        //    {
        //        bEvent = new TimeEvent(timeVar, description, timeSpan, command, unit);
        //    }
        //    else
        //    {

        //        EventTemplate eventTemplate;

        //        if (scenario.config.eventTemplates.ContainsKey(trigger))
        //        {
        //            eventTemplate = scenario.config.eventTemplates[trigger];
        //            bEvent = eventTemplate.Create(timeVar, description, trigger, unit, command, fromUnit, south, east);
        //            //Console.WriteLine("Event: " + trigger + " " + eventTemplate+" "+bEvent);

        //        }


        //        else
        //        {
        //            Log.Error(this, "ReadBattleScript Skipping Unknown Event Type '" + trigger);
        //            return null;
        //            //Log.Warn(this,"ReadBattleScript Assuming UnitEvent '" + trigger );
        //            //bEvent = new UnitEvent(trigger, unit, command, timeVar);
        //        }

        //    }
        //    events.Add(bEvent);
        //    return bEvent;
        //}

        public string Csv()
        {


            string lines = "";

            string header = "Event,ID,Command,FromId,X Coord,Z Coord,Time Variation (secs),Notes";
            int expectcount = 0;
            foreach (char c in header)
                if (c == ',') expectcount++;

            string commentline = "";
            for (int i = 0; i < expectcount; i++)
            {
                commentline += ",";
            }
            commentline += Config.NewLine;

            lines += header;
            lines += Config.NewLine;

            string block = null;
            int block_count = 0;
            foreach (BattleScriptEvent aevent in events.Sorted())
            {
                //if (aevent is ContinueEvent)
                //{
                //    lastContinueEvent = aevent as ContinueEvent;
                //}
                //else if (lastContinueEvent != null)
                //{
                //    lines += ",,,,,,," + Config.NewLine;
                //    lastContinueEvent = null;
                //}

                if (block == null)
                {
                    block = aevent.block;
                }
                else
                {
                    if (block != aevent.block)
                    {
                        if (block_count > 1)
                        {
                            lines += commentline;
                        }
                        block = aevent.block;
                        block_count = 0;

                    }
                    else
                    {
                        block_count++;
                    }
                }

                string line = aevent.ToCsv();

                //Check count in case I messed something up 
                int commacount = 0;
                foreach (char c in line)
                    if (c == ',') commacount++;

                if (commacount != 0 && commacount != expectcount)
                {
                    Log.Error(this, "Incorrect comma count (got " + commacount + " expected " + expectcount + " in \"" + line + "\"");
                }

                lines += aevent.ToCsv();
                lines += Config.NewLine;
            }

            return lines;
        }

        public List<UnitMoveToCommand> GetTimedMoveCommands()
        {

            List<UnitMoveToCommand> list = new List<UnitMoveToCommand>();

            foreach (BattleScriptEvent bEvent in events)
            {
                TimeEvent timeUnitEvent = bEvent as TimeEvent;
                if (timeUnitEvent == null) continue;
                UnitMoveToCommand command = timeUnitEvent.command as UnitMoveToCommand;
                if (command == null) continue;
                list.Add(command);

            }

            return list;
        }

        public List<UnitMoveToCommand> GetTimedMoveCommandsFor(ScenarioUnit unit)
        {

            List<UnitMoveToCommand> list = new List<UnitMoveToCommand>();

            foreach (BattleScriptEvent bEvent in events)
            {
                //FIXME not save to assume time ordered
                TimeEvent timeUnitEvent = bEvent as TimeEvent;
                if (timeUnitEvent == null) continue;
                UnitMoveToCommand command = timeUnitEvent.command as UnitMoveToCommand;
                if (command == null) continue;
                if (command.unit != unit) continue;
                list.Add(command);

            }

            return list;
        }
    }

        // class EventComparer : IComparer<Event>
        // {
        //     public int Compare(Event a, Event b)
        //     {

        //         TimeEvent at = a as TimeEvent;
        //         TimeEvent bt = b as TimeEvent;

        //         //this will break evtcont!!!!!
        //         if (at == null && bt == null) return string.Compare(a.ToString(), b.ToString());

        //         if (at == null ) return -1;
        //         if (bt == null ) return 1;

        //         return  0; //TimeSpan.Compare(at.time, bt.time);
        //     }
        // }

        public class EventTemplate
        {
            public Type type { get; set; }
            public string name { get; set; }
            public string help { get; set; }
            public EventTemplate(Type type, string name, string help)
            {
                this.type = type;
                this.name = name;
                this.help = help;
            }

            public BattleScriptEvent Create(int timeVar, string description, object trigger, object thing, Command command, ScenarioUnit fromUnit, float south, float east)
            {
                Console.WriteLine("EventTemplate.Create: " + timeVar + ", " + description + ", " + trigger + ", " + thing + ", " + command + ", " + south + ", " + east);

                return (BattleScriptEvent)Activator.CreateInstance(type, timeVar, description, trigger, thing, command, fromUnit, south, east);
            }


            public BattleScriptEvent Create()
            {
                return (BattleScriptEvent)Activator.CreateInstance(type, name);
            }

            public BattleScriptEvent Create(BattleScriptEvent parent)
            {
                BattleScriptEvent nevent = (BattleScriptEvent)Activator.CreateInstance(type, name);
                ContinueEvent evtcont = nevent as ContinueEvent;
                if (evtcont != null)
                    evtcont.parent = parent;
                return nevent;
            }

            public override string ToString()
            {
                return name + "Template";
            }
        }


        public abstract class BattleScriptEvent : INotifyPropertyChanged
        {
            public int timeVar
            {
                get { return _timeVar; }
                set { _timeVar = value; OnPropertyChanged("timeVar"); }
            }
            int _timeVar;

            public string description
            {
                get { return _description; }
                set { _description = value; OnPropertyChanged("description"); }
            }
            string _description;

            abstract public string block { get; }
            //abstract public string block
            //{
            //    get { return _block; }
            //    set { _block = value; OnPropertyChanged("block"); }
            //}
            //protected string _block;

            abstract public string ToCsv();

            public BattleScriptEvent()
            {

            }

            public BattleScriptEvent(int timevar, string description)
            {
                this._timeVar = timevar;
                this._description = description;
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


            protected void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            public virtual BattleScriptEvent Clone()
            {
                return (BattleScriptEvent)this.MemberwiseClone();
            }
        }

        public abstract class EventBase<T> : BattleScriptEvent
        {

            public virtual T trigger
            {
                get { return _trigger; }
                set
                {
                    _trigger = value;
                    OnPropertyChanged("trigger");
                    OnPropertyChanged("block");
                }
            }
            protected T _trigger;

            public EventBase(int timeVar, string description, T trigger)
                : base(timeVar, description)
            {
                this._trigger = trigger;
            }

            public override string ToString()
            {
                return trigger.ToString();
            }
        }

        public class TemporaryCommandHolder : IHasCommand
        {
            public Command command
            {
                get { return _command; }
                set
                {
                    //if (_command != null)
                    //    _command.PropertyChanged -= command_PropertyChanged;

                    _command = value;

                    //if (_command != null)
                    //    _command.PropertyChanged += command_PropertyChanged;
                    //OnPropertyChanged("command");
                }
            }
            Command _command;
        }

        public abstract class EventWithCommand<T> : EventBase<T>, IHasCommand
        {
            public Command command
            {
                get { return _command; }
                set
                {
                    if (_command != null)
                        _command.PropertyChanged -= command_PropertyChanged;

                    _command = value;

                    if (_command != null)
                        _command.PropertyChanged += command_PropertyChanged;
                    OnPropertyChanged("command");
                }
            }
            Command _command;

            protected virtual void command_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e);
            }


            public EventWithCommand(int timeVar, string description, T trigger, Command command)
                : base(timeVar, description, trigger)
            {
                this.command = command;
            }

            public override BattleScriptEvent Clone()
            {
                EventWithCommand<T> newEvent = (EventWithCommand<T>)base.Clone();
                newEvent._command = newEvent._command.Clone();
                return newEvent;
            }

        }

        public interface IEventWithCommandAndUnit : IHasCommand, IHasUnit
        {
            string ToCsv();
        }


        public abstract class EventWithCommandAndUnit<T> : EventWithCommand<T>, IEventWithCommandAndUnit
        {
            public ScenarioUnit unit
            {
                get
                {
                    IHasUnit iunitcommand = command as IHasUnit;
                    if (iunitcommand == null) return _backingUnit;
                    return iunitcommand.unit;
                }

                set
                {
                    _backingUnit = value;
                    IHasUnit iunitcommand = command as IHasUnit;
                    if (iunitcommand == null) return;
                    iunitcommand.unit = value;
                    OnPropertyChanged("unit");
                }
            }

            ScenarioUnit _backingUnit;

            protected override void command_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                IHasUnit iunitcommand = command as IHasUnit;
                if (iunitcommand != null) _backingUnit = iunitcommand.unit;

                base.command_PropertyChanged(sender, e);                
            }

            public EventWithCommandAndUnit(int timeVar, string description, T trigger, Command command, ScenarioUnit unit)
                : base(timeVar, description, trigger, command)
            {
                this.unit = unit;
            }
        }

        // evtarrived
        public class UnitPositionEvent : EventBase<string>, IHasUnit, IHasPosition
        {

            public override string block
            {
                get { return trigger + " " + this.GetHashCode().ToString(); }
            }

            public ScenarioUnit unit
            {
                get { return _unit; }
                set { _unit = value; OnPropertyChanged("unit"); }
            }
            ScenarioUnit _unit;

            public Position position
            {
                get { return _position; }
                set
                {
                    if (_position != null) _position.PropertyChanged -= position_PropertyChanged;
                    _position = value;
                    if (_position != null) _position.PropertyChanged += position_PropertyChanged;
                    OnPropertyChanged("position");
                }
            }
            Position _position;

            private void position_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                OnPropertyChanged("position");
            }


            public UnitPositionEvent(int timeVar, string description, string name, ScenarioUnit unit, Position position, Command command) :
                base(timeVar, description, name)
            {
                this.position = position;
                this._unit = unit;
            }

            public UnitPositionEvent(int timeVar, string description, object trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
                : this(timeVar, description, (string)trigger, unit, new Position(south, east), command)
            {
                position = new Position(south,east);
            }

            public UnitPositionEvent(string name)
                : this(0, String.Empty, name, null, null, null, 0, 0)
            {
                position = new Position();
            }

            public override string ToCsv()
            {
                // "Event,ID,Command,FromId,X Coord,Z Coord,Time Variation (secs),Notes";

                return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    trigger,
                    (unit == null) ? String.Empty : unit.id,
                    String.Empty, //no command
                    String.Empty, //no fromUnit
                    position == null ? String.Empty : ((int)position.south).ToString(),
                    position == null ? String.Empty : ((int)position.east).ToString(),
                    (timeVar == 0) ? String.Empty : timeVar.ToString(),
                    description
                );
            }

            public override BattleScriptEvent Clone()
            {
                UnitPositionEvent newEvent = (UnitPositionEvent)base.Clone();
                newEvent.position = new Position(position);
                return newEvent;
            }
        }

        public class ScenarioObjectiveEvent : EventWithCommand<string>, IHasScenarioObjective
        {

            public override string block
            {
                //get { return trigger + " " + this.GetHashCode().ToString(); }
                get { return trigger + " " + (scenarioObjective == null ? "" : scenarioObjective.ToString());
                }
            }

            public ScenarioObjective scenarioObjective
            {
                get { return _scenarioObjective; }
                set { _scenarioObjective = value; OnPropertyChanged("scenarioObjective"); }
            }
            ScenarioObjective _scenarioObjective;



            public IObjective objective
            {
                get { return _scenarioObjective; }
            }

            public ScenarioObjectiveEvent(int timeVar, string description, string name, Command command, ScenarioObjective objective) :
                base(timeVar, description, name, command)
            {
                this.scenarioObjective = objective;
                Console.WriteLine("Objective set to " + objective);
            }

            public ScenarioObjectiveEvent(int timeVar, string description, object trigger, ScenarioObjective objective, Command command, ScenarioUnit fromUnit, float south, float east)
                : this(timeVar, description, (string)trigger, command, objective)
            {
                Console.WriteLine("Objective passed as " + objective);

            }

            public ScenarioObjectiveEvent(string name) : this(0, String.Empty, name, null, null, null, 0, 0) { }

            public override string ToCsv()
            {

                return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    trigger,
                    (scenarioObjective == null) ? String.Empty : scenarioObjective.id,
                    (command == null) ? String.Empty : command.nameArgsCsv(),
                    (command == null) ? String.Empty : command.fromIdCsv(),
                    (command == null) ? String.Empty : command.southCsv(),
                    (command == null) ? String.Empty : command.eastCsv(),
                    (timeVar == 0) ? String.Empty : timeVar.ToString(),
                    description
                    );
            }
        }


        public class TimeEvent : EventWithCommandAndUnit<TimeSpan>
        {

            public TimeEvent(int timeVar, string description, TimeSpan time, Command command, ScenarioUnit unit)
                : base(timeVar, description, time, command, unit)
            { }

            //return (Event)Activator.CreateInstance(type,0,String.Empty,name,null, null, 0,0);

            public TimeEvent(string name) : this(0, String.Empty, new TimeSpan(10, 0, 0), null, null, null, 0, 0) { }

            public TimeEvent(int timeVar, string description, object trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
                : this(timeVar, description, (TimeSpan)trigger, command, unit)
            { }


            public override string ToCsv()
            {

                return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                Sky.ToCsv(trigger),
                (command == null) ? (unit == null) ? "" : unit.id : command.targetCsv(),
                (command == null) ? String.Empty : command.nameArgsCsv(),
                (command == null) ? String.Empty : command.fromIdCsv(),
                (command == null) ? String.Empty : command.southCsv(),
                (command == null) ? String.Empty : command.eastCsv(),
                (timeVar == 0) ? String.Empty : timeVar.ToString(),
                description
                );
            }

            public override string block
            {
                get { return Sky.ToCsv(trigger); }
            }


            public override string ToString()
            {
                return Sky.ToCsv(trigger);
            }

        }

        public class UnitEvent : EventWithCommandAndUnit<string>
        {

            public UnitEvent(int timeVar, string description, string name, Command command, ScenarioUnit unit) :
                base(timeVar, description, name, command, unit)
            {
            }

            public UnitEvent(int timeVar, string description, object trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
                : this(timeVar, description, (string)trigger, command, unit)
            {
            }

            public UnitEvent(string name) : this(0, String.Empty, name, null, null, null, 0, 0) { }

            public override string block
            {
                get { return trigger + " " + unit + " " + this.GetHashCode().ToString(); }
            }

            public override string ToCsv()
            {

                return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                trigger,
                (command == null || command.targetCsv() == String.Empty) ? (unit == null) ? "" : unit.id : command.targetCsv(),
                (command == null) ? "" : command.nameArgsCsv(),
                (command == null) ? "" : command.fromIdCsv(),
                (command == null) ? "" : command.southCsv(),
                (command == null) ? "" : command.eastCsv(),
                (timeVar == 0) ? "" : timeVar.ToString(),
                description
                );
                //return String.Format("{0},{1},{2},{3}",
                //    trigger,
                //    //(command == null) ? Command.idCommandFromXZ : command.ToCsv(),
                //    Command.ToCsv5(command),
                //    (timeVar == 0) ? "" : timeVar.ToString(),
                //    description
                //    );
            }

            // public override Command AssignCommand(CommandTemplate commandTemplate, Scenario scenario) {
            //     command = commandTemplate.Create(unit, new string[0], 0, 0, scenario);
            //     return command;
            // }            
        }

        public class ContinueEvent : EventWithCommandAndUnit<string>
        {

            public ContinueEvent(int timeVar, string description, string name, Command command, ScenarioUnit unit) :
                base(timeVar, description, name, command, unit)
            {
            }

            public ContinueEvent(int timeVar, string description, object trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
                : this(timeVar, description, (string)trigger, command, unit)
            {
            }

            public ContinueEvent(string name) : this(0, String.Empty, name, null, null, null, 0, 0) { }

            public BattleScriptEvent parent
            {
                get { return _parent; }
                set
                {
                    if (_parent != null) _parent.PropertyChanged -= parent_PropertyChanged;
                    _parent = value;
                    if (_parent != null) _parent.PropertyChanged += parent_PropertyChanged;
                    OnPropertyChanged("parent");
                    OnPropertyChanged("block");
                }
            }
            BattleScriptEvent _parent;

            private void parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "block")
                {
                    OnPropertyChanged("block");
                }
            }

            public override string block
            {
                get
                {
                    if (this.parent == null) return null;
                    return this.parent.block;
                }
            }

            public override string ToCsv()
            {
                return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                trigger,
                (command == null) ? (unit == null) ? "" : unit.id : command.targetCsv(),
                (command == null) ? "" : command.nameArgsCsv(),
                (command == null) ? "" : command.fromIdCsv(),
                (command == null) ? "" : command.southCsv(),
                (command == null) ? "" : command.eastCsv(),
                (timeVar == 0) ? "" : timeVar.ToString(),
                description
                );

                //return String.Format("{0},{1},{2},{3}",
                //    trigger,
                //    Command.ToCsv5(command),
                //    (timeVar == 0) ? "" : timeVar.ToString(),
                //    description
                //    );
            }

            // public override Command AssignCommand(CommandTemplate commandTemplate, Scenario scenario) {
            //     command = commandTemplate.Create(unit, new string[0], 0, 0, scenario);
            //     return command;
            // }            
        }



        // Commands



        public interface IEventCommand { BattleScriptEvent bEvent { get; set; } }

        public class CommandTemplate
        {
            public Type type;
            public string name
            {
                get;
                set;
            }
            public string help
            {
                get;
                set;
            }
            public string category
            {
                get;
                set;
            }

            public CommandTemplate(Type type, string name, string category, string help)
            {
                this.type = type;
                this.name = name;
                this.help = help;
                this.category = category;

            }

            public Command Create(object thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
            {
                return (Command)Activator.CreateInstance(type, this, thing, args, fromUnit, south, east, scenario);
            }

            // public Command Create(Event aevent, object[] args, float south, float east, Scenario scenario) {
            //     return (Command)Activator.CreateInstance(type, this, aevent.unit, args, south, east, scenario);
            // }

            public Command Create()
            {
                return (Command)Activator.CreateInstance(type, this);
            }

            public override string ToString()
            {
                return name + " " + help;
            }
        }

        public class Command : INotifyPropertyChanged
        {
            public virtual string name { get { return template == null? "null" : template.name; } }
            public string help { get { return template == null? "null" : template.help; } }
            CommandTemplate template;

            public Command(CommandTemplate template)
            {
                this.template = template;
            }

            public Command(CommandTemplate template, object thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : this(template)
            {
            }


            public override string ToString()
            {
                return template == null? "no template" : template.name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }

            public virtual Command Clone()
            {
                return (Command)this.MemberwiseClone();
            }

            //protected void OnPropertyChanged(PropertyChangedEventArgs e)
            //{
            //    PropertyChangedEventHandler handler = PropertyChanged;
            //    if (handler != null)
            //    {
            //        handler(this, e);
            //    }
            //}

            public virtual void CopyArgsTo(Command other)
            {
                //Copy args
                if (other == null) return;

                {
                    IHasUnit to = other as IHasUnit;
                    if (to != null)
                    {
                        IHasUnit from = this as IHasUnit;
                        if (from != null)
                        {
                            to.unit = from.unit;
                        }
                    }
                }

                {
                    IHasDirection to = other as IHasDirection;
                    if (to != null)
                    {
                        IHasDirection from = this as IHasDirection;
                        if (from != null)
                        {
                            to.direction = new Vector(from.direction.X, from.direction.Y);
                        }
                    }
                }


                {
                    IHasFormation to = other as IHasFormation;
                    if (to != null)
                    {
                        IHasFormation from = this as IHasFormation;
                        if (from != null)
                        {
                            to.formation = from.formation;
                        }
                    }
                }


                {
                    IHasPosition to = other as IHasPosition;
                    if (to != null)
                    {
                        IHasPosition from = this as IHasPosition;
                        if (from != null)
                        {
                            to.position = new Position(from.position);
                        }
                    }
                }

                {
                    IHasScenarioObjective to = other as IHasScenarioObjective;
                    if (to != null)
                    {
                        IHasScenarioObjective from = this as IHasScenarioObjective;
                        if (from != null)
                        {
                            to.scenarioObjective = from.scenarioObjective;
                        }
                    }
                }


            }

            public virtual string targetCsv()
            {
                return String.Empty;
            }

            public virtual string nameArgsCsv()
            {
                return name;
            }

            public virtual string fromIdCsv()
            {
                return String.Empty;
            }

            public virtual string southCsv()
            {
                return String.Empty;
            }

            public virtual string eastCsv()
            {
                return String.Empty;
            }


        }

        public class BlindCommand : Command
        {
            string _name;
            object _thing;
            //object _args[];
            ScenarioUnit _fromUnit;
            Position _position;
            float _east;
            public BlindCommand(string name, object thing, ScenarioUnit fromUnit, float south, float east, Scenario scenario)  
            :base(null) {
                _name = name;
                _thing = thing;
                //_args = args;
                _fromUnit = fromUnit;
                _position = new Position(south, east); 
            }

            public override string name { get { return _name; } }

            public override string targetCsv()
            {
                IHasId ihasid = _thing as IHasId;
                if (ihasid != null) return ihasid.id;
                if (_thing == null) return String.Empty;
                return _thing.ToString();
            }

            public override string nameArgsCsv()
            {
                return _name;
            }

            public override string fromIdCsv()
            {
                if (_fromUnit != null) return _fromUnit.id;
                return String.Empty;
            }

            public override string southCsv()
            {
                if (_position == null || float.IsNegativeInfinity(_position.south)) return String.Empty;
                return _position.south.ToString();
            }

            public override string eastCsv()
            {
                if (_position == null || float.IsNegativeInfinity(_position.east)) return String.Empty;
                return _position.east.ToString();
            }
        }

        public class WeatherCommand : Command
        {

            public int weather
            {
                get { return _weather; }
                set { _weather = value; OnPropertyChanged("weather"); }
            }
            int _weather;

            public WeatherCommand(CommandTemplate template) : base(template) { }

            public WeatherCommand(CommandTemplate template, object thing, string[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template)
            {
                _weather = Convert.ToInt32(args[0]);
            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}={1}",
                    name,
                    _weather
                    );
            }


        }

        public class UnitCommand : Command, IHasUnit
        {

            public float amount
            {
                get { return _amount; }
                set {
                    if (value == _amount) return;
                    if (float.IsInfinity(value) || float.IsNaN(value)) return;
                    _amount = value;
                    OnPropertyChanged("amount"); }
            }
            float _amount;


            public ScenarioUnit unit
            {
                get { return _unit; }
                set { _unit = value; OnPropertyChanged("unit"); }
            }
            ScenarioUnit _unit;


            public UnitCommand(CommandTemplate template) : base(template) { }

            public UnitCommand(CommandTemplate template, ScenarioUnit unit)
                : base(template)
            {
                this._unit = unit;
            }

            public UnitCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template)
            {
                _amount = east;
                this._unit = thing;
            }


            public override string targetCsv()
            {
                return unit == null ? String.Empty : unit.id;
            }


            public override string eastCsv()
            {
                return amount == 0 || float.IsInfinity(amount) || float.IsNaN(amount) ? String.Empty : amount.ToString();
            }
        }

        public class UnitArgsCommand : UnitCommand
        {
            public string[] args
            {
                get { return _args; }
                set { _args = value; OnPropertyChanged("args"); }
            }
            string[] _args;

            public UnitArgsCommand(CommandTemplate template) : base(template) { }

            public UnitArgsCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            { }

            public UnitArgsCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {
                _args = args as string[];
            }

            public override string nameArgsCsv()
            {
                string formattedArgs = "";
                if (_args != null)
                {
                    formattedArgs = String.Join(":", _args);
                }
                return String.Format("{0}:{1}",
                    name,
                    formattedArgs
                    );
            }

        }


        public class UnitArgMapObjectiveCommand : UnitCommand , IHasMapObjective {
            public MapObjective mapObjective
            {
                get { return _mapObjective; }
                set { _mapObjective = value; OnPropertyChanged("mapObjective"); }
            }
            MapObjective _mapObjective;

            public IObjective objective
            {
                get { return _mapObjective; }
            }

            public UnitArgMapObjectiveCommand(CommandTemplate template) : base(template) { }

            public UnitArgMapObjectiveCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            { }

            public UnitArgMapObjectiveCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {
               if (args.Length < 1) return;

                MapObjective result = args[0] as MapObjective;
                if (result != null)
                {
                    mapObjective = result;
                    return;
                }

                if (! (args[0] is string) ) {
                    Log.Error(this, "Unable to convert "+args[0]+" to MapObjective or string");
                    return;
                }

                if (! scenario.map.objectives.TryGetValue((string)args[0], out result)) {
                    Log.Error(this, "Unable to find MapObjective "+args[0]+" in map "+scenario.map.name);
                    return;
                }

                mapObjective = result;
            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                    mapObjective == null ? "" : mapObjective.id
                    );
            }

        }
        public class UnitFromUnitCommand : UnitCommand, IHasOther
        {
            public ScenarioUnit other
            {
                get { return _other; }
                set { _other = value; OnPropertyChanged("other"); }
            }
            ScenarioUnit _other;

            public UnitFromUnitCommand(CommandTemplate template) : base(template) { }

            public UnitFromUnitCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            { }

            public UnitFromUnitCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {
                other = fromUnit;
            }

            public override string fromIdCsv()
            {
                if (other == null) return String.Empty;
                return other.id;

            }

        }

        public class UnitArgUnitCommand : UnitCommand, IHasOther
        {
            public ScenarioUnit other
            {
                get { return _other; }
                set { _other = value; OnPropertyChanged("other"); }
            }
            ScenarioUnit _other;

            public UnitArgUnitCommand(CommandTemplate template) : base(template) { }

            public UnitArgUnitCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            { }

            public UnitArgUnitCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {

               if (args.Length < 1) return;

                ScenarioUnit result = args[0] as ScenarioUnit;
                if (result != null)
                {
                    other = result;
                    return;
                }

                if (! (args[0] is string) ) {
                    Log.Error(this, "Unable to convert "+args[0]+" to ScenarioUnit or string");
                    return;
                }

                if (! scenario.TryGetUnitByIdOrName( (string) args[0], out result )) {
                    Log.Error(this, "Unable to convert "+args[0]+" to ScenarioUnit");
                    return;
                }

                other = result;

            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                    other == null ? "" : other.id
                    );
            }

        }


        public class UnitArgDistanceCommand : UnitCommand
        {
            public int distance
            {
                get { return _distance; }
                set { _distance = value; OnPropertyChanged("distance"); }
            }
            int _distance;

            public UnitArgDistanceCommand(CommandTemplate template) : base(template) { }

            public UnitArgDistanceCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            { }

            public UnitArgDistanceCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {
                if (args.Length < 1) return;

                if (args[0] is int)
                {
                    distance = (int)args[0];
                    return;
                }

                if (! (args[0] is string) ) {
                    Log.Error(this, "Unable to convert "+args[0]+" to int or string");
                    return;
                }

                int result;
                if (! int.TryParse((string)args[0], out result)) {
                    Log.Error(this, "Unable to convert "+args[0]+" to int");
                    return;
                }

                distance = result;
                
            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                    distance.ToString()
                    );
            }

        }

        public class UnitPositionCommand : UnitCommand, IHasPosition
        {
            public Position position
            {
                get { return _position; }
                set
                {
                    if (_position != null) _position.PropertyChanged -= position_PropertyChanged;
                    _position = value;
                    if (_position != null) _position.PropertyChanged += position_PropertyChanged;
                    OnPropertyChanged("position");
                }
            }
            Position _position;

            private void position_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                OnPropertyChanged("position");
            }

            public UnitPositionCommand(CommandTemplate template)
                : base(template)
            {
                position = new Position();
            }

            public UnitPositionCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            {
                this.position = position;
                //this.position.SetPosition(position);
            }

            public UnitPositionCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {
                this.position = new Position(south, east);
                //this.position.SetPosition(position);

            }

            public override Command Clone()
            {
                UnitPositionCommand clone = (UnitPositionCommand)base.Clone();
                //copy position
                clone.position = new Position(this.position);
                return clone;
            }

            public override string southCsv()
            {
                if (position == null || float.IsNegativeInfinity(position.south) ) return String.Empty;
                return ((int)(position.south)).ToString();
            }

            public override string eastCsv()
            {
                if (position == null || float.IsNegativeInfinity(position.east) ) return String.Empty;
                return ((int)(position.east)).ToString();
            }
        }

        public class UnitMoveToCommand : UnitPositionCommand
        {
            public UnitMoveToCommand(CommandTemplate template) : base(template) { }

            public UnitMoveToCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit, position)
            { }

            public UnitMoveToCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            { }

        }

        public class UnitPositionArgsCommand : UnitPositionCommand
        {
            public string[] args
            {
                get { return _args; }
                set { _args = value; OnPropertyChanged("args"); }
            }
            string[] _args;

            public UnitPositionArgsCommand(CommandTemplate template) : base(template) { }

            public UnitPositionArgsCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit, position)
            { }

            public UnitPositionArgsCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing, args, fromUnit, south, east, scenario)
            {
                _args = args as string[];
            }

            public override string nameArgsCsv()
            {
                string formattedArgs = "";
                if (_args != null)
                {
                    formattedArgs = String.Join(":", _args);
                }
                return String.Format("{0}:{1}",
                    name,
                    formattedArgs
                    );
            }
        }

        public class UnitDirectionCommand : UnitCommand, IHasDirection
        {
            public Vector direction
            {
                get { return _direction; }
                set { _direction = value; OnPropertyChanged("direction"); }
            }
            Vector _direction;

            public UnitDirectionCommand(CommandTemplate template) : base(template) { }


            public UnitDirectionCommand(CommandTemplate template, ScenarioUnit unit, Vector direction)
                : base(template, unit)
            {
                this._direction = direction;
            }

            public UnitDirectionCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {
                direction = new Vector(south, east);
            }

            public override string southCsv()
            {
                return direction.X.ToString();
            }

            public override string eastCsv()
            {
                return direction.Y.ToString();
            }

        }


        public class UnitFormationCommand : UnitCommand, IHasFormation
        {

            public Formation formation
            {
                get { return _formation; }
                set { _formation = value; OnPropertyChanged("formation"); }
            }
            Formation _formation;

            public UnitFormationCommand(CommandTemplate template) : base(template) { }

            public UnitFormationCommand(CommandTemplate template, ScenarioUnit unit, Formation formation)
                : base(template, unit)
            {
                this._formation = formation;
            }

            public UnitFormationCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {
                amount = east;
                try
                {
                    formation = args[0] is Formation ? (Formation)args[0] : scenario.config.formations[(string)args[0]];
                }
                catch
                {
                    if (args.Length > 0)
                    {
                        Log.Error(this, " Battlescript UnitFormationCommand " + thing + " " + fromUnit + " no such Formation " + args[0]);
                    }
                    throw;
                }
            }


            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                    formation == null ? String.Empty : formation.id
                    );
            }

        }

        public class UnitFormTypeCommand : UnitCommand
        {
            public enum EFormType {
                Fight,
                March,
                Square,
                Assault,
                Skirmish,
                AltSkirmish,
                Line,
                ColumnHalf,
                ColumnFull,
                Special
            };

            public EFormType formType {
                get {return _formType;}
                set {_formType = value; OnPropertyChanged("formType"); }
            }
            EFormType _formType;





            public Formation formation
            {
                get { return _formation; }
                set { _formation = value; OnPropertyChanged("formation"); }
            }
            Formation _formation;

            public UnitFormTypeCommand(CommandTemplate template) : base(template) { }

            public UnitFormTypeCommand(CommandTemplate template, ScenarioUnit unit, Formation formation)
                : base(template, unit)
            {
                this._formation = formation;
            }

            public UnitFormTypeCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {

                if (args.Length < 1) return;

                if (args[0] is EFormType) {
                    formType = (EFormType)args[0];
                    return;
                }

                int index;
                if (! int.TryParse((string)args[0], out index)) {
                    Log.Error(this, args[0] + " is not a valid FormType type");
                    return;
                }

                try
                {
                    formType = (EFormType) index;
                }
                catch (ArgumentException)
                {
                    Log.Error(this, index + " is not a valid FormType type integer");
                    return;
                }
                
            }


            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                    formType == null ? String.Empty : ((int)formType).ToString()
                    );
            }

        }

        public class UnitEventCommand : UnitCommand, IEventCommand
        {
            public BattleScriptEvent bEvent
            {
                get { return _bEvent; }
                set { _bEvent = value; OnPropertyChanged("bEvent"); }
            }
            BattleScriptEvent _bEvent;

            public UnitEventCommand(CommandTemplate template) : base(template) { }

            public UnitEventCommand(CommandTemplate template, ScenarioUnit unit, BattleScriptEvent aevent)
                : base(template, unit)
            {
                this._bEvent = aevent;
            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                    name,
                   bEvent == null ? String.Empty : bEvent.ToString()
                    );
            }
        }



        public class UnitOrderCommand : UnitCommand
        {
            public Order order
            {
                get { return _order; }
                set { _order = value; OnPropertyChanged("order"); }
            }
            Order _order;

            public List<string> orderArgs
            {
                get { return _orderArgs; }
                set { _orderArgs = value; OnPropertyChanged("orderArgs"); }
            }
            List<string> _orderArgs;

            public UnitOrderCommand(CommandTemplate template)
                : base(template)
            {
            }

            public UnitOrderCommand(CommandTemplate template, ScenarioUnit unit, Position position)
                : base(template, unit)
            {
            }

            public UnitOrderCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {
            }

            public override string nameArgsCsv()
            {

                string argsString = "";
                if (_orderArgs != null && _orderArgs.Count > 0)
                {
                    foreach (string arg in _orderArgs)
                    {
                        argsString += ":" + arg;
                    }
                }

                return String.Format("{0}",
                    name
                    );
            }

        }

    /// <summary>
    /// 13:30:00,OOB_Fr_Napoleon_Bonaparte,courier:OOB_Fr_JB_Drouet:loadcour:Courier:French_Message1,,,,,
    /// </summary>
        public class CourierCommand : UnitCommand, IHasRecipient, IHasScreenMessage
        {

            public ScenarioUnit recipient
            {
                get { return _recipient; }
                set { _recipient = value; OnPropertyChanged("recipient"); }
            }
            ScenarioUnit _recipient;

            public ScreenMessage screenMessage
            {
                get { return _screenMessage; }
                set { _screenMessage = value; OnPropertyChanged("screenMessage"); }
            }
            ScreenMessage _screenMessage;

            public string screen1
            {
                get { return _screen1; }
                set { _screen1 = value; OnPropertyChanged("screen1"); }
            }
            string _screen1 = "loadcour";

            public string screen2
            {
                get { return _screen2; }
                set { _screen2 = value; OnPropertyChanged("screen2"); }
            }
            string _screen2 = "Courier";

            public CourierCommand(CommandTemplate template) : base(template) { }

            public CourierCommand(CommandTemplate template, ScenarioUnit unit, ScreenMessage message, ScenarioUnit recipient, string screen1, string screen2)
                : base(template, unit)
            {
                this._recipient = recipient;
                this._screenMessage = message;
                this._screen1 = screen1;
                this._screen2 = screen2;
            }

            public CourierCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template, thing)
            {

                this._recipient = args[0] is ScenarioUnit ? (ScenarioUnit)args[0] : scenario[(string)args[0]];
                this._screen1 = (string)args[1]; // always loadscreen?
                this._screen2 = (string)args[2]; //2 is always courier?

                if (args[3] is ScreenMessage)
                {
                    this._screenMessage = (ScreenMessage)args[3];
                }

                try
                {
                    this._screenMessage = scenario.screenMessages[(string)args[3]];
                }
                catch (System.Collections.Generic.KeyNotFoundException e)
                {
                    Log.Error(this, "Unable to find screen message '" + args[3] + "'");
                    throw;
                }

            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}:{2}:{3}:{4}",
                  name,
                  recipient == null ? String.Empty : recipient.id,
                   screen1 == null ? String.Empty : screen1,
                   screen2 == null ? String.Empty : screen2,
                   screenMessage == null ? String.Empty : screenMessage.id
                );
            }

        }

        public class LogMsgCommand : Command
        {


            public ScreenMessage message
            {
                get { return _message; }
                set { _message = value; OnPropertyChanged("message"); }
            }
            ScreenMessage _message;

            public string screen1
            {
                get { return _screen1; }
                set { _screen1 = value; OnPropertyChanged("screen1"); }
            }
            string _screen1;

            public LogMsgCommand(CommandTemplate template) : base(template) { }

            public LogMsgCommand(CommandTemplate template, ScreenMessage message, string screen1)
                : base(template)
            {
                this._message = message;
                this._screen1 = screen1;
            }

            public LogMsgCommand(CommandTemplate template, ScenarioUnit thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template)
            {

                this._screen1 = (string)args[0]; // always loadscreen?

                if (args[1] is ScreenMessage)
                {
                    this._message = (ScreenMessage)args[1];
                }

                try
                {
                    this._message = scenario.screenMessages[(string)args[1]];
                }
                catch (System.Collections.Generic.KeyNotFoundException e)
                {
                    Log.Error(this, "Unable to find screen message '" + args[1] + "'");
                    throw;
                }

            }

            // public override string ToCsv() {
            //     return name+":"+recipient.id+":"+screen1+":"+screen2+":"+message.id+",,";                
            // } 

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}:{2}",
                  name,
                   screen1,
                     message.id);
            }

        }


        public class ScenarioObjectiveCommand : Command, IHasScenarioObjective
        {

            public ScenarioObjective scenarioObjective
            {
                get { return _objective; }
                set { _objective = value; OnPropertyChanged("scenarioObjective");}
            }
            ScenarioObjective _objective;

            public IObjective objective
            {
                get { return _objective; }
            }

            public ScenarioObjectiveCommand(CommandTemplate template) : base(template) { }


            public ScenarioObjectiveCommand(CommandTemplate template, ScenarioObjective objective)
                : base(template)
            {
                this._objective = objective;
            }


            public ScenarioObjectiveCommand(CommandTemplate template, object thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
                : base(template)
            {
                this._objective = scenario.objectives[(string)args[0]];


            }

            public override string nameArgsCsv()
            {
                return String.Format("{0}:{1}",
                     name,
                    scenarioObjective.id);
            }

        }





        public class Order
        {
            string _id, _help;
            public string id { get { return _id; } }
            public string help { get { return _help; } }
            public Order(string id, string help)
            {
                this._id = id;
                this._help = help;
            }

            public string ToCsv()
            {
                return id;
            }
        }

    

    //public class EventBasicSort : IComparer<BattleScriptEvent>
    //{
    //    public int Compare(BattleScriptEvent x, BattleScriptEvent y)
    //    {
    //        TimeEvent tex = x as TimeEvent;
    //        TimeEvent tey = y as TimeEvent;

    //        if (tex != null && tey != null)
    //        {
    //            if (tex.trigger == null && tey.trigger == null) return 0;
    //            if (tex.trigger == null) return -1;
    //            if (tey.trigger == null) return 1;

    //            return tex.trigger.CompareTo(tey.trigger);
    //        }

    //        if (tex != null)
    //        {
    //            return -1;
    //        }

    //        if (tey != null)
    //        {
    //            return 1;
    //        }

    //        //leave unsorted - a hack, sir, a hack
    //        return 1;
    //    }
    //}

}

