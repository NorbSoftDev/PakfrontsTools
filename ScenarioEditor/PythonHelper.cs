using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Windows.Input;
using System.Windows.Controls;
using NorbSoftDev.SOW;
using NorbSoftDev.SOW.Utils;
using System.IO;

namespace ScenarioEditor
{
    class PythonHelper
    {

        private ScriptEngine engine = null;
        //private ScriptRuntime pythonRuntime = null;
        private ScriptScope mainScope = null;
        TextBox inputBox, outputBox;
        //MemoryStream outputStream;
        //StreamWriter outputStreamWriter;

        public PythonHelper(TextBox inputBox, TextBox outputBox)
        {
            this.inputBox = inputBox;
            this.outputBox = outputBox;
        }

        public Scenario scenario
        {
            get { return _scenario; }
            set { if (value == _scenario) return; SetScenario(value); }
        }
        Scenario _scenario;


        void SetScenario(Scenario scenario)
        {
            if (mainScope == null) return;
            mainScope.SetVariable("scenario", scenario);
            this._scenario = scenario;
            outputBox.AppendText("# Reset \"scenario\"\n");
        }

        public void InitializeIronPython()
        {
            if (engine != null) return;

            Log.Info(this, "Initializing IronPython");
            engine = Python.CreateEngine();
            mainScope = engine.CreateScope();


            // outputStream = new MemoryStream();
            // outputStreamWriter = new StreamWriter(outputStream);

            //engine.Runtime.IO.SetOutput(outputStream, outputStreamWriter);

            engine.Runtime.IO.RedirectToConsole();
            Console.SetOut(TextWriter.Synchronized(new TextBoxWriter(outputBox)));
            Console.SetError(TextWriter.Synchronized(new TextBoxWriter(outputBox)));

            Console.Out.WriteLine("# Initialized IronPython");
            engine.Runtime.LoadAssembly(typeof(NorbSoftDev.SOW.Scenario).Assembly);
            engine.Runtime.LoadAssembly(typeof(NorbSoftDev.SOW.Utils.MapArea).Assembly);
            CompileSourceAndExecute("import NorbSoftDev.SOW as SOW; import NorbSoftDev.SOW.Utils as Utils");

            //engine.Runtime.IO.SetOutput(stream, txtWriter);
            //engine.Runtime.IO.SetErrorOutput(stream, txtWriter);


            //pyEngine.AddToPath(AppDomain.CurrentDomain.BaseDirectory);


            //Options.PrivateBinding = true;
            //pythonEngine = new PythonEngine();


            //pythonEngine.SetStandardError(s);
            //pythonEngine.SetStandardOutput(s);

            //pythonEngine.AddToPath(AppDomain.CurrentDomain.BaseDirectory);

            //pythonEngine.Globals.Add("form", this);
            //pythonEngine.Globals.Add("bl", bl);
        }

        internal EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> echelonSharedSelection
        {
            get
            {
                return _unitSharedSelection;
            }

            set
            {
                _unitSharedSelection = value;
                _unitSharedSelection.CollectionChanged += sharedSelection_CollectionChanged;
            }
        }
        EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> _unitSharedSelection;

        private void sharedSelection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mainScope == null) return;
            mainScope.SetVariable("selectedEchelons", echelonSharedSelection);
            outputBox.AppendText("Reset \"selectedEchelons\"\n");
        }


        public void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            try
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ExecuteAndClear();
                }

                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    Execute();
                }

                outputBox.Foreground = System.Windows.Media.Brushes.White;

            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message+"\n");
                outputBox.Foreground = System.Windows.Media.Brushes.Red;
                outputBox.AppendText(ex.Message+"\n");
            }
        }

        public void ExecuteAndClear()
        {
            CompileSourceAndExecute(inputBox.Text);
            inputBox.Clear();
        }

        public void Execute()
        {
            CompileSourceAndExecute(inputBox.Text);
        }

        public void CompileSourceAndExecute(String code)
        {
            ScriptSource source = engine.CreateScriptSourceFromString
                        (code, SourceCodeKind.Statements);
            CompiledCode compiled = source.Compile();
            // Executes in the scope of Python

            compiled.Execute(mainScope);
            //outputBox.Text += actual;
        }

        internal void ImportScript(string pythonFilePath)
        {
            try
            {
                System.IO.StreamReader myFile = new System.IO.StreamReader(pythonFilePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                inputBox.Text += myString;

            }
            catch
            {
                Log.Error(this, "Unable to read python file " + pythonFilePath);
                return;
            }
        }

        internal void ExecuteScript(string pythonFilePath)
        {
            try
            {
                System.IO.StreamReader myFile = new System.IO.StreamReader(pythonFilePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                CompileSourceAndExecute(myString);

            }
            catch
            {
                Log.Error(this, "Unable to read python file " + pythonFilePath);
                return;
            }
        }
    }

    class TextBoxWriter : TextWriter
    {
        private TextBox _textBox;

        public TextBoxWriter(TextBox textbox)
        {
            _textBox = textbox;
        }


        public override void Write(char value)
        {
            base.Write(value);
            // When character data is written, append it to the text box.
            _textBox.AppendText(value.ToString());
            _textBox.ScrollToEnd();
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
