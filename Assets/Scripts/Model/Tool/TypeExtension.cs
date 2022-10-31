using System;
public static class TypeExtension
{
    /// <summary>
    /// 根据命名空间+类名获取类型
    /// </summary>
    /// <param name="className"></param>
    /// <param name="nameSpace"></param>
    /// <returns></returns>
    public static Type GetClassType(this string className,string nameSpace)
    {
        string fullClassName;
        if (!string.IsNullOrEmpty(nameSpace))
            fullClassName = $"{nameSpace}.{className}";
        else
            fullClassName = className;
        var cType = Type.GetType(fullClassName);
        return cType;
    }
    /// <summary>
    /// 根据类型获取类实例
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetClassInstance(this Type type)
    {
        return Activator.CreateInstance(type);
    }
}
