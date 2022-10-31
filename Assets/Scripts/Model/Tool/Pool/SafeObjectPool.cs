using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bepop.Core
{
    /// <summary>
    /// µ¥Àý³Ø
    /// </summary>
    public class SafeObjectPool<T>:Pool<T> where T : IPoolable, new()
    {
        private static SafeObjectPool<T> _instance;
        // ¼ÓËø
        private static object _lock = new object();
        protected SafeObjectPool()
        {
            mFactory = new DefaultObjectFactory<T>();
        }

        public static SafeObjectPool<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SafeObjectPool<T>();
                            //mFactory = new DefaultObjectFactory<T>();
                        }
                    }
                }
                return _instance;
            }
        }


        /// <summary>
        /// Init the specified maxCount and initCount.
        /// </summary>
        /// <param name="maxCount">Max Cache count.</param>
        /// <param name="initCount">Init Cache count.</param>
        public void Init(int maxCount, int initCount)
        {
            MaxCacheCount = maxCount;

            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
            }

            if (CurCount < initCount)
            {
                for (var i = CurCount; i < initCount; ++i)
                {
                    Recycle(new T());
                }
            }
        }

        /// <summary>
        /// Gets or sets the max cache count.
        /// </summary>
        /// <value>The max cache count.</value>
        public int MaxCacheCount
        {
            get { return mMaxCount; }
            set
            {
                mMaxCount = value;

                if (mCacheStack != null)
                {
                    if (mMaxCount > 0)
                    {
                        if (mMaxCount < mCacheStack.Count)
                        {
                            int removeCount = mCacheStack.Count - mMaxCount;
                            while (removeCount > 0)
                            {
                                mCacheStack.Pop();
                                --removeCount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allocate T instance.
        /// </summary>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// Recycle the T instance
        /// </summary>
        /// <param name="t">T.</param>
        public override bool Recycle(T t)
        {
            if (t == null || t.IsRecycled)
            {
                return false;
            }

            //if (mMaxCount > 0)
            //{
            //    if (mCacheStack.Count >= mMaxCount)
            //    {
            //        t.OnRecycled();
            //        return false;
            //    }
            //}

            t.IsRecycled = true;
            t.OnRecycled();
            mCacheStack.Push(t);

            return true;
        }

        public void printStackSize()
        {
            Log.BASE.LogInfo("mCacheStack size = " + mCacheStack.Count);
        }
    }
}
