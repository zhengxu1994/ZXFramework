using System;
using System.Collections.Generic;
using Bepop.Core;
namespace Notifaction
{
    /// <summary>
    /// 消息参数
    /// </summary>
    public class NotifyParam : IDisposable, IPoolable
    {
        public static UINotifyParamPool UINotifyParamPool { get; private set; } = new UINotifyParamPool();

        private Dictionary<string, int> _strInts;

        private Dictionary<string, string> _strStrs;

        private Dictionary<string, object> _strObjs;

        private Dictionary<string, bool> _strBools;

        private bool _isRecycled = false;

        public bool IsRecycled { get => _isRecycled; set => _isRecycled = value; }

        public static NotifyParam Create()
        {
            var param = UINotifyParamPool.Allocate();
            param.IsRecycled = false;
            return param;
        }

        public NotifyParam Int(string str, int value)
        {
            if (_strInts == null) _strInts = new Dictionary<string, int>();
            _strInts[str] = value;
            return this;
        }

        public int Int(string str)
        {
            if (_strInts == null || !_strInts.ContainsKey(str))
            {
                Log.UI.LogError($"param is null,key:{str}");
                return -1;
            }
            return _strInts[str];
        }

        public NotifyParam Str(string str, string value)
        {
            if (_strStrs == null) _strStrs = new Dictionary<string, string>();
            _strStrs[str] = value;
            return this;
        }

        public string Str(string str)
        {
            if (_strStrs == null || !_strStrs.ContainsKey(str))
            {
                Log.UI.LogError($"param is null,key:{str}");
                return null;
            }
            return _strStrs[str];
        }

        public NotifyParam Obj(string str, object value)
        {
            if (_strObjs == null) _strObjs = new Dictionary<string, object>();
            _strObjs[str] = value;
            return this;
        }

        public object Obj(string str)
        {
            if (_strObjs == null || !_strObjs.ContainsKey(str))
            {
                Log.UI.LogError($"param is null,key:{str}");
                return null;
            }
            return _strObjs[str];
        }



        public NotifyParam Bool(string str, bool value)
        {
            if (_strBools == null) _strBools = new Dictionary<string, bool>();
            _strBools[str] = value;
            return this;
        }

        public bool Bool(string str)
        {
            if (_strBools == null || !_strBools.ContainsKey(str))
            {
                //Log.UI.LogError($"param is null,key:{str}");
                return false;
            }
            return _strBools[str];
        }



        public void Dispose()
        {
            Clear();

            _strInts = null;
            _strStrs = null;
            _strObjs = null;
        }

        public void Clear()
        {
            _strInts?.Clear();
            _strStrs?.Clear();
            _strObjs?.Clear();
            _strBools?.Clear();
        }

        public void OnRecycled()
        {
            _strInts?.Clear();
            _strStrs?.Clear();
            _strObjs?.Clear();
            _isRecycled = true;
        }
    }

    public class UINotifyParamPool : Pool<NotifyParam>
    {
        public UINotifyParamPool()
        {
            mFactory = new UINotifyParamFactory();
        }

        public override bool Recycle(NotifyParam obj)
        {
            if (obj != null)
            {
                obj.OnRecycled();
                obj.IsRecycled = true;
                return true;
            }
            return false;
        }
    }


    public class UINotifyParamFactory : IObjectFactory<NotifyParam>
    {
        public NotifyParam Create()
        {
            NotifyParam param = new NotifyParam();
            return param;
        }
    }
}