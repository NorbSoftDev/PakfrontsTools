using NorbSoftDev.SOW;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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

    public enum MapMode { Select, Move, Rotate, Aim, Measure };


    class MapPanel : FrameworkElement, INotifyPropertyChanged
    {

        BitmapImage grayscaleImage;

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


        Point lastKeyDownMapPosition, lastPanPosition, lastClickMapPosition;
        bool isPanning;
        bool modal_undoStackWasActive;
        MapMode _mode;
        public MapMode mode
        {
            get { return _mode; }
            set
            {
                if (_mode == value) return;

                if (_mode == MapMode.Rotate) RemoveVisual(rotateFeedbackVisual);
                else if (value == MapMode.Rotate) AddVisual(rotateFeedbackVisual);

                _mode = value;
                OnPropertyChanged("mode");
            }
        }


        MatrixTransform mapMatrixTransform;

        DrawingVisual bgVisual;
        DrawingVisual rotateFeedbackVisual;
        DrawingVisual measureFeedbackVisual;

        MeasureFeedback measureFeedback;

        List<Visual> visuals = new List<Visual>();

        Dictionary<ScenarioEchelon, EchelonFootprint> footprintsByEchelon = new Dictionary<ScenarioEchelon, EchelonFootprint>();
        Dictionary<DrawingVisual, EchelonFootprint> echelonFootprintsByVisual = new Dictionary<DrawingVisual, EchelonFootprint>();

        Dictionary<IObjective, IFootprint> footprintsByObjective = new Dictionary<IObjective, IFootprint>();
        Dictionary<DrawingVisual, IFootprint> objectiveFootprintsByVisual = new Dictionary<DrawingVisual, IFootprint>();

        Dictionary<Command, UnitMoveToCommandFootprint> footprintsByCommand = new Dictionary<Command, UnitMoveToCommandFootprint>();
        Dictionary<DrawingVisual, UnitMoveToCommandFootprint> commandFootprintsByVisual = new Dictionary<DrawingVisual, UnitMoveToCommandFootprint>();

        Dictionary<BattleScriptEvent, UnitPositionEventFootprint> footprintsByEvent = new Dictionary<BattleScriptEvent, UnitPositionEventFootprint>();
        Dictionary<DrawingVisual, UnitPositionEventFootprint> eventFootprintsByVisual = new Dictionary<DrawingVisual, UnitPositionEventFootprint>();


        List<EchelonFootprint> _selectedFootprints = new List<EchelonFootprint>();

        internal double mapToWorldFactor = 64;
        internal ScaleTransform yardsToMapTransform, yardsToWorldTransform;
        internal ScaleTransform counterScaleTransform;

        double grayscaleImageWidth;
        public SOWScenarioEditorWindow window { get; set; }


        internal EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> unitSharedSelection
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

            //un color the previousl selected
            foreach (EchelonFootprint echelonFootprint in _selectedFootprints)
            {
                echelonFootprint.ResetBrush();

            }
            _selectedFootprints.Clear();


            //Color the now selected
            //Should also pop to top of draw order
            //or hide old and replace
            string selInfo = "";

            foreach (ScenarioEchelon echelon in unitSharedSelection)
            {
                if (echelon.unit == null) continue;

                EchelonFootprint echelonFootprint = footprintsByEchelon[echelon];
                _selectedFootprints.Add(echelonFootprint);
                echelonFootprint.hiliteBrush.Color = sideColors[0];
                selInfo += " " + echelon.unit.id;
            }

            SelectionInfo = selInfo;
        }



        //cheap hack for colors - maybe should be kept by main window
        // 0 is selection color
        internal Color[] sideColors = new Color[] {
            Colors.Yellow,
            Color.FromArgb(255, 0, 100, 255) , 
            Color.FromArgb(255, 255, 0, 0) , 
            Color.FromArgb(255, 0, 100, 0) ,
            Color.FromArgb(255, 255, 200, 0)  
        };

        public MapPanel()
        {
            Width = 300;
            Height = 350;

            unitSharedSelection = new EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>();
            counterScaleTransform = new ScaleTransform(1.0, 1.0);

            MouseWheel += new System.Windows.Input.MouseWheelEventHandler(OnMouseWheel);

            Focusable = true;

            //Since we want true pixel location when zooming in
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            mapMatrixTransform = new MatrixTransform();
            this.RenderTransform = mapMatrixTransform;

            this.yardsToMapTransform = new ScaleTransform();
            this.yardsToWorldTransform = new ScaleTransform();

            bgVisual = new DrawingVisual();
            AddVisual(bgVisual);

            rotateFeedbackVisual = new DrawingVisual();
            using (DrawingContext dc = rotateFeedbackVisual.RenderOpen())
            {
                dc.DrawLine(new Pen(Brushes.Black, 1),
                    new Point(0, 0), new Point(0, -4000)); //point North
            }
            rotateFeedbackVisual.Transform = new MatrixTransform();



            measureFeedbackVisual = new DrawingVisual();
            measureFeedback = new MeasureFeedback(measureFeedbackVisual, this);
            //measureFeedback.Redraw(new Point(0, 0), new Point(2000, 0));
        }

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

        void SetScenario(Scenario scenario)
        {

            Clear();

            bgVisual = new DrawingVisual();
            AddVisual(bgVisual);
            AddVisual(measureFeedbackVisual);

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
                mapToWorldFactor = scenario.map.extent / grayscaleImageWidth;
                this.SnapsToDevicePixels = true;
                this.Width = grayscaleImage.Width;
                this.Height = grayscaleImage.Height;

            }

            this.mapToWorldTransform = new ScaleTransform(mapToWorldFactor, mapToWorldFactor);


            double mapToYardsFactor = mapToWorldFactor / scenario.map.unitPerYard;
            this.yardsToMapTransform = new ScaleTransform(1 / mapToYardsFactor, 1 / mapToYardsFactor);
            this.yardsToWorldTransform = new ScaleTransform(scenario.map.unitPerYard, scenario.map.unitPerYard);

            AddEchelon(scenario.root);

            foreach (ScenarioObjective objective in scenario.objectives.Values)
            {
                AddScenarioObjective(objective);
            }
            scenario.objectives.CollectionChanged += objectives_CollectionChanged;

            foreach (MapObjective objective in scenario.map.objectives.Values)
            {
                AddMapObjective(objective);
            }

            foreach (Fort objective in scenario.map.forts.Values)
            {
                AddFortObjective(objective);
            }

            scenario.CollectionChanged += scenario_CollectionChanged;

            foreach (BattleScriptEvent bEvent in scenario.battleScript.events)
            {
                AddEvent(bEvent);

                //bEvent.PropertyChanged += bEvent_PropertyChanged;

                //IHasCommand iHasCommand = bEvent as IHasCommand;
                //if (iHasCommand == null) continue;
                //UnitMoveToCommand command = iHasCommand.command as UnitMoveToCommand;
                //if (command == null) continue;
                //AddOrUpdateUnitMoveToCommand(command, command.unit);
            }

            scenario.battleScript.events.CollectionChanged += events_CollectionChanged;

            //foreach (ScenarioUnit unit in scenario)
            //{
            //    IHasPosition lastLocation = unit;

            //    foreach (UnitMoveToCommand command in scenario.battleScript.GetTimedMoveCommandsFor(unit))
            //    {
            //        AddUnitMoveToCommand(command, command.unit);
            //        lastLocation = command;
            //    }
            //}

        }

        void Clear()
        {

            foreach (IFootprint f in footprintsByObjective.Values)
            {
                f.Disconnect();
            }
            footprintsByObjective.Clear();

            foreach (UnitMoveToCommandFootprint f in footprintsByCommand.Values)
            {
                f.Disconnect();
            }
            footprintsByCommand.Clear();

            foreach (UnitPositionEventFootprint f in footprintsByEvent.Values)
            {
                f.Disconnect();
            }
            footprintsByEvent.Clear();

            foreach (EchelonFootprint f in footprintsByEchelon.Values)
            {
                f.Disconnect();
            }
            footprintsByEchelon.Clear();

            ClearVisuals();

            footprintsByEchelon.Clear();
            echelonFootprintsByVisual.Clear();

            _selectedFootprints.Clear();

        }


        private void objectives_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.NewItems)
                {
                    ScenarioObjective bEvent = item as ScenarioObjective;
                    if (bEvent != null)
                    AddScenarioObjective(bEvent);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    ScenarioObjective bEvent = item as ScenarioObjective;
                    if (bEvent != null)
                    RemoveScenarioObjective(bEvent);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {

                IdDictionary<ScenarioObjective> dict = sender as IdDictionary<ScenarioObjective>;
                ClearScenarioObjectives();

                foreach (ScenarioObjective bEvent in  dict.Values)
                {

                    AddScenarioObjective(bEvent);

                }
            }
        }

        #region Events
        private void events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.NewItems)
                {
                    BattleScriptEvent bEvent = (BattleScriptEvent)item;
                    AddEvent(bEvent);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems)
                {
                    BattleScriptEvent bEvent = (BattleScriptEvent)item;
                    RemoveEvent(bEvent);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {

                ClearEvents();

                foreach (BattleScriptEvent bEvent in (EventCollection)sender)
                {

                    AddEvent(bEvent);

                }
            }
        }


        void bEvent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateEvent((BattleScriptEvent)sender);
        }

        private void AddEvent(BattleScriptEvent bEvent)
        {
            bEvent.PropertyChanged += bEvent_PropertyChanged;
            UpdateEvent(bEvent);
        }

        private void RemoveEvent(BattleScriptEvent bEvent)
        {
            bEvent.PropertyChanged -= bEvent_PropertyChanged;
            {
                UnitPositionEventFootprint footprint;
                if (footprintsByEvent.TryGetValue(bEvent, out footprint))
                {
                    footprintsByEvent.Remove(bEvent);
                    DrawingVisual visual = footprint.visual;
                    RemoveVisual(visual);
                    commandFootprintsByVisual.Remove(visual);
                    footprint.Disconnect();
                }
            }


            IHasCommand iHasCommand = bEvent as IHasCommand;
            if (iHasCommand == null) return;

            UnitMoveToCommand command = iHasCommand.command as UnitMoveToCommand;
            if (command == null) return;

            {
                UnitMoveToCommandFootprint footprint;
                if (footprintsByCommand.TryGetValue(command, out footprint))
                {
                    footprintsByCommand.Remove(command);
                    DrawingVisual visual = footprint.visual;
                    RemoveVisual(visual);
                    commandFootprintsByVisual.Remove(visual);
                    footprint.Disconnect();
                }
            }

        }

        /// <summary>
        /// Start Watching the new command
        /// </summary>
        /// <param name="bEvent"></param>
        private void UpdateEvent(BattleScriptEvent bEvent)
        {

            UnitPositionEvent upe = bEvent as UnitPositionEvent;
            if (upe != null) AddOrUpdateUnitPositionEvent(upe);

            IHasCommand iHasCommand = bEvent as IHasCommand;
            if (iHasCommand == null) return;

            UnitMoveToCommand command = iHasCommand.command as UnitMoveToCommand;
            if (command != null) AddOrUpdateUnitMoveToCommand(command);
        }



        public void ClearEvents()
        {

            foreach (KeyValuePair<DrawingVisual, UnitMoveToCommandFootprint> kvp in commandFootprintsByVisual)
            {
                RemoveVisual(kvp.Key);
                kvp.Value.Disconnect();
            }
            footprintsByCommand.Clear();
            commandFootprintsByVisual.Clear();
        }

        void AddOrUpdateUnitMoveToCommand(UnitMoveToCommand unitMoveToCommand)
        {
            UnitMoveToCommandFootprint footprint;
            if (footprintsByCommand.TryGetValue(unitMoveToCommand, out footprint))
            {
                footprint.Update(unitMoveToCommand.unit);
                return;
            }

            DrawingVisual v = new DrawingVisual();
            footprint = new UnitMoveToCommandFootprint(unitMoveToCommand, v, this, unitMoveToCommand.unit);
            AddVisual(v);
            commandFootprintsByVisual[v] = footprint;
            footprintsByCommand[unitMoveToCommand] = footprint;
        }

        void AddOrUpdateUnitPositionEvent(UnitPositionEvent unitPositionEvent)
        {
            UnitPositionEventFootprint footprint;
            if (footprintsByEvent.TryGetValue(unitPositionEvent, out footprint))
            {
                footprint.Update(unitPositionEvent.unit);
                return;
            }

            DrawingVisual v = new DrawingVisual();
            footprint = new UnitPositionEventFootprint(unitPositionEvent, v, this, unitPositionEvent.unit);
            AddVisual(v);
            eventFootprintsByVisual[v] = footprint;
            footprintsByEvent[unitPositionEvent] = footprint;
        }


        #endregion

        #region Objectives
        void AddMapObjective(MapObjective objective)
        {

            DrawingVisual v = new DrawingVisual();

            IFootprint footprint = new MapObjectiveFootprint(objective, v, this);
            AddVisual(v);
            objectiveFootprintsByVisual[v] = footprint;
            footprintsByObjective[objective] = footprint;
        }


        void AddFortObjective(Fort objective)
        {

            DrawingVisual v = new DrawingVisual();

            IFootprint footprint = new FortObjectiveFootprint(objective, v, this);
            AddVisual(v);
            objectiveFootprintsByVisual[v] = footprint;
            footprintsByObjective[objective] = footprint;
        }

        void AddScenarioObjective(ScenarioObjective objective)
        {

            DrawingVisual v = new DrawingVisual();

            IFootprint footprint = new ScenarioObjectiveFootprint(objective, v, this);
            AddVisual(v);
            objectiveFootprintsByVisual[v] = footprint;
            footprintsByObjective[objective] = footprint;
        }

   

        /// <summary>
        /// Start Watching the new command
        /// </summary>
        /// <param name="bEvent"></param>
        private void UpdateScenarioObjective(ScenarioObjective bEvent)
        {


        }

        private void RemoveScenarioObjective(ScenarioObjective bEvent)
        {
            bEvent.PropertyChanged -= bEvent_PropertyChanged;
            {
                IFootprint footprint;
                if (footprintsByObjective .TryGetValue(bEvent, out footprint))
                {
                    footprintsByObjective.Remove(bEvent);
                    DrawingVisual visual = footprint.visual;
                    RemoveVisual(visual);
                    objectiveFootprintsByVisual.Remove(visual);
                    footprint.Disconnect();
                }
            }


            //IHasCommand iHasCommand = bEvent as IHasCommand;
            //if (iHasCommand == null) return;

            //UnitMoveToCommand command = iHasCommand.command as UnitMoveToCommand;
            //if (command == null) return;

            //{
            //    UnitMoveToCommandFootprint footprint;
            //    if (footprintsByCommand.TryGetValue(command, out footprint))
            //    {
            //        footprintsByCommand.Remove(command);
            //        DrawingVisual visual = footprint.visual;
            //        RemoveVisual(visual);
            //        commandFootprintsByVisual.Remove(visual);
            //        footprint.Disconnect();
            //    }
            //}

        }


        public void ClearScenarioObjectives()
        {

            List<DrawingVisual> removed = new List<DrawingVisual>();

            foreach (KeyValuePair<DrawingVisual, IFootprint> kvp in objectiveFootprintsByVisual)
            {
                ScenarioObjectiveFootprint footprint = kvp.Value as ScenarioObjectiveFootprint;
                if (footprint == null) continue;
                RemoveVisual(kvp.Key);
                kvp.Value.Disconnect();
                removed.Add(kvp.Key);
            }
            //footprintsByCommand.Clear();
            //commandFootprintsByVisual.Clear();
        }
        #endregion

        #region Echelon

        void scenario_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            Scenario scenario = (Scenario)sender;
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object obj in e.OldItems)
                {
                    ScenarioUnit unit = obj as ScenarioUnit;
                    if (obj == null)
                    {
                        Log.Error(this, "scenario_CollectionChanged unable to cleanup removed type " + obj);
                        continue;
                    }
                    ScenarioEchelon echelon = unit.scenarioEchelon;
                    if (echelon == null) continue;
                    EchelonFootprint footprint;
                    footprintsByEchelon.TryGetValue(echelon, out footprint);

                    if (footprint == null)
                    {
                        Log.Error(this, "scenario_CollectionChanged unable to find to remove type " + unit + " " + echelon);
                        continue;
                    }

                    footprintsByEchelon.Remove(echelon);
                    echelonFootprintsByVisual.Remove(footprint.visual);
                    RemoveVisual(footprint.visual);

                }
            }

            AddEchelon(scenario.root);
        }

        void AddEchelon(ScenarioEchelon echelon)
        {
            //Do children first so they draw below

            foreach (ScenarioEchelon child in echelon.children)
            {
                AddEchelon(child);
            }

            if (echelon.unit == null) return;

            if (footprintsByEchelon.ContainsKey(echelon)) return;

            DrawingVisual v = new DrawingVisual();

            EchelonFootprint echelonFootprint = new EchelonFootprint(echelon, v, this);
            AddVisual(v);
            echelonFootprintsByVisual[v] = echelonFootprint;
            footprintsByEchelon[echelon] = echelonFootprint;
        }

        //    ScenarioUnit unit = echelon.unit;
        //    unit.PropertyChanged += unit_PropertyChanged;
        //    ((INotifyPropertyChanged)echelon).PropertyChanged += echelon_PropertyChanged;

        //}

        //private void echelon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    ScenarioUnit unit = ((ScenarioEchelon)sender).unit;
        //    unit_PropertyChanged(unit, e);
        //}

        //private void unit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    ScenarioUnit unit = (ScenarioUnit)sender;

        //    //happens if root or other empty echelon changed
        //    if (unit == null) return;

        //    EchelonFootprint echelonFootprint = footprintsByEchelon[unit.scenarioEchelon];

        //    DrawingVisual v = echelonFootprint.visual;

        //    //update formation
        //    echelonFootprint.UpdateBounds();

        //    echelonFootprint.UpdateTransform();
        //    // Move and Rotate
        //    Matrix m1 = Matrix.Identity;
        //    m1.Rotate(unit.transform.facing);
        //    m1.Translate(unit.transform.east / mapToWorldFactor, unit.transform.south / mapToWorldFactor);

        //    //TODO make speed dependent on screen space travelled
        //    MatrixAnimation matrixAnimation = new MatrixAnimation(v.Transform.Value, m1,
        //        TimeSpan.FromMilliseconds(6)
        //    );
        //    v.Transform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);

        //    if (e.PropertyName == "formation")
        //    {
        //        ApplyFormationBrigade(unit.scenarioEchelon);
        //    }
        //}
        #endregion


        #region Clicks

        // http://msdn.microsoft.com/en-us/library/ms742859%28v=vs.110%29.aspx#Implementing_Drag_And_Drop
        protected void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (mode == MapMode.Rotate)
            {
                foreach (ScenarioEchelon echelon in unitSharedSelection)
                {
                    echelon.unit.transform.facing += (float)(e.Delta / 100.0);
                }
                return;
            }
            if (e.Delta > 1) Zoom(1.0 + e.Delta / 100.0);
            else if (e.Delta < 1) Zoom(1.0 / (1.0 + -e.Delta / 100.0));
        }

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

                case Key.D1:
                    mode = MapMode.Select;
                    newMode = true;
                    break;

                case Key.D2:
                    mode = MapMode.Move;
                    newMode = true;
                    break;

                case Key.D3:
                    mode = MapMode.Rotate;
                    newMode = true;
                    break;

                case Key.D4:
                    mode = MapMode.Aim;
                    newMode = true;
                    break;

                case Key.D5:
                    mode = MapMode.Measure;
                    newMode = true;
                    break;

                case Key.F:
                    foreach (ScenarioEchelon echelon in unitSharedSelection)
                    {
                        ApplyFormationBrigade(echelon);
                    }
                    break;

                case Key.C:
                    foreach (ScenarioEchelon echelon in unitSharedSelection)
                    {
                        Center(echelon);
                        break;
                    }
                    break;

                case Key.Space:
                    isPanning = true;
                    lastPanPosition = Mouse.GetPosition(window);
                    break;



            }

            if (newMode) InitializeModalTransform();

        }



        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!_isClicky)
                switch (e.Key)
                {
                    case Key.D2:
                    case Key.D3:
                    case Key.D4:
                    case Key.D5:
                        ScenarioUndoStack undoStack = SOWScenarioEditorWindow.GetScenarioUndoStack();
                        if (modal_undoStackWasActive)
                        {
                            undoStack.scenario_SaveBulkState("D5");
                            undoStack.active = true;
                        } 
                        mode = MapMode.Select;
                        break;


                    case Key.Space:
                        isPanning = false;
                        break;
                }
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

            if (!_isClicky || e.LeftButton == MouseButtonState.Pressed && mode != MapMode.Select)
            {

                Vector d = lastKeyDownMapPosition - e.GetPosition(this);

                switch (mode)
                {

                    case MapMode.Move:
                        d = mapToWorldTransform.Value.Transform(d);
                        foreach (ScenarioEchelon echelon in unitSharedSelection)
                        {
                            echelon.unit.transform.MoveBy(-d);
                            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            {
                                ApplyFormationBrigade(echelon);
                            }
                        }
                        lastKeyDownMapPosition = mapLocation;
                        return;

                    case MapMode.Aim:
                        foreach (ScenarioEchelon echelon in unitSharedSelection)
                        {

                            Point ep = GetMapPosition(echelon.unit.transform);
                            echelon.unit.transform.Face(mapLocation - ep);
                            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            {
                                ApplyFormationBrigade(echelon);
                            }
                        }
                        return;


                    case MapMode.Rotate:

                        d.Normalize();

                        MatrixTransform hmt = (MatrixTransform)rotateFeedbackVisual.Transform;
                        Matrix hm = Matrix.Identity;
                        double mapAngle = (Math.Atan2(d.X, -d.Y) + Math.PI) * 57.2957795;
                        CurrentInfo = mapAngle + " dgs";

                        hm.Rotate(mapAngle);
                        hm.OffsetX = lastKeyDownMapPosition.X;
                        hm.OffsetY = lastKeyDownMapPosition.Y;
                        hmt.Matrix = hm;

                        foreach (ScenarioEchelon echelon in unitSharedSelection)
                        {
                            echelon.unit.transform.facing = (float)mapAngle;
                            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            {
                                ApplyFormationBrigade(echelon);
                            }
                        }
                        return;

                    case MapMode.Measure:
                        //Console.WriteLine( d.Length/yardsToWorldTransform.ScaleX +"yds");
                        d = mapToWorldTransform.Value.Transform(d);
                        string str = string.Format(
                            "{0:0.0} yds",
                            d.Length / yardsToWorldTransform.ScaleX
                            );
                        CurrentInfo = str;
                        measureFeedback.Redraw(
                            lastKeyDownMapPosition,
                            e.GetPosition(this),
                            str
                            );
                        break;

                    default:
                        break;
                }
            }

            int imageX = (int)mapLocation.X;
            int imageY = (int)mapLocation.Y;
            byte? val = grayscaleImage.GetPixelGrayscale(imageX, imageY);
            //Console.WriteLine(imageX + "," + imageY +" "+grayscaleImage.PixelWidth+","+grayscaleImage.PixelHeight+ ": " + val);
            if (val != null)
            {
                CurrentTerrain = this._scenario.map.GetTerrainAtIndex((int)val);
                //Console.WriteLine("Terrain: " + terrain);
            }


            base.OnMouseMove(e);

        }

        void InitializeModalTransform()
        {

            ScenarioUndoStack undoStack = SOWScenarioEditorWindow.GetScenarioUndoStack();

            modal_undoStackWasActive = undoStack.active;
            if (undoStack.active)
            {
                //undoStack.scenario_SaveBulkState();

                undoStack.active = false;
            }

            switch (mode)
            {

                case MapMode.Select:
                    break;

                case MapMode.Move:
                    lastKeyDownMapPosition = GetMouseMapPosition();
                    break;

                case MapMode.Rotate:

                    lastKeyDownMapPosition = GetMouseMapPosition();
                    double mapAngle = 0;
                    if (unitSharedSelection.Count > 0)
                    {
                        Vector offset = 8 * unitSharedSelection[0].unit.transform.direction;
                        offset = new Vector(offset.Y, offset.X);
                        lastKeyDownMapPosition -= offset;
                        mapAngle = unitSharedSelection[0].unit.transform.facing;
                    }

                    MatrixTransform hmt = (MatrixTransform)rotateFeedbackVisual.Transform;
                    Matrix hm = Matrix.Identity;
                    hm.Rotate(mapAngle);
                    hm.OffsetX = lastKeyDownMapPosition.X;
                    hm.OffsetY = lastKeyDownMapPosition.Y;
                    hmt.Matrix = hm;
                    break;

                case MapMode.Aim:
                    lastKeyDownMapPosition = GetMouseMapPosition();
                    break;

                case MapMode.Measure:
                    lastKeyDownMapPosition = GetMouseMapPosition();
                    break;
            }
        }

        private Point GetMapPosition(WorldTransform worldTransform)
        {
            //return matrixTransform.Inverse.Transform(new Point(mapTransform.east, mapTransform.south));
            return new Point(worldTransform.east / mapToWorldFactor, worldTransform.south / mapToWorldFactor);
        }


        private Point GetWorldPosition(Point mapPosition)
        {
            //return matrixTransform.Inverse.Transform(new Point(mapTransform.east, mapTransform.south));
            return new Point(mapPosition.X * mapToWorldFactor, mapPosition.Y * mapToWorldFactor);
        }


        private Point GetMouseMapPosition()
        {
            //return matrixTransform.Inverse.Transform(new Point(mapTransform.east, mapTransform.south));
            return Mouse.GetPosition(this);
        }


        private Point GetMouseWorldPosition()
        {
            //return matrixTransform.Inverse.Transform(new Point(mapTransform.east, mapTransform.south));
            return GetWorldPosition(Mouse.GetPosition(this));
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Retrieve the mouse pointer location relative to this panel
            Point clickPosition = e.GetPosition(this);
            lastClickMapPosition = clickPosition;

            HitTestResult result = VisualTreeHelper.HitTest(this, clickPosition);

            EchelonFootprint echelonFootprint = null;
            if (!echelonFootprintsByVisual.TryGetValue(result.VisualHit as DrawingVisual, out echelonFootprint))
            {

                Point mapLocation = e.GetPosition(this);
                Vector d = lastKeyDownMapPosition - e.GetPosition(this);
                if (_isClicky) InitializeModalTransform();
                return;
            }

            ScenarioEchelon echelon = echelonFootprint.echelon;
            //mapSelection = new Selection<ScenarioEchelon, ScenarioUnit>();

            if (!((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                unitSharedSelection.Clear();
            }

            unitSharedSelection.Add(echelon);
            unitSharedSelection.selectionWorldPoint = GetWorldPosition(clickPosition);

            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(this, unitSharedSelection,
                DragDropEffects.All);


        }
        #endregion
        #region DrapDrop
        protected override void OnDragOver(DragEventArgs e)
        {
            Point mapPosition = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, mapPosition);

            EchelonFootprint hitechelon = null;
            //Dont allow drop onto others or self
            if (echelonFootprintsByVisual.TryGetValue(result.VisualHit as DrawingVisual, out hitechelon))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            //Console.WriteLine("OnDragOVer");
        }

        protected override void OnDrop(DragEventArgs e)
        {


            e.Effects = DragDropEffects.None;
            e.Handled = true;
            Point mapPosition = e.GetPosition(this);
            //ScenarioEchelon echelon = args.Data.GetData( typeof(ScenarioEchelon) )  as ScenarioEchelon;// = window.globalDraggedItem as ScenarioEchelon;

            EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> echelonSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(e.Data as DataObject);

            if (echelonSelection != null)
            {

                HitTestResult result = VisualTreeHelper.HitTest(this, mapPosition);

                EchelonFootprint hitechelon = null;
                //Dont allow drop onto others or self
                if (echelonFootprintsByVisual.TryGetValue(result.VisualHit as DrawingVisual, out hitechelon))
                {
                    return;
                }

                Vector worldOffset = new Vector(); // 0 unless set
                foreach (ScenarioEchelon echelon in echelonSelection)
                {
                    if (echelon.unit == null) continue;
                    if (echelonSelection.selectionWorldPoint != null)
                    {
                        worldOffset = echelon.unit.transform.point - (Point)echelonSelection.selectionWorldPoint;
                    }
                    echelon.unit.transform.SetPosition(
                        (float)(mapPosition.Y * mapToWorldFactor + worldOffset.Y),
                        (float)(mapPosition.X * mapToWorldFactor + worldOffset.X)
                    );

                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        ApplyFormationBrigade(echelon);
                    }
                }
                echelonSelection.selectionWorldPoint = null;

                unitSharedSelection = echelonSelection;

                Console.WriteLine("MapPanel Dropped " + echelonSelection + " on " + mapPosition + " " + e.OriginalSource);
                return;
            }


            EventSelectionSet eventSelection = EventSelectionSet.ExtractFromDataObject(e.Data as DataObject);
            if (eventSelection != null)
            {
                foreach (BattleScriptEvent bEvent in eventSelection)
                {
                    if (bEvent is IHasPosition)
                    {
                        //Point location = ((ILocationArg)aevent).location;
                        //= (float)(mapPosition.Y * mapToWorldFactor);
                        ((IHasPosition)bEvent).position.SetPosition(
                            (float)(mapPosition.Y * mapToWorldFactor),
                            (float)(mapPosition.X * mapToWorldFactor)
                            );


                    }

                    if (bEvent is IHasCommand)
                    {
                        Command command = ((IHasCommand)bEvent).command;
                        if (command is IHasPosition)
                        {
                            ((IHasPosition)command).position.SetPosition(
                                 (float)(mapPosition.Y * mapToWorldFactor),
                                 (float)(mapPosition.X * mapToWorldFactor)
                                 );
                        }
                    }

                }
                return;
            }


            ScenarioObjectiveSelectionSet objectiveSelection = ScenarioObjectiveSelectionSet.ExtractFromDataObject(e.Data as DataObject);
            if (objectiveSelection != null)
            {
                foreach (ScenarioObjective objective in objectiveSelection)
                {
                    objective.SetPosition(
                        (float)(mapPosition.Y * mapToWorldFactor),
                        (float)(mapPosition.X * mapToWorldFactor)
                        );
                }
                return;
            }


            Console.WriteLine("[MapPanel] Dropped Invalid \"" + e.Data.GetData(typeof(object)) + "\" on " + e.OriginalSource);
            return;

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
            counterScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            counterScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
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

        //internal void Center(IObjective item)
        //{
        //    if (item == null) return;
        //    Center(item);
        //}


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

        bool _isClicky;
        public bool IsClicky
        {
            get { return _isClicky; }
            set
            {
                _isClicky = value;
                mode = MapMode.Select;
                OnPropertyChanged("IsClicky");
            }
        }
        //,Notes,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"1 - flagbearer, 2-300 men",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"special situations (row distance - column distance - sprite - facing - subform - subtype), must precede man number",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"Place a '+' after row or column distance to have the distance dependent on reg size, the distance that you add will be between reg's",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"sprite is an int and indexes into the unitglobal.csv file as to what specific sprite to use, leave 0 for default, valid values are currently 1-6",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,facing is an integer specifying the number of degrees that the unit should face off of the flag bearer,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"subform is the id of the sub formation to use for this slot, if it's not set, it uses the default for the formation",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"subtype is a number of the unit type to fill in for this slot (really only useful for div level formations), 1-Inf,2-Cav,3-Art, leave blank or zero for any",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"you only have to specify these if that specific slot uses a value, otherwise it is not necessary",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //,"you only have to include up to the value you need, so if you need a 10 row distance and all else is default you just need to do (10)",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,

        //Name,ID,Rows,Columns,RowDist,ColDist,SubForm,KeepForm,CanWheel,CanFight,MoveRateMod,AboutFace,ArtyForm,MinEnemy,FireMod,MeleeMod,Can'tMove,CantCounterCharge,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,

        //Level 5 Infantry Line (8),DRIL_Lvl5_Inf_Line,3,14,10,4.4+,DRIL_Lvl6_Inf_Line,1,1,0,-0.2,0,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //15,13,11,9,7,5,3    ,4,6,8,10,12,14,16,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //  , ,  ,  , , ,(10)2,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //  , ,  ,  , , ,1    ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,


        public void ApplyFormationBrigade(ScenarioEchelon echelon)
        {

            EchelonFootprint footprint = footprintsByEchelon[echelon];

            Formation formation = echelon.unit.formation;

            if (formation == null) return;

            //Console.WriteLine("Setting " + echelon.id + " to " + formation.id);

            // NB this will not apply the subformations, that is done below

            ScenarioUndoStack undoStack = SOWScenarioEditorWindow.GetScenarioUndoStack();
            bool undoStackWasActive = undoStack.active;
            if (undoStack.active)
            {
                //undoStack.scenario_SaveBulkState("preformation");
                undoStack.active = false;
            }


            Rect bounds = formation.BridageComputeChildPositionsYds(
                echelon, out footprint.formationLocations);


            // Leaves
            for (int childIndex = 0; childIndex < echelon.children.Count; childIndex++)
            {
                ScenarioEchelon child = (ScenarioEchelon)echelon.children[childIndex];

                if (child.unit == null) continue;

                int pIndex = childIndex + 2;
                if (pIndex >= footprint.formationLocations.Length)
                {
                    string messageBoxText = "Unable to move \"" + child.unit.id + "\" into position in formation \"" + formation.id + "\" as there is no slot " + pIndex + " for it in formation of size " + footprint.formationLocations.Length;
                    Log.Error(formation, messageBoxText);
                    string caption = "Formation Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    MessageBox.Show(messageBoxText, caption, button, icon);
                    continue;
                }

                Point p = footprint.formationLocations[childIndex + 2].position;
                p = yardsToWorldTransform.Transform(p);
                p = echelon.unit.transform.GetWorldMatrix().Transform(p);

                Vector dir = footprint.formationLocations[childIndex + 2].direction;
                dir = echelon.unit.transform.GetRotationMatrix().Transform(dir);

                Formation childFormation = footprint.formationLocations[childIndex + 2].formation;

                if (child.unit.formation != null && childFormation.level != child.unit.formation.level)
                {
                    Log.Warn(this, String.Format("May not change formation level {0} from {1} ({2}) to {3} ({4})",
                        child.unit.id, child.unit.formation.level, child.unit.formation.id, childFormation.level, childFormation.id));
                }
                else
                {
                    //Console.WriteLine("Changin Child" + child.id + " from " + child.unit.formation.id + " to " + childFormation);
                    child.unit.formation = childFormation;
                }

                child.unit.transform.Set(
                    //(float)dir.Y, (float) dir.X,
                     (float)dir.Y, (float)dir.X,
                     (float)p.Y, (float)p.X
                     );

                if (child.unit.formation != null && child.unit.formation.level > 0)
                {
                    ApplyFormationBrigade(child);
                }


            }

            if (undoStackWasActive)
            {
                undoStack.scenario_SaveBulkState("postformation");
                undoStack.active = true;
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private ScaleTransform mapToWorldTransform;

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

        Terrain _currentTerrain;
        public Terrain CurrentTerrain
        {
            get { return _currentTerrain; }
            set
            {
                _currentTerrain = value;
                OnPropertyChanged("CurrentTerrain");
            }
        }

        string _currentInfo;
        public string CurrentInfo
        {
            get { return _currentInfo; }
            set
            {
                _currentInfo = value;
                OnPropertyChanged("CurrentInfo");
            }
        }

        string _selectionInfo;
        public string SelectionInfo
        {
            get { return _selectionInfo; }
            set
            {
                _selectionInfo = value;
                OnPropertyChanged("SelectionInfo");
            }
        }

        internal void HideAllUnits()
        {
            foreach (Visual v in echelonFootprintsByVisual.Keys)
            {
                if (visuals.Contains(v))
                {
                    RemoveVisual(v);
                }
            }
        }

        internal void ShowAllUnits()
        {
            foreach (Visual v in echelonFootprintsByVisual.Keys)
            {
                if (!visuals.Contains(v))
                {
                    AddVisual(v);
                }
            }
        }

        internal void HideAllUnitsOfSide(int side)
        {
            foreach (KeyValuePair<ScenarioEchelon, EchelonFootprint> kvp in footprintsByEchelon)
            {
                if (kvp.Key.sideIndex != side) continue;
                if (visuals.Contains(kvp.Value.visual))
                {
                    RemoveVisual(kvp.Value.visual);
                }
            }
        }

        internal void ShowAllUnitsOfSide(int side)
        {
            foreach (KeyValuePair<ScenarioEchelon, EchelonFootprint> kvp in footprintsByEchelon)
            {
                if (kvp.Key.sideIndex != side) continue;
                if (!visuals.Contains(kvp.Value.visual))
                {
                    AddVisual(kvp.Value.visual);
                }
            }
        }

        internal void HideAllCommands()
        {
            foreach (Visual v in commandFootprintsByVisual.Keys)
            {
                if (visuals.Contains(v))
                {
                    RemoveVisual(v);
                }
            }
        }

        internal void ShowAllCommands()
        {
            foreach (Visual v in commandFootprintsByVisual.Keys)
            {
                if (!visuals.Contains(v))
                {
                    AddVisual(v);
                }
            }
        }


        internal void HideAllObjectivesCommands()
        {
            foreach (Visual v in objectiveFootprintsByVisual.Keys)
            {
                if (visuals.Contains(v))
                {
                    RemoveVisual(v);
                }
            }
        }

        internal void ShowAllObjectivesCommands()
        {
            foreach (Visual v in objectiveFootprintsByVisual.Keys)
            {
                if (!visuals.Contains(v))
                {
                    AddVisual(v);
                }
            }
        }


        internal void ToggleVisibility(System.Collections.ICollection items)
        {
            foreach (object item in items)
            {
                IHasCommand bEvent = item as IHasCommand;
                if (bEvent != null)
                {
                    ToggleVisibility(item);
                }
            }
        }

        internal void ToggleVisibility(object item)
        {

            BattleScriptEvent bEvent = item as BattleScriptEvent;
            if (bEvent == null) return;


            if (footprintsByEvent.ContainsKey(bEvent))
            {
                UnitPositionEventFootprint footprint = footprintsByEvent[bEvent];
                if (visuals.Contains(footprint.visual))
                {
                    RemoveVisual(footprint.visual);
                }
                else
                {
                    AddVisual(footprint.visual);
                }
            }



            IHasCommand ihcEvent = item as IHasCommand;
            if (ihcEvent != null)
            {
                UnitMoveToCommand command = ihcEvent.command as UnitMoveToCommand;
                if (command != null && footprintsByCommand.ContainsKey(command))
                {
                    UnitMoveToCommandFootprint footprint = footprintsByCommand[command];
                    if (visuals.Contains(footprint.visual))
                    {
                        RemoveVisual(footprint.visual);
                    }
                    else
                    {
                        AddVisual(footprint.visual);
                    }
                }
            }




        }

        internal static void TextOutline(string text, out Geometry textGeo, out Geometry textOutlineGeo)
        {
            //if (Bold == true) ;
            FontWeight fontWeight = FontWeights.Bold;
            //if (Italic == true)
            FontStyle fontStyle = FontStyles.Normal;

            // Create the formatted text based on the properties set.
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                defaultTypeface,
                10,
                System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
                );

            textGeo = formattedText.BuildGeometry(new System.Windows.Point(0, 0));
            textOutlineGeo = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));
        }
    }

    interface IFootprint
    {
        void Disconnect();
        void Redraw();
        DrawingVisual visual { get; set;}
    }

    class EchelonFootprint : IFootprint
    {
        public ScenarioEchelon echelon;
        public DrawingVisual visual {get; set;}
        public GeometryDrawing bounds;
        public SolidColorBrush hiliteBrush;
        MapPanel mapPanel;
        public Matrix xform;
        public Formation.Location[] formationLocations;



        SolidColorBrush fillBrush;
        Pen pen;
        Color color;

        public EchelonFootprint(ScenarioEchelon echelon, DrawingVisual visual, MapPanel mapPanel)
        {
            this.echelon = echelon;
            this.visual = visual;
            this.bounds = new GeometryDrawing();
            this.mapPanel = mapPanel;
            CreateVisual();

            ScenarioUnit unit = echelon.unit;
            unit.PropertyChanged += unit_PropertyChanged;
            ((INotifyPropertyChanged)echelon).PropertyChanged += echelon_PropertyChanged;

        }

        public void Disconnect()
        {
            echelon.unit.PropertyChanged -= unit_PropertyChanged;
            ((INotifyPropertyChanged)echelon).PropertyChanged -= echelon_PropertyChanged;
        }

        private void echelon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Redraw();

        }

        private void unit_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Redraw();
            if (e.PropertyName == "formation" && SOWScenarioEditorWindow.GetScenarioUndoStack().active == true)
            {
                mapPanel.ApplyFormationBrigade(echelon);
            }
        }

        public void Redraw()
        {
            ScenarioUnit unit = echelon.unit;

            //happens if root or other empty echelon changed
            if (unit == null) return;

            //update formation
            UpdateBounds();

            UpdateTransform();

            // Move and Rotate
            Matrix m1 = Matrix.Identity;
            m1.Rotate(unit.transform.facing);
            m1.Translate(unit.transform.east / mapPanel.mapToWorldFactor, unit.transform.south / mapPanel.mapToWorldFactor);

            //TODO make speed dependent on screen space travelled
            MatrixAnimation matrixAnimation = new MatrixAnimation(visual.Transform.Value, m1,
                TimeSpan.FromMilliseconds(6)
            );
            visual.Transform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);


        }



        void CreateVisual()
        {
            ScenarioUnit unit = (ScenarioUnit)echelon.unit;

            //fixed this by creating as Drawing then using Drawing reference ToleranceType adjust bound
            //unit.ComputeBounds();
            int colorIndex = (int)(unit.echelon.sideIndex);
            color = mapPanel.sideColors[colorIndex];
            Color transpColor = Color.Subtract(color, Color.FromArgb(100, 0, 0, 0));

            hiliteBrush = new SolidColorBrush(color);
            fillBrush = new SolidColorBrush(transpColor);
            pen = new Pen(new SolidColorBrush(color), 4);

            UpdateBounds();

            using (DrawingContext dc = visual.RenderOpen())
            {
                // bounds 
                dc.PushTransform(mapPanel.yardsToMapTransform);
                dc.DrawDrawing(bounds);
                dc.Pop();

                // marker
                dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawGeometry(
                    hiliteBrush,
                    new Pen(Brushes.Black, 1),
                    Geometry.Parse(@"M 0,0 L 4,8 L -4,8 Z")
                 );
                dc.Pop();
            }

            visual.Transform = new MatrixTransform();
            xform = Matrix.Identity;
            xform.Rotate(echelon.unit.transform.facing);
            xform.Translate(unit.transform.east / mapPanel.mapToWorldFactor, unit.transform.south / mapPanel.mapToWorldFactor);
            visual.Transform = new MatrixTransform(xform);
        }

        internal void UpdateBounds()
        {

            if (echelon.unit.formation == null) return;
            UpdateBounds(
                echelon.unit.formation.ComputeSpritePositionsYds(
                echelon, out formationLocations
                )
            );
        }

        internal void UpdateBounds(Rect rect)
        {
            bounds.Geometry = new RectangleGeometry(rect);
            bounds.Brush = fillBrush;
            bounds.Pen = pen;
        }

        internal void ResetBrush()
        {
            hiliteBrush.Color = color;
        }

        internal void UpdateTransform()
        {
            xform = Matrix.Identity;
            xform.Rotate(echelon.unit.transform.facing);
            xform.Translate(echelon.unit.transform.east / mapPanel.mapToWorldFactor, echelon.unit.transform.south / mapPanel.mapToWorldFactor);

            //TODO make speed dependent on screen space travelled
            MatrixAnimation matrixAnimation = new MatrixAnimation(visual.Transform.Value, xform,
                TimeSpan.FromMilliseconds(6)
            );
            visual.Transform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);

        }

    }

    abstract internal class ObjectiveFootprint<T> : IFootprint where T : IObjective
    {
        public T objective;
        public DrawingVisual visual { get; set; }
        public GeometryDrawing bounds;
        public SolidColorBrush hiliteBrush;
        public Matrix xform;

        public MapPanel mapPanel;

        protected SolidColorBrush fillBrush;
        protected Pen pen;
        protected Color color;

        internal ObjectiveFootprint(T objective, DrawingVisual visual, MapPanel mapPanel)
        {
            this.objective = objective;
            this.visual = visual;
            this.bounds = new GeometryDrawing();
            this.mapPanel = mapPanel;
            CreateVisual();
            if (this.objective != null) this.objective.PropertyChanged += this.objective_PropertyChanged;

        }

        private void objective_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "id")
                CreateVisual();

            Redraw();
        }

        public void Disconnect()
        {
            if (this.objective != null) this.objective.PropertyChanged -= this.objective_PropertyChanged;

        }


        public virtual void Redraw()
        {
            UpdateTransform();
        }

        abstract protected void CreateVisual();

        internal void ResetBrush()
        {
            hiliteBrush.Color = color;
        }

        internal void UpdateTransform()
        {
            xform = Matrix.Identity;
            xform.Translate(objective.east / mapPanel.mapToWorldFactor, objective.south / mapPanel.mapToWorldFactor);

            //TODO make speed dependent on screen space travelled
            MatrixAnimation matrixAnimation = new MatrixAnimation(visual.Transform.Value, xform,
                TimeSpan.FromMilliseconds(6)
            );
            visual.Transform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);

        }



    }

    internal class MapObjectiveFootprint : ObjectiveFootprint<MapObjective>
    {

        internal MapObjectiveFootprint(MapObjective objective, DrawingVisual visual, MapPanel mapPanel) : base(objective, visual, mapPanel) { }

        override protected void CreateVisual()
        {


            color = Colors.Yellow;
            Color transpColor = Color.Subtract(color, Color.FromArgb(100, 0, 0, 0));

            hiliteBrush = new SolidColorBrush(color);
            fillBrush = new SolidColorBrush(transpColor);
            pen = new Pen(Brushes.Black, 1);

            using (DrawingContext dc = visual.RenderOpen())
            {
                // bounds 
                dc.PushTransform(mapPanel.yardsToMapTransform);
                dc.DrawDrawing(bounds);
                dc.Pop();

                // marker
                dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawEllipse(
                    hiliteBrush,
                    pen,
                    new Point(0, 0),
                    4, 4
                    );
                dc.DrawLine(pen, new Point(-8, -8), new Point(8, 8));
                dc.DrawText(
                    new FormattedText(objective.id,
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        MapPanel.defaultTypeface,
                        10, MapPanel.textBrush, null),
                    new Point(0, 0));

                dc.Pop();
            }

            visual.Transform = new MatrixTransform();
            xform = Matrix.Identity;
            xform.Translate(objective.east / mapPanel.mapToWorldFactor, objective.south / mapPanel.mapToWorldFactor);
            visual.Transform = new MatrixTransform(xform);
        }
    }

    internal class FortObjectiveFootprint : ObjectiveFootprint<Fort>
    {

        internal FortObjectiveFootprint(Fort objective, DrawingVisual visual, MapPanel mapPanel) : base(objective, visual, mapPanel) { }

        static Geometry pentagon;
        static FortObjectiveFootprint()
        {
            PathFigure pathFigure = new PathFigure();

            Point[] points = new Point[6];

            int c = 0;
            for (int i = 0; i < points.Length; i++)
            {
                double ang = Math.PI / -2.0 + (2 * Math.PI * i / (double)(points.Length - 1));
                double r = 8;
                double x = Math.Cos(ang) * r;
                double y = Math.Sin(ang) * r;

                points[i] = new Point(x, y);
            }

            pathFigure.StartPoint = new Point(0, 0);

            PolyLineSegment myPolyLineSegment = new PolyLineSegment(points, true);
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(myPolyLineSegment);
            pathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(pathFigure);
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            myPathGeometry.FillRule = FillRule.EvenOdd;

            pentagon = myPathGeometry;
        }

        override protected void CreateVisual()
        {


            color = Colors.DarkOrange;
            Color transpColor = Color.Subtract(color, Color.FromArgb(100, 0, 0, 0));

            hiliteBrush = new SolidColorBrush(color);
            fillBrush = new SolidColorBrush(transpColor);
            Brush blackBrush = new SolidColorBrush(Colors.Black);
            pen = new Pen(Brushes.DarkRed, 2);
            pen.EndLineCap = PenLineCap.Round;
            pen.LineJoin = PenLineJoin.Round;

            Geometry textOutline;
            Geometry text;
            MapPanel.TextOutline(objective.id, out text, out textOutline);

            using (DrawingContext dc = visual.RenderOpen())
            {
                // marker
                dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawGeometry(fillBrush, null, textOutline);
                dc.DrawGeometry(hiliteBrush, pen, pentagon);
                dc.DrawGeometry(blackBrush, null, text);
                dc.Pop();
            }

            visual.Transform = new MatrixTransform();
            xform = Matrix.Identity;
            xform.Translate(objective.east / mapPanel.mapToWorldFactor, objective.south / mapPanel.mapToWorldFactor);
            visual.Transform = new MatrixTransform(xform);
        }
    }

    internal class ScenarioObjectiveFootprint : ObjectiveFootprint<ScenarioObjective>
    {

        internal ScenarioObjectiveFootprint(ScenarioObjective objective, DrawingVisual visual, MapPanel mapPanel) : base(objective, visual, mapPanel) { }

        static Geometry star;
        PathGeometry circle;

        static ScenarioObjectiveFootprint()
        {
            Star();
        }

        static void Star()
        {
            PathFigure pathFigure = new PathFigure();

            Point[] points = new Point[11];

            for (int i = 0; i < points.Length; i++)
            {
                double ang = Math.PI / -2.0 + (2 * Math.PI * i / (double)(points.Length - 1));
                double r = (i % 2 == 0) ? 12 : 8;
                double x = Math.Cos(ang) * r;
                double y = Math.Sin(ang) * r;

                points[i] = new Point(x, y);
            }

            pathFigure.StartPoint = new Point(0, 0);

            PolyLineSegment myPolyLineSegment = new PolyLineSegment(points, true);
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(myPolyLineSegment);
            pathFigure.Segments = myPathSegmentCollection;


            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(pathFigure);
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            myPathGeometry.FillRule = FillRule.EvenOdd;

            star = myPathGeometry;
        }

        PathGeometry RadiusCircle()
        {
            PathFigure pathFigure = new PathFigure();



            Point[] points = new Point[17];
            double r = objective.radius * mapPanel.yardsToMapTransform.ScaleX;
            for (int i = 0; i < points.Length; i++)
            {
                double ang = Math.PI / -2.0 + (2 * Math.PI * i / (double)(points.Length - 1));
                double x = Math.Cos(ang) * r;
                double y = Math.Sin(ang) * r;

                points[i] = new Point(x, y);
            }

            pathFigure.StartPoint = new Point(0, 0);

            PolyLineSegment myPolyLineSegment = new PolyLineSegment(points, true);
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(myPolyLineSegment);
            pathFigure.Segments = myPathSegmentCollection;


            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(pathFigure);
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            myPathGeometry.FillRule = FillRule.EvenOdd;

            return myPathGeometry;
        }


        override protected void CreateVisual()
        {

            color = Colors.Gold;
            Color transpColor = Color.Subtract(color, Color.FromArgb(100, 0, 0, 0));

            hiliteBrush = new SolidColorBrush(color);
            fillBrush = new SolidColorBrush(transpColor);
            Brush blackBrush = new SolidColorBrush(Colors.Black);
            Pen textPen = new Pen(hiliteBrush, .5);

            pen = new Pen(Brushes.Black, 1);
            pen.EndLineCap = PenLineCap.Round;
            pen.LineJoin = PenLineJoin.Round;

            Geometry textOutline;
            Geometry text;
            MapPanel.TextOutline(objective.name, out text, out textOutline);

            circle = RadiusCircle();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawGeometry(fillBrush, pen, circle);

                // marker
                dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawGeometry(fillBrush, null, textOutline);

                dc.DrawGeometry(hiliteBrush, pen, star);
                dc.DrawGeometry(blackBrush, null, text);
                dc.Pop();
            }

            visual.Transform = new MatrixTransform();
            xform = Matrix.Identity;
            xform.Translate(objective.east / mapPanel.mapToWorldFactor, objective.south / mapPanel.mapToWorldFactor);
            visual.Transform = new MatrixTransform(xform);
        }

        public override void Redraw()
        {
            CreateVisual();
            base.Redraw();
        }
    }

    class UnitMoveToCommandFootprint : IFootprint
    {
        public UnitMoveToCommand bCommand;
        public DrawingVisual visual { get; set; }
        public GeometryDrawing bounds;
        public SolidColorBrush hiliteBrush;
        MapPanel mapPanel;
        public Matrix xform;
        public IHasPosition previousIHasPosition;

        //internal Point from, to;

        StreamGeometry line;

        SolidColorBrush fillBrush;
        Pen pen;
        Color color = Color.FromArgb(150, 255, 140, 0);


        public UnitMoveToCommandFootprint(UnitMoveToCommand bCommand, DrawingVisual visual, MapPanel mapPanel, IHasPosition previousIHasPosition)
        {
            this.bCommand = bCommand;
            this.visual = visual;
            this.bounds = new GeometryDrawing();
            this.mapPanel = mapPanel;
            this.previousIHasPosition = previousIHasPosition;
            fillBrush = new SolidColorBrush(color);
            CreateVisual();

            this.bCommand.PropertyChanged += this.unitMoveToCommand_PropertyChanged;
            if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged += unitMoveToCommand_PropertyChanged;
        }

        public void Disconnect()
        {
            this.bCommand.PropertyChanged -= this.unitMoveToCommand_PropertyChanged;
            if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged -= unitMoveToCommand_PropertyChanged;
        }

        private void unitMoveToCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Redraw();
        }

        internal void Update(IHasPosition previousIHasPosition)
        {
            if (this.previousIHasPosition != previousIHasPosition)
            {
                if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged -= unitMoveToCommand_PropertyChanged;
                this.previousIHasPosition = previousIHasPosition;
                if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged += unitMoveToCommand_PropertyChanged;
                Redraw();
            }
        }

        void CreateVisual()
        {

            pen = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), 1);
            //pen.DashStyle = DashStyles.Dash;
            pen.EndLineCap = PenLineCap.Round;

            line = new StreamGeometry();
            Redraw();

            using (DrawingContext dc = visual.RenderOpen())
            {
                //dc.DrawGeometry(Brushes.Orange, pen, line);
                dc.DrawGeometry(fillBrush, pen, line);

            }
        }

        internal void ResetBrush()
        {
            hiliteBrush.Color = color;
        }

        public void Redraw()
        {
            line.Clear();

            if (previousIHasPosition == null || previousIHasPosition.position == null || bCommand == null || bCommand.position == null) return;

            Point from = new Point(
            previousIHasPosition.position.east / mapPanel.mapToWorldFactor,
            previousIHasPosition.position.south / mapPanel.mapToWorldFactor
            );

            Point to = new Point(
                  bCommand.position.east / mapPanel.mapToWorldFactor,
                  bCommand.position.south / mapPanel.mapToWorldFactor
                  );


            Vector dir = to - from;
            dir.Normalize();
            Vector cross = new Vector(dir.Y, -dir.X);


            Point a = new Point(
                       to.X - dir.X * 4 + cross.X * 4,
                        to.Y - dir.Y * 4 + cross.Y * 4
                        );

            Point b = new Point(
                    to.X - dir.X * 4 - cross.X * 4,
                     to.Y - dir.Y * 4 - cross.Y * 4
                     );


            using (StreamGeometryContext geometryContext = line.Open())
            {
                geometryContext.BeginFigure(from, true, true);
                PointCollection points = new PointCollection { a, to, b };
                geometryContext.PolyLineTo(points, true, true);

            }

        }


    }


    class UnitPositionEventFootprint : IFootprint
    {
        public UnitPositionEvent bEvent;
        public DrawingVisual visual { get; set; }
        public GeometryDrawing bounds;
        public SolidColorBrush hiliteBrush;
        MapPanel mapPanel;
        public Matrix xform;
        public IHasPosition previousIHasPosition;

        //internal Point from, to;

        StreamGeometry line;

        SolidColorBrush fillBrush;
        Pen pen;
        Color color = Color.FromArgb(0, 255, 0, 0);


        public UnitPositionEventFootprint(UnitPositionEvent bEvent, DrawingVisual visual, MapPanel mapPanel, IHasPosition previousIHasPosition)
        {
            this.bEvent = bEvent;
            this.visual = visual;
            this.bounds = new GeometryDrawing();
            this.mapPanel = mapPanel;
            this.previousIHasPosition = previousIHasPosition;
            fillBrush = new SolidColorBrush(color);
            CreateVisual();

            this.bEvent.PropertyChanged += this.unitMoveToCommand_PropertyChanged;
            if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged += unitMoveToCommand_PropertyChanged;
        }

        public void Disconnect()
        {
            this.bEvent.PropertyChanged -= this.unitMoveToCommand_PropertyChanged;
            if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged -= unitMoveToCommand_PropertyChanged;
        }

        private void unitMoveToCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Redraw();
        }

        internal void Update(IHasPosition previousIHasPosition)
        {
            if (this.previousIHasPosition != previousIHasPosition)
            {
                if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged -= unitMoveToCommand_PropertyChanged;
                this.previousIHasPosition = previousIHasPosition;
                if (this.previousIHasPosition != null) this.previousIHasPosition.PropertyChanged += unitMoveToCommand_PropertyChanged;
                Redraw();
            }
        }

        void CreateVisual()
        {

            pen = new Pen(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), 1);
            pen.EndLineCap = PenLineCap.Round;

            line = new StreamGeometry();
            Redraw();

            using (DrawingContext dc = visual.RenderOpen())
            {
                //dc.DrawGeometry(Brushes.Orange, pen, line);
                dc.DrawGeometry(fillBrush, pen, line);
            }
        }

        internal void ResetBrush()
        {
            hiliteBrush.Color = color;
        }

        public void Redraw()
        {
            line.Clear();

            if (previousIHasPosition == null || previousIHasPosition.position == null || bEvent == null || bEvent.position == null) return;

            Point from = new Point(
            previousIHasPosition.position.east / mapPanel.mapToWorldFactor,
            previousIHasPosition.position.south / mapPanel.mapToWorldFactor
            );

            Point to = new Point(
                  bEvent.position.east / mapPanel.mapToWorldFactor,
                  bEvent.position.south / mapPanel.mapToWorldFactor
                  );


            Vector dir = to - from;
            dir.Normalize();
            Vector cross = new Vector(dir.Y, -dir.X);


            Point a = new Point(
                       to.X - dir.X * 4 + cross.X * 4,
                        to.Y - dir.Y * 4 + cross.Y * 4
                        );

            Point b = new Point(
                    to.X - dir.X * 4 - cross.X * 4,
                     to.Y - dir.Y * 4 - cross.Y * 4
                     );


            using (StreamGeometryContext geometryContext = line.Open())
            {
                geometryContext.BeginFigure(from, true, true);
                PointCollection points = new PointCollection { a, to, b };
                geometryContext.PolyLineTo(points, true, true);

            }

        }


    }

    class MeasureFeedback
    {

        public DrawingVisual visual { get; set; }
        MapPanel mapPanel;
        internal MeasureFeedback(DrawingVisual visual, MapPanel mapPanel)
        {
            this.visual = visual;
            this.mapPanel = mapPanel;

        }

        public void Redraw(Point from, Point to, string str)
        {

            Geometry textOutline;
            Geometry text;
            MapPanel.TextOutline(str, out text, out textOutline);


            //dc.PushTransform(mapPanel.counterScaleTransform);
            //dc.DrawGeometry(fillBrush, null, textOutline);
            //dc.DrawGeometry(hiliteBrush, pen, pentagon);
            //dc.DrawGeometry(blackBrush, null, text);
            //dc.Pop();

            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawLine(new Pen(Brushes.Yellow, 4),
                    from, to);
                dc.PushTransform(new TranslateTransform(to.X, to.Y - 10));
                dc.PushTransform(mapPanel.counterScaleTransform);
                dc.DrawGeometry(Brushes.Black, null, text);
                dc.Pop();
                dc.Pop();

            }
            visual.Transform = new MatrixTransform();
        }

    }

    static class SOWExtensions
    {

        public static Matrix GetWorldMatrix(this WorldTransform worldTransform)
        {
            Matrix m = new Matrix();
            m.Rotate(worldTransform.facing);
            m.OffsetX = worldTransform.east;
            m.OffsetY = worldTransform.south;
            return m;
        }

        public static Matrix GetRotationMatrix(this WorldTransform worldTransform)
        {
            Matrix m = new Matrix();
            m.Rotate(worldTransform.facing);
            return m;
        }

        public static void TransformBy(this WorldTransform worldTransform, WorldTransform transformBy)
        {
            // TODO not really working for rotationed, just for testing
            worldTransform.south += transformBy.south;
            worldTransform.east += transformBy.east;
        }


    }

}

