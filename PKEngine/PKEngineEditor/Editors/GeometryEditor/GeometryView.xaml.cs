using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PKEngineEditor.Editors
{
    public partial class GeometryView : UserControl
    {
        private Point _clickPosition;
        private bool  _capturedLeft;
        private bool  _capturedRight;

        public GeometryView()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => SetGeometry();
        }

        public void SetGeometry(int index = -1)
        {
            if (!(DataContext is MeshRenderer vm)) return;

            if (vm.Meshes.Any() && viewport.Children.Count == 2)
            {
                viewport.Children.RemoveAt(1);
            }

            var meshIndex  = 0;
            var modelGroup = new Model3DGroup();
            foreach (var mesh in vm.Meshes)
            {
                if (index != -1 && meshIndex != index)
                {
                    ++meshIndex;
                    continue;
                }

                var mesh3D = new MeshGeometry3D
                {
                    Positions          = mesh.Positions,
                    Normals            = mesh.Normals,
                    TriangleIndices    = mesh.Indices,
                    TextureCoordinates = mesh.UVs
                };

                var diffuse  = new DiffuseMaterial(mesh.Diffuse);
                var specular = new SpecularMaterial(mesh.Specular, 50);
                var matGroup = new MaterialGroup();
                matGroup.Children.Add(diffuse);
                matGroup.Children.Add(specular);

                var model = new GeometryModel3D(mesh3D, matGroup);
                modelGroup.Children.Add(model);

                var binding = new Binding(nameof(mesh.Diffuse)) { Source = mesh };
                BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);

                if (meshIndex == index) break;
            }

            var visual = new ModelVisual3D { Content = modelGroup };
            viewport.Children.Add(visual);
        }

        private void OnGrid_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            _clickPosition = e.GetPosition(this);
            _capturedLeft  = true;
            Mouse.Capture(sender as UIElement);
        }

        private void OnGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_capturedLeft && !_capturedRight) return;

            var pos = e.GetPosition(this);
            var d   = pos - _clickPosition;
            if (_capturedLeft && !_capturedRight)
            {
                MoveCamera(d.X, d.Y, 0);
            }
            else if (!_capturedLeft && _capturedRight)
            {
                var vm      = DataContext as MeshRenderer;
                var cp      = vm!.CameraPosition;
                var yOffset = d.Y * 0.001 * Math.Sqrt(cp.X * cp.X + cp.Z * cp.Z);

                vm.CameraTarget = new Point3D(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
            }
        }

        private void MoveCamera(double dX, double dY, int i)
        {
            throw new NotImplementedException();
        }

        private void OnGrid_Mouse_RBD(object sender, MouseButtonEventArgs e)
        {
            _capturedLeft = false;
            if (!_capturedRight) Mouse.Capture(null);
        }

        private void OnGrid_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnGrid_Mouse_RBU(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}