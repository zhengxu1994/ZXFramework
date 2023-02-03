using System;
using System.Reflection;
using FairyGUI;

namespace Bepop.Core.UI
{
    /// <summary>
    /// UI界面里的子组件需要拓展的继承这个类
    /// 这个界面只需要一个创建事件和初始化事件
    /// 主要目的就是将这个组件封装起来，处理自己的逻辑不让一个界面的所有逻辑都写在一个UI类中
    /// </summary>
    public class ViewBase : UIBase
    {

        public static ViewBase Create(Type type, GComponent uiObj)
        {
            if (!type.IsSubclassOf(typeof(ViewBase)))
            {
                Log.UI.LogError("Super Type Error :{0} must be extend ViewBase", type.ToString());
                return null;
            }
            ViewBase view = null;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            ConstructorInfo ctor = type.GetConstructor(flags, Type.DefaultBinder, createOTypes, null);
            if (ctor != null)
            {
                view = ctor.Invoke(null) as ViewBase;
            }
            else
                Log.UI.LogError("Create {0} Not Find Constructor", type.ToString());
            if (view != null)
            {
                InitUIObject(view, uiObj);
                view.OnCreate();
            }
            else
                uiObj.Dispose();//找不到对应的UI，fgui对象也销毁掉
            return view;
        }

        public override void OnCreate()
        {
            
        }

        public virtual void OnInit()
        {

        }
    }
}
