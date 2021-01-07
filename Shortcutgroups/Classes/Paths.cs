using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Shortcutgroups.Classes {
    public class Paths {
        public static string Path;

        public static string ExePath;

        public static string GroupsPath;

        public static string ShortcutsPath;

        public static string JITPath;

        static Paths() {
            ExePath = Assembly.GetExecutingAssembly().Location;
            Path = Directory.GetParent(ExePath).ToString();
            GroupsPath = $@"{Path}\Groups";
            ShortcutsPath = $@"{Path}\Shortcuts";
            JITPath = $@"{Path}\JITComp";
        }

        public static BitmapImage GetResource(string resourceName) {
            string path = $"pack://application:,,,/Shortcutgroups;component/Resources/{resourceName}.png";
            return new BitmapImage(new Uri(path));
        }
    }
}
