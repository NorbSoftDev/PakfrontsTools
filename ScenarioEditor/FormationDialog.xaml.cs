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
    public partial class FormationDialog : AbstractDialog
    {
        public FormationDialog()
        {
            InitializeComponent();
        }

        private int _filterLevel;

        public int FilterLevel
        {
            get { return _filterLevel; }
            set
            {
                _filterLevel = value;
                Filter();
            }
        }

        System.Collections.IEnumerable _source;

        public override void Assign()
        {
            Formation formation = mainList.SelectedItem as Formation;
            if (formation == null)
            {
                DialogResult = false;
                return;
            }

            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext) {
                //IHasFormation thing = (IHasFormation)this.DataContext;
                thing.formation = formation;
            }
            DialogResult = true;
        }

        public override void PositionRelative()
        {
            PositionRelative(0, -60);
            //Point p = Mouse.GetPosition(Application.Current.MainWindow);
            //p = Application.Current.MainWindow.PointToScreen(p);
            //this.Top = p.Y - 60;
            //Point localP = Mouse.GetPosition((IInputElement)sender);
            //this.Left = p.X - localP.X;
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
            _source = source;

            FilterLevel = GuessLevelFilter();

            IList<ScenarioUnit> units = DataContext as IList<ScenarioUnit>;

            UnitClass unitClass = null;

            if (units == null) return;

            foreach (ScenarioUnit unit in units)
            {
                if (unitClass == null)
                {
                    unitClass = unit.unitClass;
                    continue;
                }

                if (unitClass != unit.unitClass) {
                    unitClass = null;
                    break;
                }
            }

            if (unitClass != null)
            {
                if (unitClass.fightingFormation == null)
                    Fight.IsEnabled = false;
                else
                    Fight.Content = Fight.Content + " " + unitClass.fightingFormation.id;

                if (unitClass.walkFormation == null)
                    Walk.IsEnabled = false;
                else
                    Walk.Content = Walk.Content + " " + unitClass.walkFormation.id;

                if (unitClass.squareFormation == null)
                    Square.IsEnabled = false;
                else
                    Square.Content = Square.Content + " " + unitClass.squareFormation.id;

                if (unitClass.assaultFormation == null)
                    Assault.IsEnabled = false;
                else
                    Assault.Content = Assault.Content + " " + unitClass.assaultFormation.id;

                if (unitClass.skirmishFormation == null)
                    Skirmish.IsEnabled = false;
                else
                    Skirmish.Content = Skirmish.Content + " " + unitClass.skirmishFormation.id;

                if (unitClass.lineFormation == null)
                    Line.IsEnabled = false;
                else
                    Line.Content = Line.Content + " " + unitClass.lineFormation.id;

                if (unitClass.alternativeSkirmishFormation == null)
                    Alt.IsEnabled = false;
                else
                    Alt.Content = Alt.Content + " " + unitClass.alternativeSkirmishFormation.id;

                if (unitClass.columnHalfDistanceFormation == null)
                    Half.IsEnabled = false;
                else
                    Half.Content = Half.Content + " " + unitClass.columnHalfDistanceFormation.id;

                if (unitClass.columnFullDistanceFormation == null)
                    Full.IsEnabled = false;
                else
                    Full.Content = Full.Content + " " + unitClass.columnFullDistanceFormation.id;

                if (unitClass.specialFormation == null)
                    Special.IsEnabled = false;
                else
                    Special.Content = Special.Content + " " + unitClass.specialFormation.id;
            } 

        }

        protected void Filter() {

            if (FilterLevel > -1)
            {
                List<Formation> valid = new List<Formation>();
                foreach (Formation formation in _source)
                {
                    if (formation.level == FilterLevel)
                    {
                        valid.Add(formation);
                    }
                }
                mainList.ItemsSource = valid;
            }
            else
            {
                mainList.ItemsSource = _source;
            }
        }

        protected int GuessLevelFilter()
        {
            IList<ScenarioUnit> units = DataContext as IList<ScenarioUnit>;
                if (units != null && units.Count > 0)
                {
                    return Formation.LevelFromRank(units[0].echelon.rank);
                }



                IList<IHasFormation> ihf = DataContext as IList<IHasFormation>;
                if (ihf != null && ihf.Count > 0)
                {
                    UnitFormationCommand command = ihf[0] as UnitFormationCommand;
                    if (command != null && command.unit != null)
                        return Formation.LevelFromRank(command.unit.echelon.rank);

                    if (ihf[0].formation != null)
                    {
                        return ihf[0].formation.level;
                    }

                }
            
            return -1;

        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<String> data = new List<string>(6);
            data.Add("None");
            data.Add("Lvl6 Regiment");
            data.Add("Lvl5 Brigade");
            data.Add("Lvl4 Division");
            data.Add("Lvl3 Corps");
            data.Add("Lvl2 Army");

            ComboBox comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
      
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            
            int value = comboBox.SelectedIndex;
            FilterLevel = value - 1;
        }

        protected void assign_fightingFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.fightingFormation != null) unit.formation = unit.unitClass.fightingFormation;
            }
            DialogResult = true;
        }

        protected void assign_walkFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.walkFormation != null) unit.formation = unit.unitClass.walkFormation;
            }
            DialogResult = true;
        }
        protected void assign_squareFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.squareFormation != null) unit.formation = unit.unitClass.squareFormation;
            }
            DialogResult = true;
        }
        protected void assign_assaultFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.assaultFormation != null) unit.formation = unit.unitClass.assaultFormation;
            }
            DialogResult = true;
        }
        protected void assign_skirmishFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.skirmishFormation != null) unit.formation = unit.unitClass.skirmishFormation;
            }
            DialogResult = true;
        }
        protected void assign_lineFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.lineFormation != null) unit.formation = unit.unitClass.lineFormation;
            }
            DialogResult = true;
        }

        protected void assign_alternativeSkirmishFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.alternativeSkirmishFormation != null) unit.formation = unit.unitClass.alternativeSkirmishFormation;
            }
            DialogResult = true;
        }
        protected void assign_columnHalfDistanceFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.columnHalfDistanceFormation != null) unit.formation = unit.unitClass.columnHalfDistanceFormation;
            }
            DialogResult = true;
        }
        protected void assign_columnFullDistanceFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.columnFullDistanceFormation != null) unit.formation = unit.unitClass.columnFullDistanceFormation;
            }
            DialogResult = true;
        }
        protected void assign_specialFormation(object sender, RoutedEventArgs e)
        {


            foreach (IHasFormation thing in (IEnumerable<IHasFormation>)this.DataContext)
            {
                ScenarioUnit unit = thing as ScenarioUnit;
                if (unit == null) continue;
                if (unit.unitClass.specialFormation != null) unit.formation = unit.unitClass.specialFormation;
            }
            DialogResult = true;
        }

        private void lv_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            DependencyObject src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            Type srcType = src.GetType();
            //make sure the double click was on a list item, not a scrollbar
            if (( srcType == typeof(ListViewItem) || srcType == typeof(GridViewRowPresenter) ) )
            {
                Assign();
            }
        }

    }
}
