### 我认为的UI框架需要实现功能

#### 基础功能

- 界面创建

  - 界面所需资源加载（ugui需要加载uiprefab，fgui则需要加载资源包）
  - 界面层级设置
    - UI层级 普通UI层界面
    - Top层级 顶层常驻界面一般显示玩家战力金钱等信息
    - Cover层级 覆盖层界面，一般为聊天界面
    - Tip层级 提示层界面，一些游戏内的信息提示弹出框显示
    - Guide层级 引导层界面，一般为新手引导或者其他引导显示
    - Waiting层级 waiting层界面，一般为loading界面或者一些等待界面比如请求服务器信息
  - 设置自适应
  - 界面创建成功回调事件。一般为请求界面使用的服务器数据回调。
  - 界面创建失败回调。

- 界面初始化

  - 界面继承初始化接口

- 界面显示

  - 是否刷新界面信息，刷新可能需要重新请求服务器数据。
  - 是否激活界面update

- 界面隐藏

  - 界面隐藏事件，是否销毁

- 界面寻轮

  - update帧率

- 界面暂停与界面开启

  - 暂停和开启接口

- 界面遮挡与界面遮挡恢复

  - 如果被遮挡是否停止某些逻辑的处理，恢复后重启逻辑

- 界面深度改变

  - 界面的层级调整

- 界面销毁

  - 界面销毁是否卸载使用的UI资源

- 界面管理

  - 界面初始化
  - Ui 界面打开

  - UI界面关闭
  - UI界面销毁
  - 计算UI占用内存，UI资源的加载，UI资源的卸载。

#### 进阶功能

- **导出自动化**   FGUI或者UGUI 自动导出生成对应UI类和数据类
- **加载自动化** 实现自动加载UI对象与UI类进行关联，自动获取UI中的控件等
- **资源管理**  资源规范,UI图集分类，图片名称命名等。。。。
- **常用UI接口实现** 一些icon，人物立绘，人物头像快速获取接口设计
- **快速开发模式** FGUI 实现UniRx拓展以链式模式开发。







#### UI逻辑开发，项目中的ui开发流程

- 1.在fgui编辑器内编辑ui界面，可以设置UI的分辨率，自适应，创建按钮，滑动条，列表等组件。

  - 设计分辨率为1136x640

- 2.通过fgui插件导出对应的ui包（包含资源和二进制ui界面数据）

  - 图集导出最大尺寸为1024x1024，如果超出则分图集导出。

- 3.通过fgui导出插件生成对应ui界面的脚本（有一个初始化方法根据名称获取所有的fgui对象，**按钮，进度条，滑动条，列表，动画，控制器等一切已经编辑好的对象**）

  - 关键字partial 可以将同个类的逻辑分离在不同的文件中。

  ```c#
  partial class ExtensionList{
    public ExtInfo e_LoginUI = new ExtInfo(){ExtClass = typeof(LoginUI),ExtUrl = "ui://Login/LoginUI",
                                    ExtType = ObjectType.Component };
  }
  partial class LoginUI
  {
    public static readonly string Url = "ui://Login/LoginUI";
    
    public enum CtrPageId{
      login = 0,
      reg = 1,
    }
    
    public EventListener ctrPageChanged = {get => cc0.onChange;}
    public string ctrPageSelectedPage{get => cc0.selectedPage;}
    
    public GButton btn_login{get;private set;}
    
    public GComboBox combo_language{get;private set;}
    
    private Controller cc0{get;set;}
    
    public float btn_login_x{get;private set;} = 0;
    
    public float btn_login_y{get;private set;} = 0;
    
    public override void AutoBinderUI()
    {
      btn_login = uiObj.GetChildAt<GButton>(0);
      combo_language = uiObj.GetChildAt<GComboBox>(1);
      cc0 = uiObj.GetController(0);
    }
    
    public override void InitObjPosition()
    {
         //用于手动调整自适应比例
         btn_login_x = btn_login.x;
         btn_login_y = btn_login.y;
    }
  }
  
  partial class UILogin : PanelBase
  {
     //这里面实现具体ui逻辑
     //一般会有一个init方法
     public void Init(){}
  
     //需要实现onremove 和dispose方法
  }
  ```

  

