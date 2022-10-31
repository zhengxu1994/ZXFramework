using System;
using UnityEngine;

namespace Bepop.Core
{
    public class LogStyle
    {
        public string tag;
        public string color;
        public LogStyle(string tag, string color)
        {
            this.tag = tag;
            this.color = color;
        }
    }
    public class Logger
    {
        LogStyle style;

        public Logger(string tag, string color)
        {
            style = new LogStyle(tag, color);
        }
        
        public void LogException(Exception exception)
        {
            if (Log.DebugOut)
            {
                Debug.LogException(exception);
            }
        }
        public void I(object obj)
        {
            LogInfo(obj, null);
        }

        public void I(object obj, params object[] args)
        {
            LogInfo(obj, args);
        }

        public void W(object obj)
        {
            LogWarning(obj, null);
        }
        public void E(object obj)
        {
            LogError(obj, null);
        }

        public void LogInfo(object obj, params object[] args)
        {
            if (Log.DebugOut)
            {
                if(args == null)
                {
                    string logStr = obj.ToString();
                    string text = string.Format("[{0}] <color={1}>[{2}]   {3}</color>", new object[]
                    {
                       DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                       logStr
                    });

                    Debug.Log(text);
                }
                else
                {
                    string format = obj.ToString();
                    string text = string.Format("[{0}] <color={1}>[{2}]   {3}</color>", new object[]
                    {
                       DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                       format
                    });
                    Debug.LogFormat(text, args);
                }
            }
        }
        public void LogError(object obj, params object[] args)
        {
            if (Log.DebugOut)
            {
                if (args == null)
                {
                    string logStr = obj.ToString();
                    string text = string.Format("[{0}] <color={1}>[{2}]   {3}</color>", new object[]
                    {
                       DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                       logStr
                    });
                    Debug.LogError(text);
                }
                else
                {
                    string format = obj.ToString();
                    string text = string.Format("[{0}] <b><color={1}>[{2}]   {3}</color></b>", new object[]
                    {
                        DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                        format
                    });
                    Debug.LogErrorFormat(text, args);
                }
            }
        }
        public void LogWarning(object obj, params object[] args)
        {
            if (Log.DebugOut)
            {
                if (args == null)
                {
                    string logStr = obj.ToString();
                    string text = string.Format("[{0}] <color={1}>[{2}]   {3}</color>", new object[]
                    {
                       DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                       logStr
                    });
                    Debug.LogWarning(text);
                }
                else
                {
                    string format = obj.ToString();
                    string text = string.Format("[{0}] <b><color={1}>[{2}]   {3}</color></b>", new object[]
                    {
                        DateTime.Now.ToLongTimeString(),
                        style.color,
                        style.tag,
                        format
                    });
                    Debug.LogErrorFormat(text, args);
                }
            }
        }
    }
}
