using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Shortcutgroups.Classes {
    public enum BackgroundOption {
        Light,
        Dark,
        WindowsTheme,
        WindowsAccentColor,
        Custom
    }
    public class Group {
        public string Name;
        public int Width;
        public List<Shortcut> ShortcutList;
        public BackgroundOption SelectedBackgroundOption;
        public string BackgroundColor;
        public int Opacity;

        public Group(string name, int width, List<Shortcut> shortcutList, BackgroundOption selectedBackgroundOption, string backgroundColor, int opacity) {
            Name = name;
            Width = width;
            ShortcutList = shortcutList;
            SelectedBackgroundOption = selectedBackgroundOption;
            BackgroundColor = backgroundColor;
            Opacity = opacity;
        }

        public Group(string path) {
            // Use application's absolute path; (grabs the .exe)
            // Get the GroupConfigs file
            string fullPath = $@"{path}\GroupConfigs.xml";

            XmlSerializer reader =
                new XmlSerializer(typeof(Group));
            using (StreamReader file = new StreamReader(fullPath)) {
                Group group = (Group)reader.Deserialize(file);
                Name = group.Name;
                Width = group.Width;
                ShortcutList = group.ShortcutList;
                SelectedBackgroundOption = group.SelectedBackgroundOption;
                BackgroundColor = group.BackgroundColor;
                Opacity = group.Opacity;
            }
        }

        // needed for XML serialization
        public Group() {
        }

        public void CreateConfig(ImageSource groupImage) {
            string path = $@"{Paths.GroupsPath}\{Name}";

            // Create directory for the config
            Directory.CreateDirectory(path);

            // XML config
            XmlSerializer writer =
                new XmlSerializer(typeof(Group));

            using (FileStream file = File.Create($@"{path}\GroupConfigs.xml")) {
                writer.Serialize(file, this);
            }

            // Save group image
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)groupImage));
            encoder.Save(memoryStream);
            Image image = Image.FromStream(memoryStream);
            memoryStream.Close();
            image.Save($@"{path}\GroupImage.png");

            // Save group icon
            using (FileStream fileStream = new FileStream($@"{path}\GroupIcon.ico", FileMode.Create)) {
                ImageFunctions.IconFromImage(image).Save(fileStream); // saving as icon
            }

            // Create .lnk shortcut
            // Through shellLink.cs class, pass through into the function information on how to construct the icon
            // Needed due to needing to set a unique AppUserModelID so the shortcut applications don't stack on the taskbar with the main application
            // Tricks Windows to think they are from different applications even though they are from the same .exe
            ShellLink.InstallShortcut(
                Path.GetFullPath(AppDomain.CurrentDomain.FriendlyName),
                $"nickson.shortcutgroups.{Name}",
                 $"{Name} shortcut",
                 Path.GetFullPath(@path),
                 Path.GetFullPath($@"{path}\GroupIcon.ico"),
                 $@"Shortcuts\{Name}.lnk",
                 $"\"{Name}\""
            );

            // Build the image cache
            CacheImages();
        }

        // Loads the group image
        public BitmapImage LoadGroupImage() // Needed to access img without occupying read/write
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri($@"{Paths.GroupsPath}\{Name}\GroupImage.png");
            bitmapImage.EndInit();
            return bitmapImage;
        }

        // Handle returning images of icon files (.lnk)
        private Bitmap HandleSpecialImageExtensions(string file) {
            IWshRuntimeLibrary.IWshShortcut linkIcon = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(file);
            string iconLocation = linkIcon.IconLocation.Split(',')[0];

            //Check if iconLocation exists to get an .ico from; if not then take the image from the.exe it is referring to
            //Checks for link iconLocations as those are used by some applications
            if (iconLocation != "" && !linkIcon.IconLocation.Contains("http")) {
                return Icon.ExtractAssociatedIcon(Path.GetFullPath(Environment.ExpandEnvironmentVariables(iconLocation))).ToBitmap();
            } else {
                return Icon.ExtractAssociatedIcon(Path.GetFullPath(Environment.ExpandEnvironmentVariables(linkIcon.TargetPath))).ToBitmap();
            }
        }

        // Goal is to create a folder with images of the programs pre-cached and ready to be read
        // Avoids having the images need to be rebuilt everytime which takes time and resources
        public void CacheImages() {
            // Defines the path for the icons folder
            string path = $@"{Paths.GroupsPath}\{Name}\Icons\";

            // Check and delete current images folder to completely rebuild the image cache
            // Only done on re-edits of the group and isn't done usually
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            // Creates the images folder inside of existing config folder for the group
            Directory.CreateDirectory(path);

            // Loops through each shortcut added by the user and gets the image
            // Writes the image to the new folder in a .jpg format
            // Naming scheme for the files are done through Path.GetFileNameWithoutExtension()
            for (int i = 0; i < ShortcutList.Count; i++) {
                string filePath = ShortcutList[i].FilePath;

                // Process .lnk (shortcut) files differently
                if (Path.GetExtension(filePath).ToLower() == ".lnk") {
                    HandleSpecialImageExtensions(filePath).Save($@"{path}\{Path.GetFileNameWithoutExtension(filePath)}.jpg");
                } else if (Directory.Exists(filePath)) {
                    HandleFolder.GetFolderImageAsBitmap(filePath).Save($@"{path}\{Path.GetFileNameWithoutExtension(filePath)}_FolderObjTSKGRoup.jpg");
                } else {
                    // Extracts icon from the .exe if the provided file is not a shortcut file
                    Icon.ExtractAssociatedIcon(Environment.ExpandEnvironmentVariables(filePath)).ToBitmap().Save($@"{path}\{Path.GetFileNameWithoutExtension(filePath)}.jpg");
                }
            }
        }

        // Load image from the cache
        // Takes in a programPath (shortcut) and processes it to the proper file name
        public BitmapImage LoadImageFromCache(string programPath) {
            if (File.Exists(programPath) || Directory.Exists(programPath)) {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri($@"{Paths.GroupsPath}\{Name}\Icons\{Path.GetFileNameWithoutExtension(programPath)}{(Directory.Exists(programPath) ? "_FolderObjTSKGRoup.jpg" : ".jpg")}");
                bitmapImage.EndInit();
                return bitmapImage;
            } else {
                return Paths.GetResource("Error");
            }
        }
    }
}
