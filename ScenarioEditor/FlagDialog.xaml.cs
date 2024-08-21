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
    public partial class FlagDialog
    {
        Config _config;
        int flagSlot = 0;
        public FlagDialog(int flagSlot)
        {
            this.flagSlot = flagSlot;   
            InitializeComponent();
        }

        public override void Assign()
        {
            Graphic graphic = mainList.SelectedItem as Graphic;
            if (graphic == null)
            {
                DialogResult = false;
                return;
            }

            foreach (OOBUnit thing in (IEnumerable<OOBUnit>)this.DataContext)
            {
                //IHasFormation thing = (IHasFormation)this.DataContext;
                switch (flagSlot){
                    case 2:
                        thing.flag2 = graphic;
                        break;

                    default:
                        thing.flag = graphic;
                        break;
                }
            }
            DialogResult = true;
        }

        public void InitImages(Config config) {
            _config = config;
            IList<OOBUnit> units = (IList<OOBUnit>)DataContext;
            Graphic flagGraphic = units[0].flag;
            BitmapSource bitmap = flagGraphic.GetBitmapSource(config);
            if (bitmap != null)
            {
                image.Source = bitmap;
            }
        }

        public override void PositionRelative()
        {

            PositionRelative(-20, -30);
        }

        private void selectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Graphic graphic = e.AddedItems[0] as Graphic;
            if (graphic == null) return;
            BitmapSource bitmap = graphic.GetBitmapSource(_config);
            if (bitmap != null)
            {
                image.Source = bitmap;
            }

        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
           mainList.ItemsSource = source;
            
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {

            foreach (OOBUnit thing in (IEnumerable<OOBUnit>)this.DataContext)
            {
                //IHasFormation thing = (IHasFormation)this.DataContext;
                switch (flagSlot)
                {
                    case 2:
                        thing.flag2 = null;
                        break;

                    default:
                        thing.flag = null;
                        break;
                }
            }
            DialogResult = true;
        }

    }
}
