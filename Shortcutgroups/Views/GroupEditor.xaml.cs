using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using Shortcutgroups.Classes;
using Shortcutgroups.Controls;
using Microsoft.Win32;
using System.IO;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace Shortcutgroups.Views {
    public partial class GroupEditor: UserControl {
        private GroupOverview GroupOverview;
        private Border Tab;
        public Group Group;

        private bool IsNew;

        private string[] imageExt = new string[] { ".png", ".jpg", ".jpeg", ".jpe", ".jfif" };
        private string[] extensionExt = new string[] { ".exe", ".lnk" };
        private string[] specialImageExt = new string[] { ".ico", ".exe", ".lnk" };

        private string[] newExt;

        private int maxShortcuts = 20;

        public class ViewModel {
            public IDropTarget DropHandler { get; private set; }

            public ViewModel(GroupEditor groupEditor) {
                DropHandler = new DropTarget(groupEditor);
            }
        }

        // DropHandler for drag&drop the shortcuts
        public class DropTarget: DefaultDropHandler {
            private GroupEditor GroupEditor;
            private int sourceIndex;
            public DropTarget(GroupEditor groupEditor) {
                GroupEditor = groupEditor;
            }
            public override void DragOver(IDropInfo dropInfo) {
                base.DragOver(dropInfo);
                sourceIndex = dropInfo.DragInfo.SourceIndex;
            }

            public override void Drop(IDropInfo dropInfo) {
                base.Drop(dropInfo);
                int destinationIndex = dropInfo.InsertIndex;
                if (destinationIndex != 0 && destinationIndex > sourceIndex) {
                    destinationIndex--;
                }
                GroupEditor.Sort(GroupEditor.Group.ShortcutList, sourceIndex, destinationIndex);
            }
        }

        // create new groupEditor
        public GroupEditor(GroupOverview groupOverview, Border tab, string name) {
            // Setting from profile
            ProfileOptimization.StartProfile("GroupEditor.Profile");

            InitializeComponent();

            // Setting default properties
            newExt = imageExt.Concat(specialImageExt).ToArray();
            GroupOverview = groupOverview;
            Tab = tab;
            Group = new Group() {
                Name = name,
                ShortcutList = new List<Shortcut>(),
                SelectedBackgroundOption = BackgroundOption.Light,
                BackgroundColor = Color.FromRgb(230, 230, 230).ToString(),
                Opacity = 100
            };
            IsNew = true;

            // DropHandler
            DataContext = new ViewModel(this);

            // Setting default control values
            BtnDelete.Visibility = Visibility.Hidden;

            int num = int.Parse(LblWidth.Content.ToString());
            int max = 1;
            CheckWidth(num, max);
        }

        // editing an existing group
        public GroupEditor(GroupOverview groupOverview, Border tab, Group group) {
            // Setting from profile
            ProfileOptimization.StartProfile("GroupEditor.Profile");

            InitializeComponent();

            // Setting properties
            GroupOverview = groupOverview;
            Tab = tab;
            Group = group;
            IsNew = false;

            // DropHandler
            DataContext = new ViewModel(this);

            // Setting control values from loaded group
            InpGroupName.Text = Group.Name;
            ImgGroupIcon.Source = Group.LoadGroupImage();
            LblWidth.Content = Group.Width.ToString();
            Color groupBackgroundColor = ImageFunctions.FromString(Group.BackgroundColor);
            ClrCustomColor.SelectedColor = groupBackgroundColor;
            SldOpacity.Value = Group.Opacity;
            if (Group.SelectedBackgroundOption == BackgroundOption.Dark) {
                RadDark.IsChecked = true;
            } else if (Group.SelectedBackgroundOption == BackgroundOption.WindowsTheme) {
                RadWindowsTheme.IsChecked = true;
            } else if (Group.SelectedBackgroundOption == BackgroundOption.WindowsAccentColor) {
                RadWindowsAccentColor.IsChecked = true;
            } else if (Group.SelectedBackgroundOption == BackgroundOption.Custom) {
                RadCustom.IsChecked = true;
                ClrCustomColor.IsEnabled = true;
            }
            LoadShortcuts();
        }

        // Load all shortcuts
        public void LoadShortcuts() {
            PnlShortcuts.Items.Clear();

            int num = int.Parse(LblWidth.Content.ToString());
            int max = Group.ShortcutList.Count;
            if (max == 0) {
                max = 1;
            }
            if (num > max) {
                num = max;
            }
            CheckWidth(num, max);

            foreach (Shortcut shortcut in Group.ShortcutList) {
                CtlEditorShortcut editorShortcut = new CtlEditorShortcut(this, shortcut);
                PnlShortcuts.Items.Add(editorShortcut);
            }
        }

        // Change positions of shortcut panels
        public void Sort(List<Shortcut> list, int indexA, int indexB) {
            Shortcut temp = list[indexA];
            list.RemoveAt(indexA);
            list.Insert(indexB, temp);
            LoadShortcuts();
        }

        private void InpGroupName_TextChanged(object sender, TextChangedEventArgs e) {
            LblErrorTitle.Visibility = Visibility.Hidden;
            if (InpGroupName.Text == "") {
                // Create an ImageBrush.
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource = Paths.GetResource("GroupNamePlaceholder");
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.Stretch = Stretch.None;
                // Use the brush to paint the button's background.
                InpGroupName.Background = textImageBrush;

                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = 0.75;
                scaleTransform.ScaleY = 0.75;

                TranslateTransform translateTransform = new TranslateTransform();
                translateTransform.X = -4;
                translateTransform.Y = 8;

                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleTransform);
                transformGroup.Children.Add(translateTransform);

                textImageBrush.Transform = transformGroup;
            } else {
                InpGroupName.Background = null;
            }
        }

        Timer widthUpTimer;
        bool widthUpTimerStopped = false;
        private void BtnWidthUp_MouseUp(object sender, MouseButtonEventArgs e) {
            widthUpTimerStopped = true;
            int num = int.Parse(LblWidth.Content.ToString());
            int max = Group.ShortcutList.Count;
            if (num < max) {
                num++;
                CheckWidth(num, max);
            }
        }

        private void BtnWidthUp_MouseDown(object sender, MouseButtonEventArgs e) {
            widthUpTimerStopped = false;
            Timer timerStart = new Timer();
            timerStart.Interval = 500;
            timerStart.Elapsed += WidthUpStartTimer;
            timerStart.AutoReset = false;
            timerStart.Start();
        }

        private void WidthUpStartTimer(object sender, ElapsedEventArgs e) {
            if (!widthUpTimerStopped) {
                widthUpTimer = new Timer();
                widthUpTimer.Interval = 50;
                widthUpTimer.Elapsed += WidthUpTimer_Tick;
                widthUpTimer.AutoReset = false;
                widthUpTimer.Start();
            }
        }

        private void WidthUpTimer_Tick(object sender, ElapsedEventArgs e) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                int num = int.Parse(LblWidth.Content.ToString());
                int max = Group.ShortcutList.Count;
                if (num < max) {
                    num++;
                    CheckWidth(num, max);
                } else {
                    widthUpTimerStopped = true;
                }
                if (!widthUpTimerStopped) {
                    widthUpTimer.Start();
                }
            }));
        }

        Timer widthDownTimer;
        bool widthDownTimerStopped = false;
        private void BtnWidthDown_MouseUp(object sender, MouseButtonEventArgs e) {
            widthDownTimerStopped = true;
            int num = int.Parse(LblWidth.Content.ToString());
            if (num > 1) {
                num--;
                CheckWidth(num);
            }
        }

        private void BtnWidthDown_MouseDown(object sender, MouseButtonEventArgs e) {
            widthDownTimerStopped = false;
            Timer timerStart = new Timer();
            timerStart.Interval = 500;
            timerStart.Elapsed += WidthDownStartTimer;
            timerStart.AutoReset = false;
            timerStart.Start();
        }

        private void WidthDownStartTimer(object sender, ElapsedEventArgs e) {
            if (!widthDownTimerStopped) {
                widthDownTimer = new Timer();
                widthDownTimer.Interval = 50;
                widthDownTimer.Elapsed += WidthDownTimer_Tick;
                widthDownTimer.AutoReset = false;
                widthDownTimer.Start();
            }
        }

        private void WidthDownTimer_Tick(object sender, ElapsedEventArgs e) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                int num = int.Parse(LblWidth.Content.ToString());
                if (num > 1) {
                    num--;
                    CheckWidth(num);
                } else {
                    widthDownTimerStopped = true;
                }
                if (!widthDownTimerStopped) {
                    widthDownTimer.Start();
                }
            }));
        }

        // Check min/max group width
        private void CheckWidth(int num, int max = 20) {
            LblWidth.Content = num.ToString();
            if (num == 1) {
                BtnWidthDown.IsEnabled = false;
                BtnWidthDown.Source = Paths.GetResource("ArrowGray");
            } else {
                BtnWidthDown.IsEnabled = true;
                BtnWidthDown.Source = Paths.GetResource("ArrowWhite");
            }
            if (num == max) {
                BtnWidthUp.IsEnabled = false;
                BtnWidthUp.Source = Paths.GetResource("ArrowGray");
            } else {
                BtnWidthUp.IsEnabled = true;
                BtnWidthUp.Source = Paths.GetResource("ArrowWhite");
            }
        }

        private void BtnAddGroupIcon_MouseEnter(object sender, MouseEventArgs e) {
            BtnAddGroupIcon.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void BtnAddGroupIcon_MouseLeave(object sender, MouseEventArgs e) {
            BtnAddGroupIcon.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnAddGroupIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog  // ask user to select img as group icon
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select group icon",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Image files and exe|*.png;*.jpg;*.jpeg;*.jpe;*.jfif;*.ico;*.exe;*.lnk",
                DereferenceLinks = false
            };

            if (openFileDialog.ShowDialog() == true) {
                LblErrorIcon.Visibility = Visibility.Hidden; //hide error msg

                string imageExtension = Path.GetExtension(openFileDialog.FileName).ToLower();

                // Checks if the file being dropped is an .ico/.exe/.lnk in which type icons need to be extracted / processed
                if (specialImageExt.Contains(imageExtension)) {
                    ImgGroupIcon.Source = HandleSpecialImageExtensions(openFileDialog.FileName, imageExtension);
                } else {
                    ImgGroupIcon.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                }
            }
        }

        private void BtnAddGroupIcon_Drop(object sender, DragEventArgs e) {
            string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            string imageExtension = Path.GetExtension(file).ToLower();

            if (newExt.Contains(imageExtension) && File.Exists(file)) {
                LblErrorIcon.Visibility = Visibility.Hidden; //hide error msg

                // Checks if the file being dropped is an .ico/.exe/.lnk in which type icons need to be extracted / processed
                if (specialImageExt.Contains(imageExtension)) {
                    ImgGroupIcon.Source = HandleSpecialImageExtensions(file, imageExtension);
                } else {
                    ImgGroupIcon.Source = new BitmapImage(new Uri(file));
                }
            }
        }

        // Handle returning images of .ico/.exe/.lnk
        public static BitmapSource HandleSpecialImageExtensions(string file, string extension) {
            if (extension == ".lnk") {
                IWshRuntimeLibrary.IWshShortcut lnkIcon = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(file);
                string[] icLocation = lnkIcon.IconLocation.Split(',');

                //Check if iconLocation exists to get an .ico from; if not then take the image from the.exe it is referring to
                //Checks for link iconLocations as those are used by some applications
                if (icLocation[0] != "" && !lnkIcon.IconLocation.Contains("http")) {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(Path.GetFullPath(Environment.ExpandEnvironmentVariables(icLocation[0])));
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
                } else {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(Path.GetFullPath(Environment.ExpandEnvironmentVariables(lnkIcon.TargetPath)));
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    new Int32Rect(0, 0, icon.Width, icon.Height),
                    BitmapSizeOptions.FromEmptyOptions());
                }
            } else {
                System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(file);
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            new Int32Rect(0, 0, icon.Width, icon.Height),
                            BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void BtnAddShortcut_MouseEnter(object sender, MouseEventArgs e) {
            BtnAddShortcut.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void BtnAddShortcut_MouseLeave(object sender, MouseEventArgs e) {
            BtnAddShortcut.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnAddShortcut_MouseUp(object sender, MouseButtonEventArgs e) {
            LblErrorShortcut.Visibility = Visibility.Hidden; // resetting error msg

            if (Group.ShortcutList.Count >= maxShortcuts) {
                LblErrorShortcut.Content = $"Max {maxShortcuts} shortcuts in one group";
                LblErrorShortcut.Visibility = Visibility.Visible;
            } else {
                OpenFileDialog openFileDialog = new OpenFileDialog { // ask user to select file
                    InitialDirectory = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                    Title = "Add new shortcut",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = true,
                    Filter = "Exe or Shortcut|*.exe;*.lnk",
                    DereferenceLinks = false
                };

                if (openFileDialog.ShowDialog() == true) {
                    if (openFileDialog.FileNames.Length > maxShortcuts) {
                        LblErrorShortcut.Content = $"Max {maxShortcuts} shortcuts in one group";
                        LblErrorShortcut.Visibility = Visibility.Visible;
                    } else {
                        foreach (string file in openFileDialog.FileNames) {
                            Shortcut programShortcut = new Shortcut(Environment.ExpandEnvironmentVariables(file)); //create new shortcut obj
                            Group.ShortcutList.Add(programShortcut); // add to panel shortcut list
                        }
                        LoadShortcuts();
                    }
                }
            }
        }

        private void BtnAddFolder_MouseEnter(object sender, MouseEventArgs e) {
            BtnAddFolder.Background = new SolidColorBrush(Color.FromRgb(20, 20, 27));
        }

        private void BtnAddFolder_MouseLeave(object sender, MouseEventArgs e) {
            BtnAddFolder.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void BtnAddFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            LblErrorShortcut.Visibility = Visibility.Hidden; // resetting error msg

            if (Group.ShortcutList.Count >= maxShortcuts) {
                LblErrorShortcut.Content = $"Max {maxShortcuts} shortcuts in one group";
                LblErrorShortcut.Visibility = Visibility.Visible;
            } else {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog { // ask user to select folder
                    SelectedPath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs"
                };

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Shortcut programShortcut = new Shortcut(Environment.ExpandEnvironmentVariables(folderBrowserDialog.SelectedPath)); //create new shortcut obj
                    Group.ShortcutList.Add(programShortcut); // add to panel shortcut list
                    LoadShortcuts();
                }
            }
        }

        private void BtnAdd_Drop(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            // Loops through each file to make sure they exist and to add them directly to the shortcut list
            foreach (var file in files) {
                if (extensionExt.Contains(Path.GetExtension(file)) && File.Exists(file) || Directory.Exists(file)) {
                    Shortcut programShortcut = new Shortcut(Environment.ExpandEnvironmentVariables(file)); //Create new shortcut obj
                    Group.ShortcutList.Add(programShortcut); // Add to panel shortcut list
                }
            }
            LoadShortcuts();
        }

        private void RadLight_Click(object sender, RoutedEventArgs e) {
            ClrCustomColor.IsEnabled = false;
            Group.SelectedBackgroundOption = BackgroundOption.Light;
            Group.BackgroundColor = Color.FromRgb(230, 230, 230).ToString();
        }

        private void RadDark_Click(object sender, RoutedEventArgs e) {
            ClrCustomColor.IsEnabled = false;
            Group.SelectedBackgroundOption = BackgroundOption.Dark;
            Group.BackgroundColor = Color.FromRgb(30, 30, 30).ToString();
        }

        private void RadWindowsTheme_Click(object sender, RoutedEventArgs e) {
            ClrCustomColor.IsEnabled = false;
            Group.SelectedBackgroundOption = BackgroundOption.WindowsTheme;
            if (ColorFunctions.getThemeMode()) {
                Group.BackgroundColor = Color.FromRgb(230, 230, 230).ToString();
            } else {
                Group.BackgroundColor = Color.FromRgb(30, 30, 30).ToString();
            }
        }

        private void RadWindowsAccentColor_Click(object sender, RoutedEventArgs e) {
            ClrCustomColor.IsEnabled = false;
            Group.SelectedBackgroundOption = BackgroundOption.WindowsAccentColor;
            Group.BackgroundColor = ColorFunctions.GetThemeColor().ToString();
        }

        private void RadCustom_Click(object sender, RoutedEventArgs e) {
            ClrCustomColor.IsEnabled = true;
            Group.SelectedBackgroundOption = BackgroundOption.Custom;
        }

        private void ClrCustomColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            Group.BackgroundColor = ((Color)ClrCustomColor.SelectedColor).ToString();
        }

        private void SldOpacity_MouseUp(object sender, MouseButtonEventArgs e) {
            Group.Opacity = Convert.ToInt32(SldOpacity.Value);
        }

        private void SldOpacity_DragCompleted(object sender, DragCompletedEventArgs e) {
            Group.Opacity = Convert.ToInt32(SldOpacity.Value);
        }

        private void BtnSave_MouseEnter(object sender, MouseEventArgs e) {
            BtnSave.Background = new SolidColorBrush(Color.FromRgb(60, 60, 70));
        }

        private void BtnSave_MouseLeave(object sender, MouseEventArgs e) {
            BtnSave.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
        }

        private void BtnSave_MouseUp(object sender, MouseButtonEventArgs e) {
            bool isVerified = true;
            if (InpGroupName.Text == "") { // Verify category name
                LblErrorTitle.Content = "Must type a name";
                LblErrorTitle.Visibility = Visibility.Visible;
                isVerified = false;
            } else if (!new System.Text.RegularExpressions.Regex("^[0-9a-zA-Z_ \b]+$").IsMatch(InpGroupName.Text)) {
                LblErrorTitle.Content = "Name cannot contain any special characters";
                LblErrorTitle.Visibility = Visibility.Visible;
                isVerified = false;
            }
            if (ImgGroupIcon.Source.ToString() == "pack://application:,,,/Shortcutgroups;component/Resources/AddWhite.png") { // Verify icon
                LblErrorIcon.Content = "Must select group icon";
                LblErrorIcon.Visibility = Visibility.Visible;
                isVerified = false;
            }
            if (Group.ShortcutList.Count == 0) { // Verify shortcuts
                LblErrorShortcut.Content = "Must select at least one shortcut";
                LblErrorShortcut.Visibility = Visibility.Visible;
                isVerified = false;
            }

            if (isVerified) {
                try {
                    if (!IsNew) {
                        // Delete old config
                        string configPath = $@"{Paths.GroupsPath}\{Group.Name}";
                        string shortcutPath = $@"{Paths.ShortcutsPath}\{Group.Name}.lnk";
                        var dir = new DirectoryInfo(configPath);

                        dir.Delete(true); // delete config directory
                        File.Delete(shortcutPath); // delete .lnk
                    }

                    // Creating new config
                    Group.Width = int.Parse(LblWidth.Content.ToString());
                    Group.Name = InpGroupName.Text;
                    Group.CreateConfig(ImgGroupIcon.Source); // Creating group config files

                    CloseTab();
                    GroupOverview.Reload();
                } catch (IOException ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void BtnAbort_MouseEnter(object sender, MouseEventArgs e) {
            BtnAbort.Background = new SolidColorBrush(Color.FromRgb(170, 170, 180));
        }

        private void BtnAbort_MouseLeave(object sender, MouseEventArgs e) {
            BtnAbort.Background = new SolidColorBrush(Color.FromRgb(200, 200, 210));
        }

        private void BtnAbort_MouseUp(object sender, MouseButtonEventArgs e) {
            CloseTab();
        }

        private void BtnDelete_MouseEnter(object sender, MouseEventArgs e) {
            BtnDelete.Background = new SolidColorBrush(Color.FromRgb(170, 170, 180));
        }

        private void BtnDelete_MouseLeave(object sender, MouseEventArgs e) {
            BtnDelete.Background = new SolidColorBrush(Color.FromRgb(200, 200, 210));
        }

        private void BtnDelete_MouseUp(object sender, MouseButtonEventArgs e) {
            try {
                string configPath = $@"{Paths.GroupsPath}\{Group.Name}";
                string shortcutPath = $@"{Paths.ShortcutsPath}\{Group.Name}.lnk";
                var dir = new DirectoryInfo(configPath);

                dir.Delete(true); // delete config directory
                File.Delete(shortcutPath); // delete .lnk

                CloseTab();
                GroupOverview.Reload();
            } catch (IOException ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseTab() {
            GroupOverview.MainWindow.CloseTab(Tab, Group.Name);
        }
    }
}
