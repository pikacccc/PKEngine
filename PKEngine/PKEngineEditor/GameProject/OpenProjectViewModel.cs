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

        public DateTime Date { get; set; }

        public string FullPath => $@"{ProjectPath}{ProjectName}{Project.Extension}";
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
            return null;
        }

        static OpenProjectViewModel()
        {
            try
            {
                if (!Directory.Exists(_projectDataPath)) Directory.CreateDirectory(_projectDataPath);
                _projectDataPath = $@"{_applicationDataPath}ProjectDara.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                ReadProjectData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
