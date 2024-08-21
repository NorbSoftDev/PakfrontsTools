using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ScenarioEditor
{
    public abstract class AbstractDialog : Window
    {
        public bool choiceWasMade = false;
        public AbstractDialog() : base() {

        }

        public object ReturnValue { get; set; }

        protected void cancel_Click(object sender, RoutedEventArgs e)
        {
            choiceWasMade = false;
            DialogResult = false;
        }

        protected void assign_Click(object sender, RoutedEventArgs e)
        {
            choiceWasMade = true;
            Assign();
        }

        protected void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                return;
            }
            if (e.Key == Key.Return)
            {
                Assign();
            }
        }

        public abstract void Assign();
        public abstract void SetListSource(System.Collections.IEnumerable source);
        public abstract void PositionRelative();

        public void PositionRelative(int leftOffset, int topOffset)
        {
            Window w = Application.Current.MainWindow;

            Point p = Mouse.GetPosition(w);
            p = w.PointToScreen(p);

            //Correct for multiple monitors because of course MS never tried this
            // http://stackoverflow.com/questions/6030438/wrong-coordinates-on-multiple-monitors
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            if (source != null)
            {
                p.Y = p.Y / source.CompositionTarget.TransformToDevice.M22;
                p.X = p.X / source.CompositionTarget.TransformToDevice.M11;
            }
            this.Top = Math.Min( 
                p.Y + topOffset,
                w.Top + w.Height - this.Height
                );
  
            this.Left = p.X + leftOffset;
        }
    }


}
