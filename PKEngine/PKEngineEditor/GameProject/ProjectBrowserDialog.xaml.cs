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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PKEngineEditor.GameProject
{
    /// <summary>
    /// ProjectBrowserDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectBrowserDialog : Window
    {
        private readonly CubicEase _easing = new CubicEase() { EasingMode = EasingMode.EaseInOut};

        public ProjectBrowserDialog()
        {
            InitializeComponent();
            Loaded += OnProjectBrowserDialogLoaded;
        }

        private void OnProjectBrowserDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserDialogLoaded;
            if (!OpenProjectViewModel.Projects.Any())
            {
                openProjectBtn.IsEnabled = false;
                openProjectBtn.Visibility = Visibility.Hidden;
                OnToggleBtn_Click(createProjectBtn, new RoutedEventArgs());
            }
        }

        private void OnToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjectBtn)
            {
                if (createProjectBtn.IsChecked == true)
                {
                    createProjectBtn.IsChecked = false;
                    AnimateToOpenProject();
                    openProjectView.IsEnabled = true;
                    newProjectView.IsEnabled = false;
                }
                openProjectBtn.IsChecked = true;
            }
            else
            {
                if (openProjectBtn.IsChecked == true)
                {
                    openProjectBtn.IsChecked = false;
                    AnimateToCreateProject();
                    openProjectView.IsEnabled = false;
                    newProjectView.IsEnabled = true;
                }
                createProjectBtn.IsChecked = true;
            }
        }

        private void AnimateToCreateProject()
        {
            var hightlightAnimation = new DoubleAnimation(200, 400, new Duration(TimeSpan.FromSeconds(0.2)));
            hightlightAnimation.EasingFunction = _easing;
            hightlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)));
                animation.EasingFunction = _easing;
                browserContent.BeginAnimation(MarginProperty, animation);
            };
            hightlightRect.BeginAnimation(Canvas.LeftProperty, hightlightAnimation);
        }

        private void AnimateToOpenProject()
        {
            var hightlightAnimation = new DoubleAnimation(400, 200, new Duration(TimeSpan.FromSeconds(0.2)));
            hightlightAnimation.EasingFunction = _easing;
            hightlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.5)));
                animation.EasingFunction = _easing;
                browserContent.BeginAnimation(MarginProperty, animation);
            };
            hightlightRect.BeginAnimation(Canvas.LeftProperty, hightlightAnimation);
        }
    }
}
