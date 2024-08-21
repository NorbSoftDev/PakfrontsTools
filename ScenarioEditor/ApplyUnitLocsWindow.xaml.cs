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
    /// Interaction logic for ApplyUnitLocsWindow.xaml
    /// </summary>
    public partial class ApplyUnitLocsWindow : Window
    {
        UnitLocsRoster statsRoster;

         public ApplyUnitLocsWindow(string filepath,Config config,  IList<ScenarioUnit> unitFilter)
        {
            InitializeComponent();
            statsRoster = new UnitLocsRoster(config);
            statsRoster.ReadUnitDataFromCsv(filepath, config.headers.startLocs, unitFilter);

            unitsList.ItemsSource = statsRoster.unitStats;
            this.DataContext = statsRoster;

            statsRoster.modifier = new UnitLocsModifier();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(unitsList.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("modifier");
            view.GroupDescriptions.Add(groupDescription);

        }

        protected void accept_Click(object sender, RoutedEventArgs e)
        {

            foreach (UnitStats u in statsRoster.unitStats)
            {
                u.ApplyResults();
            }

            this.Close();
        }

        protected void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void editModifier_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            UnitLocStats stats = button.CommandParameter as UnitLocStats;
            if (stats != null)
            {

                //GameDBModifierDialog window = new GameDBModifierDialog(stats.modifier);
                //window.Owner = this;
                //window.ShowDialog();

                //if (window.DialogResult == true)
                //{
                //    stats.modifier = window.modifier;
                //}

            }
        }

        private void listView_Click(object sender, RoutedEventArgs e)
        {
            UnitStats item = (sender as ListView).SelectedItem as UnitStats;
            if (item != null)
            {
                item.active = !item.active;
            }
        }
    }

    //public class ResultTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate SimpleTemplate { get; set; }
    //    public DataTemplate ComplexTemplate { get; set; }

    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {
    //        //if I have just text
    //        return SimpleTemplate;
    //        //if I have comments and other fancy stuff 
    //        //return ComplexTemplate;
    //    }
    //}
}
