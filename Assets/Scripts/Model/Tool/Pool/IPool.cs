using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bepop.Core
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPool<T>
    {
        /// <summary>
        /// 分配对象
        /// </summary>
        /// <returns></returns>
        T Allocate();

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Recycle(T obj);
    }
}
