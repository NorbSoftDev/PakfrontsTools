using NorbSoftDev.SOW;
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

namespace ScenarioEditor
{
    /// <summary>
    /// Interaction logic for NewScenarioWindow.xaml
    /// </summary>
    public partial class NewScenarioWindow : Window
    {
        SOWScenarioEditorWindow mainWindow;

        ModItem selectedMap, selectedOob;

        List<ModItem> maps, oobs;

        Random random = new Random();

        internal class ModItem
        {

            internal ModItem(Mod mod, string path) : this(mod, path,  System.IO.Path.GetFileName(path))
            {
            }

            internal ModItem(Mod mod, string path, string name) 
            {
                this.mod = mod;
                this.name = name;
                this.path = path;
            }

            public string name { get; set; }
            public string path { get; set; }
            public Mod mod { get; set; }
            public override string ToString() { return name;  }
        }
        public NewScenarioWindow(SOWScenarioEditorWindow config)
        {
            InitializeComponent();
            maps = new List<ModItem>();
            oobs = new List<ModItem>();
            this.mainWindow = config;
            this.mainWindow.PropertyChanged += mainWindow_PropertyChanged;
            Refresh();
        
        }

        void mainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Refresh();
        }

        void Refresh()
        {

            mapsList.Items.Clear();
            modsList.Items.Clear();
            oobsList.Items.Clear();
            selectedOob = null;
            selectedMap = null;
            currentMap.Content = "";
            currentOob.Content = "";

            if (mainWindow.config == null) return;
            //wierd but using menu items works and seems ok

            //Maps
            foreach (Mod mod in mainWindow.config.mods)
            {

                //Defualt mods are always active
                UserMod userMod = mod as UserMod;
                if (userMod != null)
                {

                    MenuItem modMenu = new MenuItem();
                    modMenu.Header = "[" + userMod.index + " ] " + userMod.name;
                    modMenu.DataContext = userMod;
                    modMenu.IsCheckable = true;
                    modMenu.IsChecked = userMod.active;
                    modsList.Items.Add(modMenu);
                    modMenu.Click += mainWindow.ModClicked;
                }

                if (mod.active)
                {
                    foreach (string filepath in mod.GetMaps())
                    {
                        ModItem moditem = new ModItem(mod, filepath);
                        ListViewItem lvi = new ListViewItem();
                        lvi.Content = moditem;
                        //lvi.MouseDoubleClick += moditem_MouseDoubleClick;
                        lvi.PreviewMouseUp += mapitem_MouseUp;
                        mapsList.Items.Add(lvi);
                        maps.Add(moditem);
                    }

                    foreach (string filepath in mod.GetOOBs())
                    {
                        ModItem moditem = new ModItem(mod, filepath);
                        ListViewItem lvi = new ListViewItem();
                        lvi.Content = moditem;
                        lvi.PreviewMouseUp += oobitem_MouseUp;
                        oobsList.Items.Add(lvi);
                        oobs.Add(moditem);
                    }
                }
            }

        }

        void mapitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            selectedMap = lvi.Content as ModItem;
            currentMap.Content = selectedMap.name;
        }

        void oobitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            selectedOob = lvi.Content as ModItem;
            currentOob.Content = selectedOob.name;
        }


        protected void randomMap_Click(object sender, RoutedEventArgs e)
        {
            selectedMap = maps[random.Next(maps.Count)];
            currentMap.Content = selectedMap.name;
        }

        protected void randomOOB_Click(object sender, RoutedEventArgs e)
        {
            selectedOob = oobs[random.Next(oobs.Count)];
            currentOob.Content = selectedOob.name;
        }

        protected void accept_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOob == null) return;

            mainWindow.NewScenario(selectedOob.path, selectedMap == null ? null : selectedMap.path);

            this.Close();

        }

        protected void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
