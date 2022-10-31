
using System;
using System.Collections.Generic;

namespace Bepop.Core
{
    /// <summary>
    /// 可回收List
    /// </summary>
    public class DisposableList : IDisposableList
    {
        /// <summary>
        /// 需要回收的List 对象
        /// </summary>
        List<IDisposable> mDisposableList = ListPool<IDisposable>.Get();

        /// <summary>
        /// 给List添加相关对象
        /// </summary>
        /// <param name="disposable"></param>
        public void Add(IDisposable disposable)
        {
            mDisposableList.Add(disposable);
        }

        /// <summary>
        /// 释放相关对象
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in mDisposableList)
            {
                disposable.Dispose();
            }

            mDisposableList.Release2Pool();
            mDisposableList = null;
        }
    }
}