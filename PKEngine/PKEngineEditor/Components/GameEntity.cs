using PKEngineEditor.Common;
using PKEngineEditor.GameProject;
using PKEngineEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace PKEngineEditor.Components
{
    [DataContract]
    [KnownType(typeof(Transform))]
    public class GameEntity : ViewModelBase
    {
        private bool _isEnabled = true;
        [DataMember]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private string _name;
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [DataMember]
        public Scene ParentScene { get; private set; }

        [DataMember(Name = nameof(Components))]
        private readonly ObservableCollection<Component> _components = new ObservableCollection<Component>();
        public ReadOnlyObservableCollection<Component> Components { get; private set; }


        public ICommand RenameCommand { get; private set; }
        public ICommand EnableCommand { get; private set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (_components != null)
            {
                Components = new ReadOnlyObservableCollection<Component>(_components);
                OnPropertyChanged(nameof(Components));
            }

            RenameCommand = new RelayCommand<string>(x =>
            {
                var oldName = Name;
                Name = x;

                Project.UndoRedoMgr.Add(new UndoRedoAction(nameof(Name), this, oldName, x, $"Rename entity '{oldName}' to '{x}'"));
            }, x => x != _name);
        }

        public GameEntity(Scene scene)
        {
            Debug.Assert(scene != null);

            ParentScene = scene;
            _components.Add(new Transform(this));

            OnDeserialized(new StreamingContext());
        }
    }
}
