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
    /// Interaction logic for ScenarioSaveAsDialog.xaml
    /// </summary>
    public partial class ScenarioPropertiesDialog : AbstractDialog
    {

        Scenario.EWeather[] weathers;
        Scenario.ESandbox[] sandboxs;

        Scenario scenario;
        public ScenarioPropertiesDialog(Scenario scenario, bool showDontSave)
        {
            this.scenario = scenario;
            this.DataContext = scenario;



            InitializeComponent();
            dirText.Text = scenario.mod.directory.FullName;
            playerText.Text = scenario.playerEchelon == null ? String.Empty : scenario.playerEchelon.unit == null ? String.Empty : scenario.playerEchelon.unit.name1; 
            saveAsText.Text = scenario.name;

           
            weathers = Enum.GetValues(typeof(Scenario.EWeather)).Cast<Scenario.EWeather>().ToArray<Scenario.EWeather>();
            weatherCombo.ItemsSource = weathers;
            weatherCombo.SelectedItem = scenario.initialWeather;

            sandboxs = Enum.GetValues(typeof(Scenario.ESandbox)).Cast<Scenario.ESandbox>().ToArray<Scenario.ESandbox>();
            sandboxCombo.ItemsSource = sandboxs;
            sandboxCombo.SelectedItem = scenario.sandbox;


            if (!showDontSave) dontSaveButton.Visibility = Visibility.Hidden;


        }


        private void weatherCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            int index = comboBox.SelectedIndex;

            Scenario.EWeather value = weathers[index];
            scenario.initialWeather = value;
        }

        private void sandboxCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            int index = comboBox.SelectedIndex;

            Scenario.ESandbox value = sandboxs[index];
            scenario.sandbox = value;
        }

        public override void Assign()
        {
            choiceWasMade = true;

            scenario.name = saveAsText.Text;
            this.DialogResult = true;
            this.Close();
        }


        protected void dontSave_Click(object sender, RoutedEventArgs e)
        {
            choiceWasMade = true;
            this.DialogResult = false;
            this.Close();
        }

        public override void PositionRelative()
        {
            //stub
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
            //stubb
        }

        private void saveAsText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            string text = textBox.Text;
            text = text.Trim();
            text = text.ToASCII();
            textBox.Text = text;

        }
    }
}
