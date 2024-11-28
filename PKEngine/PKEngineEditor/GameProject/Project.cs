using PKEngineEditor.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PKEngineEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".pk";

        [DataMember]
        public string Name {  get;private set; }

        [DataMember]
        public string Path { get; private set; }

        public string FullPath => $@"{Path}{Name}{Extension}";

        [DataMember(Name ="Scenes")]
        private ObservableCollection<Scene> _scene = new ObservableCollection<Scene>();

        private ReadOnlyObservableCollection<Scene> _readOnlyScene;
        public ReadOnlyObservableCollection<Scene> ReadOnlyScene
        {
            get
            {
                if (_readOnlyScene == null)
                {
                    _readOnlyScene = new ReadOnlyObservableCollection<Scene>(_scene);
                }
                return _readOnlyScene;
            }
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            _scene.Add(new Scene(this, "Default Scene"));
        }
    }
}
