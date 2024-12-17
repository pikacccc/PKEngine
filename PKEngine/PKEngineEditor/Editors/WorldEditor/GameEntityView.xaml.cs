using System.Windows.Controls;

namespace PKEngineEditor.Editors
{
    /// <summary>
    /// GameEntityView.xaml 的交互逻辑
    /// </summary>
    public partial class GameEntityView : UserControl
    {
        public static GameEntityView Instance { get; private set; }

        public GameEntityView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
        }
    }
}
