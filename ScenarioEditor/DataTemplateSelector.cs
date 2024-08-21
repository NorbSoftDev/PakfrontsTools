using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ScenarioEditor
{
    public class EventDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Choose which xml based resource to darw event fields
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns> </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is BattleScriptEvent)
            {
                BattleScriptEvent aevent;

                // These are defined in the xaml
                aevent = item as ScenarioObjectiveEvent;
                if (aevent != null) return element.FindResource("scenarioObjectiveEventTemplate") as DataTemplate;
 

                aevent = item as UnitPositionEvent;
                if (aevent != null) return element.FindResource("unitLocationEventTemplate") as DataTemplate;

                aevent = item as EventBase<string>;
                if (aevent != null) return element.FindResource("unitEventTemplate") as DataTemplate;

                aevent = item as RandomEvent;
                if (aevent != null) return element.FindResource("randomEventTemplate") as DataTemplate;

                aevent = item as TimeEvent;
                if (aevent != null) return element.FindResource("timeEventTemplate") as DataTemplate;


            
            }

            return null;
        }
    }
    /// <summary>
    /// these are defined in xaml
    /// </summary>
    public class CommandDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is Command)
            {
                // order is important

                Command acommand;
                //Event aevent = item as UnitEvent;
                //if (aevent != null) return element.FindResource("unitEventTemplate") as DataTemplate;

                acommand = item as UnitArgUnitCommand;
                if (acommand != null) return element.FindResource("unitArgUnitCommandTemplate") as DataTemplate;
                
                acommand = item as UnitFromUnitCommand;
                if (acommand != null) return element.FindResource("unitFromUnitCommandTemplate") as DataTemplate;

                acommand = item as UnitArgDistanceCommand;
                if (acommand != null) return element.FindResource("unitArgDistanceCommandTemplate") as DataTemplate;

                acommand = item as UnitArgMapObjectiveCommand;
                if (acommand != null) return element.FindResource("unitArgMapObjectiveCommandTemplate") as DataTemplate;

                acommand = item as UnitPositionCommand;
                if (acommand != null) return element.FindResource("unitPositionCommandTemplate") as DataTemplate;

                acommand = item as ScenarioObjectiveCommand;
                if (acommand != null) return element.FindResource("scenarioObjectiveCommandTemplate") as DataTemplate;

                //aevent = item as UnitDirectionCommand;
                //if (aevent != null) return element.FindResource("unitDirectionCommandTemplate") as DataTemplate;

                acommand = item as UnitFormTypeCommand;
                if (acommand != null) return element.FindResource("unitFormTypeCommandTemplate") as DataTemplate;

                acommand = item as UnitFormationCommand;
                if (acommand != null) return element.FindResource("unitFormationCommandTemplate") as DataTemplate;

                acommand = item as RandomEventCommand;
                if (acommand != null) return element.FindResource("randomEventCommandTemplate") as DataTemplate;

                //aevent = item as UnitOrderCommand;
                //if (aevent != null) return element.FindResource("unitOrderCommandTemplate") as DataTemplate;

                //aevent = item as UnitCommand;
                //if (aevent != null) return element.FindResource("unitCommandTemplate") as DataTemplate;

                acommand = item as CourierCommand;
                if (acommand != null) return element.FindResource("courierCommandTemplate") as DataTemplate;

                //aevent = item as ObjectiveCommand;
                //if (aevent != null) return element.FindResource("objectiveCommandTemplate") as DataTemplate;

                acommand = item as Command;
                if (acommand != null) return element.FindResource("commandTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
