
using System;
using System.Reflection;

namespace Bepop.Core
{
    /// <summary>
    /// 没有公共构造函数的对象工厂：相关对象只能通过反射获得
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NonPublicObjectFactory<T> : IObjectFactory<T> where T : class
    {
        public T Create()
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);

            if (ctor == null)
            {
                throw new Exception("Non-Public Constructor() not found! in " + typeof(T));
            }

            return ctor.Invoke(null) as T;
        }
    }

}