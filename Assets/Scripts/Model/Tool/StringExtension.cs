using System;
using System.Text;

public static class StringExtension
{
    public static StringBuilder sb = new StringBuilder();
    /// <summary>
    /// 从字符串数组中截取目标长度的字符串拼接成一个新的字符串
    /// </summary>
    /// <param name="strArray"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string CutArrayJointToString(this string[] strArray,int length,char symbol)
    {
        if (strArray.Length <= 0) return "";
        if (length > strArray.Length) {
            //需要加log
            return "";
        }
        sb.Clear();
        for (int i = 0; i < strArray.Length; i++)
        {
            sb.Append(strArray[i]);
            if (i != strArray.Length - 1)
                sb.Append(symbol);
        }
        return sb.ToString();
    }

    public static string[] CutArrayJointToStringArray(this string[] strArray, int length)
    {
        if (strArray.Length <= 0) return null;
        if (length > strArray.Length)
        {
            //需要加log
            return null;
        }
        string[] newArray = new string[length];
        for (int i = 0; i < length; i++)
        {
            newArray[i] = strArray[i];
        }
        return newArray;
    }
}