- 4.UIConfig 设置ui界面配置，具体可以配置ui对应的**ui包名，ui组件名称，ui类，是否全屏缩放，是否是根节点，是否带模态背景，是否开启fgui的batch合批功能，是否有top界面（通用界面，显示界面名称和玩家金钱等信息）**

  ```c#
  //手动定义的UI配置
  public class UIData
  {
    //是否是根节点
    public bool isRoot{private set; get;} = false;
    //是否带有半透明背景 点击背景会自动关闭界面
    public bool isModal{private set; get;} = false;
    //是否是全屏界面 如果是全屏界面要设置component的size 为groot.width/height
    public bool isFullScreen{private set; get;} = false;
    //打开时是否隐藏之前的界面
    public bool hideBlow{private set; get;} = false;
    //ui层级
    public LayerEnum layer{private set; get;} = LayerEnum.UI;
    //ui类名
    public string name{get;private set;}="";
    //ui包名
    public string package{get;private set;}="";
    //ui资源名称
    public string resource{get;private set;}="";
    //打开界面音效
    public string openSound{get;private set;};
    //关闭界面音效
    public string closeSound{get;private set;};
  }
  ```

  

- 5.UIManager类

  - 1.通过类名反射生成出对应的ui类，ui类自动绑定fgui的ui（通过上面的导出实现），ui类是partical类这样可以将逻辑和自动生成的代码分割。通过类名称可以关闭界面（目前如果不是在常驻列表中的包，那么在这个包没有其他界面使用时会卸载包资源。

    ```c#
    //使用fgui的异步加载方式创建UI界面
    UIPackage.CreateObjectAsync(config.package,config.resource,(GObject uiobj)=>{
      var com = uiObj.asCom;
      com.fairyBatching = batching;//是否开启动态合批
      //反射创建UI类并绑定UI组件，同时传递数据参数
      var panel = (PanelBase)ViewBase.CreateInvoke(com,uiName,objParams,true);
      if(panel != null)
      {
        //根据定义的UI配置去初始化UI的一些属性 比如是否存在全屏 是否关闭top界面 ，是否关闭其他界面，设置UI锚点
        InitPanel(uiName,uiObj,panel);
        dlg?.Invoke(panel);//创建成功回调
      }
    });
    
    private void InitPanel(string uiName,GObject obj,PanelBase panel)
    {
      UIData config = GetUIData(uiName);
      GComponent uiObj = obj.asCom;
      //如果是根节点，那么关闭其他所有界面
      if(config.isRoot)
      {
        Hashset<string>remPanels = new Hashset<string>();
        var pair = panelStack.GetEnumerator();
        while(pair.MoveNext())remPanels.Add(pair.Current);
        remPanels.BoxEach((v)=>{RemovePanel(v,false);});
        remPanels.clear();
      }
      
      panels[uiName]= panel;
      panels.layer = config.layer;
      if(panel.layer != LayerEnum.Guide)
       panelStack.AddLast(uiName);
      GComponent parent = layers[(int)config.layer];
      uiObj.name = uiName;
      //半透明背景
      if(config.isModal)
      {
        parent.AddChild(panel.CreateModal());
        if(!config.isFullScreen)
          uiObj.opaque = false;
      }
      
      parent.AddChild(uiObj);
      //Top玩家信息界面
      SetTopInfo(config);
      
      //关闭其他界面
      if(config.hideBlow)
      {
        panels.BoxEach((k,v)=>{
          if(v.Name != uiName && v.layer != LayerEnum.Guide)
            v.SetVisible(false);
        });
      }
      //OnCreate事件
      panel.OnCreate();
      //全屏界面
      if(config.isFullScreen)
      {
        SafeSpace = LocalStore.Data.GetInt("sliderPadding",50);
        if(GRoot.inst.width != DesignWidth || GRoot.inst.height != DesignHeight)
        {
          uiObj.onSizeChanged.Add(()=>{
            panel.InitObjPosition();
            //ui往中间偏移多少像素
            panel.OnSafeAreaChange(SafeSpace);
          });
        }
        else
        {
             panel.InitObjPosition();
            //ui往中间偏移多少像素
            panel.OnSafeAreaChange(SafeSpace);
        }
        uiObj.MakeFullScreen();
      }
      else
      {
        //设置中心点和锚点
        uiObj.SetPivot(0.5,0.5);
        uiObj.pivotAsAnchor = true;
        uiObj.x = GRoot.inst.width /2;
        uiObj.y = GRoot.inst.height/2;
        //1.1f 是为了做动画缩放效果
        uiObj.scaleX = 1.1f;
        uiObj.scaleY = 1.1f;
        
        GTweener tweener = uiObj.TweenScale(new Vector2(1.0f,1.0f),0.4f);
        tweener.SetEase(EaseType.ExpoOut);
      }
      
      if(config.openSound != null)
        AudioMgr.Inst.PlayEvent(config.openSound);
      GObject closeButton = uiObj.GetChild("closeButton");
      if(closeButton != null)
      {
        closebutton.onClick.Set(()=>{
          RemovePanel(uiName);
        });
      }
    }
    
    //移除界面
    public void RemovePanel(string uiName,bool isCreateRoot = true)
    {
      if(!panels.ContainsKey(uiName))
        return;
      UIData config = GetUIData(uiName);
      string pkgName = config.package;
      
      //关闭界面
      PanelBase panel = panels[uiName];
      panels.Remove(uiName);
      
      if(panelStack.Contains(uiName))
        panelStack.Remove(uiName);
      panel.Dispose();
      //播放关闭界面音效
      if(config.closeSound != null)
        AudioMgr.Inst.PlayEvent(config.closeSound);
      //判断是否需要移除ab资源包 这里的规则是除了常驻的包其他的包只要没有对象使用则卸载掉
      bool remPkg = PkgCache.IndexOf(pkgName) < 0;
      if(remPkg){
        var pair = panels.GetEnumerator();
        while(pair.MoveNext())
        {
          if(pair.Current.Key != uiName)
          {
            UIData data =GetUIData(pair.Current.Key);
            if(data.package ==  config.package)
            {
              remPkg = false;
              break;
            }
          }
        }
      }
      
      //如果需要移除包
      if(remPkg)
      {
        ResourceManager.Inst.RemoveFairyGuiPackage(pkgName);
      }
      
      //依次向前查找到第一个有commontop的panel
      var lastPanel = panelStack.Last;
      bool remCommonTop = true;
      bool topSet = false;
      bool visSet = false;
      bool isCloseFullScreen = config.isFullScreen && !config.hideBelow && config.layer != LayerEnum.Guide;
      while(lastPanel != null)
      {
        uiName = lastPanel.Value;
        config = GetUIData(uiName);'
        //设置top信息界面
        if(config.topInfo != null && !topSet)
        {
          topSet = true;
          remCommonTop = false;
          SetTopInfo(config,isCloseFullScreen);
        }
        //依次打开栈中的界面直到当前界面需要隐藏后面的界面
        if(!visSet)
        {
           if(config.layer != LayerEnum.Guide)
             panels[uiName].SetVisible(true);
          visSet = config.hideBelow;
        }
        if(topSet && visSet)
          break;
        lastPanel = lastPanel.Previous;
      }
      //没有UI界面了但是需要有根界面 就创建mainUI界面
      if(PanelCount <= 0 && isCreateRoot)
      {
        if(SceneMgr.Inst.currentScene.Name=="home")
        {
           CreateUI("MainUI",false);
           remCommonTop =false;
        }
      }
      //从上面的查找中发现这些界面都不需要显示top信息则把top信息界面隐藏或者删除
      if(remCommonTop)
        RemTop(true);
    }
    ```

    

    ```c#
    internal abstract class PanelBase : ViewBase
    {
      public LayerEnum layer = LayerEnum.UI;
      
      protected abstract void OnRemove();
      //处理刘海屏，在屏幕边缘留一定的像素，将边缘UI向内移动。
      public abstract void OnSafeAreaChange(int safeSpace);
      //根面板点击返回 处理一些场景跳转后显示的根UI界面
      public virtual void OnRootPanelClickBack(){}
    }
    
    public abstract class ExtensionBase : IDisposable
    {
      public GComponent uiObj{get;private set;} = null;
      
      private string _name = null;
      
      public string Name{get=>_name??(_name = GetType().Name);}
      
      //核心函数
      public static ExtensionBase Create(Type type,GComponent uiObj)
      {
        //不是继承extensionbase的获取是继承viewbase的不作处理
        if(!type.IsSubclassOf(typeof(ExtensionBase)) || type.IsSubclassOf(typeof(ViewBase)))
        {
          return;
        }
        ExtensionBase view = null;
        BindingFlags flags = BindFlags.Instance | BindingFlags.Public;
        //利用反射创建UI类并将fgui component 与之关联
        ConstructorInfo ctor = type.GetConstructor(flags,Type.DefaultBinder,createOTypes,null);
        if(ctor != null)
          view = ctro.Invoke(null) as ExtensionBase;
        else
          LogTool.LogError("create {0} not find constructor",type.ToString());
        if(view != null)
        {
          InitUIObject(view,uiObj);
          view.InitObjPosition();
          view.OnCreate();
        }
        else
          uiObj.Dispose();
        return view;
      }
      
      public static void InitUIObject(ExtensionBase view,GComponent uiObj)
      {
        view.uiObj = uiObj;
        view.uiObj.onDispose.Add(()=> view.RealDispose(true));
        view.AutoBinderUI();
        uiObj.data = view;
      }
    }
    
    internal abstract class ViewBase : ExtensionBase
    {
        
    }
    ```

  - 2.自适应设置

    Scale with Screen Size 根据屏幕尺寸进行缩放

    Screen Match Mode （Match Width or Height）根据设计分辨率/屏幕分辨率宽高比，取其中小的比例作为屏幕ui的缩放比例，这样保证ui永远都能显示在屏幕中。

  - 3.设置ui的层级，一般分为UI,TOP,Cover,Tip,Guide,Waiting，在GRoot节点下创建n个节点(从后往前)，将对应层级的UI添加到对应的节点下。如果是ugui可以同样创建多个canvas，将对应的ui挂在对应的canvas，设置canvas的sortingorder可以控制canvas的显示顺序。

    ```c#
    for(int i =0 ;i <(int)LayerEnum.Count;i++)
    {
      layers[i] = new GComponent();
      layers[i].gameObjectName = (LayerEnum.UI+i).ToString();
      GRoot.inst.AddChild(layers[i]);
      layers[i].MakeFullScreen();
    }
    
    GRoot.inst.onSizeChanged.Add(()=>{}
        for(int i =0 ;i <(int)LayerEnum.Count;i++){
                     
               layers[i].MakeFullScreen();      
        }             
    );
    ```

