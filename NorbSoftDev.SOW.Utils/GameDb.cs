using System;
using System.Collections.Generic;
using System.IO;


using NorbSoftDev.SOW;
// using LumenWorks.Framework.IO.Csv;

namespace NorbSoftDev.SOW.Utils
{

  public class GameDBEntry : DataEntry {
    //use key, not index, in gamedb
    public AttributeLevel fatigue;

    public int ammo, deserted, killed, wounded;

    public override void ParseCSV(CsvReader csv, Config config)
    {

        // ammo = csv.AsInt32("ammo");
        // deserted = csv.AsInt32("deserted");
        // killed = csv.AsInt32("killed");
        // wounded = csv.AsInt32("wounded");
        // status = csv.AsString("status");
        // csv.TryAsString("id", x => id = x);
        csv.TryAsASCII("id", ref id);
        csv.TryAsInt32("ammo", ref ammo);
        csv.TryAsInt32("deserted", ref deserted);
        csv.TryAsInt32("killed", ref killed);
        csv.TryAsInt32("wounded", ref wounded);

        // foreach (string k in config.attributes.Keys) {
        //   Console.WriteLine(k);
        // } 
        // csv.TryAsAttributeLevel("fatigue", config.attributes["fatigue"], ref status);

        Console.WriteLine("{0} {1} {2} {3} {4} : {5}",id, ammo,deserted,killed,wounded,fatigue);
    }
  }


    public class ScenarioEchelonGameDBAttritionSubRule : ScenarioEchelonAttritionSubRule
    {

        public DataTable<GameDBEntry> dataTable;
        public ScenarioEchelonGameDBAttritionSubRule() : base () {
        }

        // public ScenarioEchelonAttritionSubRule Clone() {
        //     return this.MemberwiseClone() as ScenarioEchelonAttritionSubRule;
        // }

        public override void Attrite(Scenario scenario, ScenarioEchelon candidate)
        {
          if (candidate.rank > ERank.Regiment) return;
          if (candidate.unit == null) return;

          GameDBEntry entry;
          if (! dataTable.table.TryGetValue(candidate.unit.id, out entry) ){
            return;
          }

          Console.WriteLine(this+" "+candidate.unit);
          // candidate.unit.headCount -= 
          //   entry.deserted == null ? 0 : (int)entry.deserted +
          //   entry.killed == null ? 0 : (int)entry.killed +
          //   entry.wounded == null ? 0 : (int)entry.wounded;
          candidate.unit.headCount -= entry.deserted + entry.killed + entry.wounded;
          candidate.unit.ammo = entry.ammo;

          //TODO lookup status

          // switch (candidate.unitType) {
          //   case UnitType.EUnitType.Infantry:
          //     candidate.unit.headCount = infantry.Attrite(candidate.unit.headCount);
          //     return;

          //   case UnitType.EUnitType.Cavalry:
          //     candidate.unit.headCount = cavalry.Attrite(candidate.unit.headCount);
          //     return;

          //   case UnitType.EUnitType.Artillery:
          //     candidate.unit.headCount = artillery.Attrite(candidate.unit.headCount);
          //     return;

          //   default:
          //       return;
          // }
        }

        public override string ToString()
        {
            return dataTable.name;
        }

      }

}