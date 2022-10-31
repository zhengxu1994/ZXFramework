using System;

namespace Bepop.Core
{
    public static class CSharpObjectExtension
    {
        /// <summary>
        /// 是否相等
        /// 
        /// 示例：
        /// <code>
        /// if (this.Is(player))
        /// {
        ///     ...
        /// }
        /// </code>
        /// </summary>
        /// <param name="selfObj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Is(this object selfObj, object value)
        {
            return selfObj == value;
        }

        public static bool Is<T>(this T selfObj, Func<T, bool> condition)
        {
            return condition(selfObj);
        }

        /// <summary>
        /// 表达式成立 则执行 Action
        /// 
        /// 示例:
        /// <code>
        /// (1 == 1).Do(()=>Debug.Log("1 == 1");
        /// </code>
        /// </summary>
        /// <param name="selfCondition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool Do(this bool selfCondition, Action action)
        {
            if (selfCondition)
            {
                action();
            }

            return selfCondition;
        }

        /// <summary>
        /// 不管表达成不成立 都执行 Action，并把结果返回
        /// 
        /// 示例:
        /// <code>
        /// (1 == 1).Do((result)=>Debug.Log("1 == 1:" + result);
        /// </code>
        /// </summary>
        /// <param name="selfCondition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool Do(this bool selfCondition, Action<bool> action)
        {
            action(selfCondition);

            return selfCondition;
        }
        
        /// <summary>
        /// 功能：判断是否为空
        /// 
        /// 示例：
        /// <code>
        /// var simpleObject = new object();
        ///
        /// if (simpleObject.IsNull()) // 等价于 simpleObject == null
        /// {
        ///     // do sth
        /// }
        /// </code>
        /// </summary>
        /// <param name="selfObj">判断对象(this)</param>
        /// <typeparam name="T">对象的类型（可不填）</typeparam>
        /// <returns>是否为空</returns>
        public static bool IsNull<T>(this T selfObj) where T : class
        {
            return null == selfObj;
        }

        /// <summary>
        /// 功能：判断不是为空
        /// 示例：
        /// <code>
        /// var simpleObject = new object();
        ///
        /// if (simpleObject.IsNotNull()) // 等价于 simpleObject != null
        /// {
        ///    // do sth
        /// }
        /// </code>
        /// </summary>
        /// <param name="selfObj">判断对象（this)</param>
        /// <typeparam name="T">对象的类型（可不填）</typeparam>
        /// <returns>是否不为空</returns>
        public static bool IsNotNull<T>(this T selfObj) where T : class
        {
            return null != selfObj;
        }

        public static void DoIfNotNull<T>(this T selfObj, Action<T> action) where T : class
        {
            if (selfObj != null)
            {
                action(selfObj);
            }
        }
    }
}