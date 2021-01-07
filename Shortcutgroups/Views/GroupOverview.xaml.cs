using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Shortcutgroups.Classes;
using Shortcutgroups.Controls;
using Shortcutgroups.Windows;

namespace Shortcutgroups.Views {
    public partial class GroupOverview: UserControl {
        public MainWindow MainWindow;

        public GroupOverview(MainWindow mainWindow) {
            // Setting from profile
            ProfileOptimization.StartProfile("GroupOverview.Profile");

            InitializeComponent();

            MainWindow = mainWindow;

            Reload();
        }

        public void Reload() {
            // flush and reload existing groups
            Groups.Children.Clear();

            string configPath = $@"{Paths.GroupsPath}";
            string[] groups = Directory.GetDirectories(configPath);

            foreach (string group in groups) {
                try {
                    LoadGroup(group);
                } catch (IOException ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void LoadGroup(string path) {
            Group group = new Group(path);
            CtlGroup ctlGroup = new CtlGroup(this, group, MainWindow);
            Groups.Children.Add(ctlGroup);
        }

        private void BtnAddGroup_MouseEnter(object sender, MouseEventArgs e) {
            BtnAddGroup.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void BtnAddGroup_MouseLeave(object sender, MouseEventArgs e) {
            BtnAddGroup.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnAddGroup_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.AddTab("NewGroup", this);
        }
    }
}
