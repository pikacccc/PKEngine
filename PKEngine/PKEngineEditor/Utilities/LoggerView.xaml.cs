using System.Windows;
using System.Windows.Controls;

namespace PKEngineEditor.Utilities
{
    /// <summary>
    /// LoggerView.xaml 的交互逻辑
    /// </summary>
    public partial class LoggerView : UserControl
    {
        public LoggerView()
        {
            InitializeComponent();
        }

        private void LogTest()
        {
            Logger.Log(MessageType.Info,    "Information message");
            Logger.Log(MessageType.Warning, "Warning message");
            Logger.Log(MessageType.Error,   "Error message");
        }

        private void OnClear_Btn_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }

        private void OnMessageFilter_Btn_Click(object sender, RoutedEventArgs e)
        {
            var filter                                = 0x0;
            if (toggleInfo.IsChecked  == true) filter |= (int)MessageType.Info;
            if (toggleWarn.IsChecked  == true) filter |= (int)MessageType.Warning;
            if (toggleError.IsChecked == true) filter |= (int)MessageType.Error;
            Logger.SetMessageFilter(filter);
        }
    }
}