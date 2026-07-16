using PKEngineEditor.Utilities;
using System.Numerics;
using System.Runtime.Serialization;

namespace PKEngineEditor.Components
{
    [DataContract]
    class Transform : Component
    {
        private Vector3 _position;
        [DataMember]
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private Vector3 _rotation;
        [DataMember]
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

        private Vector3 _scale;
        [DataMember]
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        public Transform(GameEntity owner) : base(owner)
        {
        }
    }

    sealed class MSTransform : MSComponent<Transform>
    {
        private float? _posX;
        public float? PosX
        {
            get => _posX;
            set
            {
                if (!_posX.IsTheSameAs(value))
                {
                    _posX = value;
                    OnPropertyChanged(nameof(PosX));
                }
            }
        }

        private float? _posY;
        public float? PosY
        {
            get => _posY;
            set
            {
                if (!_posY.IsTheSameAs(value))
                {
                    _posY = value;
                    OnPropertyChanged(nameof(PosY));
                }
            }
        }

        private float? _posZ;
        public float? PosZ
        {
            get => _posZ;
            set
            {
                if (!_posZ.IsTheSameAs(value))
                {
                    _posZ = value;
                    OnPropertyChanged(nameof(PosZ));
                }
            }
        }

        private float? _rotX;
        public float? RotX
        {
            get => _rotX;
            set
            {
                if (!_rotX.IsTheSameAs(value))
                {
                    _rotX = value;
                    OnPropertyChanged(nameof(RotX));
                }
            }
        }

        private float? _rotY;
        public float? RotY
        {
            get => _rotY;
            set
            {
                if (!_rotY.IsTheSameAs(value))
                {
                    _rotY = value;
                    OnPropertyChanged(nameof(RotY));
                }
            }
        }

        private float? _rotZ;
        public float? RotZ
        {
            get => _rotZ;
            set
            {
                if (!_rotZ.IsTheSameAs(value))
                {
                    _rotZ = value;
                    OnPropertyChanged(nameof(RotZ));
                }
            }
        }

        private float? _scaleX;
        public float? ScaleX
        {
            get => _scaleX;
            set
            {
                if (!_scaleX.IsTheSameAs(value))
                {
                    _scaleX = value;
                    OnPropertyChanged(nameof(ScaleX));
                }
            }
        }

        private float? _scaleY;
        public float? ScaleY
        {
            get => _scaleY;
            set
            {
                if (!_scaleY.IsTheSameAs(value))
                {
                    _scaleY = value;
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }

        private float? _scaleZ;
        public float? ScaleZ
        {
            get => _scaleZ;
            set
            {
                if (!_scaleZ.IsTheSameAs(value))
                {
                    _scaleZ = value;
                    OnPropertyChanged(nameof(ScaleZ));
                }
            }
        }

        public MSTransform(MSEntity msEntity) : base(msEntity)
        {
            Refresh();
        }

        protected override bool UpdateComponents(string PropertyName)
        {
            switch (PropertyName)
            {
                case nameof(PosX):
                    if (PosX.HasValue)
                        SelectedComponents.ForEach(x => x.Position = new Vector3(PosX.Value, x.Position.Y, x.Position.Z));
                    return true;
                case nameof(PosY):
                    if (PosY.HasValue)
                        SelectedComponents.ForEach(x => x.Position = new Vector3(x.Position.X, PosY.Value, x.Position.Z));
                    return true;
                case nameof(PosZ):
                    if (PosZ.HasValue)
                        SelectedComponents.ForEach(x => x.Position = new Vector3(x.Position.X, x.Position.Y, PosZ.Value));
                    return true;
                case nameof(RotX):
                    if (RotX.HasValue)
                        SelectedComponents.ForEach(x => x.Rotation = new Vector3(RotX.Value, x.Rotation.Y, x.Rotation.Z));
                    return true;
                case nameof(RotY):
                    if (RotY.HasValue)
                        SelectedComponents.ForEach(x => x.Rotation = new Vector3(x.Rotation.X, RotY.Value, x.Rotation.Z));
                    return true;
                case nameof(RotZ):
                    if (RotZ.HasValue)
                        SelectedComponents.ForEach(x => x.Rotation = new Vector3(x.Rotation.X, x.Rotation.Y, RotZ.Value));
                    return true;
                case nameof(ScaleX):
                    if (ScaleX.HasValue)
                        SelectedComponents.ForEach(x => x.Scale = new Vector3(ScaleX.Value, x.Scale.Y, x.Scale.Z));
                    return true;
                case nameof(ScaleY):
                    if (ScaleY.HasValue)
                        SelectedComponents.ForEach(x => x.Scale = new Vector3(x.Scale.X, ScaleY.Value, x.Scale.Z));
                    return true;
                case nameof(ScaleZ):
                    if (ScaleZ.HasValue)
                        SelectedComponents.ForEach(x => x.Scale = new Vector3(x.Scale.X, x.Scale.Y, ScaleZ.Value));
                    return true;
                default:
                    return false;
            }
        }

        protected override bool UpdateMSComponent()
        {
            PosX = GetMixedValue(x => x.Position.X);
            PosY = GetMixedValue(x => x.Position.Y);
            PosZ = GetMixedValue(x => x.Position.Z);
            RotX = GetMixedValue(x => x.Rotation.X);
            RotY = GetMixedValue(x => x.Rotation.Y);
            RotZ = GetMixedValue(x => x.Rotation.Z);
            ScaleX = GetMixedValue(x => x.Scale.X);
            ScaleY = GetMixedValue(x => x.Scale.Y);
            ScaleZ = GetMixedValue(x => x.Scale.Z);
            return true;
        }

        private float? GetMixedValue(Func<Transform, float> getValue)
        {
            var value = getValue(SelectedComponents.First());
            return SelectedComponents.Skip(1).Any(x => !value.IsTheSameAs(getValue(x))) ? null : value;
        }
    }
}
