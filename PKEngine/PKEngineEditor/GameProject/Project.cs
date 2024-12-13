using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;

namespace PKEngineEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".pk";

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }

        public string FullPath => $@"{Path}{Name}{Extension}";

        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scene = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> ReadOnlyScene{ get; private set; }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get =>_activeScene;
            set
            {
                if(_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }

        }

        public static Project CurProject => Application.Current.MainWindow.DataContext as Project;

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
        }

        public void Unload() { }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (_scene != null)
            {
                ReadOnlyScene = new ReadOnlyObservableCollection<Scene>(_scene);
                OnPropertyChanged(nameof(ReadOnlyScene));
            }
            ActiveScene = ReadOnlyScene.FirstOrDefault(s => s.IsActive);
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
        }

        public Project()
        {

        }
    }
}
