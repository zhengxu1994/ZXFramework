### Unity场景管理

#### 为什么需要场景管理？

**我可以控制场景里面的对象，场景的生命周期**

*unity场景是有哪些构成的？*

- Scene 场景对象
- Objects 场景内的对象以及加载到场景内的对象
  - 脚本 scripts
  - 预设体 prefab
  - 贴图 Texture
  - 音频 audioclip 
  - 文本

*生命周期*

- Awake 场景被创建时
- Enter 进入场景
- Update 场景更新
- Exit 离开场景
- Dispose 销毁场景

一般场景对象的加载都是异步加载，在加载时肯定需要使用Loading界面去过渡。

*场景中需要预加载的对象*

- 大量重复使用的对象
- 一进场景就需要使用到的对象
- 加载时间比较长的对象

```c#
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
```

```c#
   /// <summary>
    /// 场景管理类
    /// </summary>
    public class SceneMgr : Singleton<SceneMgr>
    {
        private SceneMgr() { }

        private SceneBase currentScene;

        private SceneBase nextScene;

        private string lastSceneName;

        private Dictionary<string, Type> GetPreLoadAssetListbyScene(string sceneName)
        {
            return null;
        }

        private bool IsLoad(string sceneName)
        {
            return currentScene != null && currentScene.sceneName == sceneName || SceneManager.GetActiveScene().name == sceneName;
        }

        public void LoadScene(string sceneName, NotifyParam param = null, Action cb = null)
        {
            if (IsLoad(sceneName))
            {
                cb?.Invoke();
                return;
            }
            //load preload assets
            var preload = GetPreLoadAssetListbyScene(sceneName);
            if (preload != null && preload.Count > 0)
            {
                ResourceManager.Instance.LoadMultiAssetsAsync(preload, (assets) => {
                    //create scene
                    param.Obj(Unist.PreLoadAssets, assets);
                    LoadScene(sceneName, param);
                });
            }
            else
            {
                LoadScene(sceneName, param);
            }
        }

        private void LoadScene(string sceneName, NotifyParam param)
        {
            if (IsLoad(sceneName)) return;
            nextScene = CreateScene(sceneName);
            nextScene.InitParam(param);
            if (nextScene == null)
            {
                Log.EX.LogError($"load scene failed :SceneName{sceneName}");
                return;
            }
            StartLoadScene(sceneName, true);
        }

        private const string scenePath = "Res/Scenes/{0}.scene";
        private void StartLoadScene(string sceneName,bool enter)
        {
            var filePath = string.Format(scenePath, sceneName);
            SceneOperationHandle handle = YooAssets.LoadSceneAsync(scenePath, LoadSceneMode.Single, enter);
            handle.Completed += (operationHandle) => {
                ChangeToNextScene();
            };
        }

        private void ChangeToNextScene()
        {
            if(currentScene != null)
            {
                currentScene.OnExit();
                lastSceneName = currentScene.sceneName;
            }
            currentScene = nextScene;
            nextScene = null;
            currentScene.OnEnter();
            
        }

        public SceneBase CreateScene(string sceneName)
        {
            SceneBase scene = null;
            switch (sceneName)
            {

                default:
                    scene = new SceneBase(sceneName);
                    break;
            }
            return scene;
        }

        public override void Dispose()
        {

            base.Dispose();
        }
    }
```

