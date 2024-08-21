using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NorbSoftDev.SOW;
using NorbSoftDev.SOW.Utils;

using System.Drawing;
using System.IO;

namespace TestConsole
{



    public class Test
    {
        public static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("[            Console Starting       ]");
            Console.WriteLine(Environment.OSVersion.Platform);

            Log.SetupUserLog();
            try
            {

                //Config config = new Config("/home/tims/Dropbox/SOWIO/SOWWL/SDK", "/home/tims/sowwl.ini");
                //Config config = Config.AutoFind();

                string homedir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                Config config = new Config(@"E:\SOW\Games\Waterloo-Dev\", @"D:\Users\Tim\Documents\SowWL\sowwl.ini");

                OrderOfBattle oob = new OrderOfBattle(config, config.baseMod, "OOB_SB_Waterloo_Campaign Br Cav");
                oob.Load();
                Scenario scenario = new Scenario(oob, false);
                scenario.PopulateUnitsFromOrderOfBattle();
                // scenario.PopulateEchelonsFromOrderOfBattle();
                // scenario.PrettyPrint();


                TerrainBitmap terrainBitmap = new TerrainBitmap(scenario.map);
                terrainBitmap.LockBits();

                {
                    int x = 0;//61;
                    int y = 0;//78;
                    Terrain terrain;
                    //Terrain terrain = terrainBitmap.GetTerrain(scenario.map, x, y);
                    //Console.WriteLine("terrainBitmap Terrain: " + terrain + " " + terrain.movementFactor + " " + terrain.isRoad);
                    //Console.WriteLine(terrainBitmap.GetPixel(0, 0));

                    terrain = terrainBitmap.GetTerrainAtPixel( x, y);
                    Console.WriteLine("lockBitmap    Terrain: " + terrain + " " + terrain.movementFactor + " " + terrain.isRoad);

                    terrain = terrainBitmap.GetTerrainAtPixel(terrainBitmap.width - 1, terrainBitmap.height - 1);
                    Console.WriteLine("lockBitmap    Terrain: " + terrain + " " + terrain.movementFactor + " " + terrain.isRoad);

                }

                Console.ReadKey();
                Environment.Exit(0);


                ScenarioEchelonCreationSubRule exampleCreationRule = new ScenarioEchelonCreationSubRule();
                RuleManager ruleManager = new RuleManager();

                ruleManager.CreateCreationRuleAtRank(scenario.root, ERank.Side, exampleCreationRule);
                Console.WriteLine("Pruning...");
                ruleManager.ApplyPruneRule(scenario);
                ruleManager.ApplyTrimRule(scenario);
                RuleManager.PrettyPrint(scenario.root, ERank.Brigade, null);


                ScenarioEchelonAttritionSubRule exampleAttritionRule = new ScenarioEchelonRandomAttritionSubRule();

                //DataTable<GameDBEntry> data = new DataTable<GameDBEntry>(config);
                //data.ReadFromCsv(Path.Combine(homedir,"Dropbox/tests/SowWL_gamedb_07-10-15_08-37-30.csv"), config.headers.gameDB);
                //ScenarioEchelonGameDBAttritionSubRule exampleAttritionRule = new ScenarioEchelonGameDBAttritionSubRule();
                //exampleAttritionRule.dataTable = data;


                ruleManager.CreateAttritionRuleAtRank(scenario.root, ERank.Side, exampleAttritionRule);
                ruleManager.ApplyAttritionRule(scenario);
                RuleManager.PrettyPrint(scenario.root, ERank.Brigade, null);


                List<Terrain> allowedTerrain = new List<Terrain>();
                foreach (Terrain terrain in scenario.map.terrains.Values) {
                    if (terrain.movementFactor < -.5) continue;
                    allowedTerrain.Add(terrain);
                }

                MapArea[,] grid = scenario.map.GetGrid(4, 4, terrainBitmap, allowedTerrain);

                ScenarioEchelon blue = scenario.root.children[0] as ScenarioEchelon;
                ScenarioEchelonDeploymentSubRule blueRule = new ScenarioEchelonDeploymentSubRule();
                blueRule.mapAreas.Add(grid[3,3]);
                ruleManager.CreateDeploymentRuleAtRank(blue, ERank.Side, blueRule);

                ruleManager.ApplyDeploymentRule(scenario);
                RuleManager.PrettyPrint(scenario.root, ERank.Brigade, null);                


                // scenario.RecursiveRunFunc(scenario.root, CreateSideRule);

                //scenario.root.PrettyPrint();




            }
            finally
            {
                Log.CloseUserLog();
            }
            Console.WriteLine();
            Console.WriteLine("Console Done. Press Enter To Continue");
            Console.ReadLine();
        }

    }


}
