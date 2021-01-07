using Shortcutgroups.Classes;
using Shortcutgroups.Windows;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Shortcutgroups.Views {
    public partial class Settings: UserControl {
        public MainWindow MainWindow;
        public Settings(MainWindow mainWindow) {
            // Setting from profile
            ProfileOptimization.StartProfile("Settings.Profile");

            InitializeComponent();

            MainWindow = mainWindow;

            LblCurrentVersion.Content = LblCurrentVersion.Content + GithubData.GetCurrentVersion();

            if (GithubData.GetCurrentVersion() != GithubData.GetLatestVersion()) {
                LblPreNewVersion.Text = "There is a new version:";
                BlkNewVersion.Visibility = Visibility.Visible;
                LblNewVersion.Text = GithubData.GetLatestVersion();
                LnkNewVersion.NavigateUri = new Uri(GithubData.GetLatestReleaseUrl());
            }
        }

        private void LnkNewVersion_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void LnkReport_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
