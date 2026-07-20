using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PKEngineEditor.Components;
using PKEngineEditor.GameProject;
using PKEngineEditor.Utilities;
using Vector3 = System.Numerics.Vector3;

namespace PKEngineEditor.Editors.WorldEditor
{
    /// <summary>
    /// TransformView.xaml 的交互逻辑
    /// </summary>
    public partial class TransformView : UserControl
    {
        private Action? _undoAction;

        private bool _propertyChanged;

        public TransformView()
        {
            InitializeComponent();
            Loaded += OnTransformViewLoaded;
        }

        private void OnTransformViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnTransformViewLoaded;
            if (!(DataContext is MSTransform vm)) return;
            vm.PropertyChanged += (s, a) => { _propertyChanged = true; };
        }

        private Action? GetAction(Func<Transform, (Transform, Vector3)> selector,
            Action<(Transform transform, Vector3)> forEachAction)
        {
            if (!(DataContext is MSTransform vm))
            {
                _undoAction = null;
                _propertyChanged = false;
                return null;
            }

            var selection = vm.SelectedComponents.Select(selector).ToList();

            return () =>
            {
                selection.ForEach(forEachAction);
                (GameEntityView.Instance.DataContext as MSEntity)?.GetComponent<MSTransform>().Refresh();
            };
        }

        private Action? GetPositionAction() => GetAction((x) => (x, x.Position), x => x.transform.Position = x.Item2);
        private Action? GetRotationAction() => GetAction((x) => (x, x.Rotation), x => x.transform.Rotation = x.Item2);
        private Action? GetScaleAction() => GetAction((x) => (x, x.Scale), x => x.transform.Scale = x.Item2);

        private void RecordAction(Action? redoAction, string name)
        {
            if (_propertyChanged)
            {
                _propertyChanged = false;
                Debug.Assert(_undoAction != null);
                Debug.Assert(redoAction != null);
                Project.UndoRedoMgr.Add(new UndoRedoAction(_undoAction, redoAction, name));
            }
        }

        private void OnPosition_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetPositionAction();
        }

        private void OnPosition_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs? e)
        {
            RecordAction(GetPositionAction(), "Position Changed");
        }

        private void OnPosition_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                OnPosition_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }

        private void OnRotation_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetRotationAction();
        }

        private void OnRotation_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs? e)
        {
            RecordAction(GetRotationAction(), "Rotation Changed");
        }

        private void OnRotation_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                OnRotation_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }

        private void OnScale_VectorBox_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;
            _undoAction = GetScaleAction();
        }

        private void OnScale_VectorBox_PreviewMouse_LBU(object sender, MouseButtonEventArgs? e)
        {
            RecordAction(GetScaleAction(), "Scale Changed");
        }

        private void OnScale_VectorBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                OnScale_VectorBox_PreviewMouse_LBU(sender, null);
            }
        }
    }
}