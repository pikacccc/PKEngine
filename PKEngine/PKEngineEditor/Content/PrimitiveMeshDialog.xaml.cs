using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PKEngineEditor.ContentToolsAPIStructs;
using PKEngineEditor.DllWrappers;
using PKEngineEditor.Editors;
using PKEngineEditor.Utilities;
using PKEngineEditor.Utilities.Controls;

namespace PKEngineEditor.Content;

public partial class PrimitiveMeshDialog : Window
{
    public PrimitiveMeshDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdatePrimitive();
    }

    static PrimitiveMeshDialog()
    {
        LoadTexture();
    }

    private static void LoadTexture()
    {
        var uris = new List<Uri>
        {
            new("pack://application:,,,/Resources/PrimitiveMeshView/PlaneTexture.png"),
        };
        foreach (var uri in uris)
        {
            var resource = Application.GetResourceStream(uri);
            if (resource == null)
            {
                Logger.Log(MessageType.Error, $"Failed to load texture,path: {uri.PathAndQuery}");
                continue;
            }


            using var reader = new BinaryReader(resource.Stream);
            var       data   = reader.ReadBytes((int)resource.Stream.Length);

            var imageSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(data)!;
            imageSource.Freeze();

            var brush = new ImageBrush(imageSource);

            brush.Transform     = new ScaleTransform(1, -1, 0.5, 0.5);
            // RotateTransform aRotateTransform = new RotateTransform();
            // aRotateTransform.CenterX = 0.5;
            // aRotateTransform.CenterY = 0.5;
            // aRotateTransform.Angle   = 90;
            // brush.RelativeTransform  = aRotateTransform;

            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Freeze();

            Textures.Add(brush);
        }
    }

    private static readonly List<ImageBrush> Textures = [];

    private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdatePrimitive();

    private void OnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();

    private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

    private float Value(ScalarBox scalarBox, float min)
    {
        float.TryParse(scalarBox.Value, out var value);
        return Math.Max(value, min);
    }

    private void UpdatePrimitive()
    {
        if (!IsInitialized) return;

        var primitiveType = (PrimitiveMeshType)primTypeComboBox.SelectedItem;
        var info          = new PrimitiveInitInfo() { Type = primitiveType };

        switch (primitiveType)
        {
            case PrimitiveMeshType.Plane:
            {
                info.SegmentX = (int)xSliderPlane.Value;
                info.SegmentZ = (int)zSliderPlane.Value;
                info.Size.X   = Value(widthScalarBoxPlane,  0.001f);
                info.Size.Z   = Value(lengthScalarBoxPlane, 0.001f);
                break;
            }
            case PrimitiveMeshType.Cube:
                break;
            case PrimitiveMeshType.UvSphere:
                break;
            case PrimitiveMeshType.IcoSphere:
                break;
            case PrimitiveMeshType.Cylinder:
                break;
            case PrimitiveMeshType.Capsule:
                break;
            default:
                break;
        }

        ContentToolsAPI.CreatePrimitiveMesh(info, out var geometry);
        (DataContext as GeometryEditor)!.SetAsset(geometry);
        OnTexture_CheckBox_Click(textureCheckBox, null!);
    }

    private void OnTexture_CheckBox_Click(object sender, RoutedEventArgs e)
    {
        Brush brush = Brushes.White;

        if ((sender as CheckBox)!.IsChecked == true)
        {
            brush = Textures[(int)primTypeComboBox.SelectedItem];
        }

        var vm = DataContext as GeometryEditor;
        foreach (var mesh in vm!.MeshRenderer.Meshes)
        {
            mesh.Diffuse = brush;
        }
    }
}