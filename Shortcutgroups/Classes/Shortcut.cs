namespace Shortcutgroups.Classes {
    public class Shortcut {
        public string FilePath { get; set; }
        public Shortcut(string filePath) {
            FilePath = filePath;
        }

        // needed for XML serialization
        public Shortcut() {
        }
    }
}
