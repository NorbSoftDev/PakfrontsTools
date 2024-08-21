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

using NorbSoftDev.SOW;

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for GameDBModifierDialog.xaml
    /// </summary>
    public partial class GameDBModifierDialog : Window
    {
        public BattleResultsModifier modifier;
        public GameDBModifierDialog(BattleResultsModifier modifier)
        {
            if (modifier == null) modifier = new BattleResultsModifier();
            this.modifier = modifier;
            InitializeComponent();
            ObservableCollectionWithItemNotify<BattleResultsModifier>  list = new ObservableCollectionWithItemNotify<BattleResultsModifier>();
            list.Add(modifier);
            modifierDataGrid.ItemsSource = list;

            
        }

        private void assign_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }


        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
