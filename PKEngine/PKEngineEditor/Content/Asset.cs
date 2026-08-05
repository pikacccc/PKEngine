using System.Diagnostics;
using PKEngineEditor.Common;

namespace PKEngineEditor.Content
{
    public enum AssetType
    {
        Unknown,
        Animation,
        Audio,
        Material,
        Mesh,
        Skeleton,
        Texture,
    }

    public abstract class Asset : ViewModelBase
    {
        public AssetType Type { get; private set; }

        public Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.Unknown, "Asset type cannot be Unknown");
            Type = type;
        }
    }
}