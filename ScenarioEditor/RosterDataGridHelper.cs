using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorbSoftDev.SOW;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace ScenarioEditor
{
    
    class RosterDataGridHelper<TEchelon, TUnit> : DataGridHelper<TEchelon, EchelonSelectionSet<TEchelon,TUnit>>
        where TEchelon : EchelonGeneric<TUnit>
        where TUnit : class, IUnit, INotifyPropertyChanged
    {

        public RosterDataGridHelper(SOWScenarioEditorWindow window,  MapPanel mapPanel, DataGrid dataGrid) 
        : base(window, mapPanel, dataGrid) {}


        public void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if (dataGrid.SelectedItems.Count < 1)
            {
                e.Handled = true;
                return;
            }

            var res = MessageBox.Show("Proceed to Delete " + dataGrid.SelectedItems.Count+" units and their children?", "Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.Cancel || res == MessageBoxResult.No)
            {
                e.Handled = true;
                return;
            }



            TEchelon[] echelons = new TEchelon[dataGrid.SelectedItems.Count];
            for (int i = 0; i < dataGrid.SelectedItems.Count; i++)
            {
                TUnit unit = (TUnit)(dataGrid.SelectedItems[i]);
                echelons[i] = (TEchelon)(unit.echelon); 
            }
            ((ObservableRoster<TUnit, TEchelon>)echelons[0].root.roster).RemoveEchelons(echelons);
        }

        public override EchelonSelectionSet<TEchelon, TUnit> GetSelectionSet()
        {
            EchelonSelectionSet<TEchelon, TUnit> selection = new EchelonSelectionSet<TEchelon, TUnit>();
            foreach (TUnit unit in dataGrid.SelectedItems)
            {
                selection.Add(unit);
            }
            return selection;
        }

        public void AddSelectCells()
        {
            foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells) {
                //cellInfo.Item
                dataGrid.SelectedItems.Add( cellInfo.Item);
            }

        }

        public void ShowFormationDialog(object sender, RoutedEventArgs e)
        {

            AddSelectCells();

            FormationDialog dialog = new FormationDialog();
            //Button button = sender as Button;
            //if (button != null) {
            //    TUnit unit = button.CommandParameter as TUnit;
            //    dataGrid.SelectedItems.Add(unit);
            //}


            dialog.DataContext = GetSelectionSet().units; ;
            dialog.SetListSource(scenario.config.formations.Values);
            dialog.PositionRelative();
            dialog.ShowDialog();
        }


        public void ShowWeaponDialog(object sender, RoutedEventArgs e)
        {
            AddSelectCells();


            WeaponDialog dialog = new WeaponDialog();
            dialog.DataContext = GetSelectionSet().units;
            dialog.SetListSource(scenario.config.weapons.Values);
            dialog.PositionRelative();
            dialog.ShowDialog();
        }


        internal void ShowFlagDialog(object sender, RoutedEventArgs e, int flagSlot)
        {
            AddSelectCells();

            FlagDialog dialog = new FlagDialog(flagSlot);
            dialog.DataContext = GetSelectionSet().units;
            dialog.InitImages(scenario.config);
            dialog.SetListSource(scenario.config.graphics.Values);
            dialog.PositionRelative();
            dialog.ShowDialog();
        }

        internal void DragOver(object sender, DragEventArgs e)
        {
            //throw new NotImplementedException();
        }

        internal void Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            DataGridRow row = GetRow((DependencyObject)e.OriginalSource);
            if (row == null) return;

            PositionSelectionSet positionSelection = PositionSelectionSet.ExtractFromDataObject(e.Data as DataObject);
            if (positionSelection != null && positionSelection.Count > 0)
            {
                if (row.Item is ScenarioUnit)
                {
                    ((ScenarioUnit)(row.Item)).position.SetPosition(positionSelection[0]);
                    return;
                }

            }


            ScenarioObjectiveSelectionSet objectiveSelection = ScenarioObjectiveSelectionSet.ExtractFromDataObject(e.Data as DataObject);
            if (objectiveSelection != null)
            {
                if (row.Item is ScenarioUnit)
                {
                    ((ScenarioUnit)(row.Item)).position.SetPosition(objectiveSelection[0]);
                    return;
                }
                return;
            }

            Console.WriteLine("RosterDataGridHelper] Dropped \"" + e.Data.GetData(typeof(object)) + "\" on row " + FindRowIndex(row) + " orig: " + e.OriginalSource);
            return;
        }
    }
}