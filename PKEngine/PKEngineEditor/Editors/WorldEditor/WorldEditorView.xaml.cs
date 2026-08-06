using System.Windows;
using System.Windows.Controls;
using PKEngineEditor.Content;
using PKEngineEditor.GameDev;

namespace PKEngineEditor.Editors
{
    /// <summary>
    /// WorldEditorView.xaml 的交互逻辑
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();
            Loaded += OnWorldEditorViewLoaded;
        }

        private void OnWorldEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorViewLoaded;
            Focus();
            //((INotifyCollectionChanged)Project.UndoRedoMgr.UndoList).CollectionChanged += (s, e) => Focus();
        }

        private void OnNewScript_Button_Click(object sender, RoutedEventArgs e)
        {
            new NewScriptDialog().ShowDialog();
        }

        private void OnCreatePrimitiveMesh_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrimitiveMeshDialog();
            dlg.ShowDialog();
        }
    }
}