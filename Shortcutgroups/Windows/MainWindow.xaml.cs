using Shortcutgroups.Classes;
using Shortcutgroups.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Shortcutgroups.Windows {
    public partial class MainWindow: Window {
        private string activeTab;
        public MainWindow() {
            InitializeComponent();

            LblCurrentVersion.Content = GithubData.GetCurrentVersion();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetActiveTab("GroupOverview");
        }

        private void BtnGettingStarted_MouseEnter(object sender, MouseEventArgs e) {
            BtnGettingStarted.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnGettingStarted_MouseLeave(object sender, MouseEventArgs e) {
            if (activeTab != "GettingStarted") {
                BtnGettingStarted.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void BtnGettingStarted_MouseUp(object sender, MouseButtonEventArgs e) {
            SetActiveTab("GettingStarted");
        }

        private void BtnGroupOverview_MouseEnter(object sender, MouseEventArgs e) {
            BtnGroupOverview.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnGroupOverview_MouseLeave(object sender, MouseEventArgs e) {
            if (activeTab != "GroupOverview") {
                BtnGroupOverview.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void BtnGroupOverview_MouseUp(object sender, MouseButtonEventArgs e) {
            SetActiveTab("GroupOverview");
        }

        private List<GroupEditor> groupEditors = new List<GroupEditor>();
        private void SetActiveTab(string tabName, GroupEditor groupEditor = null) {
            activeTab = tabName;

            if (groupEditor != null) {
                Content.Child = groupEditor;
            } else {
                Type type = Type.GetType($"Shortcutgroups.Views.{tabName}");
                UIElement inst = (UIElement)Activator.CreateInstance(type, this);
                Content.Child = inst;
            }

            foreach (Border child in Tabs.Children) {
                child.Background = new SolidColorBrush(Colors.Transparent);
            }

            Border btn = FindBorder(Tabs, $"Btn{tabName}");
            if (btn != null) {
                btn.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
            }
        }

        Border header;
        public void AddTab(string tabName, GroupOverview groupOverview, Group group = null) {
            if (groupEditors.Count == 0) {
                header = new Border() {
                    Height = 30,
                    Margin = new Thickness(10),
                    BorderThickness = new Thickness(0, 1, 0, 1),
                    BorderBrush = new SolidColorBrush(Colors.White)
                };
                TextBlock textBlock = new TextBlock {
                    Text = "Editors",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                header.Child = textBlock;
                Tabs.Children.Add(header);
            }

            if (CheckIfTabExists(tabName) && group != null) {
            } else {
                tabName = AddNewWhenTabExists(tabName);
                string tabNameWithoutSpaces = System.Text.RegularExpressions.Regex.Replace(tabName, @"\s+", "");
                Border tab = new Border() {
                    Name = $"Btn{tabNameWithoutSpaces}",
                    Height = 40
                };
                tab.MouseEnter += Tab_MouseEnter;
                tab.MouseLeave += Tab_MouseLeave;
                tab.MouseLeftButtonUp += Tab_MouseUp;
                TextBlock textBlock = new TextBlock {
                    Text = tabName,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(10, 0, 10, 0),
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                tab.Child = textBlock;
                Tabs.Children.Add(tab);

                if (group != null) {
                    groupEditors.Add(new GroupEditor(groupOverview, tab, group));
                } else {
                    groupEditors.Add(new GroupEditor(groupOverview, tab, tabName));
                }
                SetActiveTab(tabNameWithoutSpaces, groupEditors[groupEditors.Count - 1]);
            }
        }

        public void CloseTab(Border tab, string name) {
            SetActiveTab("GroupOverview");
            Tabs.Children.Remove(tab);
            groupEditors.RemoveAll(x => x.Group.Name == name);
            //Debug.Write("groupeditors: ");
            //foreach (GroupEditor item in groupEditors) {
            //    Debug.Write(item.Group.Name + ", ");
            //}
            //Debug.WriteLine("");
            if (groupEditors.Count == 0) {
                Tabs.Children.Remove(header);
            }
        }

        private bool CheckIfTabExists(string tabName) {
            string name = tabName;
            if (groupEditors.Exists(x => x.Group.Name == tabName)) {
                return true;
            }
            return false;
        }

        private string AddNewWhenTabExists(string tabName) {
            string name = tabName;
            if (groupEditors.Exists(x => x.Group.Name == tabName)) {
                name += "_n";
                return AddNewWhenTabExists(name);
            }
            return name;
        }

        private Border FindBorder(StackPanel parent, string name) {
            foreach (Border child in parent.Children) {
                if (child.Name == name) {
                    return child;
                }
            }
            return null;
        }

        private void Tab_MouseUp(object sender, MouseButtonEventArgs e) {
            string groupName = ((Border)sender).Name.Remove(0, 3);
            var groupEditor = groupEditors.Find(x => System.Text.RegularExpressions.Regex.Replace(x.Group.Name, @"\s+", "") == groupName);
            SetActiveTab(groupName, groupEditor);
        }

        private void Tab_MouseEnter(object sender, MouseEventArgs e) {
            ((Border)sender).Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void Tab_MouseLeave(object sender, MouseEventArgs e) {
            if (activeTab != ((Border)sender).Name.Remove(0, 3)) {
                ((Border)sender).Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void BtnSettings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetActiveTab("Settings");
        }
    }
}
