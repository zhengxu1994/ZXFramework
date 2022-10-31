using UnityEngine;
using YooAsset;
using static YooAsset.YooAssets;
using Resource;
public class YooAssetDemo : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ResUpdateManager.Instance.StartUpdate(this, () => {
            Debug.Log("下载回调");
            var asset = YooAssets.LoadAssetSync<GameObject>("Prefab/Cube");
            var obj = asset.AssetObject as GameObject;
            if (obj != null)
                Debug.Log($"obj:{obj.name}");
        }));
    }

    private void Update()
    {
        
    }
}
