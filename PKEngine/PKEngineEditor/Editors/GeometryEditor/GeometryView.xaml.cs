using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace PKEngineEditor.Editors
{
    public partial class GeometryView : UserControl
    {
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
    }
}