using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System;

using System.Reflection;


namespace NorbSoftDev.SOW{
    public class Test {
        public static void Main(string [] args) {
            Config config = Config.AutoFind();

            PrintResult( EchelonHelper.ComposeEchelonId(2,1,0,0,0,0));
            PrintResult( EchelonHelper.ComposeEchelonId(1,1,0,0,0,0));

            // ConstructorInfo ctor = tmeplate.type.GetConstructor(new[] { typeof(int) });
                // object instance = ctor.Invoke(new object[] { 10 });
                // http://stackoverflow.com/questions/3255697/using-c-sharp-reflection-to-call-a-constructor

            //Scenario scenario =TestOOB(config);

            // Scenario scenario = TestBlankScenario(config);


            OrderOfBattle oob = new OrderOfBattle(config, config.baseMod, "OOB_SB_Waterloo_Campaign Short");
            ScenarioUnitRoster roster =  new ScenarioUnitRoster(oob, true);


            ScenarioUnitRoster friant = new ScenarioUnitRoster(oob, false);

            roster["OOB_Fr_Louis_Friant"].MoveTo(friant);
            friant["OOB_Fr_Paul_Morvan"].MoveTo(roster);

            // friant.MoveTo(roster["OOB_Fr_Louis_Friant"]);

            // roster.MoveTo(friant["OOB_Fr_Paul_Morvan"]);

            return;


            Scenario scenario = TestUndo(config);

            //Scenario scenario = TestBattleScript(config);
           // PrintResult("---");
            //foreach (string id in scenario.map.objectives.Keys) {
            //    Objective objective = scenario.map.objectives[id];
            //    Console.WriteLine("Objective: {0} {1} {2}",objective.name, objective.x, objective.z);
            //}
            //PrintResult("---");
            //foreach (string id in scenario.objectives.Keys) {
            //    ScenarioObjective ml = scenario.objectives[id];
            //    Console.WriteLine("Map Location: {0} {1} {2}",ml.name, ml.south, ml.east);
            //    Console.WriteLine(ml.ToCsv());
            //}
            //PrintResult("---");


            // TimeEvent ev = scenario.battleScript.events[1] as TimeEvent ;
            // PrintResult(ev.ToCsv());
            // ev.command = config.commandTemplates["Auseroad"].Create(ev.unit, new string [0], 0,0,scenario);
            // PrintResult(ev.ToCsv());
            // ev.command = config.commandTemplates["Amoveto"].Create(scenario["OOB_Fr_GP_Duhesme2"], new string [0], 100, 100, scenario );
            // PrintResult(ev.ToCsv());

            // PrintResult("---");

            // UnitEvent evul = scenario.battleScript.events[13] as UnitEvent;
            // PrintResult(evul.ToCsv());
            // Command newCommand = 
            // evul.command = config.commandTemplates["Amoveto"].Create(scenario["OOB_Fr_Charles_Lefebvre-Desounettes"], new string [0], 100, 100, scenario );
            // PrintResult(evul.ToCsv());


            // Test changeing commands for events
            // foreach ( Event ev in scenario.battleScript.events ) {
            //     IEventWithCommandAndUnit evuc = ev as IEventWithCommandAndUnit;
            //     if (evuc != null) {
            //         evuc.command = config.commandTemplates["dumpdead"].Create(null, new object [0], 0, 0, scenario );
            //         //evuc.command = config.commandTemplates["Aform"].Create(scenario["OOB_Fr_GP_Duhesme2"], new object [] { config.formations["DRIL_Lvl4_Inf_DoubleLine"] }, 0, 0, scenario );
            //         //evuc.command = config.commandTemplates["Amoveto"].Create(scenario["OOB_Fr_GP_Duhesme2"], new string [0], 100, 100, scenario );
            //     }
            //     PrintResult(ev.ToCsv());
            // }


            // Test Save and Load
            scenario.name = "IOTest";
            scenario.mod = config.userMod;
            scenario.Save();
            PrintResult("Saved");

            // foreach (Graphic g in config.graphics.Values) {
            //     //ONLY convert sprites and flags?
            //     PrintResult(g.ImageMagickExtractCommand(config));
            //     // PrintResult(g.ImageMagickExtract(config));
            // }

            //EventSelectionSet aset = new EventSelectionSet();
            //aset.Add(scenario.battleScript.events[0]);
            //aset.Add(scenario.battleScript.events[1]);

            //DataObject dataObject = new DataObject ( aset);

            //// EventSelectionSet newset = dataObject.Get<EventSelectionSet,Event>();
            //EventSelectionSet newset = dataObject.GetData<EventSelectionSet>();

            //foreach (Event e in newset) {
            //    Console.WriteLine(e);
            //}

            // scenario = new Scenario(config, config.userMod, "IOTest");
            // scenario.Load();
            // PrintResult("Reload Complete");




        }


