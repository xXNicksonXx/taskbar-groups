using Shell32;
using Shortcutgroups.Classes;
using Shortcutgroups.Views;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shortcutgroups.Controls {
    public partial class CtlEditorShortcut: UserControl {
        private GroupEditor GroupEditor;
        private Shortcut Shortcut;
        public CtlEditorShortcut(GroupEditor groupEditor, Shortcut shortcut) {
            InitializeComponent();
            GroupEditor = groupEditor;
            Shortcut = shortcut;

            string imageExtension = Path.GetExtension(Shortcut.FilePath).ToLower();

            // Checks if the shortcut actually exists; if not then display an error image
            if (File.Exists(Shortcut.FilePath)) {
                if (imageExtension == ".lnk") {
                    LblShortcut.Content = HandleExtensionName(Shortcut.FilePath);
                    ImgShortcut.Source = GroupEditor.HandleSpecialImageExtensions(Shortcut.FilePath, imageExtension);
                } else {
                    LblShortcut.Content = Path.GetFileNameWithoutExtension(Shortcut.FilePath);
                    ImgShortcut.Source = new BitmapImage(new Uri($@"{Paths.GroupsPath}\{Name}\GroupImage.png"));
                }
            } else if (Directory.Exists(Shortcut.FilePath)) {
                LblShortcut.Content = Path.GetFileName(Shortcut.FilePath);
                ImgShortcut.Source = HandleFolder.GetFolderImage(Shortcut.FilePath);
            } else {
                ImgShortcut.Source = Paths.GetResource("Error");
            }
        }

        public static string HandleExtensionName(string file) {
            string fileName = Path.GetFileName(file);
            file = Path.GetDirectoryName(Path.GetFullPath(file));
            Shell shell = new Shell();
            Folder shellFolder = shell.NameSpace(file);
            FolderItem shellItem = shellFolder.Items().Item(fileName);

            return shellItem.Name;
        }

        private void ControlEditorShortcut_MouseEnter(object sender, MouseEventArgs e) {
            ControlEditorShortcut.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void ControlEditorShortcut_MouseLeave(object sender, MouseEventArgs e) {
            ControlEditorShortcut.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnDelete_MouseEnter(object sender, MouseEventArgs e) {
            BtnDelete.Background = new SolidColorBrush(Color.FromRgb(60, 60, 70));
        }

        private void BtnDelete_MouseLeave(object sender, MouseEventArgs e) {
            BtnDelete.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnDelete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GroupEditor.Group.ShortcutList.Remove(Shortcut);
            GroupEditor.LoadShortcuts();
        }
    }
}
