using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorbSoftDev.SOW
{

    public class NamedEvent : INotifyPropertyChanged
    {

        public NamedEvent(string tag) {
            this.tag = tag;
        }
        public string tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                OnPropertyChanged("tag");
            }
        }
        string _tag;

        public List<RandomEvent> events = new List<RandomEvent>();

        internal void Add(RandomEvent randomEvent)
        {
            if (events.Contains(randomEvent)) return;
            randomEvent.trigger = this;
            events.Add(randomEvent);
            OnPropertyChanged("tag");
        }

        internal void Remove(RandomEvent randomEvent)
        {
            if (!events.Contains(randomEvent)) return;
            randomEvent.trigger = null;
            events.Remove(randomEvent);
            OnPropertyChanged("tag");
        }



        internal string GetTagFor(RandomEvent randomEvent)
        {
            return "evtran" + tag + (events.IndexOf(randomEvent) + 1);
        }

        public override string ToString()
        {
            return _tag;
        }

        // Create the OnPropertyChanged method to raise the event 
        public event PropertyChangedEventHandler PropertyChanged;

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
    }
    public class RandomEventCommand : Command
    {

        //Dictionary<string, NamedEvent> _namedEvents;

        NamedEvent _namedEvent;
        public NamedEvent namedEvent
        {
            get { return _namedEvent; }
            set {
                if (_namedEvent == value) return;
                _namedEvent = value;
                OnPropertyChanged("namedEvent");

               OnPropertyChanged("tag");
            }
        }
        //string _tag;

        public string tag
        {
            get { if (_namedEvent == null) return "";  return _namedEvent.tag; }
            //set {
               
            //   OnPropertyChanged("tag");
            //}
        }
        //string _tag;

        //public List<RandomEvent> events = new List<RandomEvent>();


        public RandomEventCommand(CommandTemplate template) : base(template) { }

        public RandomEventCommand(CommandTemplate template, NamedEvent namedEvent)
            : base(template)
        {
            _namedEvent = namedEvent;
        }

        public RandomEventCommand(CommandTemplate template, object thing, object[] args, ScenarioUnit fromUnit, float south, float east, Scenario scenario)
            : base(template)
        {
            string atag = (string)args[0];
            _namedEvent = scenario.battleScript.GetOrAddNamedEvent(atag);
            
        }

        public RandomEventCommand(CommandTemplate template, string tag, Scenario scenario)
            : base(template)
        {
            _namedEvent = scenario.battleScript.GetOrAddNamedEvent(tag);
            //this.tag = tag;
            //_namedEvents = scenario.battleScript.namedEvents;
            //_namedEvents[tag] = this;
        }

        //public void LinkToNamedEvent(Scenario scenario, string atag)
        //{
        //               if (!  scenario.battleScript.namedEvents.TryGetValue(atag, out _namedEvent) )
        //               {
        //       _namedEvent = new NamedEvent(atag);
        //       scenario.battleScript.namedEvents.Add(_namedEvent.tag, _namedEvent);
        //               }
           
        //}

        //public void LinkToNamedEvent(Scenario scenario, NamedEvent namedEvent)
        //{

        //        _namedEvent = new NamedEvent(atag);
        //        scenario.battleScript.namedEvents.Add(_namedEvent.tag, _namedEvent);
            

        //}



        public override string nameArgsCsv()
        {
            return String.Format("{0}:{1}:{2}",
                name,
                tag,
                _namedEvent == null ? String.Empty : _namedEvent.events.Count.ToString()
                );
        }

        //internal string GetTagFor(RandomEvent randomEvent)
        //{
        //    return "evtran" + tag + (events.IndexOf(randomEvent) + 1);
        //}



        public override string ToString()
        {
            return tag;
        }

    }

    public interface IHasNamedEventTrigger
    {
        NamedEvent trigger { get; set; }
    }

    public class RandomEvent : EventWithCommandAndUnit<NamedEvent>, IHasNamedEventTrigger
    {

        public override NamedEvent trigger
        {
            get { return _trigger; }
            set
            {
                if (_trigger == value) return;
                if (_trigger != null)
                {
                 //   _trigger.Remove(this);
                }


                _trigger = value;

                if (_trigger != null)
                {
                    _trigger.Add(this);
                }
                OnPropertyChanged("trigger");
                OnPropertyChanged("block");
            }
        }

        public RandomEvent(int timeVar, string description, NamedEvent randomEventWithCommand, Command command, ScenarioUnit unit)
            : base(timeVar, description, randomEventWithCommand, command, unit)
        {
            if (this.trigger != null) this.trigger.Add(this);
        }


        public RandomEvent(int timeVar, string description, object trigger, ScenarioUnit unit, Command command, ScenarioUnit fromUnit, float south, float east)
            : this(timeVar, description, trigger as NamedEvent, command, unit)
        {
        }

        public RandomEvent(string name) : this(0, String.Empty, null, null, null, null, 0, 0) { }

        public override string block
        {
            get
            {
                return tag;
                //this.GetHashCode().ToString();
            }
        }

        public override string ToCsv()
        {

            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            tag,
            (command == null) ? (unit == null) ? "" : unit.id : command.targetCsv(),
            (command == null) ? "" : command.nameArgsCsv(),
            (command == null) ? "" : command.fromIdCsv(),
            (command == null) ? "" : command.southCsv(),
            (command == null) ? "" : command.eastCsv(),
            (timeVar == 0) ? "" : timeVar.ToString(),
            description
            );

        }

        public string tag
        {
            get { return trigger == null ? "evtran" : trigger.GetTagFor(this); }
        }

        public override string ToString()
        {
            return tag;
        }
    }
}
