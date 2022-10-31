using FairyGUI;
using Notifaction;
using System;
using System.Reflection;
namespace Bepop.Core.UI
{
    /// <summary>
    /// 所有UI的基类
    /// </summary>
    public abstract class UIBase : IDisposable
    {
        /// <summary>
        /// UI界面绑定的fgui对象
        /// </summary>
        public GComponent UIObj { get; protected set; } = null;

        public bool Visible {
            get { return UIObj.visible; }
            protected set {
                UIObj.visible = value;
            }
        }

        private NotifyParam _param;

        public NotifyParam Param {
            get {
                return _param;
            }
            protected set {
                _param = value;
            }
        }

        public string Name { get; private set; }

        private bool _disposed = false;

        protected static Type[] createOTypes = new Type[] { };

        //反射创建UI界面方法
        public static T Create<T>(GComponent uiObj,string uiName)where T : UIBase
        {
            return Create(typeof(T), uiObj,uiName) as T;
        }

        public static UIBase Create(Type type, GComponent uiObj,string uiName)
        {
            if (!type.IsSubclassOf(typeof(UIBase)))
            {
                Log.UI.LogError("Super Type Error :{0} must be extend ExtensionBase", type.ToString());
                return null;
            }
            UIBase view = null;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            ConstructorInfo ctor = type.GetConstructor(flags, Type.DefaultBinder, createOTypes, null);
            if (ctor != null)
            {
                view = ctor.Invoke(null) as UIBase;
            }
            else
                Log.UI.LogError("Create {0} Not Find Constructor", type.ToString());
            if (view != null)
            {
                InitUIObject(view, uiObj);
                view.InitObjPosition();
                view.OnCreate();
                view.Name = uiName;
            }
            else
                uiObj.Dispose();//找不到对应的UI，fgui对象也销毁掉
            return view;
        }

        public static void InitUIObject(UIBase view, GComponent uiObj)
        {
            view.UIObj = uiObj;
            //view.UIObj.onDispose.Add(() => view.RealDispose(true));
            view.AutoBinderUI();
            uiObj.data = view;
        }

        //绑定fgui对象方法
        public virtual void AutoBinderUI() { }
        //初始化UI界面控件坐标 
        public virtual void InitObjPosition() { }
        //UI界面创建事件
        public abstract void OnCreate();

        //UI界面销毁事件
        public virtual void Dispose() {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        private void OnDispose ()
        {
            if (_disposed) return;
            
            GTween.Kill(this);
            GTween.Kill(UIObj);

            if (UIObj != null)
                UIObj.Dispose();
            UIObj = null;
            _disposed = true;
        }

        ~UIBase()
        {
            OnDispose();
        }
    }
}
