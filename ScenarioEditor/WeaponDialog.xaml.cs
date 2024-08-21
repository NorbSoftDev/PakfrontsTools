using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for FormationDialog.xaml
    /// </summary>
    public partial class WeaponDialog
    {
        int flagSlot = 0;
        public WeaponDialog()
        {
            InitializeComponent();
        }

        public override void Assign()
        {
            Weapon weapon = mainList.SelectedItem as Weapon;
            if (weapon == null)
            {
                DialogResult = false;
                return;
            }

            foreach (OOBUnit thing in (IEnumerable<OOBUnit>)this.DataContext)
            {
                thing.weapon = weapon;
            }
            DialogResult = true;
        }


        public override void PositionRelative()
        {

            PositionRelative(-20, -30);
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
           mainList.ItemsSource = source;
            
        }

    }
}
