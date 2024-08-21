using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScenarioEditor
{
    class EventDataGridHelper : DataGridHelper<BattleScriptEvent, EventSelectionSet>
    {

        public EventDataGridHelper(SOWScenarioEditorWindow window, MapPanel mapPanel, DataGrid dataGrid)
            : base(window, mapPanel, dataGrid)
        {

        }

        internal void CommandDrop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            DataGridRow row = GetRow((DependencyObject)e.OriginalSource);
            if (row == null) return;

            DataObject dataObject = e.Data as DataObject;

            EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> unitSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(dataObject);

            if (unitSelection != null && unitSelection.Count > 0)
            {
                if (row.Item is IHasCommand)
                {
                    Command command = ((IHasCommand)(row.Item)).command;

                    if (command is IHasOther)
                    {
                        ((IHasOther)(command)).other = unitSelection[0].unit;
                        return;
                    }
                    if (command is IHasRecipient)
                    {
                        ((IHasRecipient)(command)).recipient = unitSelection[0].unit;
                        return;
                    }

                    //keep this at the end as a catch-all
                    if (command is IHasUnit)
                    {
                        ((IHasUnit)(command)).unit = unitSelection[0].unit;
                        return;
                    }
                    if (command is IHasPosition)
                    {
                        IHasPosition from = unitSelection[0].unit as IHasPosition;
                        ((IHasPosition)(command)).position.SetPosition(from.position);
                    }

                }
            }

            ScenarioObjectiveSelectionSet scenarioObjectiveSelection = ScenarioObjectiveSelectionSet.ExtractFromDataObject(e.Data as DataObject);

            if (scenarioObjectiveSelection != null)
            {

                bool eventHasScenarioObjective = row.Item is IHasScenarioObjective;
                bool commandHasScenarioObjective = row.Item is IHasCommand && ((IHasCommand)(row.Item)).command is IHasScenarioObjective;

                if (commandHasScenarioObjective)
                {
                    Command command = ((IHasCommand)(row.Item)).command;
                    if (command is IHasScenarioObjective)
                    {
                        ((IHasScenarioObjective)(command)).scenarioObjective = scenarioObjectiveSelection[0];
                        return;
                    }

                    if (command is IHasPosition)
                    {
                        ((IHasPosition)(command)).position.SetPosition(scenarioObjectiveSelection[0]);
                        return;
                    }
                }

                if (eventHasScenarioObjective)
                {
                    IHasScenarioObjective aevent = ((IHasScenarioObjective)(row.Item));
                    aevent.scenarioObjective = scenarioObjectiveSelection[0];
                    return;
                }
            }

            ScreenMessageSelectionSet screenMessageSelection = ScreenMessageSelectionSet.ExtractFromDataObject(e.Data as DataObject);

            if (screenMessageSelection != null)
            {

                bool commandHasScreenMessage = row.Item is IHasCommand && ((IHasCommand)(row.Item)).command is IHasScreenMessage;

                if (commandHasScreenMessage)
                {
                    Command command = ((IHasCommand)(row.Item)).command;
                    if (command is IHasScreenMessage)
                    {
                        ((IHasScreenMessage)(command)).screenMessage = screenMessageSelection[0];
                        return;
                    }

 
                }


            }
        }

        internal void Drop(object sender, DragEventArgs e)
        {

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            DataGridRow row = GetRow((DependencyObject)e.OriginalSource);
            if (row == null) return;

            

            DataObject dataObject = e.Data as DataObject;

            EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> unitSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(dataObject);

            if (unitSelection != null && unitSelection.Count > 0)
            {
                if (row.Item is IHasUnit)
                {
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

            ScenarioObjectiveSelectionSet scenarioObjectiveSelection = ScenarioObjectiveSelectionSet.ExtractFromDataObject(e.Data as DataObject);

            if (scenarioObjectiveSelection != null)
            {

                bool eventHasScenarioObjective = row.Item is IHasScenarioObjective;
                bool commandHasScenarioObjective = row.Item is IHasCommand && ((IHasCommand)(row.Item)).command is IHasScenarioObjective;

                if (commandHasScenarioObjective && (!eventHasScenarioObjective) )
                {
                    Command command = ((IHasCommand)(row.Item)).command;
                    if (command is IHasScenarioObjective)
                    {
                        ((IHasScenarioObjective)(command)).scenarioObjective = scenarioObjectiveSelection[0];
                        return;
                    }

                    if (command is IHasPosition)
                    {
                        ((IHasPosition)(command)).position.SetPosition(scenarioObjectiveSelection[0]);
                        return;
                    }
                }

                if (eventHasScenarioObjective)
                {
                    IHasScenarioObjective aevent = ((IHasScenarioObjective)(row.Item));
                    
                        aevent.scenarioObjective = scenarioObjectiveSelection[0];
                        return;
                    

                }


            }

            MapObjectiveSelectionSet mapObjectiveSelection = MapObjectiveSelectionSet.ExtractFromDataObject(e.Data as DataObject);

            if (mapObjectiveSelection != null && row.Item is IHasCommand)
            {
                Command command = ((IHasCommand)(row.Item)).command;
                if (command is IHasMapObjective)
                {
                    ((IHasMapObjective)(command)).mapObjective = mapObjectiveSelection[0];
                    return;
                }

                if (command is IHasPosition)
                {
                    ((IHasPosition)(command)).position.SetPosition(mapObjectiveSelection[0]);
                    return;
                }
            }

            EventSelectionSet eventSelection = EventSelectionSet.ExtractFromDataObject(e.Data as DataObject);
            DataGridCell cell = GetCell((DependencyObject)e.OriginalSource);
            if (eventSelection != null)
            {
                BattleScriptEvent thisEvent = ((BattleScriptEvent)(row.Item));

                //use ordering in battlescript, not ordering of selection
                Dictionary<int, BattleScriptEvent> toInsert = new Dictionary<int, BattleScriptEvent>();
                foreach (BattleScriptEvent aevent in eventSelection)
                {
                    // do not allow drop onto self
                    if (aevent == thisEvent) return;
                    int eventIndex = scenario.battleScript.events.IndexOf(aevent);
                    //toInsert[eventIndex] = aevent.Clone();
                    toInsert[eventIndex] = aevent;//.Clone();
                }

                foreach (BattleScriptEvent otherEvent in eventSelection)
                {
                    scenario.battleScript.events.Remove(otherEvent);
                }

                int insertIndex = scenario.battleScript.events.IndexOf(thisEvent);

                foreach (KeyValuePair<int, BattleScriptEvent> kvp in toInsert.OrderBy(i => i.Key))
                {
                    scenario.battleScript.events.Insert(insertIndex, kvp.Value);
                    insertIndex++;
                }





                //for (int i = eventSelection.Count - 1 ; i > -1; i--) 
                //    // (Event otherEvent in eventSelection.Reverse())
                //{
                //    Event otherEvent = eventSelection[i];
                //    scenario.battleScript.events.Insert(insertIndex,otherEvent);
                //}

            }

            Console.WriteLine("[EventDataGridHelper] Dropped \"" + e.Data.GetData(typeof(object)) + "\" on row " + FindRowIndex(row) + " orig: " + e.OriginalSource);
            return;

        }

        public void DuplicateBefore(object sender, RoutedEventArgs e)
        {

            // Event bEvent = (Event)(((MenuItem)sender).CommandParameter);

            // Scenario scenario = (Scenario)window.DataContext;

            // Event newEvent = bEvent.Clone();

            // int index = scenario.battleScript.events.IndexOf(bEvent) - 1;
            // index = index < 0 ? 0 : index;
            // scenario.battleScript.events.Insert(index, newEvent);


            //Scenario scenario = (Scenario)window.DataContext;
            int insertIndex = scenario.battleScript.events.Count - 1;

            Dictionary<int, BattleScriptEvent> toInsert = new Dictionary<int, BattleScriptEvent>();
            foreach (BattleScriptEvent aevent in dataGrid.SelectedItems)
            {
                int eventIndex = scenario.battleScript.events.IndexOf(aevent);
                if (insertIndex > eventIndex) insertIndex = eventIndex;
                toInsert[eventIndex] = aevent.Clone();

            }
            insertIndex = insertIndex > 0 ? insertIndex - 1 : 0;


            foreach (KeyValuePair<int, BattleScriptEvent> kvp in toInsert.OrderBy(i => i.Key))
            {
                scenario.battleScript.events.Insert(insertIndex, kvp.Value);
                insertIndex++;
            }
        }

        public void DuplicateAfter(object sender, RoutedEventArgs e)
        {

            //Scenario scenario = (Scenario)window.DataContext;
            int insertIndex = 0;

            Dictionary<int, BattleScriptEvent> toInsert = new Dictionary<int, BattleScriptEvent>();
            foreach (BattleScriptEvent aevent in dataGrid.SelectedItems)
            {
                int eventIndex = scenario.battleScript.events.IndexOf(aevent);
                if (insertIndex < eventIndex) insertIndex = eventIndex;
                toInsert[eventIndex] = aevent.Clone();

            }
            insertIndex += 1;

            foreach (KeyValuePair<int, BattleScriptEvent> kvp in toInsert.OrderBy(i => i.Key))
            {
                scenario.battleScript.events.Insert(insertIndex, kvp.Value);
                insertIndex++;
            }

        }
        public void CreateEvtContClick(object sender, RoutedEventArgs e)
        {
            ICommandSource menuItem = (ICommandSource)sender;
            if (menuItem == null)
            {
                return;
            }
            BattleScriptEvent parent = (BattleScriptEvent)(menuItem.CommandParameter);
            int index = window.scenario.battleScript.events.IndexOf(parent) + 1;

            if (parent is ContinueEvent)
            {
                // TODO get parent from evt if thing that called is a evtcont
                parent = ((ContinueEvent)parent).parent;
            }


            BattleScriptEvent bevent = window.config.eventTemplates["evtcont"].Create(parent);

            window.scenario.battleScript.events.Insert(index, bevent);
        }

        public void CreateEventAfter(object sender, RoutedEventArgs e)
        {

            int index = 0;

            ICommandSource menuItem = (ICommandSource)sender;

            if (menuItem != null)
            {
                BattleScriptEvent bEvent = (BattleScriptEvent)(menuItem.CommandParameter);
                index = window.scenario.battleScript.events.IndexOf(bEvent) + 1;
            }

            AddEventDialog dialog = new AddEventDialog(scenario, index);
            dialog.SetListSource(window.config.eventTemplates.Values);
            dialog.DataContext = window.scenario.battleScript.events;
            dialog.PositionRelative();
            dialog.ShowDialog();
        }

        public void CreateEvent(object sender, RoutedEventArgs e)
        {
            AddEventDialog dialog = new AddEventDialog(scenario);
            dialog.SetListSource(window.config.eventTemplates.Values);
            dialog.DataContext = window.scenario.battleScript.events;
            dialog.PositionRelative();
            dialog.ShowDialog();
        }


        internal void Delete(object sender, RoutedEventArgs e)
        {
            int index = 0;

            ICommandSource menuItem = (ICommandSource)sender;

            if (menuItem != null)
            {
                BattleScriptEvent bEvent = (BattleScriptEvent)(menuItem.CommandParameter);
                window.scenario.battleScript.events.Remove(bEvent);
            }

        }

        internal void UnitButtonClick(object sender, RoutedEventArgs e)
        {
            ScenarioUnit unit = (ScenarioUnit)((Button)sender).Content;
            mapPanel.Center(unit);
        }

        public void ShowCommandDialog(object sender)
        {
            CommandDialog dialog = new CommandDialog();
            dialog.DataContext = GetSelectionSet();
            dialog.SetListSource(scenario.config.commandTemplates.Values);
            dialog.PositionRelative();
            dialog.ShowDialog();
        }

        public void ShowFormTypeDialog(object sender)
        {
            FormTypeDialog dialog = new FormTypeDialog();
            dialog.DataContext = GetSelectionSet();
            dialog.PositionRelative();
            dialog.ShowDialog();
        }


        //internal void ShowMoveSpecDialog(object sender)
        //{
        //    MoveSpecDialog dialog = new MoveSpecDialog();
        //    dialog.DataContext = GetSelectionSet();
        //    dialog.PositionRelative();
        //    dialog.ShowDialog();
        //}

        internal void ShowEventDialog(object sender)
        {
            EventDialog dialog = new EventDialog();
            dialog.DataContext = GetSelectionSet();
            dialog.SetListSource(scenario.config.eventTemplates.Values);
            dialog.PositionRelative();
            dialog.ShowDialog();
        }

        internal void ShowRandomEventDialog()
        {
            RandomEventDialog dialog = new RandomEventDialog(scenario);
            dialog.DataContext = GetSelectionSet();
            dialog.ShowDialog();
        }


        internal void ShowTimeDialog(object sender, RoutedEventArgs e, object other)
        {

            System.Windows.Data.CollectionViewGroup group = other as System.Windows.Data.CollectionViewGroup;
            EventSelectionSet selectionSet;
            if (group != null)
            {
                selectionSet = new EventSelectionSet(group.Items);
            }
            else
            {
                selectionSet = GetSelectionSet();
            }
            if (selectionSet == null || selectionSet.Count < 1) return;

            NorbSoftDev.SOW.TimeEvent firstEvent = selectionSet[0] as NorbSoftDev.SOW.TimeEvent;
            if (firstEvent == null) return;

            TimeDialog dialog = new TimeDialog();
            dialog.DataContext = firstEvent.trigger;
            dialog.scenario = scenario;
            dialog.PositionRelative();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                TimeSpan timeSpan = (TimeSpan)(dialog.ReturnValue);
                foreach (var i in selectionSet)
                {
                    NorbSoftDev.SOW.TimeEvent te = i as NorbSoftDev.SOW.TimeEvent;
                    if (te == null) continue;
                    te.trigger = timeSpan;

                    //TODO trigger resort?
                }
            }
        }


        public bool DayWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return false;
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;
            int amt = e.Delta / 120;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(amt, 0, 0, 0));
            return true;
        }

        public bool HourWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return false;
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;
            int amt = e.Delta / 120;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(amt, 0, 0));
            return true;

        }
        public bool MinuteWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return false;
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;
            int amt = e.Delta / 120;
            if (amt == 0 && e.Delta < 0) amt = -1;
            if (amt == 0 && e.Delta > 0) amt = 1;

            //timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(0, amt, 0));

            int n;
            if (Int32.TryParse(textBox.Text, out n))
            {
                n += amt;
                textBox.Text = n.ToString();
            }

            //TimeSpan timeSpan = timeEvent.trigger.Add(new TimeSpan(0, amt, 0));
            //textBox.Text = timeSpan.Minutes.ToString();
            return true;
        }


        public bool SecondWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return false;
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;
            int amt = e.Delta / 120;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(0, 0, amt));
            return true;

        }

        public bool IntWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return false;
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;
            int amt = e.Delta / 120;
            if (amt == 0 && e.Delta < 0) amt = -1;
            if (amt == 0 && e.Delta > 0) amt = 1;

            int n;
            if (Int32.TryParse(textBox.Text, out n))
            {
                n += amt;
                textBox.Text = n.ToString();
            }

            return true;
        }

        public void DayChanged(object sender)
        {
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == "")
            {
                return;// changedTo = 0;
            }
            else if (!Int32.TryParse(textBox.Text, out changedTo) || changedTo < 0 || changedTo > 3)
            {
                textBox.Text = timeEvent.trigger.Days.ToString();
                return;
            }

            int amt = changedTo - timeEvent.trigger.Days;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(amt, 0, 0, 0));

        }

        public void HourChanged(object sender)
        {
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == "")
            {
                return;// changedTo = 0;
            }
            else if (!Int32.TryParse(textBox.Text, out changedTo) || changedTo < 0 || changedTo > 23)
            {
                textBox.Text = timeEvent.trigger.Hours.ToString();
                return;
            }

            if (changedTo < 0 || changedTo > 23)
            {
                textBox.Text = timeEvent.trigger.Hours.ToString();
                return;
            }

            int amt = changedTo - timeEvent.trigger.Hours;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(amt, 0, 0));

        }

        //public void MinuteChanged(object sender, TextChangedEventArgs e)
        //{
        //    TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;

        //    TextBox textBox = (TextBox)sender;
        //    int changedTo;
        //    if (textBox.Text == "")
        //    {
        //        return;// changedTo = 0;
        //    }
        //    else if (!Int32.TryParse(textBox.Text, out changedTo) || changedTo < 0)
        //    {
        //        textBox.Text = timeEvent.trigger.Minutes.ToString();
        //        return;
        //    }

        //    int amt = changedTo - timeEvent.trigger.Minutes;
        //    timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(0, amt, 0));
        //}


        internal void MinuteChanged(object sender)
        {
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == "")
            {
                return;// changedTo = 0;
            }
            else if (!Int32.TryParse(textBox.Text, out changedTo) || changedTo < 0)
            {
                textBox.Text = timeEvent.trigger.Minutes.ToString();
                return;
            }

            int amt = changedTo - timeEvent.trigger.Minutes;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(0, amt, 0));
        }

        public void SecondChanged(object sender)
        {
            TimeEvent timeEvent = (((FrameworkElement)sender).DataContext) as TimeEvent;

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == "")
            {
                return;// changedTo = 0;
            }
            else if (!Int32.TryParse(textBox.Text, out changedTo))
            {
                textBox.Text = timeEvent.trigger.Seconds.ToString();
                return;
            }

            int amt = changedTo - timeEvent.trigger.Seconds;
            timeEvent.trigger = timeEvent.trigger.Add(new TimeSpan(0, 0, amt));

        }


        internal void Sort()
        {
            window.scenario.battleScript.events.Sort();
        }
    }
}
