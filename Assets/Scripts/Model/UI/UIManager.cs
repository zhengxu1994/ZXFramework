using System;
using System.Collections.Generic;
using FairyGUI;
using Notifaction;
using Resource;
using UnityEngine;

namespace Bepop.Core.UI
{
    public class UIPackageReference
    {
        public int ReferenceCount { get; private set; } = 0;

        public UIPackage Package;

        public string PackageName;

        public UIPackageReference(string name,UIPackage package)
        {
            PackageName = name;
            Package = package;
            ReferenceCount = 0;
        }

        public void AddReferenceCount()
        {
            ReferenceCount++;
        }

        public void ReduceAddReferenceCount()
        {
            ReferenceCount--;
        }
    }
    /// <summary>
    /// UI管理类
    /// 主要处理一些简单的UI开关和资源释放
    /// 偏业务逻辑的代码写在UI拓展类ExtensionMgr里面
    /// </summary>
    public partial class UIManager : Singleton<UIManager>
    {
        //常驻缓存界面
        public readonly HashSet<string> _pkgCache = new HashSet<string>() {
        "LoadImg","Common"};

        public bool HasLoadCache = false;

        private Dictionary<string, UIPackageReference> _loadedPkg = new Dictionary<string, UIPackageReference>();

        private GComponent[] _uiLayers = new GComponent[(int)UILayerEnum.Count];

        private Dictionary<string, PanelBase> _createdPanels = new Dictionary<string, PanelBase>();

        private LinkedList<PanelBase> _panels = new LinkedList<PanelBase>();

        private string _nowShowPanelName = "";

        public int SafeSpace = 50;

        private const int DesignWidth = 1136;
        private const int DesignHeight = 640;

        public CommonTopBase CommonTop = null;

        public bool HasPanel<T>()
        {
            var name = typeof(T).Name;
            return _createdPanels.ContainsKey(name);
        }

        private UIManager() { }

        //初始化多个UI层级root对象，对应的UI挂载在对象下。
        public override void OnSingletonInit()
        {
            SelfAdaption();
            InitUILayers();
            RegistScreenSizeChange();
            InitUIConfig();
            base.OnSingletonInit();
        }

