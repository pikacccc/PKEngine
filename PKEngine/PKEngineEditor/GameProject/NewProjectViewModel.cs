using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

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
        public string? ProjectFilePath { get; set; }
    }

    public class NewProjectViewModel : ViewModelBase
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
                    ValidateProjectPath();
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
                    ValidateProjectPath();
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
                    temp.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), temp.ProjectFile));
                    _projectTemplates.Add(temp);
                }

                ValidateProjectPath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to read project templates");
                throw;
            }
        }

        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMsg = "Type in a project name.";
                IsValid = false;
            }
            else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ErrorMsg = "Invalid character(s) used in project name.";
                IsValid = false;
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMsg = "Select a valid project folder.";
                IsValid = false;
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMsg = "Invalid character(s) used in project path.";
                IsValid = false;
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMsg = "Selected project folder already exists and is not empty.";
                IsValid = false;
            }
            else
            {
                ErrorMsg = string.Empty;
                IsValid = true;
            }
            return IsValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            ValidateProjectPath();
            if (!IsValid) return string.Empty;

            if (!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";
            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach (var folder in template.Folders)
                {
                    var directoryName = Path.GetDirectoryName(path);
                    var tempPath = Path.Combine(directoryName, folder);
                    var fullPath = Path.GetFullPath(tempPath);
                    Directory.CreateDirectory(fullPath);
                }
                var dirInfo = new DirectoryInfo(path + @".Pk\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.png")));
                File.Copy(template.ScreenShotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "ScreenShot.png")));

                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = string.Format(projectXml, ProjectName, path);
                var projectFilePath = Path.GetFullPath(Path.Combine(path,$"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectFilePath, projectXml);
                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create {ProjectName}");
                throw;
            }
        }
    }
}
