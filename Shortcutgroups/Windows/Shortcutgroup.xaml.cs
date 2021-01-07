using Shortcutgroups.Classes;
using Shortcutgroups.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfScreenHelper;

namespace Shortcutgroups.Windows {
    public partial class Shortcutgroup: Window {
        private Group Group;
        private List<CtlShortcut> ShortcutList;
        public SolidColorBrush HoverColor;

        private Point mousePosition;

        public Shortcutgroup(string group, int cursorX, int cursorY) {
            // Setting from profile
            ProfileOptimization.StartProfile("Shortcutgroup.Profile");

            InitializeComponent();

            mousePosition = new Point(cursorX, cursorY);

            if (Directory.Exists($@"{Paths.GroupsPath}\{group}")) {
                ShortcutList = new List<CtlShortcut>();
                Group = new Group($@"{Paths.GroupsPath}\{group}");

                SolidColorBrush background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Group.BackgroundColor));
                byte opacity = (byte)(Group.Opacity / 100.0 * 255.0);
                Background = new SolidColorBrush(Color.FromArgb(opacity, background.Color.R, background.Color.G, background.Color.B));

                var R = ((SolidColorBrush)Background).Color.R;
                var G = ((SolidColorBrush)Background).Color.G;
                var B = ((SolidColorBrush)Background).Color.B;
                //if backcolor is light, set hover color darker
                if (R * 0.2126 + G * 0.7152 + B * 0.0722 > 255 / 2) {
                    HoverColor = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
                }
                //if backcolor is dark, set hover color lighter
                else {
                    HoverColor = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                }
                LoadGroup();
                SetLocation();
            } else {
                Application.Current.Shutdown();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e) {
            Close();
        }

        private void SetLocation() {
            List<Rect> taskbarList = FindDockedTaskbars();
            Rect taskbar = new Rect();
            Rect screen = new Rect();

            int i = 0;
            double locationX;
            double locationY;
            foreach (Screen scr in Screen.AllScreens) {
                if (scr.Bounds.Contains(mousePosition)) {
                    screen.Location = scr.Bounds.TopLeft;
                    screen.Size = scr.Bounds.Size;
                    taskbar = taskbarList[i];
                }
                i++;
            }
            int windowWidth = Convert.ToInt32(Width);
            int windowHeight = Convert.ToInt32(Height);

            // Clicked on taskbar
            if (taskbar.Contains(mousePosition)) {
                if (taskbar.Top == screen.Top && taskbar.Width == screen.Width) {
                    // TOP
                    locationY = screen.Y + taskbar.Height + 10;
                    locationX = mousePosition.X - windowWidth / 2;
                } else if (taskbar.Bottom == screen.Bottom && taskbar.Width == screen.Width) {
                    // BOTTOM
                    locationY = screen.Y + screen.Height - windowHeight - taskbar.Height - 10;
                    locationX = mousePosition.X - (windowWidth / 2);
                } else if (taskbar.Left == screen.Left) {
                    // LEFT
                    locationY = mousePosition.X - (windowHeight / 2);
                    locationX = screen.X + taskbar.Width + 10;
                } else {
                    // RIGHT
                    locationY = mousePosition.X - (windowHeight / 2);
                    locationX = screen.X + screen.Width - windowWidth - taskbar.Width - 10;
                }
            }
            // not clicked on taskbar
            else {
                locationY = mousePosition.Y - windowHeight - 20;
                locationX = mousePosition.X - (windowWidth / 2);
            }


            Left = locationX;
            Top = locationY;
            if (Left < screen.Left)
                Left = screen.Left + 10;
            if (Top < screen.Top)
                Top = screen.Top + 10;
            if (Left + Width > screen.Right)
                Left = screen.Right - Width - 10;
        }

