using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class AddEventDialog
    {

        int _index = -1;
        Scenario scenario;
        public AddEventDialog(Scenario scenario)
        {
            this.scenario = scenario;
            InitializeComponent();
        }

        public AddEventDialog(Scenario scenario, int index) : this(scenario)
        {
            _index = index;
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
            mainList.ItemsSource = source;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mainList.ItemsSource);
            view.Filter = UserFilter;
        }


        public class TmpHolder : IHasNamedEventTrigger
        {
           public NamedEvent trigger { get; set; }
        }
        void ranevt_Click(object sender, RoutedEventArgs e)
        {

            TmpHolder holder = new TmpHolder();
            List<TmpHolder> tmpList = new List<TmpHolder>();
            tmpList.Add(holder);

           

            RandomEventDialog dialog = new RandomEventDialog(scenario);
            dialog.DataContext = tmpList;
            dialog.ShowDialog();

            if (dialog.DialogResult != true || holder.trigger == null) return;

            RandomEvent ranevt = new RandomEvent(0, "", holder.trigger, null, null);
            ObservableCollection<BattleScriptEvent> thing = (ObservableCollection<BattleScriptEvent>)this.DataContext;

            BattleScriptEvent parent = null;
            if (_index > 0) parent = thing[_index - 1];
            BattleScriptEvent bevent = ranevt;

            int insertAt = _index;
            if (_index < 0)
            {
                insertAt = thing.Count;
            }

            thing.Insert(insertAt, bevent);
            DialogResult = true;
            Close();

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

        public override void Assign()
        {
            EventTemplate template = mainList.SelectedItem as EventTemplate;
            if (template == null)
            {
                DialogResult = false;
                return;
            }

            //this is used for evtcont
            //Event parent = sender as Event;

            ObservableCollection<BattleScriptEvent> thing = (ObservableCollection<BattleScriptEvent>)this.DataContext;

            BattleScriptEvent parent = null;
            if ( _index > 0) parent = thing[_index-1];

            int insertAt = _index;
            if (_index <= 0)
            {
                insertAt = thing.Count;
            }

            BattleScriptEvent bevent = template.Create(parent);

            //insert time event in chronological order
            TimeEvent tevent = bevent as TimeEvent;
            if (tevent != null)
            {
                int timeInsertAt = 0;
                int i = 0;
                foreach (BattleScriptEvent o in thing)
                {
                    TimeEvent ot = o as TimeEvent;
                    if (ot == null)
                    {
                        //TODO set location just before
                        continue;
                        i++;

                    }

                    int compare = tevent.trigger.CompareTo(ot.trigger);
                    bool gotPast = false;
                    switch (compare)
                    {
                        case -1:
                            timeInsertAt = i+1;
                            break;
                        case 0:
                            timeInsertAt = i+1;
                            break;
                        case 1:
                            timeInsertAt = i;
                            gotPast = true;
                            break;
                    }
                    if (gotPast) break;
                    i++;
                }
                insertAt = timeInsertAt;
            }

            thing.Insert(insertAt, bevent);
            DialogResult = true;


        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            EventTemplate template = item as EventTemplate;

            if (template == null) return false;
 
           return template.name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
            CollectionViewSource.GetDefaultView(mainList.ItemsSource).Refresh();
        }
    }
}
