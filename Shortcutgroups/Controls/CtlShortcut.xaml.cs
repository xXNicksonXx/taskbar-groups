using Shortcutgroups.Classes;
using Shortcutgroups.Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Shortcutgroups.Controls {
    public partial class CtlShortcut: UserControl {
        private Shortcutgroup Shortcutgroup;
        private Shortcut Shortcut;
        private Group Group;
        public CtlShortcut(Shortcutgroup shortcutgroup, Shortcut shortcut, Group group) {
            InitializeComponent();
            Shortcutgroup = shortcutgroup;
            Shortcut = shortcut;
            Group = group;

            ImgShortcut.Source = Group.LoadImageFromCache(Shortcut.FilePath);
        }

        public void ControlShortcut_MouseEnter(object sender, MouseEventArgs e) {
            ControlShortcut.Background = Shortcutgroup.HoverColor;
        }

        public void ControlShortcut_MouseLeave(object sender, MouseEventArgs e) {
            ControlShortcut.Background = new SolidColorBrush(Colors.Transparent);
        }

        public void ControlShortcut_MouseUp(object sender, MouseButtonEventArgs e) {
            OpenFile(Shortcut.FilePath);
        }

        // open file
        public void OpenFile(string path) {
            Process process = new Process();
            process.EnableRaisingEvents = false;
            process.StartInfo.FileName = path;

            try {
                process.Start();
            } catch (Exception Ex) {
                MessageBox.Show(Ex.Message);
            }
        }
    }
}