        private void LoadGroup() {
            Width = Group.Width * 50;
            Height = 50;
            int x = 0;
            int y = 0;
            int columns = 1;

            // Check if icon caches exist for the category being loaded
            // If not then rebuild the icon cache
            if (!Directory.Exists($@"{Paths.GroupsPath}\{Group.Name}\Icons\")) {
                Group.CacheImages();
            }

            foreach (Shortcut shortcut in Group.ShortcutList) {
                if (columns > Group.Width) { // creating new row if there are more psc than max width
                    x = 0;
                    y += 50;
                    Height += 50;
                    columns = 1;
                }

                BuildShortcut(x, y, shortcut);
                x += 50;
                columns++;
            }
        }

        private void BuildShortcut(int x, int y, Shortcut shortcut) {
            CtlShortcut ctlShortcut = new CtlShortcut(this, shortcut, Group);
            Shortcuts.Children.Add(ctlShortcut);
            ShortcutList.Add(ctlShortcut);
        }

        // Search for active taskbars on screen
        public static List<Rect> FindDockedTaskbars() {
            List<Rect> dockedTaskbars = new List<Rect>();
            foreach (Screen screen in Screen.AllScreens) {
                if (!screen.Bounds.Equals(screen.WorkingArea)) {
                    Rect dockedTaskbar = new Rect();

                    var leftDockedWidth = Math.Abs((Math.Abs(screen.Bounds.Left) - Math.Abs(screen.WorkingArea.Left)));
                    var topDockedHeight = Math.Abs((Math.Abs(screen.Bounds.Top) - Math.Abs(screen.WorkingArea.Top)));
                    var rightDockedWidth = ((screen.Bounds.Width - leftDockedWidth) - screen.WorkingArea.Width);
                    var bottomDockedHeight = ((screen.Bounds.Height - topDockedHeight) - screen.WorkingArea.Height);
                    if ((leftDockedWidth > 0)) {
                        dockedTaskbar.X = screen.Bounds.Left;
                        dockedTaskbar.Y = screen.Bounds.Top;
                        dockedTaskbar.Width = leftDockedWidth;
                        dockedTaskbar.Height = screen.Bounds.Height;
                    } else if ((rightDockedWidth > 0)) {
                        dockedTaskbar.X = screen.WorkingArea.Right;
                        dockedTaskbar.Y = screen.Bounds.Top;
                        dockedTaskbar.Width = rightDockedWidth;
                        dockedTaskbar.Height = screen.Bounds.Height;
                    } else if ((topDockedHeight > 0)) {
                        dockedTaskbar.X = screen.WorkingArea.Left;
                        dockedTaskbar.Y = screen.Bounds.Top;
                        dockedTaskbar.Width = screen.WorkingArea.Width;
                        dockedTaskbar.Height = topDockedHeight;
                    } else if ((bottomDockedHeight > 0)) {
                        dockedTaskbar.X = screen.WorkingArea.Left;
                        dockedTaskbar.Y = screen.WorkingArea.Bottom;
                        dockedTaskbar.Width = screen.WorkingArea.Width;
                        dockedTaskbar.Height = bottomDockedHeight;
                    } else {
                        // Nothing found!
                    }

                    dockedTaskbars.Add(dockedTaskbar);
                }
            }

            if (dockedTaskbars.Count == 0) {
                // Taskbar is set to "Auto-Hide".
            }
            return dockedTaskbars;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            MouseEventArgs me = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            switch (e.Key) {
                case Key.D1:
                    if (ShortcutList.Count > 0)
                        ShortcutList[0].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D2:
                    if (ShortcutList.Count > 1)
                        ShortcutList[1].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D3:
                    if (ShortcutList.Count > 2)
                        ShortcutList[2].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D4:
                    if (ShortcutList.Count > 3)
                        ShortcutList[3].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D5:
                    if (ShortcutList.Count > 4)
                        ShortcutList[4].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D6:
                    if (ShortcutList.Count > 5)
                        ShortcutList[5].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D7:
                    if (ShortcutList.Count > 6)
                        ShortcutList[6].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D8:
                    if (ShortcutList.Count > 7)
                        ShortcutList[7].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D9:
                    if (ShortcutList.Count > 8)
                        ShortcutList[8].ControlShortcut_MouseEnter(sender, me);
                    break;
                case Key.D0:
                    if (ShortcutList.Count > 9)
                        ShortcutList[9].ControlShortcut_MouseEnter(sender, me);
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e) {
            MouseButtonEventArgs mbe = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter) {
                foreach (CtlShortcut ctlShortcut in ShortcutList)
                    ctlShortcut.ControlShortcut_MouseUp(sender, mbe);
            }

            MouseEventArgs me = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            switch (e.Key) {
                case Key.D1:
                    if (ShortcutList.Count > 0) {
                        ShortcutList[0].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[0].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D2:
                    if (ShortcutList.Count > 1) {
                        ShortcutList[1].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[1].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D3:
                    if (ShortcutList.Count > 2) {
                        ShortcutList[2].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[2].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D4:
                    if (ShortcutList.Count > 3) {
                        ShortcutList[3].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[3].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D5:
                    if (ShortcutList.Count > 4) {
                        ShortcutList[4].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[4].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D6:
                    if (ShortcutList.Count > 5) {
                        ShortcutList[5].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[5].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D7:
                    if (ShortcutList.Count > 6) {
                        ShortcutList[6].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[6].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D8:
                    if (ShortcutList.Count > 7) {
                        ShortcutList[7].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[7].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D9:
                    if (ShortcutList.Count > 8) {
                        ShortcutList[8].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[8].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
                case Key.D0:
                    if (ShortcutList.Count > 9) {
                        ShortcutList[9].ControlShortcut_MouseLeave(sender, me);
                        ShortcutList[9].ControlShortcut_MouseUp(sender, mbe);
                    }
                    break;
            }
        }
    }
}
