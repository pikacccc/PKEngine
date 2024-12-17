using PKEngineEditor.Components;
using PKEngineEditor.GameProject;
using System.Windows;
using System.Windows.Controls;

namespace PKEngineEditor.Editors
{
    /// <summary>
    /// ProjectLayoutView.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectLayoutView : UserControl
    {
        public ProjectLayoutView()
        {
            InitializeComponent();
        }

        private void OnAddGameEntity_Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var vm = btn.DataContext as Scene;
            vm.AddGameEntityCommand.Execute(new GameEntity(vm) { Name = "GameEntity" });
        }

        private void OnGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectItems = (sender as ListBox).SelectedItems;
            if (selectItems.Count != 0)
            {
                var entity = selectItems[0];
                GameEntityView.Instance.DataContext = entity;
            }
            else {
                GameEntityView.Instance.DataContext = null;
            }
        }
    }
}
