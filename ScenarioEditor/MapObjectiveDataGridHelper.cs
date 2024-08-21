using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorbSoftDev.SOW;
using System.Windows.Controls;
using System.Windows;

namespace ScenarioEditor
{
    class MapObjectiveDataGridHelper : DataGridHelper<MapObjective, MapObjectiveSelectionSet>
    {

        public MapObjectiveDataGridHelper(SOWScenarioEditorWindow window, MapPanel mapPanel, DataGrid dataGrid)
            : base(window, mapPanel, dataGrid) { }

        internal void Drop(object sender, DragEventArgs e)
        {

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            DataGridRow row = GetRow((DependencyObject)e.OriginalSource);
            if (row == null) return;


            EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> unitSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(e.Data as DataObject);

            //if (unitSelection != null && unitSelection.Count > 0)
            //{

                //if (row.Item is IHasCommand)
                //{
                //    Command command = ((IHasCommand)(row.Item)).command;
                //    if (command is IHasObjective)
                //    {
                //        ((IHasObjective)(command)).IHasMapObjective = unitSelection[0].unit;
                //        return;
                //    }
                //}

            //}

            Console.WriteLine("[ObjectiveDataGridHelper] Dropped \"" + e.Data.GetData(typeof(object)) + "\" on row " + FindRowIndex(row) + " orig: " + e.OriginalSource);
            return;

        }

    }
}