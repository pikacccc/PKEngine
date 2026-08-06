using PKEngineEditor.Content;

namespace PKEngineEditor.Editors
{
    public interface IAssetEditor
    {
        Asset Asset { get; }
        
        void SetAsset(Asset asset);
    }
}