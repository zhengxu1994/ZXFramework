

using System;

namespace Bepop.Core
{
    /// <summary>
    /// MonoSingleton路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)] //这个特性只能标记在Class上
    public class MonoSingletonPath : Attribute
    {
        private string mPathInHierarchy;

        public MonoSingletonPath(string pathInHierarchy)
        {
            mPathInHierarchy = pathInHierarchy;
        }

        public string PathInHierarchy
        {
            get { return mPathInHierarchy; }
        }
    }
}