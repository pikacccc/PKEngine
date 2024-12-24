using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace PKEngineEditor.GameProject
{
    [DataContract]
    public class ProjectData
    {
        [DataMember]
        public string? ProjectName { get; set; }

        [DataMember]
        public string? ProjectPath { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        public string FullPath => $@"{ProjectPath}{ProjectName}{Project.Extension}";

        public byte[]? Icon { get; set; }
        public byte[]? ScreenShot { get; set; }
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData>? Projects { get; set; }
    }

    public class OpenProjectViewModel
    {
        private static readonly string _applicationDataPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\PKEngineEditor\";
        private static readonly string _projectDataPath;

        private static readonly ObservableCollection<ProjectData> _projects = new ObservableCollection<ProjectData>();
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }


        private static void ReadProjectData()
        {
            if (File.Exists(_projectDataPath))
            {
                var projects = Serializer.FromFile<ProjectDataList>(_projectDataPath).Projects.OrderByDescending(s => s.Date);
                _projects.Clear();
                foreach (var project in projects)
                {
                    if (File.Exists(project.FullPath))
                    {
                        project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Pk\Icon.png");
                        project.ScreenShot = File.ReadAllBytes($@"{project.ProjectPath}\.Pk\ScreenShot.png");
                        _projects.Add(project);
                    }
                }
            }
        }

        private static void WriteProjectData()
        {
            var projects = _projects.OrderBy(s => s.Date).ToList();
            Serializer.ToFile(new ProjectDataList { Projects = projects }, _projectDataPath);
        }

        public static Project Open(ProjectData data)
        {
            ReadProjectData();
            var project = _projects.FirstOrDefault(s => s.FullPath == data.FullPath);
            if (project != null)
            {
                project.Date = DateTime.Now;
            }
            else
            {
                project = data;
                project.Date = DateTime.Now;
                _projects.Add(project);
            }
            WriteProjectData();
            return Project.Load(project.FullPath);
        }

        static OpenProjectViewModel()
        {
            try
            {
                if (!Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_applicationDataPath);
                _projectDataPath = $@"{_applicationDataPath}ProjectDara.xml";
                ReadProjectData();
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to read project data");
                throw;
            }
        }
    }
}
