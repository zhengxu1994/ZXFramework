using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bepop.Core
{
    /// <summary>
    /// 可以池化的对象
    /// 需要实现回收方法
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 重置,回收
        /// </summary>
        void OnRecycled();
        bool IsRecycled { get; set; }
    }
}
