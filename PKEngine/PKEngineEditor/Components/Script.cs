using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace PKEngineEditor.Components
{
    [DataContract]
    class Script : Component
    {
        private string _name = null!;

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

        public Script(GameEntity owner) : base(owner)
        {
        }

        public override IMSComponent GetMSComponent(MSEntity msEntity) => new MSScript(msEntity);

        public override void WriteBinary(BinaryWriter bw)
        {
            var nameBytes = Encoding.UTF8.GetBytes(Name);
            bw.Write(nameBytes.Length);
            bw.Write(nameBytes);
        }
    }

    sealed class MSScript : MSComponent<Script>
    {
        private string _name = null!;

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

        public MSScript(MSEntity msEntity) : base(msEntity)
        {
            Refresh();
        }

        protected override bool UpdateComponents(string PropertyName)
        {
            if (PropertyName == nameof(Name))
            {
                SelectedComponents.ForEach(c => c.Name = _name);
                return true;
            }

            return false;
        }

        protected override bool UpdateMSComponent()
        {
            Name = MSEntity.GetMixedValue(SelectedComponents, x => x.Name)!;
            return true;
        }
    }
}