- 6.ExtensionBase 可以选择将组件也导出对应的ui类，继承了ExtensionBase类的一般为界面内的子界面，与父节点一同销毁。

  ```c#
  //比如上面的LoginUI
  public RegisterUI register{get;private set;}//自动生成 绑定RegisterUI类 在AutoBindUI时一同初始化
  
  partial class ExtensionList
  {
     public ExtInfo e_LoginUI = new ExtInfo(){ExtClass = typeof(LoginUI),ExtUrl = "ui://Login/RegisterUI",
                                    ExtType = ObjectType.Component };
  }
  
  partial class RegisterUI : ExtensionBase
  {
    //
    public override void AutoBindUI(){}
  }
  
  ```

  ExtensionBase这些类是如何自动创建出来的？

  - 1.pi.extensionCreator();调用拓展委托方法
  - 2.根据类型实例化对应类，这些类继承fgui组件类
  - 3.调用实例化类构造中传递的回调方法。 ExtensionBase ret = ExtensionBase.Cteate(type,com);
  - 4.根据反射创建自定义的UI类

  ```c#
  internal static class ExtensionMgr
  {
    public static void InitExtension()
    {
      ExtensionList list = new ExtensionList();//这个里面就是上面partial class Extensionlist所有集合
      
      BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingsFlags.Static;
      FieldInfo[] fields = typeof(ExtensionList).GetFields(flags);
      ExtInfo info;
      //遍历字段 创建对应的拓展组件并创建回调，当拓展组件被fgui根据xml文件初始化时调用回调，执行UI类的初始化方法
      for(int i = 0;i<fields.Length;i++)
      {
        info = fields[i].GetValue(list) as ExtInfo;
        //大类型UI界面不自动生成
        if(info.ExtClass.IsSubclassOf(typeof(ViewBase)))
          continue;
        SetExtension(info.ExtClass,info.ExtUrl,info.ExtType);
      }
    }
    
    private static void SetExtension(System.Type type,string url,ObjectType objType)
    {
      //fgui的api 可以拓展组件类，只要类继承对应的组件类就可以，比如ExtGButton : GButton
      FguiExtension.SetExtension(url,objType,(GComponnt com)=>{
        //根据xml创建后反射绑定对应UI类 （第三步）
        ExtensionBase ret = ExtensionBase.Cteate(type,com);
      });
    }
    
    public static void SetExtension(string url,ObjectType type,Action<GComponent>callback)
    {
      if(type == ObjectType.Loader)
      {
        UIObjectFactory.SetLoaderExtension(()=>{
          return new ExtLoader(callback);        
        });
      }
      else
      {
        //（第二步）
        UIObjectFactory.SetPackageItemExtension(url,()=>{
          GComponent com = null;
          switch(type)
          {
            case ObjectType.Component:
              com = new ExtCompnent(callback);
              break;
            case ObjectType.Button:
              com = new ExtButton(callback);
              break;
            case ObjectType.List:
              com = new ExtList(callback);
              break;
              //...各个类型的component拓展 传递回调（ExtensionBase ret = ExtensionBase.Cteate(type,com);）
          }
        });
      }
    }
    //FGUI API 注册拓展
    //GComponentCreator 是一个delegate委托类型
    public static void SetPackageItemExtension(string url,GComponentCreator creator)
    {
      if(url == null)
        throw new Exception("Invaild url:"+url);
      packageItem pi = UIPackage.GetItemByURL(url);
      if(pi = null)
        pi.extensionCreator =creator;
      packageItemExtensions[url] = creator;
    }
    
    //在UIPackage 调用LoadPackage()方法时会解析对应包内是否有组件绑定了委托
    /*
    case PackageItemType.Component:
    {
    int extension = buffer.ReadByte();
    if(extension >0)
    pi.objectType = (ObjectType)extension;
    else
    pi.objectType = ObjectType.Component;
    pi.rawData  = buff.ReadBuffer();
    UIObjectFactory.ResolvePackageitemExtension(pi);//解析包方法
    }
    break
    */
     //在创建fgui对象时会判断有没有委托，如果有委托的话则调用委托方法 (第一步)
     public static GObject NewObject(PackageItem pi, System.Type userClass = null)
          {
              GObject obj;
              if (pi.type == PackageItemType.Component)
              {
                  if (userClass != null)
                  {
                      Stats.LatestObjectCreation++;
                      obj = (GComponent)Activator.CreateInstance(userClass);
                  }
                  else if (pi.extensionCreator != null)
                  {
                      Stats.LatestObjectCreation++;
                      obj = pi.extensionCreator();
                  }
                  else
                      obj = NewObject(pi.objectType);
              }
              else
                  obj = NewObject(pi.objectType);
  
              if (obj != null)
                  obj.packageItem = pi;
  
              return obj;
          }
  }
  ```





