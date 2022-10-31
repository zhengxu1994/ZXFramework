using System.Collections;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;
using System.Diagnostics;
using Notifaction;
using System;
using UnityEngine.SceneManagement;
using Resource;
using YooAsset;
using Skill;

namespace SceneMgr
{
    /// <summary>
    /// 场景管理类
    /// </summary>
    public class SceneMgr : Singleton<SceneMgr>
    {
        private SceneMgr() { }

        private SceneBase currentScene;

        private SceneBase nextScene;

        private string lastSceneName;

        private SceneFactory factory = new SceneFactory();

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
                    param.Obj(UniStr.PreLoadAssets, assets);
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
            return factory.GetScene(sceneName);
        }

        public override void Dispose()
        {

            base.Dispose();
        }
    }
}
