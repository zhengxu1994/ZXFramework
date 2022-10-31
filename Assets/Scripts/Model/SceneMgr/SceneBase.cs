using System;
using System.Collections.Generic;
using Bepop.Core;
using Notifaction;
using Resource;
using YooAsset;

namespace SceneMgr
{
    using UnityObj = UnityEngine.Object;
    
    /// <summary>
    /// 场景基类
    /// </summary>
    public class SceneBase
    {
        public string sceneName { get; protected set; }

        private HashSet<string> loadedAssets = new HashSet<string>();

        private HashSet<string> loadedSubAssets = new HashSet<string>();

        protected Dictionary<string, UnityObj> preloadObjs = new Dictionary<string, UnityObj>();

        private NotifyParam param;
        public SceneBase(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void InitParam(NotifyParam param)
        {
            this.param = param;
        }

        public virtual void OnEnter()
        {
            Log.BASE.LogInfo($"Enter Scene,SceneName:{sceneName}");
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnExit()
        {
            Log.BASE.LogInfo($"Exit Scene,SceneName:{sceneName}");
            loadedAssets.ForEach((asset) => {
                ResourceManager.Instance.ReleaseRes(asset, AssetType.Default);
            });
            loadedAssets.Clear();
            loadedSubAssets.ForEach((asset) => {
                ResourceManager.Instance.ReleaseRes(asset, AssetType.SubAssets);
            });
            loadedSubAssets.Clear();
            preloadObjs.ForEach((k, v) => {
                ResourceManager.Instance.ReleaseRes(k);
            });
            preloadObjs.Clear();
            ResourceManager.Instance.UnLoadUnusedAssets();
            param.Dispose();
            param = null;
        }

        public virtual void OnDispose()
        {
            Log.BASE.LogInfo($"Dispose Scene,SceneName:{sceneName}");
        }
    }
}
