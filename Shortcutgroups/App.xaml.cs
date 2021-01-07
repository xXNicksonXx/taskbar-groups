using System.Windows;
using System.Reflection;
using System.IO;
using System;
using System.Windows.Forms;
using Shortcutgroups.Windows;
using Shortcutgroups.Classes;
using System.Runtime;

namespace Shortcutgroups {
    public partial class App: System.Windows.Application {
        public static string[] arguments = Environment.GetCommandLineArgs();

        private void Application_Startup(object sender, StartupEventArgs e) {
            // Add necessary folders
            Directory.CreateDirectory(Paths.GroupsPath);
            Directory.CreateDirectory(Paths.ShortcutsPath);

            // Add ProfileOptimization (increases performace)
            Directory.CreateDirectory(Paths.JITPath);
            ProfileOptimization.SetProfileRoot(Paths.JITPath);

            if (arguments.Length > 1) {
                int cursorX = Cursor.Position.X;
                int cursorY = Cursor.Position.Y;

                Shortcutgroup shortcutgroup = new Shortcutgroup(arguments[1], cursorX, cursorY);
                shortcutgroup.Show();
            } else {
                new MainWindow().Show();
            }
        }
    }
}
