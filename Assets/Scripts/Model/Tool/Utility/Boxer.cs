using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Bepop.Core
{

    public class ClassHeader
    {
        // 作为偏移标记使用，没其他意义
        public byte value = 0;
        /// <summary>
        /// 获取头部大小
        /// </summary>
        public static int HeaderSize
        {
            get{
                return UnsafeUtility.GetFieldOffset(typeof(ClassHeader).GetField(nameof(value)));
            }
        }
    }



    public static class Boxer<T> where T : struct
    {
        // 提前box创建一个模板
        // __makeref,__refvalue等也可以操作避免装箱
        private static readonly object _BOXER = Activator.CreateInstance<T>();

        public unsafe static object Box(ref T target)
        {
            byte* ptr = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(_BOXER, out ulong gcHandler);
            byte* dataPtr = ptr + ClassHeader.HeaderSize;
            // 内存复制
            UnsafeUtility.CopyStructureToPtr(ref target, dataPtr);
            UnsafeUtility.ReleaseGCObject(gcHandler);
            return _BOXER;
        }
    }
}
