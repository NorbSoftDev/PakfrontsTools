using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;

namespace NorbSoftDev.SOW {
    public class OOBUnit : IUnit, INotifyPropertyChanged, IComparable<OOBUnit>  {




        string _userName;
        public string userName
        {
            get
            {
                return this._userName;
            }
            set
            {
                if (this._userName == value) return;
                this._userName = value;
                OnPropertyChanged("userName");
            }
        } 

        string _id; 
        public string id { 
            get {
                return this._id;
            } 
            set {
                if (this._id == value) return;
                this._id = value;
                OnPropertyChanged("id");
            } 
        } 

        string _name1; 
        public string name1 { 
            get {
                return this._name1;
            } 
            set {
                if (this._name1 == value) return;
                this._name1 = value;
                OnPropertyChanged("name1");
            } 
        } 

        string _name2; 
        public string name2 { 
            get {
                return this._name2;
            } 
            set {
                if (this._name2 == value) return;
                this._name2 = value;
                OnPropertyChanged("name2");
            } 
        } 

        UnitClass _unitClass; 
        public UnitClass unitClass { 
            get {
                return this._unitClass;
            } 
            set {
                if (this._unitClass == value) return;
                this._unitClass = value;
                OnPropertyChanged("unitClass");
            } 
        } 

        int _oobconfig; 
        public int oobconfig { 
            get {
                return this._oobconfig;
            } 
            set {
                if (this._oobconfig == value) return;
                this._oobconfig = value;
                OnPropertyChanged("oobconfig");
            } 
        }

        string _portrait; 
        public string portrait { 
            get {
                return this._portrait;
            } 
            set {
                if (this._portrait == value) return;
                this._portrait = value;
                OnPropertyChanged("portrait");
            } 
        }
         
        Weapon _weapon; 
        public Weapon weapon { 
            get {
                return this._weapon;
            } 
            set {
                if (this._weapon == value) return;
                this._weapon = value;
                OnPropertyChanged("weapon");
            } 
        }

        Graphic _flag; 
        public Graphic flag { 
            get {
                return this._flag;
            } 
            set {
                if (this._flag == value) return;
                this._flag = value;
                OnPropertyChanged("flag");
            } 
        }

        Graphic _flag2; 
        public Graphic flag2 { 
            get {
                return this._flag2;
            } 
            set {
                if (this._flag2 == value) return;
                this._flag2 = value;
                OnPropertyChanged("flag2");
            } 
        }


        //public AttributeLevel ability
        //{ 
        //    get {
        //        return attributes["ability"];
        //        //foreach (KeyValuePair<string, AttributeLevel> kvp in attributes) return kvp.Value;
        //        //return null;

        //        //foreach (KeyValuePair<string, AttributeLevel> kvp in attributes) return kvp.Key+"|"+kvp.Value.ToString();
        //        //return "NEVER";
        //        //AttributeLevel value;
        //        //attributes.TryGetValue("abilty", out value);
        //        //if(value == null) return "NONE";
        //        //return value.ToString();
        //        //string ret = "";
        //        //foreach (KeyValuePair<string, AttributeLevel> kvp in attributes)
        //        //    ret += " " + kvp.Key + ":" + kvp.Value;
        //        //return ret;
        //    }
        //}

        ObservableDictionary<string, AttributeLevel> _attributes;
        public ObservableDictionary<string, AttributeLevel> attributes
        {
            get
            {
                return this._attributes;
            }
            set
            {
                this._attributes = value;
                _attributes.CollectionChanged += attributes_CollectionChanged;
                OnPropertyChanged("attribute");
            }
        }


        public bool TryGetAttributeLevel(string attributeKey, out AttributeLevel level)
        {
            return this._attributes.TryGetValue(attributeKey, out level);

        }

        private void attributes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("");
        }


        public int CompareTo(OOBUnit that)
        {
            return this.oobEchelon.CompareTo(that.oobEchelon);
        }

        //Scenario too
        int _headCount; 
        public int headCount { 
            get {
                return this._headCount;
            } 
            set {
                this._headCount = value;
                //_formation.ComputeBounds(this);
                OnPropertyChanged("headCount");
            } 
        }

        int _ammo; 
        public int ammo { 
            get {
                return this._ammo;
            } 
            set {
                this._ammo = value;
                OnPropertyChanged("ammo");
            } 
        }

        Formation _formation; 
        public Formation formation { 
            get {
                return this._formation;
            } 
            set {
                if (this._formation != null) this._formation.PropertyChanged -= formation_PropertyChanged;
                this._formation = value;
                if (this._formation != null) this._formation.PropertyChanged += formation_PropertyChanged;

                OnPropertyChanged("formation");
            } 
        }
        private void formation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //ComputeBounds();
            OnPropertyChanged(e);
        }

        //Rect _bounds;
        //public Rect bounds
        //{
        //    get
        //    {
        //        return _bounds;
        //    }
        //}

        //public void ComputeBounds()
        //{
        //    _bounds = formation.ComputeBounds(this);
        //}

        //UnitState _fatigue; 
        //public UnitState fatigue { 
        //    get {
        //        return this._fatigue;
        //    } 
        //    set {
        //        this._fatigue = value;
        //        OnPropertyChanged("fatigue");
        //    } 
        //}

        //UnitState _morale; 
        //public UnitState morale { 
        //    get {
        //        return this._morale;
        //    } 
        //    set {
        //        this._morale = value;
        //        OnPropertyChanged("morale");
        //    } 
        //} 
        //Scenario Only
        public WorldTransform transform { get { return null; } }
 
        // other
        public UnitStatus unitStatus { get { return null; } }
        public XP xp {get; set;}

        OOBEchelon _echelon;
        public  IEchelon echelon {
            get { return _echelon; } 
            set {
                _echelon = (OOBEchelon)value;
                OnPropertyChanged("echelon");
            }
        }

        public OOBEchelon oobEchelon
        {
            get { return _echelon; }
            set
            {
                _echelon = value;
                OnPropertyChanged("echelon");
            }
        }

        // public Echelon side { get { return _echelon.side; } }
        // public Echelon army { get { return _echelon.army; } }
        // public Echelon corps { get { return _echelon.corps; } }
        // public Echelon division { get { return _echelon.division; } }
        // public Echelon brigade { get { return _echelon.brigade; } }


        #region Constructor
        protected OOBUnit()
        {
            attributes = new ObservableDictionary<string, AttributeLevel>(StringComparer.OrdinalIgnoreCase);
        }

        internal OOBUnit(string id) : this() {
            this._id = id;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

              // Create the OnPropertyChanged method to raise the event 
      protected void OnPropertyChanged(string name)
      {
          // PropertyChangedEventHandler handler = PropertyChanged;
          // if (handler != null)
          // {
          //     handler(this, new PropertyChangedEventArgs(name));
          // }
          
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

      //public OOBUnit ShallowCopy()
      //{
      //    return (OOBUnit)this.MemberwiseClone();

      //}

      public OOBUnit ShallowCopy(string id)
      {
          OOBUnit newUnit = (OOBUnit)this.MemberwiseClone();
          newUnit._echelon = null;
          newUnit._id = id;
          newUnit.PropertyChanged = null;
          return newUnit;
      }

      public OOBUnit ShallowCopy()
      {
          OOBUnit newUnit = (OOBUnit)this.MemberwiseClone();
          newUnit._echelon = null;
          newUnit.PropertyChanged = null;
          return newUnit;
      }


      public override string ToString() {
        return base.ToString()+"["+id+"]";
      }

    }
}