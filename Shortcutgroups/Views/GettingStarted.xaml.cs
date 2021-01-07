using Shortcutgroups.Windows;
using System.Runtime;
using System.Windows.Controls;

namespace Shortcutgroups.Views {
    public partial class GettingStarted: UserControl {
        public MainWindow MainWindow;
        public GettingStarted(MainWindow mainWindow) {
            // Setting from profile
            ProfileOptimization.StartProfile("GettingStarted.Profile");

            InitializeComponent();

            MainWindow = mainWindow;
        }
    }
}
