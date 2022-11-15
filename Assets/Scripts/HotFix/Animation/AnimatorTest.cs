using System.Collections.Generic;
using Resource;
using UnityEngine;
using YooAsset;

public class AnimatorTest : MonoBehaviour
{
    public GameObject animator;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ResUpdateManager.Instance.StartUpdate(this, () =>
        {
            Debug.Log("下载回调");
            List<Object> sps = new List<Object>();
            for (int i = 0; i <= 7; i++)
            {
                var objects = ResourceManager.Instance.LoadSubAsset($"SpriteRes/h1001_{i}", typeof(Sprite));
                for (int j = 0; j < objects.Length; j++)
                {
                    sps.Add(objects[j]);
                }
            }

            var ani = animator.AddComponent<FrameAnimator>();
            ani.Init(sps.ToArray());
            ani.ChangeFrame("atk", 1, -1);
            ani.Play();
        }));

    }
}
