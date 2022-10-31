using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bepop.Core
{
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Pool<T> : IPool<T>
    {
        #region ICountObserverable
        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>The current count.</value>
        public int CurCount
        {
            get { return mCacheStack.Count; }
        }
        #endregion

        protected IObjectFactory<T> mFactory;

        /// <summary>
        /// 存储相关数据的栈
        /// </summary>
        protected readonly Stack<T> mCacheStack = new Stack<T>();

        /// <summary>
        /// default is 5
        /// </summary>
        protected int mMaxCount = 12;

        public virtual T Allocate()
        {
            return mCacheStack.Count == 0
                ? mFactory.Create()
                : mCacheStack.Pop();
        }

        public abstract bool Recycle(T obj);
    }
}
