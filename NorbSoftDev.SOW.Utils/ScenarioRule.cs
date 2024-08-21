using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NorbSoftDev.SOW;
// using LumenWorks.Framework.IO.Csv;
using System.ComponentModel;

using System.Collections.ObjectModel;


namespace NorbSoftDev.SOW.Utils
{



    public class ScenarioEchelonBaseRule : INotifyPropertyChanged, IScenarioEchelonRule
    {


        private bool _active = true;
        public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged(""); //force everyting to update
            }
        }

        // private ScenarioEchelon _attachedTo;
        // public ScenarioEchelon attachedTo
        // {
        //     get { return _attachedTo; }
        //     set
        //     {
        //         _attachedTo = value;



        //         OnPropertyChanged(""); //force everyting to update
        //     }
        // }

        public ScenarioEchelonBaseRule() : base() { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void all_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
        }


    }

    public class ScenarioEchelonRule : ScenarioEchelonBaseRule
    {

        private ScenarioEchelonAttritionSubRule _attrition;
        public ScenarioEchelonAttritionSubRule attrition
        {
            get { return _attrition; }
            set
            {
                _attrition = value;
                OnPropertyChanged("attrition"); //force everyting to update
            }
        }

        private ScenarioEchelonCreationSubRule _creation;
        public ScenarioEchelonCreationSubRule creation
        {
            get { return _creation; }
            set
            {
                _creation = value;
                OnPropertyChanged("creation"); //force everyting to update
            }
        }

        private ScenarioEchelonDeploymentSubRule _deploy;
        public ScenarioEchelonDeploymentSubRule deploy
        {
            get { return _deploy; }
            set
            {
                _deploy = value;
                OnPropertyChanged("deploy"); //force everyting to update
            }
        }
    }

    public class AttritionRuleRange : INotifyPropertyChanged 
    {

        public AttritionRuleRange(float min, float max) {
            this.min = min;
            this.max = max;
        }

        float _min, _max;



        public float max { get {return _max;}
            set { 
                if (_max == value) return;
                _max = value < 0 ? 0 : value;
                OnPropertyChanged("max");
            }
        }

        public float min { get {return _min;}
            set { 
                if (_min == value) return;
                _min = value < 0 ? 0 : value;
                OnPropertyChanged("min");
            }
        }


        public int Attrite(float value) {
          if (value <= 0) return 0;

          return (int) (value * (RuleManager.random.NextDouble() * (max-min) + min));


        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void all_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
        }

        public override string ToString()
        {
            return min+"-"+max;
        }
    }




    public abstract class ScenarioEchelonAttritionSubRule : ScenarioEchelonBaseRule {
      public ScenarioEchelonAttritionSubRule() : base() {}

        public ScenarioEchelonAttritionSubRule Clone() {
            return this.MemberwiseClone() as ScenarioEchelonAttritionSubRule;
        }

        public abstract void Attrite(Scenario scenario, ScenarioEchelon candidate);

    } 

    public class ScenarioEchelonRandomAttritionSubRule : ScenarioEchelonAttritionSubRule
    {

        public AttritionRuleRange infantry { get; set;}
        public AttritionRuleRange cavalry { get; set;}
        public AttritionRuleRange artillery { get; set;}

        public ScenarioEchelonRandomAttritionSubRule() : base () {
            infantry = new AttritionRuleRange( 0.95f, 1.0f);
            cavalry = new AttritionRuleRange( 0.95f, 1.0f);
            artillery = new AttritionRuleRange( 0.95f, 1.0f);
        }



        public override void Attrite(Scenario scenario, ScenarioEchelon candidate)
        {
          if (candidate.rank > ERank.Regiment) return;
          if (candidate.unit == null) return;


          switch (candidate.unitType) {
            case UnitType.EUnitType.Infantry:
              candidate.unit.headCount = infantry.Attrite(candidate.unit.headCount);
              return;

            case UnitType.EUnitType.Cavalry:
              candidate.unit.headCount = cavalry.Attrite(candidate.unit.headCount);
              return;

            case UnitType.EUnitType.Artillery:
              candidate.unit.headCount = artillery.Attrite(candidate.unit.headCount);
              return;

            default:
                return;
          }
        }

        public override string ToString()
        {
            return "Attrition Inf:+"+infantry + " Cav:"+cavalry+" Art:"+artillery;
        }
    }





      public class CreationRuleRange : INotifyPropertyChanged {

        public CreationRuleRange(int current, int min, int max) {
            this.current = current;
            this.min = min;
            this.max = max;
            this.trim = .25f;
        }

        int _current, _min, _max;

        public int current { get {return _current;}
            set { 
                if (_current == value) return;
                _current = value < 0 ? 0 : value;
                OnPropertyChanged("current");
            }
        }

                public int max { get {return _max;}
            set { 
                if (_max == value) return;
                _max = value < 0 ? 0 : value;
                OnPropertyChanged("max");
            }
        }

                public int min { get {return _min;}
            set { 
                if (_min == value) return;
                _min = value < 0 ? 0 : value;
                OnPropertyChanged("min");
            }
        }

        float _trim;
        public float trim {
            get { return _trim; }
          set {
              value = value < 0 ? 0 : value > 1 ? 1 : value;

              if (_trim == value) return;
              _trim = value;
              OnPropertyChanged("trim");

          }
        }

        public int Compare(ScenarioEchelon echelon, int value ) {
            if (value < min) return -1;
            if (value > max ) return 1;
            return 0;
        }

        public bool ShouldPrune(int subtract) {
            if (subtract == 0) return true;

            if (current <= max) return false;
            
            if (current - subtract > min) {
                return true;
            }
            return false;
        }

        public int Trim(int value) {
          if (value == 0) return 0;

          if (current <= max) return 0;

          int diff = current-max;
          diff = diff > value ? value : diff;

          int trimcount =(int)( value * trim);
          diff = diff > trimcount ? trimcount : diff;
          Console.WriteLine("Will Trim "+diff+" from "+value );

          current -= diff;
          return diff;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void all_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("");
        }

        public override string ToString()
        {
            return string.Format("{0}-{1} {2:P2}",min,max,trim);
        }
    }

    public class ScenarioEchelonCreationSubRule : ScenarioEchelonBaseRule {


        public ScenarioEchelonCreationSubRule() : base () {
        }

        public void SetRulesFrom(ScenarioEchelon attachedTo) {
                infantry = new CreationRuleRange(attachedTo.nInfantry, 2000, 4000);
                cavalry = new CreationRuleRange(attachedTo.nCavalry, 10, 100);
                artillery = new CreationRuleRange(attachedTo.nArtillery, 10, 100);
        }


        public ScenarioEchelonCreationSubRule Clone() {
            return this.MemberwiseClone() as ScenarioEchelonCreationSubRule;
        }

        //TODO add notiy
        public CreationRuleRange infantry { get; set;}
        public CreationRuleRange cavalry { get; set;}
        public CreationRuleRange artillery { get; set;}

        public bool ShouldPrune(Scenario scenario, ScenarioEchelon candidate)
        {

            bool infantryPrune = infantry.ShouldPrune(candidate.nInfantry); 
            bool cavalryPrune = cavalry.ShouldPrune(candidate.nCavalry); 
            bool artilleryPrune = artillery.ShouldPrune(candidate.nArtillery); 

            // Console.WriteLine(candidate +": "+infantryPrune+  +candidate.nInfantry +" ? "+infantry.min+" < "+infantry.current +" < "+infantry.max);
            // if ( infantryPrune && cavalryPrune && artilleryPrune) {
            if ( infantryPrune && cavalryPrune && artilleryPrune) {
                infantry.current -= candidate.nInfantry;
                cavalry.current -= candidate.nCavalry;
                artillery.current -= candidate.nArtillery;
                return true;
            }

            return false;
        }

        public void TrimLeaves(Scenario scenario, ScenarioEchelon candidate)
        {
          if (candidate.rank > ERank.Regiment) return;
          if (candidate.unit == null) return;

        // public enum EUnitType {None=0,Infantry=1,Cavalry=2,Artillery=3,Ordnance=4,Courier=5} 

          switch (candidate.unitType) {
            case UnitType.EUnitType.Infantry:
              candidate.unit.headCount -= infantry.Trim(candidate.unit.headCount);
              return;

            case UnitType.EUnitType.Cavalry:
              candidate.unit.headCount -= cavalry.Trim(candidate.unit.headCount);
              return;

            case UnitType.EUnitType.Artillery:
              candidate.unit.headCount -= artillery.Trim(candidate.unit.headCount);
              return;

              default:
                return;
          }


        }

        public override string ToString()
        {
            return "Inf:" + infantry + " Cav:" + cavalry + " Art:" + artillery;
        }

    }

    public class ScenarioEchelonDeploymentSubRule : ScenarioEchelonBaseRule {



        ObservableCollection<MapArea> _mapAreas = new ObservableCollection<MapArea>();
        public ObservableCollection<MapArea> mapAreas {
          get { return _mapAreas; }
          set {
              if (_mapAreas == value) return;
              _mapAreas = value;
              //TODO wire collection chagned
              OnPropertyChanged("mapAreas");
          }
        }


        ERank _doFormationAtRank = ERank.Corps;
        public ERank doFormationAtRank {
            get { return _doFormationAtRank; }
          set {
              if (_doFormationAtRank == value) return;
              _doFormationAtRank = value;
              OnPropertyChanged("doFormationAtRank");
          }
        }


        float _facing = 0;
        public float facing {
            get { return _facing; }
          set {
              if (_facing == value) return;
              _facing = value;
              OnPropertyChanged("facing");
          }
        }

        public ScenarioEchelonDeploymentSubRule() : base () {
          
        }

        public ScenarioEchelonDeploymentSubRule Clone() {
            return this.MemberwiseClone() as ScenarioEchelonDeploymentSubRule;
        }

        public bool Deploy(Scenario scenario, ScenarioEchelon candidate)
        {
          

          ScenarioUnit unit = candidate.unit;
          if (unit == null) return false;
          if (mapAreas == null || mapAreas.Count < 1 ) return false;

          ERank rank = candidate.rank;
          if ( rank < doFormationAtRank) return false;

          MapArea area = RuleManager.GetRandom<MapArea>(mapAreas);
          Log.Info(this, "Deploying " + candidate + " " + unit + " " + rank);
          if (unit.transform == null) unit.transform = new WorldTransform();

          unit.transform.SetPosition(area.GetRandomPosition());
          unit.transform.facing = facing;

          if (rank == doFormationAtRank)
          {
              area.map.ApplyFormation(candidate);
          }

          return true;
        }

        public override string ToString()
        {
            return string.Join(":", _mapAreas)+" @ " + doFormationAtRank.ToString();
        }
      }

    public class RuleManager {

      public static Random random = new Random();

     public bool CreateAttritionRuleAtRank(ScenarioEchelon current, ERank atRank, ScenarioEchelonAttritionSubRule exampleRule)
       {
           if (current.rank == atRank)
           {
              ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
              // this will overwrite existing, non ScenarioEchelonRules
              if (rule == null) {
                 rule = new ScenarioEchelonRule();
                 current.rule = rule;
              } 

              rule.active = true;

               ScenarioEchelonAttritionSubRule subrule = exampleRule.Clone();
               rule.attrition = subrule;
               return true;
           }

           bool didCreate = false;
           foreach (ScenarioEchelon child in current.children)
           {
               if ( CreateAttritionRuleAtRank(child, atRank, exampleRule) ) didCreate = true;
           }
           return didCreate;
       }

       public bool CreateCreationRuleAtRank(ScenarioEchelon current, ERank atRank, ScenarioEchelonCreationSubRule exampleRule)
       {
           if (current.rank == atRank)
           {
              ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
              // this will overwrite existing, non ScenarioEchelonRules
              if (rule == null) {
                 rule = new ScenarioEchelonRule();
                 current.rule = rule;
              } 

              rule.active = true;

               ScenarioEchelonCreationSubRule subrule = exampleRule.Clone();
               rule.creation = subrule;
               subrule.SetRulesFrom(current);
               return true;
           }

           bool didCreate = false;
           foreach (ScenarioEchelon child in current.children)
           {
               if ( CreateCreationRuleAtRank(child, atRank, exampleRule) ) didCreate = true;
           }
           return didCreate;
       }


     public bool CreateDeploymentRuleAtRank(ScenarioEchelon current, ERank atRank, ScenarioEchelonDeploymentSubRule exampleRule)
       {
           if (current.rank == atRank)
           {
              ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
              // this will overwrite existing, non ScenarioEchelonRules
              if (rule == null) {
                 rule = new ScenarioEchelonRule();
                 current.rule = rule;
              } 

              rule.active = true;

               ScenarioEchelonDeploymentSubRule subrule = exampleRule.Clone();
               rule.deploy = subrule;
               return true;
           }

           bool didCreate = false;
           foreach (ScenarioEchelon child in current.children)
           {
               if ( CreateDeploymentRuleAtRank(child, atRank, exampleRule) ) didCreate = true;
           }
           return didCreate;
       }

        public void ApplyAllRules(Scenario scenario) {
            ApplyAttritionRule( scenario);
            ApplyPruneRule( scenario);
            ApplyTrimRule( scenario);
            ApplyDeploymentRule(scenario);
        }

       public void ApplyTrimRule(Scenario scenario) {
           TrimLeaves(scenario, scenario.root, null);
       }

       public void ApplyPruneRule(Scenario scenario){

           List<ScenarioEchelon> pruneList = new List<ScenarioEchelon>();

           PopulatePruneList(scenario, scenario.root, null, pruneList);
           Console.WriteLine("pruneList:"+pruneList.Count);

           //print stats
           int ncorps = 0, ndivisions = 0, nbrigades= 0, nregiments = 0;
           foreach (ScenarioEchelon current in pruneList) {
                switch( current.rank ) {
                    case ERank.Corps:
                        ncorps++;
                        break;
                    case ERank.Division:
                        ndivisions++;
                        break;                
                    case ERank.Brigade:
                        nbrigades++;
                        break;
                    case ERank.Regiment:
                        nregiments++;
                        break;
                    default:
                    break;
               }
           }
           Console.WriteLine( "Prune Corps:"+ncorps+" Divisions:"+ndivisions+" Brigades:"+nbrigades+" Regiments"+nregiments);

           // prune branches that do not terminate in regiments
           PruneEmptyBranches(scenario, scenario.root, pruneList);

           // PrettyPrint(scenario.root, ERank.Brigade, pruneList);

           foreach (ScenarioEchelon current in pruneList)
           {
               scenario.RemoveEchelon(current);
           }
       }


       public void ApplyAttritionRule(Scenario scenario) {
           Attrite(scenario, scenario.root, null);
       }

      public void Attrite(Scenario scenario, ScenarioEchelon current, ScenarioEchelonAttritionSubRule parentRule)
       {

           ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
           ScenarioEchelonAttritionSubRule subrule = null;
           if (rule != null) {
            subrule = rule.attrition;
           }

           if (subrule == null) subrule = parentRule;

           if (subrule != null && current.rank <= ERank.Regiment)
           {
              subrule.Attrite(scenario, current);
           }

           //do we need to prun children
           foreach (ScenarioEchelon child in ShuffledChildren(current))
           {
               Attrite(scenario, child, subrule);
           }
        }

       public void ApplyDeploymentRule(Scenario scenario) {

           Deploy(scenario, scenario.root, null);

       }

      public void Deploy(Scenario scenario, ScenarioEchelon current, ScenarioEchelonDeploymentSubRule parentRule)
       {

           string n = current.ToString();

           ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
           ScenarioEchelonDeploymentSubRule subrule = null;
           if (rule != null) {
            subrule = rule.deploy;
           }

           if (subrule == null) subrule = parentRule;


           if (subrule != null)
           {
              subrule.Deploy(scenario, current);
           }

           foreach (ScenarioEchelon child in ShuffledChildren(current))
           {
               Deploy(scenario, child, subrule);
           }
        }


       public void PopulatePruneList(Scenario scenario, ScenarioEchelon current, ScenarioEchelonCreationSubRule parentRule, List<ScenarioEchelon> pruneList)
       {
           ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
           ScenarioEchelonCreationSubRule subrule = null;
           if (rule != null) {
            subrule = rule.creation;
           }

           if (subrule == null) subrule = parentRule;

           bool didPrune = false;
           if (subrule != null)
           {
               didPrune = subrule.ShouldPrune(scenario, current);
           }

           if (didPrune)
           {
               pruneList.Add(current);
               return;
           }

           //do we need to prun children

           foreach (ScenarioEchelon child in  ShuffledChildren(current))
           {
               PopulatePruneList(scenario, child, subrule, pruneList);
           }
        }


        public bool PruneEmptyBranches(Scenario scenario, ScenarioEchelon current, List<ScenarioEchelon> pruneList){
           // I'm a leaf and I'm not pruned
            if (current.rank <= ERank.Regiment && ! pruneList.Contains(current)) {
                return false;
            }

            // already in prune list
            if (pruneList.Contains(current)) return true;

            foreach (ScenarioEchelon child in current.children)
            {
                if ( PruneEmptyBranches(scenario, child, pruneList) == false) {
                    return false; 
                }
            }            

            pruneList.Add(current);
            return true;
       }


       public void TrimLeaves(Scenario scenario, ScenarioEchelon current, ScenarioEchelonCreationSubRule parentRule)
       {

           ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;
           ScenarioEchelonCreationSubRule subrule = null;
           if (rule != null) {
            subrule = rule.creation;
           }

           if (subrule == null) subrule = parentRule;

           if (subrule != null && current.rank <= ERank.Regiment)
           {
              subrule.TrimLeaves(scenario, current);
           }

           //do we need to prun children
           foreach (ScenarioEchelon child in ShuffledChildren(current))
           {
               TrimLeaves(scenario, child, subrule);
           }
        }



        public List<NorbSoftDev.SOW.EchelonGeneric<NorbSoftDev.SOW.ScenarioUnit>> ShuffledChildren (ScenarioEchelon echelon)  
        {  

            List<NorbSoftDev.SOW.EchelonGeneric<NorbSoftDev.SOW.ScenarioUnit>> list = echelon.children.ToList();
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = random.Next(n + 1);  
                NorbSoftDev.SOW.EchelonGeneric<NorbSoftDev.SOW.ScenarioUnit> value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
            return list;
        }

        public static void ShuffleInPlace<T>( List<T> list) {
            int n = list.Count;
            if (n < 2) return;
            while (n > 1) {  
                n--;  
                int k = random.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
        }

        public static T GetRandom<T>( IList<T> list) {
          if (list == null || list.Count < 1) return default(T);
          if (list.Count == 1) return list[0];
          return list[random.Next(list.Count)];
        }


       public static void PrettyPrint(ScenarioEchelon current, ERank stopAt, List<ScenarioEchelon> pruneList ) {

            bool willPrune = pruneList == null ? false : pruneList.Contains(current);

            if (willPrune) Console.ForegroundColor = ConsoleColor.Red;
            ScenarioEchelonCreationSubRule creationRule = null;

            ScenarioEchelonRule rule = current.rule as ScenarioEchelonRule;

            if (rule != null) {
                Console.BackgroundColor = ConsoleColor.Blue;
                creationRule = rule.creation as ScenarioEchelonCreationSubRule; 
            }


            Console.Write( String.Format( "{0,5} {1,-24} i:{2,4} c:{3,4} a:{4,4} : {5} {6} {7} {8}",
                current.symbol, current.unit == null ? "null" : current.unit.id,
                current.nInfantry, current.nCavalry, current.nArtillery,
                current.unit == null ? "" : current.unit.transform.south+","+current.unit.transform.east,
                creationRule == null ? "" : creationRule.infantry.min.ToString() +"<"+creationRule.infantry.current.ToString() +"<"+ creationRule.infantry.max.ToString(),
                creationRule == null ? "" : creationRule.cavalry.min.ToString() +"<"+creationRule.cavalry.current.ToString() +"<"+ creationRule.cavalry.max.ToString(),
                creationRule == null ? "" : creationRule.artillery.min.ToString() +"<"+creationRule.artillery.current.ToString() +"<"+ creationRule.artillery.max.ToString()

            ));

            Console.WriteLine(" ");
            Console.ResetColor();
            if (willPrune) return;

            if (current.rank <= stopAt) {
                if (current.children.Count > 0 )Console.WriteLine("    ..."+current.children.Count);
                return;
            }

            foreach(ScenarioEchelon child in current.children) {
                PrettyPrint(child, stopAt, pruneList);

            } 

       } 

    }

}