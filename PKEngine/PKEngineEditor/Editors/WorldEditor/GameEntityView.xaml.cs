using PKEngineEditor.Components;
using PKEngineEditor.GameProject;
using PKEngineEditor.Utilities;
using System.Windows.Controls;

namespace PKEngineEditor.Editors
{
    /// <summary>
    /// GameEntityView.xaml 的交互逻辑
    /// </summary>
    public partial class GameEntityView : UserControl
    {
        private Action _undoAction;
        private string _protertyName;

        public static GameEntityView Instance { get; private set; }

        public GameEntityView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;

            DataContextChanged += (_, __) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _protertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetISEnabledAction()
        {
            var vm = DataContext as MSEntity;
            var selection = vm.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void OnName_TextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _undoAction = GetRenameAction();
        }

        private void OnName_TextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (_protertyName == nameof(MSEntity.Name) && _undoAction != null)
            {
                var redoAction = GetRenameAction();
                Project.UndoRedoMgr.Add(new UndoRedoAction(_undoAction, redoAction, "Rename game entity"));
                _protertyName = null;
            }
            _undoAction = null;
        }

        private void OnIsEnabled_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as MSEntity;
            var undoAction = GetISEnabledAction();
            vm.IsEnabled = (sender as CheckBox).IsChecked == true;
            var redoAction = GetISEnabledAction();
            Project.UndoRedoMgr.Add(new UndoRedoAction(undoAction, redoAction, vm.IsEnabled == true ? "Enable game entity" : "Disable game Entity"));
        }
    }
}
