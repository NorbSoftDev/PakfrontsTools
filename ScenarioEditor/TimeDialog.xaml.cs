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
    public partial class TimeDialog 
    {

        public Scenario scenario;

        //public EventSelectionSet SelectionSet
        //{
        //    set
        //    {
        //        _selectionSet = value;
        //        if (_selectionSet != null && _selectionSet.Count > 0)
        //            foreach (var i in _selectionSet)
        //            {
        //                NorbSoftDev.SOW.TimeEvent te = i as NorbSoftDev.SOW.TimeEvent;
        //                if (te == null) continue;
        //                DataContext = te.trigger;
        //                break;
        //            }
        //        //DataContext = timeSpan;

        //    }
        //    get
        //    {
        //        return _selectionSet;
        //    }
        //}
        //EventSelectionSet _selectionSet;

        public TimeDialog()
        {
            InitializeComponent();
        }

        //private void assign_Click(object sender, RoutedEventArgs e)
        //{
        //    Assign();
        //}

        override public void Assign() {
            //TimeSpan timeSpan = ((TimeSpan)DataContext);
            //foreach (var i in _selectionSet)
            //{
            //    NorbSoftDev.SOW.TimeEvent te = i as NorbSoftDev.SOW.TimeEvent;
            //    if (te == null) continue;
            //    te.trigger = timeSpan;

            //}
            ReturnValue = DataContext;
            DialogResult = true;
        }

        public override void PositionRelative()
        {
            PositionRelative(-20, -30);
            //Point p = Mouse.GetPosition(Application.Current.MainWindow);
            //p = Application.Current.MainWindow.PointToScreen(p);
            //this.Top = p.Y - 25;
            //Point localP = Mouse.GetPosition((Button)sender);
            //this.Left = p.X - localP.X;
        }

        public override void SetListSource(System.Collections.IEnumerable source)
        {
           
        }

        //public Config Config
        //{
        //    get { return _config; }
        //    set
        //    {
        //        _config = value;
        //        //IList<TimeEvent> settingOn = DataContext as IList<TimeEvent>;
        //        //if (settingOn != null && settingOn.Count > 0)
        //        //{

        //        //    timeSpan = settingOn[0].trigger;
        //        //}
        //        //else
        //        //{

        //        //    mainList.ItemsSource = _config.eventTemplates.Values;
        //        //}
        //    }
        //}
        //Config _config;

        //private void assign_Cancel(object sender, RoutedEventArgs e)
        //{

        //    DialogResult = false;
        //}

        public void DaysWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            //if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return;
            int amt = e.Delta / 120;
            TimeSpan timeSpan = ((TimeSpan)DataContext);
            timeSpan = timeSpan.Add(new TimeSpan(amt, 0, 0, 0));
            if (timeSpan < scenario.startTime)
            {
                timeSpan = scenario.startTime;
            }
            DataContext = timeSpan;
            return;
        }


        public void HoursWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            //if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return;
            int amt = e.Delta / 120;
            TimeSpan timeSpan = ((TimeSpan)DataContext);
            timeSpan = timeSpan.Add(new TimeSpan(amt, 0, 0));
            if (timeSpan < scenario.startTime)
            {
                timeSpan = scenario.startTime;
            }
            DataContext = timeSpan;
            return;

        }

        public void MinutesWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            //if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return;
            int amt = e.Delta / 120;
            TimeSpan timeSpan = ((TimeSpan)DataContext);
            timeSpan = timeSpan.Add(new TimeSpan(0, amt, 0));
            if (timeSpan < scenario.startTime)
            {
                timeSpan = scenario.startTime;
            }
            DataContext = timeSpan;
            return;
        }

        public void SecondsWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            //if (!textBox.IsMouseOver || !textBox.IsKeyboardFocusWithin) return;
            int amt = e.Delta / 120;
            TimeSpan timeSpan = ((TimeSpan)DataContext);
            timeSpan = timeSpan.Add(new TimeSpan(0, 0, amt));
            if (timeSpan < scenario.startTime)
            {
                timeSpan = scenario.startTime;
            }
            DataContext = timeSpan;
            return;
        }

        /// <summary>
        /// Exclude illegal chars, and handle special function ekys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private new void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9) ; // it`s number
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ; // it`s number
            else if (e.Key == Key.Escape) Console.WriteLine("Escape");
            else if (e.Key == Key.Return) Console.WriteLine("Return");
            else if (e.Key == Key.Tab || e.Key == Key.CapsLock || e.Key == Key.LeftShift || e.Key == Key.LeftCtrl ||
                e.Key == Key.LWin || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.RightCtrl || e.Key == Key.RightShift ||
                e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Right || e.Key == Key.Delete ||
                e.Key == Key.System || e.Key == Key.Back || 
                e.Key == Key.Add || e.Key == Key.Subtract || e.Key ==  Key.OemPlus || e.Key ==  Key.OemMinus ); // it`s a system key (add other key here if you want to allow)
            else
                e.Handled = true; // the key will suppressed
        }

        public void DaysChanged(object sender, RoutedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == String.Empty)
            {
                return;
            }

            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (Int32.TryParse(textBox.Text, out changedTo))
            {

                int amt;
                if (textBox.Text[0] == '+' || textBox.Text[0] == '-')
                {
                    amt = changedTo;
                }
                else
                {
                    amt = changedTo - timeSpan.Days;
                }

                timeSpan = timeSpan.Add(new TimeSpan(amt, 0, 0));


                if (timeSpan < scenario.startTime)
                {
                    timeSpan = scenario.startTime;
                    e.Handled = false;
                }

            }
            textBox.Text = timeSpan.Days.ToString();
            DataContext = timeSpan;
            return;
        }

        public void HoursChanged(object sender, RoutedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == String.Empty)
            {
                return;
            }

            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (Int32.TryParse(textBox.Text, out changedTo))
            {

                int amt;
                if (textBox.Text[0] == '+' || textBox.Text[0] == '-')
                {
                    amt = changedTo;
                }
                else
                {
                    amt = changedTo - timeSpan.Hours;
                }

                timeSpan = timeSpan.Add(new TimeSpan(amt, 0, 0));


                if (timeSpan < scenario.startTime)
                {
                    timeSpan = scenario.startTime;
                    e.Handled = false;
                }

            }
            textBox.Text = timeSpan.Hours.ToString();
            DataContext = timeSpan;
            return;
        }

        public void MinutesChanged(object sender, RoutedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == String.Empty)
            {
                return;
            }

            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (Int32.TryParse(textBox.Text, out changedTo))
            {

                int amt;
                if (textBox.Text[0] == '+' || textBox.Text[0] == '-')
                {
                    amt = changedTo;
                }
                else
                {
                    amt = changedTo - timeSpan.Minutes;
                }

                timeSpan = timeSpan.Add(new TimeSpan(0, amt, 0));


                if (timeSpan < scenario.startTime)
                {
                    timeSpan = scenario.startTime;
                    e.Handled = false;
                }

            }
            textBox.Text = timeSpan.Minutes.ToString();
            DataContext = timeSpan;
            return;
        }

        public void SecondsChanged(object sender, RoutedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            int changedTo;
            if (textBox.Text == String.Empty)
            {
                return;
            }

            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (Int32.TryParse(textBox.Text, out changedTo))
            {

                int amt;
                if (textBox.Text[0] == '+' || textBox.Text[0] == '-')
                {
                    amt = changedTo;
                }
                else
                {
                    amt = changedTo - timeSpan.Seconds;
                }

                timeSpan = timeSpan.Add(new TimeSpan( 0, 0, amt));


                if (timeSpan < scenario.startTime)
                {
                    timeSpan = scenario.startTime;
                    e.Handled = false;
                }

            }
            textBox.Text = timeSpan.Seconds.ToString();
            DataContext = timeSpan;
            return;
        }

        private void days_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (e.Key == Key.Escape)
            {
                textBox.Text = timeSpan.Days.ToString();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Return)
            {
                DaysChanged(sender, e);
                Assign();
                e.Handled = true;
                return;
            }
        }

        private void hours_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (e.Key == Key.Escape)
            {
                textBox.Text = timeSpan.Hours.ToString();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Return)
            {
                HoursChanged(sender, e);
                Assign();
                e.Handled = true;
                return;
            }
        }

        private void minutes_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (e.Key == Key.Escape)
            {
                textBox.Text = timeSpan.Minutes.ToString();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Return)
            {
                MinutesChanged(sender, e);
                Assign();
                e.Handled = true;
                return;
            }
        }

        private void seconds_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TimeSpan timeSpan = ((TimeSpan)DataContext);

            if (e.Key == Key.Escape)
            {
                textBox.Text = timeSpan.Seconds.ToString();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Return)
            {
                SecondsChanged(sender, e);
                Assign();
                e.Handled = true;
                return;
            }
        }

    }
}
