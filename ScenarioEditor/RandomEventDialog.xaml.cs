using NorbSoftDev.SOW;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class RandomEventDialog : Window
    {

        public Scenario scenario;

        public RandomEventDialog(Scenario scenario)
        {
            this.scenario = scenario;
            InitializeComponent();
            mainList.ItemsSource = scenario.battleScript.namedEvents.Values;

        }

        private void assign_Click(object sender, RoutedEventArgs e)
        {
            NamedEvent namedEvent = mainList.SelectedItem as NamedEvent;
            Assign(namedEvent);
        }

        void Assign(NamedEvent namedEvent)
        {
            if (namedEvent == null)
            {
                DialogResult = false;
                return;
            }

            System.Collections.IList list = this.DataContext as System.Collections.IList;

            foreach (object thing in list)
            {
                IHasNamedEventTrigger bEvent = thing as IHasNamedEventTrigger;
                if (bEvent != null)
                {
                    bEvent.trigger = namedEvent;
                    
                }

                IHasCommand cEvent = thing as IHasCommand;
                if (cEvent != null)
                {
                    RandomEventCommand cCommand = cEvent.command as RandomEventCommand;
                    if (cCommand != null)
                    {
                        cCommand.namedEvent = namedEvent;
;
                        //cEvent.command = namedEvent;
                    }
                    //else
                    //{
                    //    cEvent.command = namedEvent; 
                    //}
                }

            }
            DialogResult = true;
        }

        private void create_Click(object sender, RoutedEventArgs e)
        {
            string tag = createTextBox.Text.ToASCII();
            tag = Regex.Replace(tag, @"\s+", string.Empty);

            NamedEvent namedEvent = scenario.battleScript.GetOrAddNamedEvent(tag);
            Assign(namedEvent);


            //CommandTemplate commandTemplate;

            //scenario.config.commandTemplates.TryGetValue("ranevt", out commandTemplate);


            //RandomEventCommand randomEventCommand = new RandomEventCommand(commandTemplate, tag, scenario);

            //Assign(randomEventCommand);
   
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
