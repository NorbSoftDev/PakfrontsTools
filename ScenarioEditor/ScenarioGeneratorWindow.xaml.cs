using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using NorbSoftDev.SOW;
using NorbSoftDev.SOW.Utils;
using System.Drawing;

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for GeneratorWindow.xaml
    /// </summary>
    public partial class ScenarioGeneratorWindow : Window
    {
   
       public Scenario scenario;
       RuleManager ruleManager;

       public ScenarioGeneratorWindow(Scenario other)
        {
            InitializeComponent();

            //scenario.PopulateUnitsFromOrderOfBattle();
            this.scenario = new Scenario(other);

            if (other.Count < 1)
            {
                this.scenario.PopulateUnitsFromOrderOfBattle();
            }
            else
            {
                this.scenario.PopulateUnitsFromScenarioUnitRoster(other);
            }

            this.DataContext = this.scenario;
            mapPanel.Scenario = this.scenario;

            // Missing DataTable and GameDb classes
            //ruleManager = new RuleManager();

            //DataTable<GameDBEntry> data = new DataTable<GameDBEntry>(scenario.config);
            ////data.ReadFromCsv("/home/tims/Dropbox/tests/SowWL_gamedb_07-10-15_08-37-30.csv", scenario.config.headers.gameDB);
            //ScenarioEchelonGameDBAttritionSubRule exampleAttritionRule = new ScenarioEchelonGameDBAttritionSubRule();
            //exampleAttritionRule.dataTable = data;
            //ruleManager.CreateAttritionRuleAtRank(scenario.root, ERank.Side, exampleAttritionRule);

            //ScenarioEchelonCreationSubRule exampleCreationRule = new ScenarioEchelonCreationSubRule();
            //ruleManager.CreateCreationRuleAtRank(scenario.root, ERank.Side, exampleCreationRule);

            //List<Terrain> allowedTerrain = new List<Terrain>();
            //foreach (Terrain terrain in scenario.map.terrains.Values)
            //{
            //    if (terrain.movementFactor < -.5) continue;
            //    allowedTerrain.Add(terrain);
            //}

            //TerrainBitmap terrainBitmap = new TerrainBitmap(scenario.map);
            //MapArea[,] grid = scenario.map.GetGrid(4, 4, terrainBitmap, allowedTerrain);

            //MapArea[] areas = new MapArea[] { grid[0, 0], grid[3, 3], grid[0, 2] };
            //float [] facings = new float [] { 180, 0, 90 };

            //for (int i = 0; i <  scenario.root.children.Count; i++) 
            //{
            //    ScenarioEchelon side = scenario.root.children[i] as ScenarioEchelon;
            //    Log.Info(this, "Setting Deploy rule for " + i + " " + side.ToString());
            //    ScenarioEchelonDeploymentSubRule deployRule = new ScenarioEchelonDeploymentSubRule();
            //    deployRule.mapAreas.Add( areas[i] );
            //    deployRule.facing = facings[i];
            //    ruleManager.CreateDeploymentRuleAtRank(side, ERank.Side, deployRule);
            //}

            // old
            //ScenarioEchelon red = scenario.root.children[1] as ScenarioEchelon;
            //ScenarioEchelonDeploymentSubRule redRule = new ScenarioEchelonDeploymentSubRule();
            //redRule.mapAreas.Add(grid[0, 0]);
            //ruleManager.CreateDeploymentRuleAtRank(red, ERank.Corps, redRule);




        }

 

        //public NewScenarioWindow(SOWScenarioEditorWindow config)
        //{
        //    InitializeComponent();

        //    this.mainWindow = config;
        //    this.mainWindow.PropertyChanged += mainWindow_PropertyChanged;
        //    Refresh();
        
        //}

//        void mainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
//        {
//            Refresh();
//        }

//        void Refresh()
//        {



////            if (mainWindow.config == null) return;

//            //Maps
//            foreach (Mod mod in scenario.config.mods)
//            {

//                //Defualt mods are always active
//                UserMod userMod = mod as UserMod;
//                if (userMod != null)
//                {

//                    MenuItem modMenu = new MenuItem();
//                    modMenu.Header = "[" + userMod.index + " ] " + userMod.name;
//                    modMenu.DataContext = userMod;
//                    modMenu.IsCheckable = true;
//                    modMenu.IsChecked = userMod.active;
//                    modsList.Items.Add(modMenu);
//                }

//                if (mod.active)
//                {
//                    foreach (string filepath in mod.GetMaps())
//                    {
//                        ModItem moditem = new ModItem(mod, filepath);
//                        ListViewItem lvi = new ListViewItem();
//                        lvi.Content = moditem;
//                        //lvi.MouseDoubleClick += moditem_MouseDoubleClick;
//                        lvi.PreviewMouseUp += mapitem_MouseUp;
//                        mapsList.Items.Add(lvi);
//                    }

//                    foreach (string filepath in mod.GetOOBs())
//                    {
//                        ModItem moditem = new ModItem(mod, filepath);
//                        ListViewItem lvi = new ListViewItem();
//                        lvi.Content = moditem;
//                        lvi.PreviewMouseUp += oobitem_MouseUp;
//                        oobsList.Items.Add(lvi);
//                    }
//                }
//            }

//        }

//        void mapitem_MouseUp(object sender, MouseButtonEventArgs e)
//        {
//            ListViewItem lvi = sender as ListViewItem;
//            selectedMap = lvi.Content as ModItem;
//            currentMap.Content = selectedMap.name;
//        }

//        void oobitem_MouseUp(object sender, MouseButtonEventArgs e)
//        {
//            ListViewItem lvi = sender as ListViewItem;
//            selectedOob = lvi.Content as ModItem;
//            currentOob.Content = selectedOob.name;
//        }

       protected void create_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyAllRules(scenario);

       }

       protected void attrite_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyAttritionRule(scenario);
       }

       protected void prune_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyPruneRule(scenario);
       }

       protected void trim_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyTrimRule(scenario);
       }

       protected void deploy_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyDeploymentRule(scenario);
       }

       protected void all_Click(object sender, RoutedEventArgs e)
       {
           ruleManager.ApplyAllRules(scenario);

       }
        protected void accept_Click(object sender, RoutedEventArgs e)
        {
            if (   System.Windows.Interop.ComponentDispatcher.IsThreadModal )
                this.DialogResult = true;

            this.Close();

        }

        protected void close_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                this.DialogResult = false;
            this.Close();
        }

        private void stv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void stv_DragOver(object sender, DragEventArgs e)
        {

        }

        private void stv_Drop(object sender, DragEventArgs e)
        {

        }

        private void stv_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void stv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void stv_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    
    
    }

}
