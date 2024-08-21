using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScenarioEditor
{
    class ScenarioObjectiveDataGridHelper : DataGridHelper<ScenarioObjective, ScenarioObjectiveSelectionSet>
    {

        public ScenarioObjectiveDataGridHelper(SOWScenarioEditorWindow window, MapPanel mapPanel, DataGrid dataGrid)
       : base(window, mapPanel, dataGrid) {}

        internal void Drop(object sender, DragEventArgs e)
        {
  
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            DataGridRow row = GetRow((DependencyObject)e.OriginalSource);
            if (row == null) return;

            
            EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> unitSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(e.Data as DataObject);

            if (unitSelection != null && unitSelection.Count > 0)
            {
                if( row.Item is IHasUnit ) {
                    ((IHasUnit)(row.Item)).unit = unitSelection[0].unit;
                    return;
                }

                if (row.Item is IHasCommand)
                {
                    Command command = ((IHasCommand)(row.Item)).command;
                    if (command is IHasUnit)
                    {
                        ((IHasUnit)(command)).unit = unitSelection[0].unit;
                        return;
                    }
                }
                
            }

            Console.WriteLine("[ObjectiveDataGridHelper] Dropped \"" + e.Data.GetData(typeof(object)) + "\" on row " + FindRowIndex(row) + " orig: " + e.OriginalSource);
            return;
            
        }

        internal void ShowBeginTimeDialog(object sender, RoutedEventArgs e)
        {

            ScenarioObjectiveSelectionSet selectionSet = GetSelectionSet();
            if (selectionSet == null || selectionSet.Count < 1) return;

            TimeDialog dialog = new TimeDialog();
            dialog.DataContext = selectionSet[0].beg; 
            dialog.scenario = scenario;
            dialog.PositionRelative();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                TimeSpan timeSpan = (TimeSpan)(dialog.ReturnValue);
                foreach (var i in selectionSet)
                {
                    ScenarioObjective te = i as ScenarioObjective;
                    if (te == null) continue;
                    te.beg = timeSpan;
                }
            }
        }

        internal void ShowEndTimeDialog(object sender, RoutedEventArgs e)
        {

            ScenarioObjectiveSelectionSet selectionSet = GetSelectionSet();
            if (selectionSet == null || selectionSet.Count < 1) return;

            TimeDialog dialog = new TimeDialog();
            dialog.DataContext = selectionSet[0].end;
            //dialog.SelectionSet = selectionSet;
            dialog.scenario = scenario;
            dialog.PositionRelative();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                TimeSpan timeSpan = (TimeSpan)(dialog.ReturnValue);
                foreach (var i in selectionSet)
                {
                    ScenarioObjective te = i as ScenarioObjective;
                    if (te == null) continue;
                    te.end = timeSpan;
                }
            }
        }
    }
}
