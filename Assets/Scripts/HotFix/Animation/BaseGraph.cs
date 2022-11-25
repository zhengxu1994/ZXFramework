using System;
using System.Collections.Generic;
using UnityEngine;
using Resource;
using Object = UnityEngine.Object;
public class BaseGraph : IDisposable
{
    public GameObject goRoot { get; protected set; }

    public FrameAnimator frameAnimator { get; protected set; }
    public BaseGraph(string name)
    {
        goRoot = new GameObject();
        var objects = CreatePlayer(name);
        frameAnimator = goRoot.AddComponent<FrameAnimator>();
        frameAnimator.Init(objects);
    }

    private static List<Object> cacheSps = new List<Object>();
    public static Object[] CreatePlayer(string name)
    {
        cacheSps.Clear();
        for (int i = 0; i <= 7; i++)
        {
            var objects = ResourceManager.Instance.LoadSubAsset($"SpriteRes/{name}_{i}", typeof(Sprite));
            for (int j = 0; j < objects.Length; j++)
            {
               cacheSps.Add(objects[j]);
            }
        }
        return cacheSps.ToArray();
    }

    public void PlayAction(string act,int dir,int times = -1)
    {
        frameAnimator.ChangeFrame(act, dir, times);
        frameAnimator.Play();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}