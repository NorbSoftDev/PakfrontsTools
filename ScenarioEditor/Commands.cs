using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorbSoftDev.SOW;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;

namespace ScenarioEditor
{


    public class RelayCommand : ICommand
    {
        #region Fields
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion   // Fields
        #region Constructors
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute"); 
            _execute = execute; 
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) { 
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        } 
        public void Execute(object parameter) {
            _execute(parameter); 
        }
        #endregion // ICommand Members
    }


    public class BulkUndoableRelayCommand : ICommand
    {
        IUndoStack _undoStack;
        #region Fields
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion   // Fields
        #region Constructors
        public BulkUndoableRelayCommand(IUndoStack undoStack, Action<object> execute) : this(undoStack, execute, null) { }
        public BulkUndoableRelayCommand(IUndoStack undoStack, Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._undoStack = undoStack;
            _execute = execute; 
            _canExecute = canExecute;
        }

        #endregion // Constructors
        #region ICommand Members
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(parameter); }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        } 
        
        public void Execute(object parameter) {
            bool oldActive = _undoStack.active;
            if (oldActive)
            {
                _undoStack.active = false;
            }

            _execute(parameter);

            if (oldActive) {
                _undoStack.scenario_SaveBulkState(parameter.ToString());
                _undoStack.active = true;             
            }
        }
        #endregion // ICommand Members
    }

    //public class SaveScenarioCommand : ICommand
    //{

    //    public void Execute(object parameter)
    //    {

    //        ISOWFile sowFile = parameter as ISOWFile;
    //        if (sowFile == null) throw new Exception("No ISOWFile");

    //        // Configure the message box to be displayed 
    //        if (!sowFile.CanSafelySave())
    //        {
    //            string messageBoxText = "Do you want to save over exiting " + sowFile + " " + sowFile.name + " in " + sowFile.mod.name + "?";
    //            string caption = "SOW Data Editor";
    //            MessageBoxButton button = MessageBoxButton.YesNoCancel;
    //            MessageBoxImage icon = MessageBoxImage.Warning;

    //            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

    //            switch (result)
    //            {
    //                case MessageBoxResult.Yes:
    //                    sowFile.Save();
    //                    return;
    //                default:
    //                    return;
    //            }
    //        }

    //        else
    //        {
    //            sowFile.Save();
    //        }

    //    }


    //    public bool CanExecute(object parameter)
    //    {

    //        ISOWFile sowFile = parameter as ISOWFile;
    //        if (sowFile == null) return false;
    //        if (sowFile.isDirty) return true;
    //        return false;
    //    }


    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //}

    //public class SaveISOWFileCommand : ICommand
    //{

    //    public void Execute(object parameter)
    //    {

    //        ISOWFile sowFile = parameter as ISOWFile;
    //        if (sowFile == null) throw new Exception("No ISOWFile");

    //        // Configure the message box to be displayed 
    //        if (!sowFile.CanSafelySave())
    //        {
    //            string messageBoxText = "Do you want to save over exiting " + sowFile + " " + sowFile.name + " in " + sowFile.mod.name + "?";
    //            string caption = "SOW Data Editor";
    //            MessageBoxButton button = MessageBoxButton.YesNoCancel;
    //            MessageBoxImage icon = MessageBoxImage.Warning;

    //            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

    //            switch (result)
    //            {
    //                case MessageBoxResult.Yes:
    //                    sowFile.Save();
    //                    return;
    //                default:
    //                    return;
    //            }
    //        }

    //        else
    //        {
    //            sowFile.Save();
    //        }

    //    }


    //    public bool CanExecute(object parameter)
    //    {

    //        ISOWFile sowFile = parameter as ISOWFile;
    //        if (sowFile == null) return false;
    //        if (sowFile.isDirty) return true;
    //        return false;
    //    }


    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //}



    public class UndoCommand : ICommand
    {

        public void Execute(object parameter)
        {

            ScenarioUndoStack undoStack = parameter as ScenarioUndoStack;
            undoStack.Undo();

        }

        public bool CanExecute(object parameter)
        {
            ScenarioUndoStack undoStack = parameter as ScenarioUndoStack;
            if (undoStack == null) return false;
            return undoStack.canUndo;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    public class RedoCommand : ICommand
    {

        public void Execute(object parameter)
        {

            ScenarioUndoStack undoStack = parameter as ScenarioUndoStack;
            undoStack.Redo();

        }

        public bool CanExecute(object parameter)
        {
            ScenarioUndoStack undoStack = parameter as ScenarioUndoStack;
            if (undoStack == null) return false;
            return undoStack.canRedo;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    public class PrintISOWFileCommand : ICommand
    {

        public void Execute(object parameter)
        {

            ISOWFile sowFile = parameter as ISOWFile;
            sowFile.PrettyPrint();

        }



        public bool CanExecute(object parameter)
        {

            ISOWFile sowFile = parameter as ISOWFile;
            if (sowFile == null) return false;
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    //public class AssignPlayerCommand : ICommand
    //{
    //    public void Execute(object parameter)
    //    {
    //        if (parameter == null) return;

    //        ScenarioEchelon echelon = parameter as ScenarioEchelon;
    //        if (echelon == null)
    //        {
    //            ScenarioUnit unit = parameter as ScenarioUnit;
    //            if (unit == null) return;
    //            echelon = (ScenarioEchelon)unit.echelon;
    //        }
    //        if (echelon == null) return;
    //        Scenario scenario = SOWScenarioEditorWindow.
    //        if (scenario == null) return;

    //        EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> scenarioSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(e.Data as DataObject);
    //        if (scenarioSelection == null || scenarioSelection.Count < 1) return;
    //        scenario.playerEchelon = scenarioSelection[0];
            
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        if (parameter == null) return false;
    //        ScenarioEchelon echelon = parameter as ScenarioEchelon;
    //        if (echelon == null)
    //        {
    //            ScenarioUnit unit = parameter as ScenarioUnit;
    //            if (unit == null) return false;
    //            echelon = (ScenarioEchelon)unit.echelon;
    //        }
    //        if (echelon == null) return false;

    //        return true;
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }
    //}

    public class RandomizePositionsCommand : ICommand
    {
        Random random = new Random();
        public void Execute(object parameter)
        {
            Scenario scenario = parameter as Scenario;
            Rect rect = new Rect(0, 0,
                scenario.map.extent, scenario.map.extent
                );
            Randomize(scenario.root, rect);
        }

        void Randomize(ScenarioEchelon echelon, Rect rect)
        {
            if (echelon.unit != null)
            {
                echelon.unit.transform.south = (float)(rect.Left + random.NextDouble() * rect.Width);

                echelon.unit.transform.east = (float)(rect.Top + random.NextDouble() * rect.Height);
            }

            foreach (ScenarioEchelon child in echelon.children)
            {
                Randomize(child, rect);
            }
        }

        public bool CanExecute(object parameter)
        {

            ISOWFile sowFile = parameter as ISOWFile;
            if (sowFile == null) return false;
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }



    abstract public class DeleteEchelonCommand<T, R> : ICommand
        where T : class, IUnit, INotifyPropertyChanged
        where R : EchelonGeneric<T>
    {

        public DeleteEchelonCommand()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter">can be echelon or unit</param>
        public void Execute(object parameter)
        {
            if (parameter == null) return;

            R echelon = parameter as R;
            if (echelon == null)
            {
                T unit = parameter as T;
                if (unit == null) return;
                echelon = (R)unit.echelon;
            }
            if (echelon.parent == null) return;

            // Commands are lame and fore me to use static
            //FIXME: Crsh occuring when undone as org does not have key for unit after removed
            SOWScenarioEditorWindow.GetScenarioUndoStack().scenario_SaveBulkState("delete");
            SOWScenarioEditorWindow.GetScenarioUndoStack().active = false;

            ((ObservableRoster<T, R>)echelon.root.roster).RemoveEchelon(echelon);
            SOWScenarioEditorWindow.GetScenarioUndoStack().active = true;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter == null) return false;
            R echelon = parameter as R;
            if (echelon == null)
            {
                T unit = parameter as T;
                if (unit == null) return false;
                echelon = (R)unit.echelon;
            }

            return echelon.parent != null;
        }

        public abstract IUndoStack undoStack { get; }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    public class DeleteScenarioEchelonCommand : DeleteEchelonCommand<ScenarioUnit, ScenarioEchelon>
    {
        public DeleteScenarioEchelonCommand() : base() { }

        override public IUndoStack undoStack { get { return SOWScenarioEditorWindow.GetScenarioUndoStack(); } }
    }

    public class DeleteOOBEchelonCommand : DeleteEchelonCommand<OOBUnit, OOBEchelon>
    {
        public DeleteOOBEchelonCommand() : base() { }
        override public IUndoStack undoStack { get { throw new Exception(); } }

    }


    public class AddChilOOBEchelonCommand : ICommand
    {
        public AddChilOOBEchelonCommand() { }

        public void Execute(object parameter)
        {
            if (parameter == null) return;

            OOBEchelon echelon = (OOBEchelon)parameter;
            if (echelon.parent == null) return;
            ((OrderOfBattle)echelon.root.roster).CreateChild(echelon);

        }

        public bool CanExecute(object parameter)
        {
            if (parameter == null) return false;
            OOBEchelon echelon = (OOBEchelon)parameter;
            return !echelon.isLeaf;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    //public class DuplicateBattleScriptEvent: ICommand

    //{
    //    public DuplicateBattleScriptEvent() { }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="parameter">must be Event</param>
    //    public void Execute(object parameter)
    //    {
    //        if (parameter == null) return;

    //        Event bEvent = parameter as Event;
    //        if (bEvent == null) return;

    //        Event newEvent = bEvent.Clone();
            

    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        if (parameter == null) return false;
    //        Event bEvent = parameter as Event;
    //        if (bEvent == null) return false;

    //        return true;
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //}
}
