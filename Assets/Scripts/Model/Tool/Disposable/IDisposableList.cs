using System;

namespace Bepop.Core
{
    /// <summary>
    /// 回收 非托管资源 List 接口
    /// </summary>
    public interface IDisposableList : IDisposable
    {
        void Add(IDisposable disposable);
    }

}