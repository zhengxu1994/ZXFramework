using System.IO;
using YooAsset.Editor;

public class AssetPackGUI : IPackRule
{
    public string GetBundleName(PackRuleData data)
    {
        var directory = Path.GetDirectoryName(data.AssetPath);
        var assetName = Path.GetFileNameWithoutExtension(data.AssetPath);
        assetName = assetName.Replace("!a", "");
        return Path.Combine(directory, assetName);
    }
}