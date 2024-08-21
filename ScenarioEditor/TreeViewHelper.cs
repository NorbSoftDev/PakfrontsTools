using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorbSoftDev.SOW;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace ScenarioEditor
{
    class TreeViewHelper<T, R>
        where T : EchelonGeneric<R>
        where R : IUnit
    {
        SOWScenarioEditorWindow window;
        TreeView treeView;

        Point _lastMouseDown;

        List<TreeViewItem> _markedTreeItems = new List<TreeViewItem>();


        internal EchelonSelectionSet<T, R> sharedEchelonSelection
        {
            get
            {
                return _sharedEchelonSelection;
            }

            set
            {
                _sharedEchelonSelection = value;
                _sharedEchelonSelection.CollectionChanged += sharedSelection_CollectionChanged;
            }
        }
        EchelonSelectionSet<T, R> _sharedEchelonSelection;
        private bool isScenarioTree;


        private void sharedSelection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ExpandAndMark(_sharedEchelonSelection);
        }

        public void ExpandAndMark(IList<T> echelons) {
             //unmark unused items
            if (_markedTreeItems.Count > 0)
            {
                foreach (TreeViewItem item in _markedTreeItems)
                {
                    IEchelon echelon = item.ItemsSource as IEchelon;
                    if (echelon != null) item.Background = RankToBrushConverter.RankToBrush(echelon);
                    else item.Background = treeView.Background;
                }
            }

            // mark selected items



                List<T> itemsToMark = new List<T>();
                foreach (T echelon in echelons)
                {
                    itemsToMark.Add(echelon);
                }
                _markedTreeItems.Clear(); //shud not be needed

                ExpandAndFindItems(treeView, itemsToMark, _markedTreeItems);
                foreach (TreeViewItem item in _markedTreeItems)
                {
                    item.Background = Brushes.Yellow;
                }
            
        }

        public TreeViewHelper(SOWScenarioEditorWindow window, TreeView treeView, bool isScenarioTree)
        {
            this.window = window;
            this.treeView = treeView;
            GetScrollViewer();
            this.isScenarioTree = isScenarioTree;
            sharedEchelonSelection = new EchelonSelectionSet<T, R>();
        }

        public void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            T echelon = (T)e.NewValue;

            // This seems to fire repeatedly, due to the why searching the vivual tree tickles the SelectedItems
            //List<T> newSelection = new List<T>();

            //newSelection.Add(echelon);

            //if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            //{
            //    foreach (T child in echelon.children)
            //    {
            //        newSelection.Add(child);
            //    }
            //}

            //if (!((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            //{
            //    sharedSelection.Clear();
            //}

            //sharedSelection.AddRange(newSelection);

            if (echelon == null) return;
            R unit = echelon.unit;
            if (unit == null) return;
            window.SelectedUnitChanged(unit.id);

        }


        // apparently this is a hacky way and its better to use a TreeViewModel:
        // http://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode
        // http://dotnet-experience.blogspot.com/2011/04/wpf-treeview-drag-n-drop.html


        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point currentPosition = e.GetPosition(treeView);

            if (
                Math.Abs(currentPosition.X - _lastMouseDown.X) < 10.0 &&
                Math.Abs(currentPosition.Y - _lastMouseDown.Y) < 10.0)
            {
                _lastMouseDown = currentPosition;
                return;
            }
            _lastMouseDown = currentPosition;

            // Do this heere to avoid selection changed firings

            ////translate screen point to be relative to ItemsControl    
            HitTestResult result = VisualTreeHelper.HitTest(treeView, _lastMouseDown);

            ////find the item at that point    
            var item = result.VisualHit as FrameworkElement;

            T data = item.DataContext as T;
            if (data == null)
            {
                Console.WriteLine("Missed Click " + item.DataContext);
                return;
            }

            List<T> newSelection = new List<T>();

            newSelection.Add(data);

            //// THis confuses the MapPanel, as shift has a meaning there too, so drops get confused
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {

                //newSelection.AddRange( data.SelectMany(x => x.children).ToList() );
                List<IEchelon> allDescendents = data.AllDescendantsGeneric();

                //newSelection.AddRange(allDescendents);
                //List<R> kids = data.SelectMany(x => x.children).ToList();

                foreach (IEchelon child in allDescendents)
                {
                    T c = child as T;
                    if (c == null) continue;
                    newSelection.Add(c);
                   
                }
            }

            if (!((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                sharedEchelonSelection.Clear();
            }

            sharedEchelonSelection.AddRange(newSelection);
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("Cehl " + e.OriginalSource + " " + currentPosition);

            if (e.LeftButton == MouseButtonState.Pressed && treeView.SelectedValue != null)
            {
                Point currentPosition = e.GetPosition(treeView);
                //Console.WriteLine("Move " + e.OriginalSource + " " + currentPosition);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {

                    DragDrop.DoDragDrop(treeView, sharedEchelonSelection, DragDropEffects.All);


                }
            }
        }

        private ScrollViewer _scrollViewer = null;
        public ScrollViewer GetScrollViewer()
        {
            //DependencyObject border = VisualTreeHelper.GetChild(treeView, 0);
            //ScrollViewer m_scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            if (_scrollViewer == null)
            _scrollViewer = treeView.Template.FindName("_tv_scrollviewer_", treeView) as ScrollViewer;

            return _scrollViewer;
        }

        Point _lastDragPosition;
        int _dragTicks = 0;
        public void DragOver(object sender, DragEventArgs e)
        {
            Point currentPosition = e.GetPosition(treeView);

            ScrollViewer scrollViewer  = GetScrollViewer();
            double dragDelta = Math.Abs(currentPosition.Y - _lastDragPosition.Y) ;
            _lastDragPosition = currentPosition;

            // a hacky timer to prevent scrolling when just passing over
            if (dragDelta > 5)
            {
                _dragTicks = 0;
            }
            else
            {
                _dragTicks++;
            }


            if (scrollViewer != null
                   && _dragTicks > 100)
            {
                double tolerance = 30;
                double verticalPos = currentPosition.Y;
                double offset = 20;

                if (verticalPos < tolerance) // Top of visible list? 
                {
                    //Scroll up
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
                    _lastDragPosition = currentPosition;
                    e.Effects = DragDropEffects.Scroll;
                    return;
                }
                else if (verticalPos > treeView.ActualHeight - tolerance) //Bottom of visible list? 
                {
                    //Scroll down
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
                    _lastDragPosition = currentPosition;
                    e.Effects = DragDropEffects.Scroll;
                    return;
                }
            }

            if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
            {
                // Verify that this is a valid drop and then store the drop target
                //object item = GetNearestContainer(e.OriginalSource as UIElement);
                T targetItem = GetNearestTreeViewItemItemsSource(e.OriginalSource as UIElement);


                // we are on the tree, but not a treeitem
                if (targetItem == null)
                {
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                    return;
                }
                

                if (CheckDropTarget(e.Data as DataObject, targetItem))
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }


        public void Drop(object sender, DragEventArgs args)
        {
            //try
            //{
            args.Effects = DragDropEffects.None;
            args.Handled = true;

            // Verify that this is a valid drop and then store the drop target
            T targetItem = GetNearestTreeViewItemItemsSource(args.OriginalSource as UIElement);
            // object draggedItem = args.Data.GetData(typeof(object));
            string[] formats = args.Data.GetFormats();

            bool addChildren = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            if (targetItem != null)
            {
                // drooped on an echelon
                args.Effects = DragDropEffects.Move;
                try
                {
                    PlaceDataObjectOnEchelon(args.Data as DataObject, targetItem, addChildren);
                }
                catch (RosterAddException e)
                {
                    Log.Error(this, e.Message);
                    MessageBox.Show(e.Message, "Unable to drop ");

                }

            }
            else if (isScenarioTree)
            {
                    //Dropped on TreeView, but not on echelin item
                    PlaceDataObjectOnScenario(args.Data as DataObject);
                    args.Effects = DragDropEffects.Move;
                    args.Handled = true;

            }


            //}
            //catch (Exception)
            //{
            //}
        }

        public bool CheckDropTarget(DataObject data, T targetItem)
        {

            //handle the case where an OOB is dragged into a scenario
           
            {
                ScenarioEchelon targetSEch = targetItem as ScenarioEchelon;
                //OOBEchelon draggedOOBEch = data.GetData(typeof(OOBEchelon)) as OOBEchelon;
                //Selection<OOBEchelon, OOBUnit> oobSelection = data.GetData(typeof(Selection<OOBEchelon, OOBUnit>)) as Selection<OOBEchelon, OOBUnit>;
                EchelonSelectionSet<OOBEchelon, OOBUnit> oobSelection = EchelonSelectionSet<OOBEchelon, OOBUnit>.ExtractFromDataObject(data);
                if (targetSEch != null && oobSelection != null)
                {
                    foreach (OOBEchelon echelon in oobSelection)
                        if (!echelon.CanBeChildOf(targetSEch)) return false;
                    return true;
                }
            }

            // delete
            {
                OOBEchelon targetOOBEch = targetItem as OOBEchelon;
                //ScenarioEchelon draggedScnEch = data.GetData(typeof(ScenarioEchelon)) as ScenarioEchelon;
                EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> scenarioSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(data);
                if (targetOOBEch != null && scenarioSelection != null)
                {
                    // can delete by draggin from scenario to oob
                    return true;
                }
            }

            T targetEchelon = targetItem as T;
            //T draggedEchelon = data.GetData(typeof(T)) as T;
            EchelonSelectionSet<T, R> selection = EchelonSelectionSet<T, R>.ExtractFromDataObject(data);

            if (targetEchelon != null)
            {
                foreach (T echelon in selection)
                    if (!(echelon.CanBeChildOf(targetEchelon) || echelon.CanBeSiblingOf(targetEchelon)))
                        return false;
                return true;

                // return draggedEchelon.CanBeChildOf(targetEchelon) || draggedEchelon.CanBeSiblingOf(targetEchelon);
            }


            return false;
        }

        public void PlaceDataObjectOnScenario(DataObject data)
        {
            EchelonSelectionSet<OOBEchelon, OOBUnit> oobSelection = EchelonSelectionSet<OOBEchelon, OOBUnit>.ExtractFromDataObject(data);

            if (oobSelection != null && oobSelection.root != null)
            {

                //if (MessageBox.Show("Would you Add " + oobSelection.Count + " units", "Add", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                //    return;


                foreach (OOBEchelon echelon in oobSelection)
                {
                    Log.Info(this, "Dropping in " + echelon);
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        window.scenario.InsertUnitWithChildren(echelon.unit);
                    }
                    else
                    {
                        try
                        {
                            window.scenario.InsertUnit(echelon.unit);
                        }
                        catch (Exception e)
                        {
                            Log.Error(this, e.Message+" "+e.InnerException == null ? "" : e.InnerException.Message);
                        }
                    }
                }

                return;
            }
        }

        public void PlaceDataObjectOnEchelon(DataObject data, T targetEchelon, bool addChildren)
        {

            //Asking user wether he want to drop the dragged TreeViewItem here or not

            //special case for when dragging from oob to another roster (oob or scenario)
            {
                //OOBEchelon draggedOOBEch = data.GetData(typeof(OOBEchelon)) as OOBEchelon;
                EchelonSelectionSet<OOBEchelon, OOBUnit> oobSelection = EchelonSelectionSet<OOBEchelon, OOBUnit>.ExtractFromDataObject(data);

                    List<T> tomark = new List<T>();


                if (oobSelection != null && oobSelection.root != null && oobSelection.root.roster != targetEchelon.root.roster)
                {
                    if (oobSelection.Count > 1)
                    {
                        if (addChildren)
                        {
                            if (MessageBox.Show("Add " + oobSelection.Count + " units with subordinates to " + targetEchelon.ToString() + "?", "Add", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                return;
                        }
                        else
                        {
                            if (MessageBox.Show(" Add " + oobSelection.Count + " units to " + targetEchelon.ToString() + "?", "Add", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                return;
                        }
                    }

                    foreach (OOBEchelon echelon in oobSelection)
                    {
                        if (!echelon.CanBeChildOf(targetEchelon))
                        {
                            Log.Error(this, "Cannnot drop " + echelon + " onto " + targetEchelon + " across rosters");
                            return;
                        }
                    }

                    foreach (OOBEchelon echelon in oobSelection)
                    {
                        
                        Log.Info(this, "Dropping in " + echelon);
                        if (addChildren)
                        {
                            R inserted = targetEchelon.InsertUnitWithChildren(echelon.unit);
                            if (inserted != null)
                                tomark.Add(inserted.echelon as T);
                        }
                        else
                        {
                            R inserted = targetEchelon.InsertUnit(echelon.unit);
                            if (inserted != null)
                                tomark.Add(inserted.echelon as T);

                        }
                    }

                    ExpandAndMark(tomark);
                    return;
                }
            }

            //Removing by draggin back to oob
            {
               // ScenarioEchelon draggedScnEch = data.GetData(typeof(ScenarioEchelon)) as ScenarioEchelon;
                EchelonSelectionSet<ScenarioEchelon, ScenarioUnit> scenarioSelection = EchelonSelectionSet<ScenarioEchelon, ScenarioUnit>.ExtractFromDataObject(data);

                OOBEchelon targetOOBEch = targetEchelon as OOBEchelon;
                if (scenarioSelection != null && targetOOBEch != null)
                {
                    if (MessageBox.Show("Remove " + scenarioSelection.Count + " units and their subordinates from Scenario?", "Remove", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        return;

                    foreach (ScenarioEchelon echelon in scenarioSelection)
                    {
                        echelon.RemoveFromRoster();
                    }
                    return;
                }
            }

            // Due to bad unboxing from DataObject (you need to know exact type, not subtype)
            // we have to depend on proper casting from above
            //T draggedEchelon = data.GetData(typeof(T)) as T;

            EchelonSelectionSet<T, R> selection = EchelonSelectionSet<T, R>.ExtractFromDataObject(data);

            if (selection.Contains(targetEchelon)) return;

            if (selection == null || selection.Count < 1)
            {
                MessageBox.Show("Failed to drag Empty Selection into " + targetEchelon.ToString() + "", "Move", MessageBoxButton.OK);
                return;
            }

            if (MessageBox.Show("Move " + selection.Count + " units to " + targetEchelon.ToString() + "", "Move", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;


            foreach (T echelon in selection)
            {

                if (echelon.CanBeChildOf(targetEchelon))
                {
                    // make Child

                    targetEchelon.InsertChild(targetEchelon.Count > 0 ? targetEchelon.Count - 1 : 0, echelon);
                }
                else
                {
                    //make sibling
                    T parent = (T)targetEchelon.parent;
                    parent.InsertChild(parent.IndexOf(targetEchelon), echelon);
                }


            }
            return;
        }

        public T GetNearestTreeViewItemItemsSource(UIElement origElement)
        {

            //object container = scenarioTree.ItemContainerGenerator.ContainerFromItem(element);// as TreeViewItem;
            //Console.WriteLine("Found Container " + container+" for "+element);
            //return null;


            //// Walk up the element tree to the nearest tree view item.
            TreeViewItem treeViewItem = origElement as TreeViewItem;
            TreeView treeView = origElement as TreeView;
            UIElement nelement = origElement;
            while ((treeViewItem == null && treeView == null) && (nelement != null))
            {
                nelement = VisualTreeHelper.GetParent(nelement) as UIElement;
                treeViewItem = nelement as TreeViewItem;
                treeView = nelement as TreeView;
                //we got to parent treeView without finding a TreeViewItem
                if (treeView != null)
                {
                    return null;
                }
            }
            //Console.WriteLine("GotNearest " + (T)container.ItemsSource);

            if (treeViewItem != null)
                return (T)treeViewItem.ItemsSource;

            return null; 
        }


        /// <summary>
        /// Finds the provided object in an ItemsControl's children returns the ones that match
        /// Note that this tickles the SelectedItem callback
        /// </summary>
        /// <param name="parentContainer">The parent container whose children will be searched for the selected item</param>
        /// <param name="itemToSelect">The item to select</param>
        /// <returns>True if the item is found and selected, false otherwise</returns>
        private bool ExpandAndFindItems(ItemsControl parentContainer, List<T> itemsToFind, List<TreeViewItem> found)
        {
            //check all items at the current level
            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if the data item matches the item we want to select, set the corresponding
                //TreeViewItem IsSelected to true
                if (itemsToFind.Contains(item)  && currentContainer != null)
                {
                    //currentContainer.IsSelected = true;
                    found.Add(currentContainer);
                    itemsToFind.Remove((T)item);
                    currentContainer.BringIntoView();
                    currentContainer.Focus();
                    if (itemsToFind.Count < 1) return true;

                }
            }

            //if we get to this point, the selected item was not found at the current level, so we must check the children
            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                //if children exist
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    //keep track of if the TreeViewItem was expanded or not
                    bool wasExpanded = currentContainer.IsExpanded;

                    //expand the current TreeViewItem so we can check its child TreeViewItems
                    currentContainer.IsExpanded = true;

                    //if the TreeViewItem child containers have not been generated, we must listen to
                    //the StatusChanged event until they are
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        //store the event handler in a variable so we can remove it (in the handler itself)
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                if (ExpandAndFindItems(currentContainer, itemsToFind, found) == false)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        });
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (ExpandAndFindItems(currentContainer, itemsToFind, found) == false)
                        {
                            //restore the current TreeViewItem's expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else //otherwise the node was found and selected, so return true
                        {
                            return true;
                        }
                    }
                }
            }

            //no item was found
            return false;
        }


        public static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// probably will not b e efective if it hasnt already been expanded once due to lazy evel
        /// </summary>
        /// <param name="rank"></param>
        public void CollapseBelowRank(ERank rank)
        {
                CollapseBelowRank(rank, treeView);
        }

        public void CollapseBelowRank(ERank rank, ItemsControl parentContainer)
        {

            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer == null)
                {
                    continue;
                }

                IEchelon echelon = item as IEchelon;
                if (echelon != null && echelon.rank > rank)
                {
                    currentContainer.IsExpanded = true;
                    treeView.UpdateLayout();
                    CollapseBelowRank(rank, currentContainer);
                }
                else 
                {
                    currentContainer.IsExpanded = false;
                    CollapseBelowRank(rank, currentContainer);
                }
            }
        }


        #region Static Visual Utility
        static U FindVisualParent<U>(UIElement child) where U : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                U found = parent as U;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }
        #endregion

    }
}