        static Scenario TestBattleScript(Config config) {
            PrintResult("---");

            Scenario scenario = new Scenario(config, config.GetModByName("Waterloo Benchmarking"), "Waterloo 50 K complex");
            //Scenario scenario = new Scenario(config, config.GetModByName("Gettysburg"), "AN01-Sept17-The Cornfield (C-Brig)");//"Waterloo volley fire test");
            scenario.Load();

            // scenario.orderOfBattle.PrettyPrint();

            PrintResult("---");

            Console.WriteLine(scenario.battleScript.Csv());

            // Command command = config.commandTemplates["night"].Create();
            // PrintResult("Command:"+command.ToCsv());

            // ScenarioUnit unit = scenario["OOB_Fr_JeanMartin_Petit"];
            // CommandTemplate template = config.commandTemplates["aboutface"];
            // command = (Command) Activator.CreateInstance(template.type, template.name, template.help, unit);
            // PrintResult("Command:"+command.ToCsv());
            // PrintResult("---");

            return scenario;
        }

        static Scenario TestUndo(Config config) {
            Scenario scenario = TestNewScenario(config);

            PrintResult("---");

            ScenarioUnit unit = scenario["OOB_Fr_JeanMartin_Petit"];

            // TestUndo
            ScenarioUndoStack undo = new ScenarioUndoStack(scenario, 10);
            undo.scenario_SaveBulkState("test");
            undo.active = true;

            PrintResult("Initial Events");
            Console.WriteLine(scenario.battleScript.Csv());

            //BattleScriptEvent bEvent = scenario.battleScript.AddNew(0,"test add","evtcont", unit, "aboutface", null, 0,0);
            //PrintResult(bEvent.ToCsv());
            //PrintResult("Changed Events");
            //Console.WriteLine(scenario.battleScript.Csv());

            undo.Undo();
            PrintResult("Undo Events");
            Console.WriteLine(scenario.battleScript.Csv());

            undo.Redo();
            PrintResult("Redo Events");
            Console.WriteLine(scenario.battleScript.Csv());

            UnitEvent uEvent = (UnitEvent) scenario.battleScript.events[0];
            uEvent.unit = scenario["OOB_Fr_1_Gren_1_Btn"];
            PrintResult("Changed Event Item");
            Console.WriteLine(scenario.battleScript.Csv());

            undo.Undo();
            PrintResult("Undo Events");
            Console.WriteLine(scenario.battleScript.Csv());





            return scenario;

            PrintResult("-- Initial headCount "+unit.headCount);
            unit.headCount  += 10;
            PrintResult("-- Changed headCount "+unit.headCount);
            unit.headCount  += 10;
            PrintResult("-- Changed headCount "+unit.headCount);
            undo.Undo();
            PrintResult("-- After Undo headCount "+unit.headCount);            
            undo.Undo();
            PrintResult("-- After Undo headCount "+unit.headCount);
            undo.Redo();
            PrintResult("-- After Redo headCount "+unit.headCount);
            undo.Redo();
            PrintResult("-- After Redo headCount "+unit.headCount);
            PrintResult("---");

            {
                ScenarioEchelon echelon = (ScenarioEchelon) scenario["OOB_Fr_1_Gren_1_Btn"].echelon;
                PrintResult("-- Initial parent "+echelon.parent);
                echelon.parent = (ScenarioEchelon) scenario["OOB_Fr_Joseph_Christiani"].echelon;

                PrintResult("-- Changed parent "+echelon.parent);
                // scenario.PrettyPrint();
                undo.Undo();
                PrintResult("-- After Undo parent "+echelon.parent);
                undo.Redo();
                PrintResult("-- After Redo parent "+echelon.parent);
                
                PrintResult("---");
            }

            {
                undo.Clear();
                undo.oob_SaveBulkState("Test");

                OOBEchelon echelon = (OOBEchelon) scenario.orderOfBattle["OOB_Fr_1_Gren_1_Btn"].echelon;
                PrintResult("-- Initial parent "+echelon.parent);
                echelon.parent = (OOBEchelon) scenario.orderOfBattle["OOB_Fr_Joseph_Christiani"].echelon;

                PrintResult("-- Changed parent "+echelon.parent);
                // scenario.PrettyPrint();
                undo.Undo();
                PrintResult("-- After Undo parent "+echelon.parent);
                undo.Redo();
                PrintResult("-- After Redo parent "+echelon.parent);
                
                PrintResult("---");
            }


            return scenario;

            // scenario.Save();

            // PrintResult("");
            // Scenario scenario = TestLoadScenario(config);




            // ScenarioUnit unit = scenario["OOB_Fr_JeanMartin_Petit"];
            // ScenarioEchelon echelon = (ScenarioEchelon) unit.echelon;
            // Formation.Location [] positionYds;
            // foreach (string name in new [] { unit.formation.id, "DRIL_Lvl5_Inf_Square"}) {
            //     Formation formation = config.formations[name];
            //     PrintResult(formation.id);
            //     formation.BridageComputeChildPositionsYds(echelon, out positionYds);
            //     for (int i = 0; i < positionYds.Length; i++) {
            //         PrintResult("[{0,-3}] {1,7:F2}, {2,7:F2} | {3,7:F2}, {4,7:F2}",
            //             i+1, positionYds[i].position.X, positionYds[i].position.Y,  positionYds[i].direction.X, positionYds[i].direction.Y );
            //     }
            //     PrintResult("---");
            // }



            // unit = scenario["OOB_Fr_1_Gren_1_Btn"];
            // echelon = (ScenarioEchelon) unit.echelon;
            // unit.formation.ComputeSpritePositionsYds(echelon, out positionYds);
            // for (int i = 0; i < positionYds.Length; i++) {
            //     PrintResult("[{0,-3}] {1,5:F2}, {2,5:F2} {3,5:F2}, {4,5:F2}",
            //         i+1, positionYds[i].position.X, positionYds[i].position.Y,  positionYds[i].direction.X, positionYds[i].direction.Y );
            // }


            // float dirSouth = -1; float dirEast = 0; float facing = WorldTransform.DirToFacing(dirSouth, dirEast);

            // PrintResult("North " + dirSouth + "," + dirEast + " = " + facing +" = "+ WorldTransform.FacingToSouth(facing)+","+ WorldTransform.FacingToEast(facing));

            // dirSouth = 0; dirEast = 1; facing = WorldTransform.DirToFacing(dirSouth, dirEast);
            // PrintResult("East " + dirSouth + "," + dirEast + " = " + facing + " = " + WorldTransform.FacingToSouth(facing) + "," + WorldTransform.FacingToEast(facing));

            // dirSouth = 1; dirEast = 0; facing = WorldTransform.DirToFacing(dirSouth, dirEast);
            // PrintResult("South " + dirSouth + "," + dirEast + " = " + facing + " = " + WorldTransform.FacingToSouth(facing) + "," + WorldTransform.FacingToEast(facing));

            // dirSouth = 0; dirEast = -1; facing = WorldTransform.DirToFacing(dirSouth, dirEast);
            // PrintResult("West " + dirSouth + "," + dirEast + " = " + facing + " = " + WorldTransform.FacingToSouth(facing) + "," + WorldTransform.FacingToEast(facing));

            // foreach (Weapon w in config.weapons.Values) {
            //     PrintResult(w);
            // }

            // foreach (Attribute a in config.attributes.Values) {
            //     PrintResult(a.name);
            //     foreach( AttributeLevel level  in a) {
            //     PrintResult(" "+level+" |" +level.definedIn);
            //     }
            // }
        }

