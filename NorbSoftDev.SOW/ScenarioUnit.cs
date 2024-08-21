using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;

namespace NorbSoftDev.SOW {
    public class ScenarioUnit : IUnit, INotifyPropertyChanged, IHasPosition, IComparable<ScenarioUnit>
    {


        #region Constructor
        internal ScenarioUnit(OOBUnit refUnit, List<Attribute> attributeNames)
            : this(attributeNames)
        {
            this.refUnit = refUnit;
            transform = new WorldTransform();
            foreach (Attribute attribute in attributeNames)
            {
                AttributeLevel attributeLevel;
                refUnit.attributes.TryGetValue(attribute.name, out attributeLevel);
                this.attributes[attribute.name] = attributeLevel;
            }
        }

        internal ScenarioUnit(ScenarioUnit other, List<Attribute> attributeNames)
            : this(attributeNames)
        {
            this.refUnit = other.refUnit;
            this._headCount = other._headCount;
            foreach (Attribute attribute in attributeNames)
            {
                AttributeLevel attributeLevel;
                other.attributes.TryGetValue(attribute.name, out attributeLevel);
                this.attributes[attribute.name] = attributeLevel;
            }
        }

        internal ScenarioUnit(List<Attribute> attributeNames)
            : this()
        {
            foreach (Attribute attribute in attributeNames)
            {
                attributes[attribute.name] = null;
            }
        }

        protected ScenarioUnit()
        {
            attributes = new ObservableDictionary<string, AttributeLevel>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        public string test { get; set; }

        public string userName { get { return _refUnit.userName; } }
        public string id { get { return _refUnit.id;} }
        public string name1 { get { return _refUnit.name1;} }
        public string name2 { get { return _refUnit.name2;} }
        public UnitClass unitClass { get { return _refUnit.unitClass;} }
        public int oobconfig { get { return _refUnit.oobconfig;} }
        public string portrait { get { return _refUnit.portrait;} }
        public Weapon weapon { get { return _refUnit.weapon;} }
        public Graphic flag { get { return _refUnit.flag;} }
        public Graphic flag2 { get { return _refUnit.flag2;} }



        ObservableDictionary<string, AttributeLevel> _attributes;
        public ObservableDictionary<string, AttributeLevel> attributes
        { 
            get {
                return this._attributes;
            } 
            set {
                this._attributes = value;
                _attributes.CollectionChanged += attributes_CollectionChanged;
                OnPropertyChanged("attribute");
            }
        }

        public bool TryGetAttributeLevel(string attributeKey, out AttributeLevel level)
        {
            if (this._attributes.TryGetValue(attributeKey, out level))
            {
                return true;
            }
            return _refUnit.TryGetAttributeLevel(attributeKey, out level);

        }


        private void attributes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("");
        }

        // public UnitAttribute ability { get { return _refUnit.ability;} }
        // public UnitAttribute command { get { return _refUnit.command;} }
        // public UnitAttribute control { get { return _refUnit.control;} }
        // public UnitAttribute leadership { get { return _refUnit.leadership;} }
        // public UnitAttribute style { get { return _refUnit.style;} }
        // public UnitAttribute experience { get { return _refUnit.experience;} }
 
        // public UnitAttribute close { get { return _refUnit.close;} }
        // public UnitAttribute open { get { return _refUnit.open;} }
        // public UnitAttribute edged { get { return _refUnit.edged;} }
        // public UnitAttribute firearm { get { return _refUnit.firearm;} }
        // public UnitAttribute marksmanship { get { return _refUnit.marksmanship;} }
        // public UnitAttribute horsemanship { get { return _refUnit.horsemanship; } }

        // public UnitAttribute surgeon { get { return _refUnit.surgeon;} }
        // public UnitAttribute calisthenics { get { return _refUnit.calisthenics;} }

        /// Useful for moving with other units, or campaign stuff
        // internal IAttach _attachedTo;
        // public IAttach attachedTo {
        //     get {
        //         return _attachedTo;
        //     }
        // }

        // public bool AttachTo( IAttach value) {
        //         if (_attachedTo != null && !_attachedTo.OnDetach(this)) {
        //             return false;
        //         }

        //         if (value != null && !value.OnAttach(this) ){
        //             //possibly reattach?
        //             return false;
        //         }

        //         _attachedTo = value;
        //         return true;
        // }


