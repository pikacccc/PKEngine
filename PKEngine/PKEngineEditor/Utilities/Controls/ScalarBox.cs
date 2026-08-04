namespace PKEngineEditor.Utilities.Controls
{
    public class ScalarBox : NumberBox
    {
        static ScalarBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalarBox),
                                                     new System.Windows.FrameworkPropertyMetadata(typeof(ScalarBox)));
        }
    }
}