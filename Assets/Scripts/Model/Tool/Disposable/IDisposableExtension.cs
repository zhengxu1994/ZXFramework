using System;
using UnityEngine;
namespace Bepop.Core
{
    public static class IDisposableExtensions
    {
        /// <summary>
        /// 给继承IDisposable接口的对象 拓展相关Add方法
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component"></param>
        [Obsolete]
        public static void AddTo(this IDisposable self, IDisposableList component)
        {
            component.Add(self);
        }

        /// <summary>
        /// 给继承IDisposable接口的对象 拓展相关Add方法
        /// </summary>
        /// <param name="self"></param>
        /// <param name="disposableList"></param>
        public static void AddToDisposeList(this IDisposable self,IDisposableList disposableList)
        {
            disposableList.Add(self);
        }

        /// <summary>
        /// 与 GameObject 绑定销毁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="gameObject"></param>
        public static void DisposeWhenGameObjectDestroyed(this IDisposable self, GameObject gameObject)
        {
            gameObject.GetOrAddComponent<OnDestroyDisposeTrigger>()
                .AddDispose(self);
        }

        /// <summary>
        /// 与 GameObject 绑定销毁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component"></param>
        public static void DisposeWhenGameObjectDestroyed(this IDisposable self, Component component)
        {
            component.gameObject.GetOrAddComponent<OnDestroyDisposeTrigger>()
                .AddDispose(self);
        }
        
        
        /// <summary>
        /// 与 GameObject 绑定销毁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="gameObject"></param>
        public static void DisposeWhenGameObjectDisabled(this IDisposable self, GameObject gameObject)
        {
            gameObject.GetOrAddComponent<OnDisableDisposeTrigger>()
                .AddDispose(self);
        }

        /// <summary>
        /// 与 GameObject 绑定销毁
        /// </summary>
        /// <param name="self"></param>
        /// <param name="component"></param>
        public static void DisposeWhenGameObjectDisabled(this IDisposable self, Component component)
        {
            component.gameObject.GetOrAddComponent<OnDisableDisposeTrigger>()
                .AddDispose(self);
        }
    }
}