        //Scenario too
        internal int? _headCount = null;
        public int headCount { 
            get { 
                //return this._headCount == null ? _refUnit.headCount : (int)this._headCount;
                return (int)(this._headCount ?? _refUnit.headCount);
            }
            set {

                if (this._headCount == value) return;
                this._headCount = value;
                //ComputeBounds();
                OnPropertyChanged("headCount");
            }              
        }

        internal int? _ammo = null;
        public int ammo { 
            get { 
                return this._ammo == null ? _refUnit.ammo : (int)this._ammo;
            }
            set {
                if (this._ammo == value) return;
                this._ammo = value;
                OnPropertyChanged("ammo");
            } 
        }

        internal Formation _formation = null;
        public Formation formation { 
            get { 
                return this._formation == null ? _refUnit.formation : (Formation)this._formation;
            }
            set {
                if (this._formation == value) return;
                if (this._formation != null ) this._formation.PropertyChanged -= formation_PropertyChanged;
                this._formation = value;

                //if (this._formation == null)
                //{
                //    throw new Exception(this.id + " had formation set to null");
                //}

                if (this._formation == null)
                {
                    Log.Error(this, this.id + " had formation set to null");
                }
                else
                {
                    this._formation.PropertyChanged += formation_PropertyChanged;
                }
                
                //ComputeBounds();
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

        //public void ComputeBounds() {
        //     _bounds = formation.ComputeBounds(this);
        //}


        //Scenario Only
        WorldTransform _transform;
        public WorldTransform transform {
            get { return _transform; } 
            set {
                _transform = value;
                _transform.PropertyChanged += transform_PropertyChanged;
                //OnPropertyChanged("transform");
                //OnPropertyChanged("location");
            }
        }

        public Position position
        {
            get { return _transform; }
            set { _transform.SetPosition(value);// OnPropertyChanged("transform"); OnPropertyChanged("location");
            }
        }

        private void transform_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

 
        // other
        public UnitStatus unitStatus {get; set;}
        public XP xp {get; set;}




        ScenarioEchelon _echelon;
        public IEchelon echelon {
            get { return _echelon; } 
            set {
                if (this._echelon == value) return;
                _echelon = (ScenarioEchelon) value;
                OnPropertyChanged("echelon");
            }
        }

        public ScenarioEchelon scenarioEchelon
        {
            get { return _echelon; }
            set
            {
                if (this._echelon == value) return;
                _echelon = value;
                OnPropertyChanged("echelon");
            }
        }

        #region IComparable
        public int CompareTo(ScenarioUnit that)
        {
            return this.scenarioEchelon.CompareTo(that.scenarioEchelon);
        }
        #endregion

        // public Echelon side { get { return _echelon.side; } }
        // public Echelon army { get { return _echelon.army; } }
        // public Echelon corps { get { return _echelon.corps; } }
        // public Echelon division { get { return _echelon.division; } }
        // public Echelon brigade { get { return _echelon.brigade; } }


        OOBUnit _refUnit;
        internal OOBUnit refUnit
        {
            get { return _refUnit; }
            set
            {
                // pass changes from refUnit upwards as if we changed
                if (refUnit != null) _refUnit.PropertyChanged -= refUnit_PropertyChanged;
                if (value != null) value.PropertyChanged += refUnit_PropertyChanged;
                _refUnit = value;
            }
        }

        void refUnit_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e);
        }

        public string padding
        {
            get
            {
                if (echelon == null) return "";
                return "".PadLeft((int)ERank.Root - (int)echelon.rank, '.')+echelon.rank.ToString().Substring(0,3);
            }
        }



        public ScenarioUnit MoveTo(ScenarioUnitRoster roster) {
            ScenarioUnit newUnit = roster.InsertUnitWithChildren(this);
            ScenarioEchelon origEchelon = (ScenarioEchelon)this.echelon;
            origEchelon.parent.Remove(origEchelon);
            origEchelon.RemoveFromRoster();
            return newUnit;
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

              // Create the OnPropertyChanged method to raise the event 
      protected void OnPropertyChanged(string name)
      {
          PropertyChangedEventHandler handler = PropertyChanged;
          if (handler != null)
          {
              handler(this, new PropertyChangedEventArgs(name));
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

      public override string ToString() {
        return id;
      }


    }
}