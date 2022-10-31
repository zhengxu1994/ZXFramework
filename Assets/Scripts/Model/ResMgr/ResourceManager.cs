using System;
using System.Collections;
using System.Collections.Generic;
using Bepop.Core;
using FairyGUI;
using UnityEditor;
using YooAsset;

namespace Resource
{
    using UnityObj = UnityEngine.Object;
    public enum AssetType
    {
        Default,
        SubAssets
    }
    public partial class ResourceManager : Singleton<ResourceManager>
    {

        private ResourceManager() { }

        private Dictionary<string, AssetOperationHandle> assetsHandles = new Dictionary<string, AssetOperationHandle>();

        private Dictionary<string, SubAssetsOperationHandle> subAssetHandles = new Dictionary<string, SubAssetsOperationHandle>();
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public UnityObj LoadAsset(string path,Type type)
        {
            if (assetsHandles.ContainsKey(path))
                return assetsHandles[path].AssetObject;
            var handle = YooAssets.LoadAssetSync(path, type);

            assetsHandles.Add(path, handle);
            if(handle.Status == EOperationStatus.Failed)
            {
                Log.EX.LogError($"load asset failed:{path},error:{handle.LastError} ");
            }
            return handle.AssetObject;
        }

        public UnityObj[] LoadSubAsset(string path,Type type)
        {
            if (subAssetHandles.ContainsKey(path))
                return subAssetHandles[path].AllAssetObjects;
            var subHandler = YooAssets.LoadSubAssetsSync(path, type);
            subAssetHandles.Add(path, subHandler);
            if(subHandler.Status == EOperationStatus.Failed)
            {
                Log.EX.LogError($"load subasset failed:{path},error:{subHandler.LastError} ");
            }
            return subHandler.AllAssetObjects;
        }

        public void LoadMultiAssetsAsync(Dictionary<string,Type> assets,Action<Dictionary<string,UnityObj>> cb)
        {
            if (assets == null || assets.Count <= 0) {
                cb?.Invoke(null);
            }
            var dic = new Dictionary<string, UnityObj>();
            int count = assets.Count;
            int loadedCount = 0;
            assets.ForEach((k, v) => {
                LoadAssetAsync(k, v, (asset) => {
                    dic[k] = asset;
                    loadedCount++;
                    if(loadedCount == count)
                    {
                        cb?.Invoke(dic);
                    }
                });
            });
 
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public void LoadAssetAsync(string path,Type type,Action<UnityObj> callback)
        {
            if(assetsHandles.TryGetValue(path,out var handle))
            {
                if(handle.IsDone)
                {
                    callback?.Invoke(handle.AssetObject);
                    return;
                }
            }

            if(handle == null)
            {
                handle = YooAssets.LoadAssetAsync(path, type);
                assetsHandles[path] = handle;
            }
            handle.Completed += (AssetOperationHandle temp) =>
            {
                if (handle.Status == EOperationStatus.Failed)
                {
                    Log.EX.LogError($"load asset failed:{path},error:{handle.LastError} ");
                }
                callback?.Invoke(temp.AssetObject);
            };
        }

        public void LoadSubAssetAsync(string path, Type type, Action<UnityObj[]> callback)
        {
            if (subAssetHandles.TryGetValue(path, out var handle))
            {
                if (handle.IsDone)
                {
                    callback?.Invoke(handle.AllAssetObjects);
                    return;
                }
            }

            if (handle == null)
            {
                handle = YooAssets.LoadSubAssetsAsync(path, type);
                subAssetHandles[path] = handle;
            }
            handle.Completed += (SubAssetsOperationHandle temp) =>
            {
                if (handle.Status == EOperationStatus.Failed)
                {
                    Log.EX.LogError($"load asset failed:{path},error:{handle.LastError} ");
                }
                callback?.Invoke(temp.AllAssetObjects);
            };
        }

        public override void Dispose()
        {
            assetsHandles.ForEach((k, v) => {
                v.Release();
            });
            assetsHandles.Clear();
            base.Dispose();
        }

        private const string _uiUrl = "Assets/Fgui/";
        //load uipackage
        public UIPackage LoadFairyGuiPackage(string pkgName)
        {
            string descFilePath = _uiUrl + pkgName;
            return UIPackage.AddPackage(descFilePath, (string name, string extension,System.Type type
                ,out DestroyMethod destroyMethod) => {

                    string resFile = name.Replace(_uiUrl, "") + extension;
                    destroyMethod = DestroyMethod.Unload;
                    //TODO:
                    return null;
                    }
            );
        }

        public void ReleaseRes(string path,AssetType assetType = AssetType.Default)
        {
            if (assetType == AssetType.Default)
                ReleaseAsset(path);
            else if (assetType == AssetType.SubAssets)
                ReleaseSubAsset(path);
        }

        private void ReleaseAsset(string path)
        {
            if(assetsHandles.TryGetValue(path,out var handle))
            {
                handle.Release();
                assetsHandles.Remove(path);
            }
        }

        private void ReleaseSubAsset(string path)
        {
            if (subAssetHandles.TryGetValue(path, out var handle))
            {
                handle.Release();
                subAssetHandles.Remove(path);
            }
        }

        public void RemoveFairyGuiPackage(string pkgName)
        {
            UIPackage package = UIPackage.GetByName(pkgName);
            if (package != null)
                UIPackage.RemovePackage(pkgName);
        }

        public void UnLoadUnusedAssets()
        {
            YooAssets.UnloadUnusedAssets();
        }
    }
}