#### 从底层角度思考UI框架需要实现的几个功能

- 自动化，为了解决大量重复的代码和可能因为书写错误导致的bug，所以需要自动化生成UI代码类工具，自动化资源检测。
  - FGUI编辑器内的界面导出后生成对应的cs代码，UI界面创建时获取界面的UI控件。
  - 在导出时自动化检测（根据命名规范）（sicon，bicon，包名-图片名-noalpha/alpha，big图片名），根据命名前缀判断图片是否打入图集，打入哪个图集，打包策略。
    - sicon,很小的icon可以不超过100x100的尺寸的可以打入到common包内，common包大小为2048x2048，尽量将common包充足使用。（代码内常驻，因为icon经常使用）
    - bicon,一些大一点的图片，比如人物头像，物品图片的，打到一张hero包内，包大小也可以使用2048x2048。（代码内常驻，图集经常使用）
    - 包名-图片名-alpha/noalpha ,只在单个包内使用的图片打入一个图集，如果包内的所有图片都是不带透明通道的，那么在导出时设置为不分离alpha通道。
    - big图片不进行打包，专门放到一个单独的包内使用，在使用时加载对应的图片资源，用完卸载。
- 资源管理，开发者不用去关心去使用资源和卸载资源，只需要调用一个接口就能创建出对应的UI界面。
  - 不同平台不同的资源路径，需要得到正确的资源加载路径。
  - 支持同步和异步的资源加载。同步可以加载一些简单的界面（界面使用的资源不多），异步可以加载一些复杂的界面（界面使用的资源比较大），这个跟界面管理结合。
