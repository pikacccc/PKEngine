using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace PKEngineEditor.GameProject
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string? ProjectType { get; set; }
        [DataMember]
        public string? ProjectFile { get; set; }
        [DataMember]
        public List<string>? Folders { get; set; }

        public byte[]? Icon { get; set; }
        public byte[]? ScreenShot { get; set; }
        public string? IconFilePath { get; set; }
        public string? ScreenShotFilePath { get; set; }
        public string? ProjectPath { get; set; }
    }

    class NewProjectViewModel : ViewModelBase
    {
        private readonly string _templatePath = @"..\..\PKEngineEditor\ProjectTemplates";

        private string _projectName = "NewProject";
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (value != _projectName)
                {
                    _projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\PKProject\";
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (value != _projectPath)
                {
                    _projectPath = value;
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = new ObservableCollection<ProjectTemplate>();

        private ReadOnlyObservableCollection<ProjectTemplate> _readOnlyProjectTemplates;
        public ReadOnlyObservableCollection<ProjectTemplate> ReadOnlyProjectTemplates
        {
            get
            {
                if (_readOnlyProjectTemplates == null)
                {
                    _readOnlyProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);
                }
                return _readOnlyProjectTemplates;
            }
        }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string _errorMsg;
        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (_errorMsg != value)
                {
                    _errorMsg = value;
                    OnPropertyChanged(nameof(ErrorMsg));
                }
            }
        }

        public NewProjectViewModel()
        {
            try
            {
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                foreach (var templateFile in templateFiles)
                {
                    var temp = Serializer.FromFile<ProjectTemplate>(templateFile);
                    temp.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), "Icon.png"));
                    temp.Icon = File.ReadAllBytes(temp.IconFilePath);
                    temp.ScreenShotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), "ScreenShot.png"));
                    temp.ScreenShot = File.ReadAllBytes(temp.ScreenShotFilePath);
                    temp.ProjectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), temp.ProjectFile));
                    _projectTemplates.Add(temp);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";
            IsValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMsg = "Type in a project name.";
            }
            return true;
        }
    }
}
