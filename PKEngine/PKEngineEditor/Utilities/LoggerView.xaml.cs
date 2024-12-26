using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            Logger.Log(MessageType.Info, "Information message");
            Logger.Log(MessageType.Warning, "Warning message");
            Logger.Log(MessageType.Error, "Error message");
        }

        private void OnClear_Btn_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }
    }
}
