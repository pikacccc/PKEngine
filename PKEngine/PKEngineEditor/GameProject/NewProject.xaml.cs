using System.Windows;
using System.Windows.Controls;

namespace PKEngineEditor.GameProject
{
    /// <summary>
    /// NewProject.xaml 的交互逻辑
    /// </summary>
    public partial class NewProject : UserControl
    {
        public NewProject()
        {
            InitializeComponent();
        }

        private void OnCreateProjectBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var data = DataContext as NewProjectViewModel;
            var projectPath = data.CreateProject(templateListBox.SelectedItem as ProjectTemplate);
            var wind = Window.GetWindow(this);
            var dialogResult = false;
            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogResult = true;
                var project = OpenProjectViewModel.Open(new ProjectData { ProjectName = data.ProjectName, ProjectPath = projectPath });
                wind.DataContext = project;
            }
            wind.DialogResult = dialogResult;
            wind.Close();
        }
    }
}
