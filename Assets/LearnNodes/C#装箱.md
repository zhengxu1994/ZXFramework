###  C#装箱

**什么是装箱:**

*将值类型**转换**为引用类型，就叫装箱。转换可以发生在赋值，参数传递，变量初始化等各种情况下。*

**clr在装箱时做来什么？：**

- 在托管对中分配内存，大小是值类型的各字段的总大小，以及额外的对象头header大小（类型对象指针和同步索引块）
- 内存复制，将值类型的所有字段复制到第一步申请的非header内存中
- 返回托管对中的内存地址，作为装箱对象使用。

**装箱的危害**

- 浪费cpu资源，内存资源，有许多box等il指令，占用托管内存
- 影响垃圾回收gc的效率 (装箱拆箱完后这些内存如果不在被引用，那么在gc时就会遍历这些对象，对象数量多了自然影响gc效率)

**怎么避免呢？，完全避免装箱拆箱是不可能的，可以重复利用这些创建的装箱对象**

用的知识点：

- classheader大小
- 类field的offset大小
- 怎么内存复制

```c#
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
```