- UI界面管理，开发者不用去关心UI与UI之间的关系，只需要调用开启和关闭UI界面接口即可。
  - 界面初始化，第一次加载界面或者重新创建界面时调用自动化生成的init接口，获取控件，同时根据界面配置设置界面是否全屏，层级等信息（这里可以使用链式拓展？需要想怎么实现）
  - 开启界面，检测是否存在隐藏的界面，界面使用的资源是否被加载，调用UI界面的onshow接口，支持同步和异步，如果是异步要显示加载中界面。
  - 界面关闭，支持是否销毁界面是否卸载资源，调用界面onhide接口，如果销毁界面需要判断界面使用的资源是否有其他界面，如果没有需要卸载UI资源（根据传递的参数）。
- 在底层偏上一点更偏向业务逻辑的拓展功能，让开发更加的快速流畅，使用unirx开发流。





### UIManager管理类

- UI界面打开
  -  如果需要打开的界面已经存在那么直接获取，如果需要隐藏后面的界面那么将栈中界面关闭，并显示添加到栈顶。
  -  如果需要打开的界面是根节点界面那么关闭栈中所有界面并清空栈数据，创建新根节点界面并加入栈中
  - 创建
    - 如果包不存在那么先加载包数据
    - 获取初始化的fgui组件，通过反射初始化出对应的UI类并绑定，再添加到缓存队列中。
- UI界面关闭
  - 关闭当前界面触发关闭事件，如果remove=false，那么将界面visible设为false，并从栈中抛出，并将栈顶的界面返回。
  - 如果返回的栈顶界面需要隐藏后面的界面那么直接显示界面即可，如果不需要显示那么递归到需要显示的界面即可。
  - 如果当前界面为最后一个界面那么需要判断是否强制移除根节点界面，只有在其他根节点界面替换时才能移除。
  - 如果需要销毁UI界面那么调用界面销毁事件，并判断资源引用是否为0，如果是0并且不是常驻资源则卸载。
  - 将返回的UI界面visible设为true并触发界面的显示时间。