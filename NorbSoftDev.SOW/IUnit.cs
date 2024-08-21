using System.Collections.Generic;

namespace NorbSoftDev.SOW {
    public interface IUnit  :  IHasFormation, IHasId {
        //OOB only
        string userName { get; }
        string name1 {get; }
        string name2 {get; }
        UnitClass unitClass {get; }
        int oobconfig {get; }
        string portrait {get; }
        Weapon weapon {get; }
        Graphic flag {get; }
        Graphic flag2 {get; }


        ObservableDictionary<string, AttributeLevel> attributes
        { 
            get; 
        }

        bool TryGetAttributeLevel(string attributeKey, out AttributeLevel level);
        // UnitAttribute ability {get; }
        // UnitAttribute command {get; }
        // UnitAttribute control {get; }
        // UnitAttribute leadership {get; }
        // UnitAttribute style {get; }
        // UnitAttribute experience {get; }

        // UnitAttribute close {get; }
        // UnitAttribute open {get; }
        // UnitAttribute edged {get; }
        // UnitAttribute firearm {get; }
        // UnitAttribute marksmanship {get; }
        // UnitAttribute horsemanship { get; }
        // UnitAttribute surgeon {get; }
        // UnitAttribute calisthenics {get; }

        //Scenario too
        int headCount {get; }


        int ammo {get; }
        //Formation formation {get; }
        // UnitState fatigue { get; }
        // UnitState morale { get; }

        //Scenario Only
        WorldTransform transform {get; }

        // other
        UnitStatus unitStatus {get; }
        XP xp {get; }

        IEchelon echelon { get; set; }
        // Echelon side { get; }
        // Echelon army { get; }
        // Echelon corps { get; }
        // Echelon division { get; }
        // Echelon brigade { get; }

    }
 
    //public interface IUnitGeneric<T,R> where T : IUnitGeneric<T,R> where  R : EchelonGeneric<T> {
    //    R echelon {get; set;}
    //}

    //public interface IUnitGeneric<T, R>
    //    where T : IUnitGeneric<T, R>
    //    where R : Echelon
    //{
    //    R echelon { get; set; }
    //}

}
