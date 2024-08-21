using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;


namespace NorbSoftDev.SOW
{
    public interface IHasId { string id { get; } }
    public interface IHasUnit { ScenarioUnit unit { get; set; } }
    public interface IHasFormation { Formation formation { get; set; } }
    
    public interface IHasCommand { Command command { get; set;} }
    public interface IHasDirection { Vector direction { get; set; } }
    public interface IHasObjective { IObjective objective { get; } }
    public interface IHasScenarioObjective : IHasObjective { ScenarioObjective scenarioObjective { get; set; } }
    public interface IHasMapObjective : IHasObjective { MapObjective mapObjective { get; set; } }
    public interface IHasPosition : INotifyPropertyChanged { Position position { get; set; } }
    public interface IHasOther : INotifyPropertyChanged { ScenarioUnit other { get; set; } }
    public interface IHasRecipient : INotifyPropertyChanged { ScenarioUnit recipient { get; set; } }
    public interface IHasScreenMessage : INotifyPropertyChanged { ScreenMessage screenMessage { get; set; } }
    public interface IObjective : INotifyPropertyChanged, IPosition, IHasId { }

    public interface IPosition {
        float south {get; set;} 
        float east {get; set;} 
        void SetPosition(float south, float east);
    }


    public interface ILoad {
            void PreLoad();
            void Load();

        }

    public interface IAttach {
        void OnDetach(ScenarioEchelon scenarioEchelon);
    }
}


#if UNITY_STANDALONE
namespace System.Windows {
	public struct Point {
		public double X,Y;
		public Point(double X, double Y) {
			this.X = X;
			this.Y = Y;
		}
	}
	
	public struct Vector {
		public double X,Y;
		public Vector(double X, double Y) {
			this.X = X;
			this.Y = Y;
		}

		public void Normalize() {
			double len = Math.Sqrt(X*X+Y*Y);
			X /= len;
			Y /= Y;
		}
	}
	
	public struct Rect {
		public Rect(
			double X, double Y,
			double width, double height) {
			this.X = X;
			this.Y = Y;
			this.Width = width;
			this.Height = height;
		}
		public double X,Y,Width,Height;
	}
}

 namespace System.Collections.Specialized
 {
//     /// <summary>
//     /// This enum describes the action that caused a CollectionChanged event.
//     /// </summary>
//     public enum NotifyCollectionChangedAction
//     {
//         /// <summary> One or more items were added to the collection. </summary>
//         Add,
//         /// <summary> One or more items were removed from the collection. </summary>
//         Remove,
//         /// <summary> One or more items were replaced in the collection. </summary>
//         Replace,
//         /// <summary> One or more items were moved within the collection. </summary>
//         Move,
//         /// <summary> The contents of the collection changed dramatically. </summary>
//         Reset,
//     }


// 	public enum NotifyCollectionChangedAction {Add, Move};
// 	public class NotifyCollectionChangedEventArgs : EventArgs {
// 		public NotifyCollectionChangedAction action;
// 		public object item;
// 		public int index;
// 		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object item, int index) {
// 			this.action = action;
// 			this.item = item;
// 			this.index = index;
// 		}
// 	}

// 	public delegate void NotifyCollectionChangedEventHandler(
// 		Object sender,
// 		NotifyCollectionChangedEventArgs e
// 		);

 	public interface INotifyCollectionChanged {		
 		event NotifyCollectionChangedEventHandler CollectionChanged;
 	}
 }

namespace System.Collections.ObjectModel
{
	[Serializable]
	public class ObservableCollection<T> : List<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		//public event Action<int> CollectionChanged = delegate { };
		//public event Action Updated = delegate { };

		public ObservableCollection() : base () {} 
		public ObservableCollection (IEnumerable<T> collection) :base( collection) {}


		#region INotifyPropertyChanged implementation
		/// <summary>
		/// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
		/// </summary>
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				PropertyChanged += value;
			}
			remove
			{
				PropertyChanged -= value;
			}
		}
		#endregion INotifyPropertyChanged implementation

		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
#if !FEATURE_NETCORE
        [field:NonSerializedAttribute()]
#endif
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
//                using (BlockReentrancy())
//                {
                    CollectionChanged(this, e);
//                }
            }
        }


        #region Private Methods
        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Private Methods




		public new void Add(T item)
		{
			base.Add(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, -1);
		}
		public new void Remove(T item)
		{
			base.Remove(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, -1);
		}
		// public new void AddRange(IEnumerable<T> collection)
		// {
		// 	base.AddRange(collection);
		// 	Updated();
		// }
		// public new void RemoveRange(int index, int count)
		// {
		// 	base.RemoveRange(index, count);
		// 	Updated();
		// }
		public new void Clear()
		{
			base.Clear();
			OnCollectionReset();
		}
		public new void Insert(int index, T item)
		{
			base.Insert(index, item);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}
		public new void InsertRange(int index, IEnumerable<T> collection)
		{
			base.InsertRange(index, collection);
//            OnCollectionChanged(NotifyCollectionChangedAction.Add, index);
			OnCollectionReset();
		}
		public new void RemoveAll(Predicate<T> match)
		{
			base.RemoveAll(match);
			OnCollectionReset();
		}
		
		
		public new T this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
			}
		}
		
		
	}
}

#endif



