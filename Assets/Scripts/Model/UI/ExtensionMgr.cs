using System;
using System.Collections.Generic;
using System.Reflection;
using FairyGUI;
using FairyGUI.Utils;
using Resource;

namespace Bepop.Core.UI
{
    public class ExtInfo
    {
        public Type ExtClass;

        public string ExtUrl;

        public ObjectType ExtType;
    }


    /// <summary>
	/// ui拓展
	/// </summary>
    public class ExtensionMgr
    { 
        public static void SetExtension(System.Type type, string url, ObjectType objType)
        {
            //fgui的api 可以拓展组件类，只要类继承对应的组件类就可以，比如ExtGButton : GButton
            FguiExtension.SetExtension(url, objType, (GComponent com) => {
                //根据xml创建后反射绑定对应UI类 （第三步）
                ViewBase ret = ViewBase.Create(type,com);
            });
        }
    }

    public static partial class UIManagerExt
    {
        public static void PreLoadCachePackage(this UIManager manager,Action<int,int> loadCallBack)
        {
            int totalNum = manager._pkgCache.Count;
            if (totalNum > 0 && !manager.HasLoadCache)
            {
                int index = 0;
                foreach (var pkg in manager._pkgCache)
                {
                    ResourceManager.Instance.LoadFairyGuiPackage(pkg);
                    if(pkg != null)
                    {
                        loadCallBack(++index, totalNum);
                    }
                }
                manager.HasLoadCache = true;
            }
        }
     
    }

    public static class GComponentExt
    {
        public static T GetChild<T>(this GComponent obj,string name)where T : GObject
        {
            GObject ret = obj.GetChild(name);
            if (ret != null)
                return ret as T;
            throw new Exception(string.Format($"get child{name}error by{obj.name}"));
        }

        public static T GetChildAt<T>(this GComponent obj, int index) where T : GObject
        {
            GObject ret = obj.GetChildAt(index);
            if (ret != null)
                return ret as T;
            throw new Exception(string.Format($"get child{index}error by{obj.name}"));
        }
    }

    internal class ExtGComponent : GComponent
    {
        protected Action<GComponent> func;

        public ExtGComponent(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGLaber : GLabel
    {
        protected Action<GComponent> func;

        public ExtGLaber(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGButton : GButton
    {
        protected Action<GComponent> func;

        public ExtGButton(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGComboBox : GComboBox
    {
        protected Action<GComponent> func;

        public ExtGComboBox(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGList : GList
    {
        protected Action<GComponent> func;

        public ExtGList(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGProgressBar : GProgressBar
    {
        protected Action<GComponent> func;

        public ExtGProgressBar(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGTree : GTree
    {
        protected Action<GComponent> func;

        public ExtGTree(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGScrollBar : GScrollBar
    {
        protected Action<GComponent> func;

        public ExtGScrollBar(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGSlider : GSlider
    {
        protected Action<GComponent> func;

        public ExtGSlider(Action<GComponent> callback)
        {
            func = callback;
        }

        public override void ConstructFromXML(XML xml)
        {
            func?.Invoke(this.asCom);
        }
    }

    internal class ExtGLoader : GLoader
    {
        protected Action<GComponent> func;

        public ExtGLoader(Action<GComponent> callback)
        {
            func = callback;
        }
    }

    public static class FguiExtension
    {
        public static void SetExtension(string url, ObjectType type, Action<GComponent> callback)
        {
            if (type == ObjectType.Loader)
            {
                UIObjectFactory.SetLoaderExtension(() => {
                    return new ExtGLoader(callback);
                });
            }
            else
            {
                //（第二步）
                UIObjectFactory.SetPackageItemExtension(url, () => {
                    GComponent com = null;
                    switch (type)
                    {
                        //...各个类型的component拓展 传递回调（ExtensionBase ret = ExtensionBase.Cteate(type,com);）
                        case ObjectType.Component:
                            com = new ExtGComponent(callback);
                            break;
                        case ObjectType.Button:
                            com = new ExtGButton(callback);
                            break;
                        case ObjectType.List:
                            com = new ExtGList(callback);
                            break;
                        case ObjectType.ComboBox:
                            com = new ExtGComboBox(callback);
                            break;
                        case ObjectType.Label:
                            com = new ExtGLaber(callback);
                            break;
                        case ObjectType.ProgressBar:
                            com = new ExtGProgressBar(callback);
                            break;
                        case ObjectType.ScrollBar:
                            com = new ExtGScrollBar(callback);
                            break;
                        case ObjectType.Slider:
                            com = new ExtGSlider(callback);
                            break;
                        case ObjectType.Tree:
                            com = new ExtGTree(callback);
                            break;
                    }
                    return com;
                });
            }
        }
    }

}