        public static void PrintResult(object result) {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(result.ToString());
            Console.ResetColor();
        }

        public static void TestOOB(Config config) {
            int cnt = 0;
            int fails = 0;

            // string filepath = Path.Combine(config.SOWDir,"Mods","Waterloo","OOBs");
            Mod mod = config.baseMod; //config.GetMod( new DirectoryInfo(filepath) );

            OrderOfBattle orderOfBattle = new OrderOfBattle(config, mod, "OOB_SB_Waterloo_Campaign Very Short");
            orderOfBattle.Load();
            orderOfBattle.PrettyPrint();
            PrintResult("OOB Stats: "+orderOfBattle.stats);
            PrintResult("oob isDirty:"+orderOfBattle.isDirty);

            NotifyTester notifyTester = new NotifyTester();

            // foreach (OOBEchelon side in orderOfBattle.root) {
            //     foreach (OOBEchelon army in side) {
            //         army.unit.PropertyChanged += notifyTester.OnChanged;
            //         army.unit.name1 = "ppop";
            //     }
            // }
            
            string key = "OOB_Fr_1_Gren_1_Btn";
            // test dirty
            OOBUnit unit = orderOfBattle[key];
            PrintResult("oob isDirty:"+orderOfBattle.isDirty);
            unit.ammo = 9999;
            PrintResult("oob isDirty:"+orderOfBattle.isDirty);

            OOBEchelon parent = (OOBEchelon)((OOBEchelon)unit.echelon).parent;
            OOBEchelon newe = orderOfBattle.CreateChild(parent);
            orderOfBattle.PrettyPrint();
            PrintResult("OOB Stats: "+orderOfBattle.stats);


            unit.name1 = "Old";
            newe.unit.name1 = "Neeew";
            orderOfBattle.PrettyPrint();
            PrintResult("OOB Stats: "+orderOfBattle.stats);
            orderOfBattle.PrettyPrintUnits();



            // test reparenting
            // OOBUnit unit = orderOfBattle[key];
            // OOBEchelon oldparent = (OOBEchelon)unit.oobEchelon.parent;
            // PrintResult();
            // oldparent.PrettyPrint();
            // PrintResult("oob isDirty:"+orderOfBattle.isDirty);


            // OOBUnit parent = orderOfBattle["OOB_Pr_x_von_Schallern"];
            // PrintResult();
            // unit.oobEchelon.parent = parent.oobEchelon;
            // parent.oobEchelon.PrettyPrint();
            // PrintResult("oob isDirty:"+orderOfBattle.isDirty);
            // PrintResult();
            // oldparent.PrettyPrint();




            // foreach (string fp in config.EnumerateOrderOfBattleFiles()) {
            //     cnt++;
            //     // try {
            //     Log.Info();
            //     OrderOfBattle orderOfBattle = new OrderOfBattle(config, "", Path.GetFileNameWithoutExtension(fp));
            //     orderOfBattle.ReadCsv(fp);
            //     // } catch (Exception e) {
            //     //     Log.Info(e.Message);
            //     //     fails++;
            //     // }
            //     // Log.Info();
            //     orderOfBattle.PrettyPrint();
            //     Environment.Exit(0);
            // }
            // Log.Info(this,"Failed:"+fails+"/"+cnt);

        }

