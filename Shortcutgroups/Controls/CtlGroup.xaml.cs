using Shortcutgroups.Classes;
using Shortcutgroups.Views;
using Shortcutgroups.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Shortcutgroups.Controls {
    public partial class CtlGroup: UserControl {
        private GroupOverview GroupOverview;
        private Group Group;
        private MainWindow MainWindow;

        public CtlGroup(GroupOverview groupOverview, Group group, MainWindow mainWindow) {
            InitializeComponent();
            GroupOverview = groupOverview;
            Group = group;
            MainWindow = mainWindow;

            LblBackgroundColor.Background = new SolidColorBrush(ImageFunctions.FromString(Group.BackgroundColor));
            ImgGroupIcon.Source = Group.LoadGroupImage();
            LblGroupname.Text = Group.Name;

            if (!Directory.Exists($@"{Paths.GroupsPath}\{Group.Name}\Icons\")) {
                Group.CacheImages();
            }

            foreach (Shortcut shortcut in Group.ShortcutList) {
                CreateShortcut(shortcut);
            }
        }

        private void CreateShortcut(Shortcut shortcut) {
            // creating shortcut picturebox from shortcut
            var shortcutPanel = new Image {
                Height = 25,
                Width = 25,
                Source = Group.LoadImageFromCache(shortcut.FilePath)
            };
            PnlShortcutIcons.Children.Add(shortcutPanel);
        }

        private void ControlGroup_MouseEnter(object sender, MouseEventArgs e) {
            ControlGroup.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void ControlGroup_MouseLeave(object sender, MouseEventArgs e) {
            ControlGroup.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnEditGroup_MouseEnter(object sender, MouseEventArgs e) {
            BtnEditGroup.Background = new SolidColorBrush(Color.FromRgb(60, 60, 70));
        }

        private void BtnEditGroup_MouseLeave(object sender, MouseEventArgs e) {
            BtnEditGroup.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnEditGroup_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.AddTab(Group.Name, GroupOverview, Group);
        }

        private void BtnOpenFolder_MouseEnter(object sender, MouseEventArgs e) {
            BtnOpenFolder.Background = new SolidColorBrush(Color.FromRgb(60, 60, 70));
        }

        private void BtnOpenFolder_MouseLeave(object sender, MouseEventArgs e) {
            BtnOpenFolder.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnOpenFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            // Build path based on the directory of the main .exe file
            string filePath = Path.GetFullPath(new Uri($@"{Paths.ShortcutsPath}\{Group.Name}.lnk").LocalPath);

            // Open directory in explorer and highlight file
            Process.Start("explorer.exe", $"/select, \"{filePath}\"");
        }
    }
}
