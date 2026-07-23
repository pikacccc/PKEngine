using PKEngineEditor.GameProject;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Path = System.IO.Path;

namespace PKEngineEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string PkEnginePath { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnMainWindowLoaded;
            Closing += OnMainWindowClosing;
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            Closing -= OnMainWindowClosing;
            Project.CurProject?.Unload();
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            GetEnginePath();
            OpenProjectBrowsereDialog();
        }

        private void GetEnginePath()
        {
            var enginePath = Environment.GetEnvironmentVariable("PK_ENGINE", EnvironmentVariableTarget.User);
            if (enginePath == null || !Directory.Exists(Path.Combine(enginePath, @"Engine\EngineAPI")))
            {
                var dlg = new EnginePathDialog();
                if (dlg.ShowDialog() == true)
                {
                    PkEnginePath = dlg.PkEnginePath;
                    Environment.SetEnvironmentVariable("PK_ENGINE", dlg.PkEnginePath.ToUpper(), EnvironmentVariableTarget.User);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                PkEnginePath = enginePath;
            }
        }

        private void OpenProjectBrowsereDialog()
        {
            var browser = new ProjectBrowserDialog();
            if (browser.ShowDialog() == false || browser.DataContext == null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Project.CurProject?.Unload();
                DataContext = browser.DataContext;
            }
        }
    }
}