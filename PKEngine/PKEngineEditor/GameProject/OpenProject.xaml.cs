using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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


            Loaded += (_, _) =>
            {
                var item =
                    projectListBox.ItemContainerGenerator.ContainerFromIndex(projectListBox.SelectedIndex) as
                        ListBoxItem;
                item?.Focus();
            };
        }

        private void OnOpen_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }

        private void OpenSelectedProject()
        {
            var  project   = OpenProjectViewModel.Open((projectListBox.SelectedItem as ProjectData)!);
            bool dialogRes = false;
            var  wind      = Window.GetWindow(this);
            if (project != null)
            {
                dialogRes        = true;
                wind.DataContext = project;
            }

            wind!.DialogResult = dialogRes;
            wind.Close();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }
    }
}