using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PKEngineEditor.GameProject;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.GameDev;

public partial class NewScriptDialog : Window
{
    public NewScriptDialog()
    {
        InitializeComponent();
        Owner           = Application.Current.MainWindow;
        scriptPath.Text = @"GameCode\";
    }

    private static readonly string _cppCode = @"#include ""{0}.h""

namespace {1} {{

    REGISTER_SCRIPT({0});
    void {0}::begin_play()
    {{

    }}

    void {0}::update(float dt)
    {{

    }}

}}";

    private static readonly string _hCode = @"#pragma once

namespace {1} {{

    class {0} : public pk::script::entity_script
    {{
    public:
        constexpr explicit {0}(entity entity) : entity_script{{entity}}{{}}
        
        void update(float) override;
        void begin_play() override;
    }};

}}";

    private static readonly string _nameSpace = GetNameSpaceFromProjectName();

    private static string GetNameSpaceFromProjectName()
    {
        var projectName = Project.CurProject.Name;
        projectName = projectName.Replace(' ', '_');
        return projectName;
    }

    private bool Validate()
    {
        bool   isValid = false;
        var    name    = scriptName.Text.Trim();
        var    path    = scriptPath.Text.Trim();
        string errMsg  = string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            errMsg = "Type in a script name.";
        }
        else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(x => char.IsWhiteSpace(x)))
        {
            errMsg = "Invalid character(s) used in script name.";
        }
        else if (string.IsNullOrEmpty(path))
        {
            errMsg = "Select a valid script folder.";
        }
        else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            errMsg = "Invalid character(s) used in script path.";
        }
        else if (!Path.GetFullPath(Path.Combine(Project.CurProject.Path, path))
                      .Contains(Path.Combine(Project.CurProject.Path,    @"GameCode\")))
        {
            errMsg = "Script must be added to (a sub-folder of) GameCode.";
        }
        else if (File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.CurProject.Path, path),
                                                           $"{name}.cpp"))) ||
                 File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.CurProject.Path, path), $"{name}.h"))))
        {
            errMsg = $"Script {name}  is already exists in this folder.";
        }
        else
        {
            isValid = true;
        }

        if (!isValid)
        {
            messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
        }
        else
        {
            messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;
        }

        messageTextBlock.Text = errMsg;
        return isValid;
    }

    private void CreateScript(string name, string path, string solution, string projectName)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
        var h   = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

        using (var sw = File.CreateText(cpp))
        {
            sw.Write(_cppCode, name, _nameSpace);
        }

        using (var sw = File.CreateText(h))
        {
            sw.Write(_hCode, name, _nameSpace);
        }

        var files = new[] { cpp, h };

        for (int i = 0; i < 3; ++i)
        {
            if (!VisualStudio.AddFilesToSolution(solution, projectName, files)) Thread.Sleep(1000);
            else break;
        }
    }

    private async void OnCreate_Button_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        IsEnabled                = false;
        busyAnimation.Opacity    = 0;
        busyAnimation.Visibility = Visibility.Visible;

        var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
        busyAnimation.BeginAnimation(OpacityProperty, fadeIn);
        try
        {
            var name        = scriptName.Text.Trim();
            var path        = Path.GetFullPath(Path.Combine(Project.CurProject.Path, scriptPath.Text.Trim()));
            var solution    = Project.CurProject.Solution;
            var projectName = Project.CurProject.Name;
            await Task.Run(() => { CreateScript(name, path, solution, projectName); });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Logger.Log(MessageType.Error, $"Failed to create script {scriptName.Text}");
        }
        finally
        {
            var fadeOut = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
            fadeOut.Completed += (_, _) =>
                                 {
                                     busyAnimation.Opacity    = 0;
                                     busyAnimation.Visibility = Visibility.Hidden;
                                     Close();
                                 };
            busyAnimation.BeginAnimation(OpacityProperty, fadeOut);
        }
    }

    private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!Validate()) return;
        var name = scriptName.Text.Trim();
        messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.CurProject.Name}";
    }

    private void OnScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Validate();
    }
}