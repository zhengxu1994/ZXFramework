using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bepop.Core
{
    /// <summary>
    /// ���Գػ��Ķ���
    /// ��Ҫʵ�ֻ��շ���
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// ����,����
        /// </summary>
        void OnRecycled();
        bool IsRecycled { get; set; }
    }
}