        private void SelfAdaption()
        {
            GRoot.inst.SetContentScaleFactor(DesignWidth, DesignHeight, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
        }

        private void InitUILayers()
        {
            for (int i = 0; i < (int)UILayerEnum.Count; i++)
            {
                _uiLayers[i] = new GComponent();
                _uiLayers[i].gameObjectName = (UILayerEnum.UI + i).ToString();
                GRoot.inst.AddChild(_uiLayers[i]);
                _uiLayers[i].MakeFullScreen();
            }
        }

        public void AddChildToUILayerRoot(UILayerEnum layer,GObject child,int addToIndex = -1)
        {
            int index = (int)layer;
            if (_uiLayers == null || _uiLayers.Length <= 0 || index >= _uiLayers.Length)
            {
                Log.UI.LogError($"layer is null,layer:{layer}");
                return;
            }
            if(addToIndex != -1)
            _uiLayers[index].AddChildAt(child, addToIndex);
            else
                _uiLayers[index].AddChild(child);
        }

        private void RegistScreenSizeChange()
        {
            GRoot.inst.onSizeChanged.Add(() => {
                for (int i = 0; i < (int)UILayerEnum.Count; i++)
                {
                    _uiLayers[i].MakeFullScreen();
                }
            });
        }

        private void RemoveScreenSizeChange()
        {
            GRoot.inst.onSizeChanged.Clear();
        }

        /// <summary>
        /// 初始化UI界面配置，可以是手写或者走配置表
        /// 个人觉得手写好，需要改动直接修改代码就好，使用配置表还要反复导出，可以在打包的时候使用配置表
        /// </summary>
        private void InitUIConfig()
        {
#if UNITY_EDITOR
            InitUIConfigWithCode();
#else
            InitUIConfigWithData();
#endif
        }

        private void SetFullScreen(PanelBase panel)
        {
            if (GRoot.inst.width != DesignWidth || GRoot.inst.height != DesignHeight)
            {
                panel.UIObj.onSizeChanged.Add(() => {
                    panel.InitObjPosition();
                    //ui往中间偏移多少像素
                    panel.OnSafeAreaChange(SafeSpace);
                });
            }
            else
            {
                panel.InitObjPosition();
                panel.OnSafeAreaChange(SafeSpace);
            }
            panel.UIObj.MakeFullScreen();
        }

        private void SetPivotCenter(PanelBase panel)
        {
            var uiObj = panel.UIObj;
            uiObj.SetPivot(0.5f, 0.5f);
            uiObj.pivotAsAnchor = true;
            uiObj.x = GRoot.inst.width / 2;
            uiObj.y = GRoot.inst.height / 2;
            //1.1f 是为了做动画缩放效果
            uiObj.scaleX = 1.1f;
            uiObj.scaleY = 1.1f;

            GTweener tweener = uiObj.TweenScale(new Vector2(1.0f, 1.0f), 0.4f);
            tweener.SetEase(EaseType.ExpoOut);
        }

        private void InitPanel(string uiName,PanelBase panel,NotifyParam param = null)
        {
            if (!_createdPanels.ContainsKey(uiName))
                _createdPanels.Add(uiName, panel);
            var config = GetUIData(uiName);
            bool isRoot = config.IsRoot;
            bool isFullScreen = config.IsFullScreen;
            bool hasModal = config.IsModal;
            bool isHideBlow = config.IsHideBlow;
            var layer = config.UILayer;
            string openSound = config.OpenSound;
            panel.Layer = layer;

            if(isRoot)
                RemoveAll(true);

            if (isHideBlow)
                HideAll();

            if (isFullScreen)
                SetFullScreen(panel);
            else
                SetPivotCenter(panel);

            SetTopInfo(config);

            if (hasModal)
            {
                var graph = panel.CreateModal();
                if(graph.parent == null)
                {
                    _uiLayers[(int)layer].AddChild(graph);
                }
                if (!config.IsFullScreen)
                    panel.UIObj.opaque = false;
                else
                    panel.UIObj.opaque = true;
            }

            //playSound

            //openButton
            GObject closeButton = panel.UIObj.GetChild("closeButton");
            if(closeButton != null)
            {
                closeButton.onClick.Set(() => {
                    RemovePanel(panel, false);
                });
            }
            if (panel.UIObj.parent != _uiLayers[(int)layer])
                _uiLayers[(int)layer].AddChild(panel.UIObj);

            panel.OnShow(param);
            panel.OnResume();
            _panels.AddLast(panel);
        }

        public PanelBase OpenPanel<T>(NotifyParam param = null)
        {
            Type type = typeof(T);
            string name = type.Name;

            if (!type.IsSubclassOf(typeof(PanelBase)))
            {
                Log.UI.LogError($"can not find panel class {name}");
                return null;
            }
            return OpenPanel(name,type, param);
        }

        public void OpenPanelAsync<T>(Action<PanelBase> callback = null,NotifyParam param = null)
        {
            Type type = typeof(T);
            string name = type.Name;

            if (!type.IsSubclassOf(typeof(PanelBase)))
            {
                Log.UI.LogError($"can not find panel class {name}");
                return;
            }
            OpenPanelAsync(name, type, callback, param);
        }

        //UI界面打开（支持同步和异步）
        private PanelBase OpenPanel(string uiName,Type type,NotifyParam param)
        {
            var config = GetUIData(uiName);
            if(config == null)
            {
                Log.UI.LogError($"ui config is null,uiname :{uiName}");
                return null;
            }

            PanelBase panel = null;

            if(_createdPanels.ContainsKey(uiName))
            {
                panel = _createdPanels[uiName];
                InitPanel(uiName, panel, param);
            }
            else
            {
               
                //has package  load pkg
                if (!_loadedPkg.ContainsKey(config.Package))
                {
                    var pkg = ResourceManager.Instance.LoadFairyGuiPackage(config.Package);
                    if (pkg == null)
                    {
                        Log.UI.LogError($"load ui package failed,pkgName{config.Name}");
                        return null;
                    }
                    UIPackageReference reference = new UIPackageReference(pkg.name, pkg);
                    reference.AddReferenceCount();
                    _loadedPkg.Add(config.Package, reference);
                }
                else
                    _loadedPkg[config.Package].AddReferenceCount();

                var com = UIPackage.CreateObject(config.Package, config.Resource).asCom;
                com.fairyBatching = config.IsBatching;
                panel = (PanelBase)PanelBase.Create(type, com, uiName);
                if (panel != null)
                {
                    panel.OnCreate();
                    InitPanel(uiName, panel,param);
                }
                return panel;
            }

            return panel;
        }

        private void OpenPanelAsync(string uiName,Type type, Action<PanelBase> callback = null, NotifyParam param = null)
        {
            var config = GetUIData(uiName);
            if (config == null)
            {
                Log.UI.LogError($"ui config is null,uiname :{uiName}");
                return;
            }
            PanelBase panel = null;

            if (_createdPanels.ContainsKey(uiName))
            {
                panel = _createdPanels[uiName];
                InitPanel(uiName, panel, param);
                callback?.Invoke(panel);
            }
            else
            {

                //has package  load pkg
                if (!_loadedPkg.ContainsKey(config.Package))
                {
                    var pkg = ResourceManager.Instance.LoadFairyGuiPackage(config.Package);
                    if (pkg == null)
                    {
                        Log.UI.LogError($"load ui package failed,pkgName{config.Name}");
                        return;
                    }
                    UIPackageReference reference = new UIPackageReference(pkg.name, pkg);
                    reference.AddReferenceCount();
                    _loadedPkg.Add(config.Package, reference);
                }
                else
                    _loadedPkg[config.Package].AddReferenceCount();

                UIPackage.CreateObjectAsync(config.Package, config.Resource, (GObject obj) => {
                    if(obj == null)
                    {
                        Log.UI.LogError($"load component failed,com name{config.Resource}");
                        return;
                    }
                    var com = obj.asCom;
                    com.fairyBatching = config.IsBatching;
                    panel = (PanelBase)PanelBase.Create(type, com, uiName);
                    if(panel != null)
                    {
                        panel.OnCreate();
                        InitPanel(uiName, panel,param);
                        callback?.Invoke(panel);
                    }
                });
            }
        }

        public void RemoveAll(bool destroy = false)
        {
            if (_panels.Count <= 0) return;
            var last = _panels.Last;
            while (last != null)
            {
                HideOrDestroyPanel(last.Value, destroy);
                last = last.Previous;
            }
            _panels.Clear();
        }

        public void HideAll()
        {
            foreach (var panel in _panels)
            {
                panel.OnHide();
            }
        }

        public void PopPanel(bool destroy = false)
        {
            var last = _panels.Last;
            RemovePanel(last.Value, destroy);
        }

        public void RemovePanel<T>(bool destroy = false)
        {
            string name = typeof(T).Name;
            RemovePanel(name, destroy);
        }

        public void RemovePanel(PanelBase panel,bool destroy = false)
        {
            string name = panel.Name;
            RemovePanel(name, destroy);
        }

        //UI界面关闭
        private void RemovePanel(string uiName,bool destroy =false)
        {
            //不存在
            if(!_createdPanels.ContainsKey(uiName))
            {
                Log.UI.LogError($"remove panel is null,panel name{uiName}");
                return;
            }
            var config = GetUIData(uiName);


            //全部移除
            if (config.IsRoot)
            {
                RemoveAll(destroy);
                return;
            }

            var panel = _panels.Last.Value;
            HideOrDestroyPanel(panel, destroy);
            _panels.RemoveLast();

            //开启界面直到界面隐藏后面的界面
            foreach (var item in _panels)
            {
                if(item.Visible == false)
                {
                    var itemConfig = GetUIData(item.Name);
                    if (itemConfig.TopInfo != null && itemConfig.IsHideBlow)
                        SetTopInfo(itemConfig);
                    item.OnShow(null);
                }
                else
                {
                    item.OnResume();
                }
                if(GetUIData(item.Name).IsHideBlow)
                    break;
            }
        }


        private void HideOrDestroyPanel(PanelBase panel,bool destroy)
        {
            panel.OnHide();
            panel.OnPause();
            if (destroy)
            {
                _createdPanels.Remove(panel.Name);
                panel.Dispose();
                _uiLayers[(int)panel.Layer].RemoveChild(panel.UIObj);
                RemovePackage(GetUIData(panel.Name).Package);
            }
        }


        private void RemovePackage(string pkgName)
        {
            if (!_loadedPkg.ContainsKey(pkgName))
            {
                Log.UI.LogError($"package is null,PackageName{pkgName}");
                return;
            }
            UIPackageReference pkg = _loadedPkg[pkgName];
            pkg.ReduceAddReferenceCount();
            if(pkg.ReferenceCount <= 0)
            {
                ResourceManager.Instance.RemoveFairyGuiPackage(pkg.PackageName);
                _loadedPkg.Remove(pkg.PackageName);
            }
        }

        public void SetTopInfo(UIData data, bool showAnim = true)
        {
            if (data != null && data.TopInfo != null)
            {
                if (CommonTop == null)
                {
                    GObject top = UIPackage.CreateObject("Common", "CommonTop");
                    AddChildToUILayerRoot(UILayerEnum.Top, top, 0);
                    CommonTop = top.data as CommonTopBase;
                    CommonTop.SetCommonTopInfo(data, true, showAnim);
                }
                else
                {
                    CommonTop.SetCommonTopInfo(data, true, showAnim);
                }
            }
            else if(CommonTop != null && data.IsFullScreen && data.UILayer == UILayerEnum.UI)
                CommonTop.SetCommonTopInfo(data, false, showAnim);
        }
    }
}
