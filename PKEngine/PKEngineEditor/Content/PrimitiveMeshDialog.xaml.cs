using System.Windows;
using System.Windows.Controls;
using PKEngineEditor.ContentToolsAPIStructs;
using PKEngineEditor.DllWrappers;
using PKEngineEditor.Utilities.Controls;

namespace PKEngineEditor.Content;

public partial class PrimitiveMeshDialog : Window
{
    public PrimitiveMeshDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdatePrimitive();
    }

    private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();

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
    }
}