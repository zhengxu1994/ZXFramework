using System;

namespace Bepop.Core
{
    /// <summary>
    /// 自定义可回收类
    /// </summary>
    public class CustomDisposable : IDisposable
    {
        /// <summary>
        /// 委托对象
        /// </summary>
        private Action mOnDispose = null;

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="onDispose"></param>
        public CustomDisposable(Action onDispose)
        {
            mOnDispose = onDispose;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            mOnDispose.Invoke();
            mOnDispose = null;
        }
    }
}