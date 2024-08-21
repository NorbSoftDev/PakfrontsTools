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
    /// Interaction logic for FormationDialog.xaml
    /// </summary>
    public partial class CommandDialog 
    {
        public CommandDialog()
        {
            InitializeComponent();

        }


        string filter;
        public override void SetListSource(System.Collections.IEnumerable source)
        {
            mainList.ItemsSource = source;
            // http://www.wpf-tutorial.com/listview-control/listview-grouping/
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mainList.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("category");
            view.GroupDescriptions.Add(groupDescription);

            view.Filter = UserFilter;
        }

        public override void PositionRelative()
        {

            PositionRelative(-20, -30);
            //Point p = Mouse.GetPosition(Application.Current.MainWindow);
            //p = Application.Current.MainWindow.PointToScreen(p);
            //this.Top = p.Y - 60;
            //Point localP = Mouse.GetPosition((IInputElement)sender);
            //this.Left = p.X - localP.X;
        }



        override public void Assign() {
            CommandTemplate commandTemplate = mainList.SelectedItem as CommandTemplate;
            if (commandTemplate == null)
            {
                DialogResult = false;
                return;
            }

            foreach (object thing in (IEnumerable<BattleScriptEvent>)this.DataContext) {
                IHasCommand bEvent = thing as IHasCommand;
                if (bEvent == null) continue;
                Command command = commandTemplate.Create();
                if (bEvent.command != null) bEvent.command.CopyArgsTo(command);
                bEvent.command = command;
            }
            DialogResult = true;
        }


        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(filter))
            {
                return true;
            }
            CommandTemplate template = item as CommandTemplate;

            if (template == null) return false;

            return template.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            filter = txtFilter.Text;
            if (String.IsNullOrEmpty(filter))
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mainList.ItemsSource);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("category");
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(groupDescription);
            }
            else
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mainList.ItemsSource);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("category");
                view.GroupDescriptions.Clear();
            }
            CollectionViewSource.GetDefaultView(mainList.ItemsSource).Refresh();
        }
        //private void cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}
    }




}
