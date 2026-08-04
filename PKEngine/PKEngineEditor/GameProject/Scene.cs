using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;
using PKEngineEditor.Common;
using PKEngineEditor.Components;
using PKEngineEditor.Utilities;

namespace PKEngineEditor.GameProject;

[DataContract]
public class Scene : ViewModelBase
{
    [DataMember(Name = nameof(GameEntities))]
    private ObservableCollection<GameEntity> _gameEntities = new();

    private bool   _isActive;
    private string _name = null!;

    public Scene(Project project, string name)
    {
        Debug.Assert(project != null);
        Name    = name;
        Project = project;
        OnDeserialized(new StreamingContext());
    }

    [DataMember]
    public string Name
    {
        get => _name;
        set
        {
            if (value != _name)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    [DataMember] public Project Project { get; private set; }

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value != _isActive)
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    public ReadOnlyObservableCollection<GameEntity> GameEntities { get; private set; } = null!;


    public ICommand AddGameEntityCommand    { get; private set; } = null!;
    public ICommand RemoveGameEntityCommand { get; private set; } = null!;

    private void AddGameEntity(GameEntity entity, int index = -1)
    {
        Debug.Assert(!_gameEntities.Contains(entity));
        entity.IsActive = IsActive;
        if (index == -1)
            _gameEntities.Add(entity);
        else
            _gameEntities.Insert(index, entity);
    }

    private void RemoveGameEntity(GameEntity entity)
    {
        Debug.Assert(_gameEntities.Contains(entity));
        entity.IsActive = false;
        _gameEntities.Remove(entity);
    }

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        if (_gameEntities == null) _gameEntities = new ObservableCollection<GameEntity>();
        if (_gameEntities != null)
        {
            GameEntities = new ReadOnlyObservableCollection<GameEntity>(_gameEntities);
            OnPropertyChanged(nameof(GameEntity));
        }

        foreach (var entity in _gameEntities!) entity.IsActive = IsActive;

        InitCommands();
    }

    private void InitCommands()
    {
        AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
                                                            {
                                                                AddGameEntity(x);
                                                                var entityIndex = _gameEntities.Count - 1;

                                                                Project.UndoRedoMgr.Add(new UndoRedoAction(
                                                                 () => { RemoveGameEntity(x); },
                                                                 () => { AddGameEntity(x, entityIndex); },
                                                                 $"Add {x.Name} to {Name}"));
                                                            });

        RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
                                                               {
                                                                   var entityIndex = _gameEntities.IndexOf(x);
                                                                   RemoveGameEntity(x);

                                                                   Project.UndoRedoMgr.Add(new UndoRedoAction(
                                                                    () => { AddGameEntity(x, entityIndex); },
                                                                    () => { RemoveGameEntity(x); },
                                                                    $"Remove {x.Name} from {Name}"));
                                                               });
    }
}