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

    internal class DataGridHelper<T, R>
        where T : class, INotifyPropertyChanged
        where R : IList<T>, new()
    {

        protected Point _lastMouseDown;
        protected SOWScenarioEditorWindow window;
        protected DataGrid dataGrid;
        protected MapPanel mapPanel;


        public DataGridHelper(SOWScenarioEditorWindow window, MapPanel mapPanel, DataGrid dataGrid)
        {
            this.window = window;
            this.dataGrid = dataGrid;
            this.mapPanel = mapPanel;

        }

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMouseDown = e.GetPosition(dataGrid);
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            DataGridCell cell = GetCell((DependencyObject)e.OriginalSource);
            if (cell == null)
            {
                return;
            }

            if (cell.Column is DataGridComboBoxColumn)
            {
                // these need mouse control to do their opening and closing
                return;
            }

            if (cell.IsEditing) return;

            int colindex = cell.Column.DisplayIndex;

            if (e.LeftButton == MouseButtonState.Pressed)
            //&& (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Point currentPosition = e.GetPosition(dataGrid);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 20.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 20.0))
                {

                    R selection = this.GetSelectionSet();
                    DataObject dataObject = new DataObject(selection);
                    dataObject.SetData("DragSource", sender);

                    try
                    {
                       DragDrop.DoDragDrop(dataGrid,
                            //selection,
                            dataObject,
                            DragDropEffects.Move);
                    }
                    catch
                    {
                        Log.Error(this, "Bad Drag " + selection);
                    }
                   
                    
                    Console.WriteLine("[DataGridHelper]  MouseMove" + e.OriginalSource + " " + selection.Count);


                    //DragDropEffects finalDropEffect = DragDrop.DoDragDrop(dataGrid, dataGrid.SelectedValue,
                    //    DragDropEffects.Move);
                }
            }
        }

        public DataGridCell GetCell(DependencyObject dep)
        {

            if (dep is DataGridCell) return (DataGridCell)dep;

            //Stepping through the visual tree
            while ((dep != null) && !(dep is DataGridCell))
            {
                //dep = VisualTreeHelper.GetParent(dep) as DependencyObject;
                if (dep is Visual || dep is System.Windows.Media.Media3D.Visual3D)
                    dep = VisualTreeHelper.GetParent(dep) as DependencyObject;
                else
                    break;
            }

            //Is the dep a cell or outside the bounds of Window1?
            if (dep == null | !(dep is DataGridCell))
            {
                return null;
            }

            return (DataGridCell)dep;
        }

        public DataGridRow GetRow(DependencyObject dep)
        {
            dep = GetCell(dep);
            while ((dep != null) && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            return dep as DataGridRow;
        }

        public int FindRowIndex(DataGridRow row)
        {
            DataGrid dataGrid =
                ItemsControl.ItemsControlFromItemContainer(row)
                as DataGrid;

            int index = dataGrid.ItemContainerGenerator.
                IndexFromContainer(row);

            return index;
        }

        public virtual R GetSelectionSet()
        {
            R selection = new R();
            foreach (object obj in dataGrid.SelectedItems)
                //foreach (T item in dataGrid.SelectedItems)
            {
                T item = obj as T;
                if (item == null) continue;
                selection.Add(item);
            }
            return selection;
        }


        public Scenario scenario
        {
            get
            {
                return (Scenario)window.DataContext;
            }
        }


    }
}
