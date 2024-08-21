using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NorbSoftDev.SOW
{
    public class ObservableCollectionWithItemNotify<T> :
		ObservableCollection<T> where T: INotifyPropertyChanged 
    {

        public ObservableCollectionWithItemNotify()
        {
            this.CollectionChanged += items_CollectionChanged;
        }


        public ObservableCollectionWithItemNotify(IEnumerable<T> collection) :base( collection)
        {
            this.CollectionChanged += items_CollectionChanged;
            foreach (INotifyPropertyChanged item in collection)
                item.PropertyChanged += item_PropertyChanged;

        }

        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e != null)
            {
                if(e.OldItems!=null)
                    foreach (INotifyPropertyChanged item in e.OldItems)
                        item.PropertyChanged -= item_PropertyChanged;

                if(e.NewItems!=null)
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        item.PropertyChanged += item_PropertyChanged;
            }
        }

        #region ItemPropertyChanged

        public event PropertyChangedEventHandler ItemPropertyChanged;


        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (ItemPropertyChanged != null) 
                ItemPropertyChanged(sender, e);

        }
        #endregion

    }
}