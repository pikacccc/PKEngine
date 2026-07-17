using PKEngineEditor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PKEngineEditor.Components
{
    interface IMSComponent { }

    [DataContract]
    abstract class Component : ViewModelBase
    {
        [DataMember]
        public GameEntity Owner { get;private set; }

        public Component(GameEntity owner) {
            Debug.Assert(owner != null);
            Owner = owner;
        }

        public abstract IMSComponent GetMSComponent(MSEntity msEntity);
    }

    abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : Component
    {
        public List<T> SelectedComponents { get; }

        protected abstract bool UpdateComponents(string PropertyName);
        protected abstract bool UpdateMSComponent();

        private bool _enableUpdate = true;

        public void Refresh()
        {
            _enableUpdate = false;
            UpdateMSComponent();
            _enableUpdate = true;
        }
        public MSComponent(MSEntity msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntities?.Any()==true);
            SelectedComponents = msEntity.SelectedEntities.Select(e=>e.GetComponent<T>()).ToList();
            PropertyChanged += (s,e)=> { if (_enableUpdate) UpdateComponents(e.PropertyName); };
        }
    }
}
