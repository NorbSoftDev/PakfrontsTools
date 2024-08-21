using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for FormationDialog.xaml
    /// </summary>
    public partial class EventDialog
    {

        public Type eventType { get; set; }

        public EventDialog()
        {
            InitializeComponent();
        }

        public override void Assign()
        {
            EventTemplate eventTemplate = mainList.SelectedItem as EventTemplate;
            if (eventTemplate == null)
            {
                DialogResult = false;
                return;
            }

            foreach (BattleScriptEvent thing in (IEnumerable<BattleScriptEvent>)this.DataContext)
            {
                if (thing.GetType() != eventType) continue;
                EventBase<string> bEvent = thing as EventBase<string>;
                if (bEvent == null) continue;
                bEvent.trigger = eventTemplate.name;

            }
            DialogResult = true;
        }

        public override void PositionRelative()
        {

            PositionRelative(-20, -30);
            //Point p = Mouse.GetPosition(Application.Current.MainWindow);
            //p = Application.Current.MainWindow.PointToScreen(p);
            //this.Top = p.Y - 60;
            //Point localP = Mouse.GetPosition((IInputElement)sender);
            //this.Left = p.X - localP.X;
        }


        public override void SetListSource(System.Collections.IEnumerable source)
        {
            IList<BattleScriptEvent> settingOn = DataContext as IList<BattleScriptEvent>;
            if (settingOn != null)
            {

                eventType = settingOn[0].GetType();
                List<EventTemplate> valid = new List<EventTemplate>();
                foreach (EventTemplate template in source)
                {
                    if (template.type == eventType)
                    {
                        valid.Add(template);
                    }
                }
                mainList.ItemsSource = valid;
            }
            else
            {

                mainList.ItemsSource = source;
            }
        }

        //public Config Config
        //{
        //    get { return _config; }
        //    set
        //    {
        //        _config = value;
        //        IList<Event> settingOn = DataContext as IList<Event>;
        //        if (settingOn != null)
        //        {

        //            eventType = settingOn[0].GetType();
        //            List<EventTemplate> validTemplates = new List<EventTemplate>();
        //            foreach (EventTemplate template in _config.eventTemplates.Values)
        //            {
        //                if (template.type == eventType)
        //                {
        //                    validTemplates.Add(template);
        //                }
        //            }
        //            mainList.ItemsSource = validTemplates;
        //        }
        //        else
        //        {

        //            mainList.ItemsSource = _config.eventTemplates.Values;
        //        }
        //    }
        //}
        //Config _config;
    }
}
