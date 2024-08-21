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
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {

        SOWScenarioEditorWindow mainWindow;

        public HelpWindow(SOWScenarioEditorWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            string help = mainWindow.config.GetResourceFile("Help\\index.html");
            var uri = new System.Uri(help);
            var converted = uri.AbsoluteUri;
            helpWebBrowser.Navigate(uri);
        }

        void OnTopClick(object sender, RoutedEventArgs e) {
            this.Topmost = !this.Topmost;
                                OnTopMenu.IsCheckable = true;
                    OnTopMenu.IsChecked = this.Topmost;
        }
    }
}