        public static Scenario TestNewScenario(Config config) {
            Mod mod = config.baseMod;
            OrderOfBattle orderOfBattle = new OrderOfBattle(config, mod, "OOB_SB_Waterloo_Campaign_Very_Short");
            Scenario scenario = new Scenario(orderOfBattle, true);
            // scenario.PrettyPrint();
            PrintResult("OOB Stats: "+orderOfBattle.stats);


            return scenario;

            //NotifyTester notifyTester = new NotifyTester();

            //foreach (ScenarioEchelon side in scenario.root.children) {
            //    foreach (ScenarioEchelon army in side.children) {
            //        army.unit.PropertyChanged += notifyTester.OnChanged;
            //    }
            //}

            //foreach (OOBEchelon side in orderOfBattle.root.children) {
            //    foreach (OOBEchelon army in side.children) {
            //        army.unit.PropertyChanged += notifyTester.OnChanged;
            //        army.unit.ammo = 100;
            //    }
            //}

            // scenario.Save();

            // // reload it
            // Scenario rescenario = new Scenario(config, scenario.mod , scenario.name );
            // rescenario.Load();

            // scenario.orderOfBattle.mod = config.userMod;
            // scenario.orderOfBattle.name = "TestOut";
            // scenario.orderOfBattle.Save();

            // orderOfBattle = new OrderOfBattle(config, config.userMod, "TestOut");
            // orderOfBattle.Load();

            // PrintResult("Deep Copy");
            // Scenario copy = scenario.Copy();
            // PrintResult("Done Copy");

            // PrintResult("Clone, Save, and Reload");
            // Scenario clone = scenario.ShallowCopy();
            // clone.name = "Clone";
            // clone.Save();
            // clone.Clear();
            // clone.Load();
            // PrintResult("Done Clone, Save, and Reload");
            // PrintResult(scenario.name);

        }

