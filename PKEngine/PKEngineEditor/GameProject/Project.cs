using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

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

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> ReadOnlyScenes { get; private set; }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }

        }

        public static Project CurProject => Application.Current.MainWindow.DataContext as Project;

        public static UndoRedoManager UndoRedoMgr { get; } = new UndoRedoManager();

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public ICommand AddSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        private void AddScene(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }

        private void RemoveScene(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }

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
            if (_scenes != null)
            {
                ReadOnlyScenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(ReadOnlyScenes));
            }
            ActiveScene = ReadOnlyScenes.FirstOrDefault(s => s.IsActive);

            InitCommands();
        }

        private void InitCommands()
        {
            AddSceneCommand = new RelayCommand<object>(x =>
            {
                AddScene($"New Scene {_scenes.Count}");
                var newscene = _scenes.Last();
                var sceneIndex = _scenes.Count - 1;

                UndoRedoMgr.Add(new UndoRedoAction(
                    () => { RemoveScene(newscene); },
                    () => { _scenes.Insert(sceneIndex, newscene); },
                    $"Add {newscene.Name}"));
            });

            RemoveSceneCommand = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenes.IndexOf(x);
                RemoveScene(x);

                UndoRedoMgr.Add(new UndoRedoAction(
                    () => { _scenes.Insert(sceneIndex, x); },
                    () => { RemoveScene(x); },
                    $"Remove {x.Name}"));
            }, x => !x.IsActive);

            UndoCommand = new RelayCommand<object>((x) => UndoRedoMgr.Undo());

            RedoCommand = new RelayCommand<object>((x) => UndoRedoMgr.Redo());

            SaveCommand = new RelayCommand<object>((x) => Save(this));
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
