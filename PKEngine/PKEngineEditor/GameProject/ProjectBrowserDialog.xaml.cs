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
using System.Windows.Shapes;

namespace PKEngineEditor.GameProject
{
    /// <summary>
    /// ProjectBrowserDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectBrowserDialog : Window
    {
        public ProjectBrowserDialog()
        {
            InitializeComponent();
        }

        private void OnToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjectBtn)
            {
                if(createProjectBtn.IsChecked == true)
                {
                    createProjectBtn.IsChecked = false;
                    browserContent.Margin =new Thickness(0);
                }
                openProjectBtn.IsChecked = true;
            }
            else
            {
                if(openProjectBtn.IsChecked == true)
                {
                    openProjectBtn.IsChecked = false;
                    browserContent.Margin = new Thickness(-800,0,0,0);
                }
                createProjectBtn.IsChecked = true;
            }
        }
    }
}
