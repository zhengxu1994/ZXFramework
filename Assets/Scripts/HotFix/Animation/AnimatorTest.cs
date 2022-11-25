using System.Collections.Generic;
using Resource;
using UnityEngine;
using YooAsset;

public class AnimatorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ResUpdateManager.Instance.StartUpdate(this, () =>
        {
            Debug.Log("下载回调");
            var graph = new BaseGraph("h1001");
            graph.PlayAction("atk", 1, -1);
            graph.frameAnimator.FrameRate = 10;
        }));

    }
}
