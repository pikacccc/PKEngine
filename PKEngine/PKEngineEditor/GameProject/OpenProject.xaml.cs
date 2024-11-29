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

namespace PKEngineEditor.GameProject
{
    /// <summary>
    /// OpenProject.xaml 的交互逻辑
    /// </summary>
    public partial class OpenProject : UserControl
    {
        public OpenProject()
        {
            InitializeComponent();
        }

        private void OnOpen_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }

        private void OpenSelectedProject()
        {
            var project = OpenProjectViewModel.Open(projectListBox.SelectedItem as ProjectData);
            bool dialogRes = false;
            var wind = Window.GetWindow(this);
            if (project != null)
            {
                dialogRes = true;
                wind.DataContext = project;
            }
            wind.DialogResult = dialogRes;
            wind.Close();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }
    }
}