        public static Scenario TestBlankScenario(Config config) {
            Mod mod = config.baseMod;
            OrderOfBattle orderOfBattle = new OrderOfBattle(config, mod, "OOB_SB_Waterloo_Campaign Very Short");
            orderOfBattle.Load();
            orderOfBattle.PrettyPrint();

            Scenario scenario = new Scenario(orderOfBattle, false);
            scenario.PrettyPrint();
            PrintResult("OOB Stats: "+orderOfBattle.stats);

            return scenario;

            // string key = "OOB_Fr_JeanMartin_Petit";// "OOB_Fr_1_Gren_1_Btn";
            // OOBUnit oobUnit = orderOfBattle[key];
            // ScenarioUnit newUnit = scenario.InsertUnitWithChildren(oobUnit);
            // scenario.PrettyPrint();

            // oobUnit = orderOfBattle["OOB_Br_2_Ft_GD_2_Btn"];

            // scenario.InsertUnitWithChildren(oobUnit, newUnit.scenarioEchelon );
            // // scenario.PrettyPrint();

            // return scenario;



        }

        public static Scenario TestLoadScenario(Config config)
        {

            Scenario scenario = new Scenario(config, config.userMod, "UnnamedScenario");
            scenario.Load();
            scenario.PrettyPrint();
            return scenario;

        }

        //public static void TestOOB(Config config)
        //{
        //    OrderOfBattle oob = new OrderOfBattle(config, config.baseMod, "UnnamedScenario");
        //    oob.Load();
        //   oob.PrettyPrint();
        //}

        public static void TestScenarios(Config config) {
            int cnt = 0;
            int fails = 0;

            string filepath;
            Scenario scenario;

            // filepath = "../Scourge of War - Gettysburg/Work/SDK/Scenarios/GB MP01-July1-McPherson's Ridge (Div)/scenario.csv";
            // filepath = Path.Combine(config.SOWDir,"Mods","Waterloo","Scenarios","Scaling French", "scenario.csv");
            filepath = Path.Combine(config.SOWDir,"");

            scenario = new Scenario(config, null , Path.GetFileNameWithoutExtension(filepath) );
            scenario.ReadCsv(filepath);
            scenario.PrettyPrint();


            // foreach (string fp in config.EnumerateScenarioFiles()) {
            //     cnt++;
            //     // try {
            //     Log.Info();
            //     scenario = new Scenario(config, "", Path.GetFileNameWithoutExtension(fp));
            //     scenario.ReadCsv(fp);
            //     // } catch (Exception e) {
            //     //     Log.Info(e.Message);
            //     //     fails++;
            //     // }
            //     // Log.Info();
            //     scenario.orderOfBattle.PrettyPrint();
            //     Environment.Exit(0);
            // }
            // Log.Info(this,"Failed:"+fails+"/"+cnt);

        	// OrderOfBattle oob = new OrderOfBattle(config, "", "test");
         //    Scenario scenario = new Scenario(config, "", "test");
         //    scenario._orderOfBattle = oob;

        	// oob.ReadCsv("../Scourge of War - Gettysburg/Work/SDK/OOBs/oob_Gettysburg_Day1_Jackson.csv"); // has 3 "1,1,1,0,0,0"
         //    scenario.ReadCsv("../Scourge of War - Gettysburg/Work/SDK/Scenarios/GB06-July1-Three Times is Not a Charm (C-Corps)/scenario.csv");
        }
    }

    public class NotifyTester {
        public void OnChanged(object sender, PropertyChangedEventArgs e) {
            Console.WriteLine("NotifyTester "+sender+" changed "+e);
        }
    }
}