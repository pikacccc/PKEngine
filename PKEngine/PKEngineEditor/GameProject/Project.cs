using PKEngineEditor.Common;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using PKEngineEditor.Components;
using PKEngineEditor.DllWrappers;
using PKEngineEditor.GameDev;

namespace PKEngineEditor.GameProject
{
    public enum BuildConfiguration
    {
        Debug,
        DebugEditor,
        Release,
        ReleaseEditor,
    }

    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".pk";

        [DataMember] public string Name { get; private set; } = "New Project";

        [DataMember] public string Path { get; private set; }

        public string FullPath => $@"{Path}{Name}{Extension}";

        public string Solution => $@"{Path}{Name}.sln";

        private static readonly string[] _buildConfigurations = { "Debug", "DebugEditor", "Release", "ReleaseEditor" };

        [DataMember(Name = "Scenes")] private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> ReadOnlyScenes { get; private set; }

        private int _buildConfig;

        [DataMember]
        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig != value)
                {
                    _buildConfig = value;
                    OnPropertyChanged(nameof(BuildConfig));
                }
            }
        }

        public BuildConfiguration StandAloneBuildConfig =>
            BuildConfig == 0 ? BuildConfiguration.Debug : BuildConfiguration.Release;

        public BuildConfiguration DllBuildConfig =>
            BuildConfig == 0 ? BuildConfiguration.DebugEditor : BuildConfiguration.ReleaseEditor;

        private string[] _availableScripts;

        public string[] AvailableScripts
        {
            get => _availableScripts;
            set
            {
                if (_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
                }
            }
        }

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

        public ICommand BuildCommand { get; private set; }

        public ICommand DebugStartCommand { get; private set; }

        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }

        public ICommand DebugStopCommand { get; private set; }

        private static string GetConfigurationName(BuildConfiguration config) => _buildConfigurations[(int)config];

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
            Logger.Log(MessageType.Info, $"Project saved to {project.FullPath}");
        }

        private void SaveToBinary()
        {
            var configName = GetConfigurationName(StandAloneBuildConfig);
            var bin = $@"{Path}x64\{configName}\game.bin";
            using (var bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(ActiveScene.GameEntities.Count);
                foreach (var entity in ActiveScene.GameEntities)
                {
                    bw.Write(0);
                    bw.Write(entity.Components.Count);
                    foreach (var component in entity.Components)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WriteBinary(bw);
                    }
                }
            }
        }

        private async Task RunGame(bool debug)
        {
            var configName = GetConfigurationName(StandAloneBuildConfig);
            await Task.Run(() => VisualStudio.BuildSolution(this, configName, debug));
            if (VisualStudio.BuildSucceeded)
            {
                SaveToBinary();
                await Task.Run(() => VisualStudio.Run(this, configName, debug));
            }
        }

        private async Task StopGame() => await Task.Run(() => VisualStudio.Stop());

        private async Task BuildGameCodeDll(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDll();

                await Task.Run(() =>
                    VisualStudio.BuildSolution(this, GetConfigurationName(DllBuildConfig), showWindow));
                if (VisualStudio.BuildSucceeded)
                {
                    LoadGameCodeDll();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void LoadGameCodeDll()
        {
            var configName = GetConfigurationName(DllBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";
            AvailableScripts = null;
            if (File.Exists(dll) && EngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = EngineAPI.GetScriptNames();
                ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList()
                    .ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, $"Game code loaded from {dll}");
            }
            else
            {
                Logger.Log(MessageType.Warning, $"Game code could not be loaded from {dll}");
            }
        }

        private void UnloadGameCodeDll()
        {
            ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList()
                .ForEach(x => x.IsActive = false);
            if (EngineAPI.UnloadGameCodeDll() != 0)
            {
                AvailableScripts = null;
                Logger.Log(MessageType.Info, "Game code unloaded");
            }
        }

        public void Unload()
        {
            UnloadGameCodeDll();
            VisualStudio.CloseVisualStudio();
            UndoRedoMgr.Reset();
        }

        [OnDeserialized]
        public async void OnDeserialized(StreamingContext context)
        {
            try
            {
                if (_scenes != null)
                {
                    ReadOnlyScenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                    OnPropertyChanged(nameof(ReadOnlyScenes));
                }

                ActiveScene = ReadOnlyScenes.FirstOrDefault(s => s.IsActive);
                Debug.Assert(ActiveScene != null);
                await BuildGameCodeDll(false);

                InitCommands();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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

            UndoCommand = new RelayCommand<object>(_ => UndoRedoMgr.Undo(), _ => UndoRedoMgr.UndoList.Any());

            RedoCommand = new RelayCommand<object>(_ => UndoRedoMgr.Redo(), _ => UndoRedoMgr.RedoList.Any());

            SaveCommand = new RelayCommand<object>(_ => Save(this));

            BuildCommand = new RelayCommand<bool>(async (x) => await BuildGameCodeDll(x),
                x => !(VisualStudio.IsDebugging() && VisualStudio.BuildDone));

            DebugStartCommand = new RelayCommand<object>(async (_) => await RunGame(true),
                _ => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);

            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(async (_) => await RunGame(false),
                _ => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);

            DebugStopCommand = new RelayCommand<object>(async (_) => await StopGame(),
                _ => VisualStudio.IsDebugging());

            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(BuildCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
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