using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScenarioEditor
{
    class ScenarioGeneratorMapPanel : FrameworkElement, INotifyPropertyChanged
    {

        BitmapImage grayscaleImage;
        double grayscaleImageWidth;


        static public Typeface defaultTypeface = new Typeface(
                SystemFonts.MessageFontFamily,
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Condensed);

        static public Brush textBrush = new SolidColorBrush(Colors.DarkGreen);

        // This should make binding possible, but needs more research
        //public Scenario Scenario
        //{
        //    get { return (Scenario)this.GetValue(ScenarioProperty); }
        //    set { 
        //        this.SetValue(ScenarioProperty, value); 
        //        SetScenario(value); 
        //    }
        //}
        //public static readonly DependencyProperty ScenarioProperty = DependencyProperty.Register(
        //  "Scenario", typeof(Boolean), typeof(MapPanel), new PropertyMetadata(false));

        public Scenario Scenario
        {
            get { return _scenario; }
            set
            {
                if (value == _scenario) return;
                SetScenario(value);
            }
        }
        Scenario _scenario;

        Window window;

        MatrixTransform mapMatrixTransform;
        Point lastKeyDownMapPosition, lastPanPosition, lastClickMapPosition;
        bool isPanning;

        DrawingVisual bgVisual;
        List<Visual> visuals = new List<Visual>();


        public ScenarioGeneratorMapPanel()
        {
            Width = 300;
            Height = 350;

            window = Application.Current.MainWindow;

            //unitSharedSelection = new EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>();
            //counterScaleTransform = new ScaleTransform(1.0, 1.0);

            //ClipToBounds = true;
            //Clip = new RectangleGeometry(new Rect(0, 0, 200, 200));

            Focusable = true;
            MouseWheel += new System.Windows.Input.MouseWheelEventHandler(OnMouseWheel);

            //Since we want true pixel location when zooming in
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            mapMatrixTransform = new MatrixTransform();
            mapMatrixTransform.Matrix.Scale(.25f, .25f);

            this.RenderTransform = mapMatrixTransform;
            
        }

        #region Visuals
        protected void AddVisual(Visual v)
        {
            visuals.Add(v);
            AddVisualChild(v);
            AddLogicalChild(v);
        }

        protected void RemoveVisual(Visual v)
        {
            visuals.Remove(v);
            RemoveVisualChild(v);
            RemoveLogicalChild(v);
        }

        public void ClearVisuals()
        {

            foreach (Visual v in visuals)
            {
                RemoveVisualChild(v);
                RemoveLogicalChild(v);
            }
            visuals.Clear();
        }

        // The two necessary overrides, implemented for the single Visual:
        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visuals.Count)
                throw new ArgumentOutOfRangeException("index");
            return visuals[index];
        }

        void Clear()
        {



        }
        #endregion

        #region InputListening
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.IsRepeat) return;

            bool newMode = false;
            switch (e.Key)
            {

                case Key.Space:
                    isPanning = true;
                    lastPanPosition = Mouse.GetPosition(window);
                    break;
            }

        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

        
                switch (e.Key)
                {

                    case Key.Space:
                        isPanning = false;
                        break;
                }
        }

        // http://msdn.microsoft.com/en-us/library/ms742859%28v=vs.110%29.aspx#Implementing_Drag_And_Drop
        protected void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 1) Zoom(1.0 + e.Delta / 100.0);
            else if (e.Delta < 1) Zoom(1.0 / (1.0 + -e.Delta / 100.0));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //Not sure why I need to force focus, but otherwise it is not given even if Focusable=True
            Keyboard.Focus(this);
            // Retrieve the mouse pointer location relative to the Window

            if (isPanning)
            {
                // Do this in window space to avoid double transforming
                Point winLocation = Mouse.GetPosition(window);

                Vector d = winLocation - lastPanPosition;
                if (d.LengthSquared < 4) return;
                Pan(d);
                lastPanPosition = winLocation;
                return;
            }


            Point mapLocation = e.GetPosition(this);

            if ( e.LeftButton == MouseButtonState.Pressed )
            {

                Vector d = lastKeyDownMapPosition - e.GetPosition(this);

                //switch (mode)
                //{

                //    case MapMode.Move:
                //        d = mapToWorldTransform.Value.Transform(d);
                //        foreach (ScenarioEchelon echelon in unitSharedSelection)
                //        {
                //            echelon.unit.transform.MoveBy(-d);
                //            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                //            {
                //                ApplyFormationBrigade(echelon);
                //            }
                //        }
                //        lastKeyDownMapPosition = mapLocation;
                //        return;

                //    case MapMode.Aim:
                //        foreach (ScenarioEchelon echelon in unitSharedSelection)
                //        {

                //            Point ep = GetMapPosition(echelon.unit.transform);
                //            echelon.unit.transform.Face(mapLocation - ep);
                //            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                //            {
                //                ApplyFormationBrigade(echelon);
                //            }
                //        }
                //        return;


                //    case MapMode.Rotate:

                //        d.Normalize();

                //        MatrixTransform hmt = (MatrixTransform)rotateFeedbackVisual.Transform;
                //        Matrix hm = Matrix.Identity;
                //        double mapAngle = (Math.Atan2(d.X, -d.Y) + Math.PI) * 57.2957795;
                //        CurrentInfo = mapAngle + " dgs";

                //        hm.Rotate(mapAngle);
                //        hm.OffsetX = lastKeyDownMapPosition.X;
                //        hm.OffsetY = lastKeyDownMapPosition.Y;
                //        hmt.Matrix = hm;

                //        foreach (ScenarioEchelon echelon in unitSharedSelection)
                //        {
                //            echelon.unit.transform.facing = (float)mapAngle;
                //            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                //            {
                //                ApplyFormationBrigade(echelon);
                //            }
                //        }
                //        return;

                //    case MapMode.Measure:
                //        //Console.WriteLine( d.Length/yardsToWorldTransform.ScaleX +"yds");
                //        d = mapToWorldTransform.Value.Transform(d);
                //        string str = string.Format(
                //            "{0:0.0} yds",
                //            d.Length / yardsToWorldTransform.ScaleX
                //            );
                //        CurrentInfo = str;
                //        measureFeedback.Redraw(
                //            lastKeyDownMapPosition,
                //            e.GetPosition(this),
                //            str
                //            );
                //        break;

                //    default:
                //        break;
                //}
            }

            int imageX = (int)mapLocation.X;
            int imageY = (int)mapLocation.Y;
            byte? val = grayscaleImage.GetPixelGrayscale(imageX, imageY);
            //Console.WriteLine(imageX + "," + imageY +" "+grayscaleImage.PixelWidth+","+grayscaleImage.PixelHeight+ ": " + val);
            if (val != null)
            {
                //CurrentTerrain = this._scenario.map.GetTerrainAtIndex((int)val);
                //Console.WriteLine("Terrain: " + terrain);
            }


            base.OnMouseMove(e);

        }
        #endregion

        #region ZoomPan
        internal void Zoom(double zoom)
        {

            FrameworkElement parent = (FrameworkElement)LogicalTreeHelper.GetParent(this);
            double xCenter = parent.ActualWidth / 2.0;
            double yCenter = parent.ActualHeight / 2.0;

            //scale map and units
            Matrix m1 = mapMatrixTransform.Value;
            m1.ScaleAt(zoom, zoom, xCenter, yCenter);
            AnimateTo(m1, 300);

            //counterscale markers
            double counterScale = 1.0 / m1.M11;
            AnimationTimeline scaleAnim = new DoubleAnimation(counterScale, TimeSpan.FromMilliseconds(300));
            //counterScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            //counterScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        }

        internal void ZoomOff()
        {

            Matrix m1 = mapMatrixTransform.Value;
            m1.M11 = 1; m1.M12 = 0;
            m1.M21 = 0; m1.M22 = 1;
            AnimateTo(m1, 300);
        }


        internal void Center(ScenarioEchelon item)
        {

            if (item.unit == null) return;
            Center(item.unit.transform);

        }

        internal void Center(ScenarioUnit item)
        {
            if (item == null) return;
            Center(item.transform);
        }

        internal void Center(IHasPosition item)
        {
            if (item == null) return;
            Center(item.position);
        }


        internal void Center(Position mapTransform)
        {
            if (mapTransform == null) return;
            CenterPercent(
                mapTransform.east / _scenario.map.extent,
                mapTransform.south / _scenario.map.extent
                );
        }

        internal void CenterPercent(double xPercent, double yPercent)
        {

            FrameworkElement parent = (FrameworkElement)LogicalTreeHelper.GetParent(this);
            double xCenter = parent.ActualWidth / 2.0;
            double yCenter = parent.ActualHeight / 2.0;

            //assume not rotate, shear, or on-uniform scaling
            Matrix m1 = mapMatrixTransform.Value;
            m1.OffsetX = (-xPercent * grayscaleImageWidth) * m1.M11 + xCenter;
            m1.OffsetY = (-yPercent * grayscaleImageWidth) * m1.M11 + yCenter;
            AnimateTo(m1, 600);
        }

        internal void Pan(Vector vector)
        {

             Matrix m1 = mapMatrixTransform.Matrix;
            m1.OffsetX += vector.X * 2.5;
            m1.OffsetY += vector.Y * 2.5;
            AnimateTo(m1, 0);
        }

        internal void SlideToXPercent(double xPercent)
        {

            Matrix m1 = mapMatrixTransform.Value;
            //assumes no rotate or shear
            m1.OffsetX = -xPercent * grayscaleImageWidth * m1.M11;
            AnimateTo(m1, 60);
        }

        internal void SlideToYPercent(double yPercent)
        {

            Matrix m1 = mapMatrixTransform.Value;
            //assumes no rotate or shear
            m1.OffsetY = -yPercent * grayscaleImageWidth * m1.M11;
            AnimateTo(m1, 60);
        }



        double _xPercent;
        public double XPercent
        {
            get
            {
                return _xPercent;
            }

            set
            {
                SlideToXPercent(value);
            }
        }


        double _yPercent;
        public double YPercent
        {
            get
            {
                return _yPercent;
            }

            set
            {
                SlideToYPercent(value);
            }
        }


        internal void AnimateTo(Matrix m1, int milliseconds)
        {
            //TODO make speed dependent on screen space travelled
            MatrixAnimation matrixAnimation = new MatrixAnimation(mapMatrixTransform.Value, m1,
                TimeSpan.FromMilliseconds(milliseconds)
            );
            this.mapMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);

            Point wh = new Point(Width, Height);
            wh = m1.Transform(wh);

            _xPercent = (m1.OffsetX) / (-grayscaleImageWidth * m1.M11);
            _yPercent = (m1.OffsetY) / (-grayscaleImageWidth * m1.M11);

            OnPropertyChanged("");

        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        // Create the OnPropertyChanged method to raise the event 
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion


        void SetScenario(Scenario scenario)
        {

            Clear();

            bgVisual = new DrawingVisual();
            AddVisual(bgVisual);

            this._scenario = scenario;
            scenario.map.Load();
            if (scenario.map.grayscaleFilePath != null)
            {

                System.IO.FileInfo fileInfo = new System.IO.FileInfo(scenario.map.grayscaleFilePath);
                grayscaleImage = new BitmapImage();
                grayscaleImage.BeginInit();
                grayscaleImage.CacheOption = BitmapCacheOption.OnLoad;
                grayscaleImage.UriSource = new Uri(fileInfo.FullName, UriKind.Relative);
                grayscaleImage.EndInit();

                //Image image = new Image();
                //image.Source = src;
                //image.Stretch = Stretch.Uniform;

                using (DrawingContext dc = bgVisual.RenderOpen())
                {
                    Rect rc = new Rect(0, 0, grayscaleImage.PixelWidth, grayscaleImage.PixelHeight);
                    dc.DrawImage(grayscaleImage, rc);
                }

                grayscaleImageWidth = grayscaleImage.PixelWidth;

                this.SnapsToDevicePixels = true;
                this.Width = grayscaleImage.Width;
                this.Height = grayscaleImage.Height;

            }

            AddGrids();
        }

        void AddGrids()
        {
            DrawingVisual v = new DrawingVisual();

            //IFootprint footprint = new MapObjectiveFootprint(objective, v, this);
            //AddVisual(v);
            //objectiveFootprintsByVisual[v] = footprint;
            //footprintsByObjective[objective] = footprint;

            AddGrid(v);
            AddVisual(v);

        }

        void AddGrid(DrawingVisual visual)
        {
            Color color = Colors.Yellow;
            Color transpColor = Color.Subtract(color, Color.FromArgb(100, 0, 0, 0));

            Brush hiliteBrush = new SolidColorBrush(color);
            Brush fillBrush = new SolidColorBrush(transpColor);
            Pen pen = new Pen(Brushes.Black, 1);

            using (DrawingContext dc = visual.RenderOpen())
            {
                // bounds 
                //dc.PushTransform(mapPanel.yardsToMapTransform);
                //dc.DrawDrawing(bounds);
                //dc.Pop();

                //// marker
                //dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawRectangle(
                    hiliteBrush,
                    pen,
                    new Rect(0,0,200,200)
                    );
                //dc.DrawLine(pen, new Point(-8, -8), new Point(8, 8));
                //dc.DrawText(
                //    new FormattedText(objective.id,
                //        CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                //        MapPanel.defaultTypeface,
                //        10, MapPanel.textBrush, null),
                //    new Point(0, 0));

                //dc.Pop();
            }

            visual.Transform = new MatrixTransform();
            //xform = Matrix.Identity;
            //xform.Translate(objective.east / mapPanel.mapToWorldFactor, objective.south / mapPanel.mapToWorldFactor);
            //visual.Transform = new MatrixTransform(xform);
        }


    }
}
