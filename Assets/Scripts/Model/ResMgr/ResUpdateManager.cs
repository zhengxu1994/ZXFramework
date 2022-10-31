using System;
using Bepop.Core;
using System.Collections;
using UnityEngine;
using YooAsset;
using static YooAsset.YooAssets;

namespace Resource
{
    class DecryptServices : IDecryptionServices
    {
        public ulong GetFileOffset()
        {
            return 32;
        }
    }
    public class ResUpdateManager : Singleton<ResUpdateManager>
    {
        private ResUpdateManager()
        {

        }

        private Action updateCallback = null;

        public IEnumerator StartUpdate(MonoBehaviour mono,Action callback)
        {
            string downloadUrl = "http://127.0.0.1/yooasset/StandaloneOSX/1";
            int version = 1;
            this.updateCallback = callback;
            //这里会请求服务器获取游戏相关数据 并做对应的版本处理
#if UNITY_EDITOR
        var createParam = new EditorSimulateModeParameters();
        createParam.LocationServices = new DefaultLocationServices("Assets/Res");
#else
            var createParam = new YooAssets.HostPlayModeParameters();
            createParam.LocationServices = new DefaultLocationServices("Assets/Res");
            createParam.DecryptionServices = new DecryptServices();
            createParam.ClearCacheWhenDirty = false;

            createParam.DefaultHostServer = downloadUrl;
            createParam.FallbackHostServer = downloadUrl;
#endif
            yield return YooAssets.InitializeAsync(createParam);

            mono.StartCoroutine(UpdateStaticVersion(mono));
        }

        /// <summary>
        /// 获取资源版本
        /// </summary>
        /// <returns></returns>
        public IEnumerator UpdateStaticVersion(MonoBehaviour mono)
        {
            UpdateStaticVersionOperation operation = YooAssets.UpdateStaticVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                int resourceVersion = operation.ResourceVersion;
                Debug.Log($"Update resource Version : {resourceVersion}");
                mono.StartCoroutine(UpdatePatchManifest(mono,resourceVersion));
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 更新补丁清单
        /// </summary>
        /// <param name="resourceVersion"></param>
        /// <returns></returns>
        private IEnumerator UpdatePatchManifest(MonoBehaviour mono, int resourceVersion)
        {
            UpdateManifestOperation operation = YooAssets.UpdateManifestAsync(resourceVersion);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                Debug.Log("更新成功");
                mono.StartCoroutine(Download());
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 补丁包下载
        /// </summary>
        /// <returns></returns>
        IEnumerator Download()
        {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            DownloaderOperation downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);

            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                updateCallback?.Invoke();
                yield break;
            }

            //需要下载的文件总数和总大小
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            //注册回调方法
            downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
            downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
            downloader.OnDownloadOverCallback = OnDownloadOverFunction;
            downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

            //开启下载
            downloader.BeginDownload();
            yield return downloader;

            //检测下载结果
            if (downloader.Status == EOperationStatus.Succeed)
            {
                //下载成功
                //进入游戏
                updateCallback?.Invoke();
            }
            else
            {
                //下载失败
            }
        }

        private void OnDownloadErrorFunction(string fileName, string error)
        {
            //下载文件失败 error
        }


        private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            //ui显示处理
        }


        private void OnDownloadOverFunction(bool isSucceed)
        {
            //下载结束
        }


        private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
        {
            //显示下载失败文件信息
        }
    }
}
