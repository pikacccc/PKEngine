using PKEngineEditor.Common;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace PKEngineEditor.Components
{
    public interface IMSComponent
    {
    }

    [DataContract]
    public abstract class Component : ViewModelBase
    {
        [DataMember] public GameEntity Owner { get; private set; }

        public abstract IMSComponent GetMSComponent(MSEntity msEntity);

        public abstract void WriteBinary(BinaryWriter bw);

        public Component(GameEntity owner)
        {
            Debug.Assert(owner != null);
            Owner = owner;
        }
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
            Debug.Assert(msEntity?.SelectedEntities?.Any() == true);
            SelectedComponents = msEntity.SelectedEntities.Select(e => e.GetComponent<T>()).ToList()!;
            PropertyChanged += (_, e) =>
            {
                if (_enableUpdate) UpdateComponents(e.PropertyName!);
            };
        }
    }
}