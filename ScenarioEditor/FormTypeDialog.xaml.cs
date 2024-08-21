using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class FormTypeDialog : AbstractDialog
    {
        public FormTypeDialog()
        {
            InitializeComponent();
            //ObservableCollection<string> list = new ObservableCollection<string>();
            //foreach (UnitFormTypeCommand.EFormType formType in Enum.GetValues(typeof(UnitFormTypeCommand.EFormType)).Cast<UnitFormTypeCommand.EFormType>()) {
            //    list.Add(formType.ToString());
            //}
            //mainList.ItemsSource  = list;
            foreach (UnitFormTypeCommand.EFormType formType in Enum.GetValues(typeof(UnitFormTypeCommand.EFormType)).Cast<UnitFormTypeCommand.EFormType>())
            {
                mainList.Items.Add(formType);
            }
        }


        public override void Assign()
        {
            int index = mainList.SelectedIndex;

            UnitFormTypeCommand.EFormType formType;
            try
            {
                formType = (UnitFormTypeCommand.EFormType)index;
            }
            catch (ArgumentException)
            {
                Log.Error(this, index + " is not a valid FormType type integer");
                return;
            }

            foreach ( BattleScriptEvent thing in (IEnumerable<BattleScriptEvent>)this.DataContext)
            {
                IHasCommand ihc =  thing as IHasCommand;
                if (ihc == null) continue;
                UnitFormTypeCommand command = ihc.command as UnitFormTypeCommand;
                if (command == null) continue;
                
                command.formType = formType;
            }
            DialogResult = true;
        }

        public override void PositionRelative()
        {
            PositionRelative(0, -60);
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {


        }

    }
}
