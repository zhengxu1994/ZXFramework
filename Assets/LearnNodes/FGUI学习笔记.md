# FGUI学习笔记

#### FGUI中适配

##### FGUI中有3中适配模式

1.适应宽度和高度，取设备分辨率的宽与设计分辨率的宽得出宽度比，再取高度得到高度比，取宽度比和高度比这两个值中的小的一个作为整体ui的缩放比例，这种缩放方式保证内容缩放后始终在屏幕内。如果有留边，则留边部分可以通过关联系统进一步处理。这种方式是适应性很强的处理方式。

2.适应宽度，设备分辨率宽比设计分辨率宽得到比例缩放，高边可能会超出屏幕。

3.适应高度，设备分辨率高比设计分辨率高得到比例缩放，宽边可能会超出屏幕。

###### 什么是设计分辨率：

通常我们会选择一个固定的分辨率进行UI设计和制作，这个分辨率称为设计分辨率。例如1136×640,1280×720都是比较常用的设计分辨率。选定一个设计分辨率后，最大的UI界面（通常就是全屏界面）的大小就限制在这个分辨率。



#### FGUI包的定义

##### FairyGUI是以包为单位组织资源的。包在文件系统中体现为一个目录。assets目录下每个子目录都表示一个包，资源包内的每个资源都有一个是否导出的属性，一个包只能使用其他包设置为已导出的资源，而不设置为导出的资源是不可访问的。同时，只有设置为导出的组件才可以使用代码动态创建。资源包已导出的资源在资源库显示时，图标右下角有一个小红点。

##### FGUI包的发布与使用

###### 发布：

右键包，点击发布。![image-20210704162835958](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20210704162835958.png)

![image-20210704163459249](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704163459249.png)

需要设置几点：

1.发布路径，就是fgui包数据的保存路径

2.拓展名，就是fgui包的数据格式，一般默认使用二进制格式的，任何平台都能读取，占用内存也小。

3.允许发布代码，将fgui的ui发布成对应ui类。

4.包设置里面设置文件名，如果没有就是默认包的名字，设置图片的尺寸，可以勾选为本包生产代码。



#### 包的依赖：

FairyGUI是不处理包之间的依赖关系的，如果B包导出了一个元件B1，而A包的A1元件使用了元件B1，那么在创建A1之前，必须保证B包已经被载入，否则A1里的B1不能正确显示（但不会影响程序正常运行）。**这个载入需要由开发者手动调用，FairyGUI不会自动载入**。

如何查询包的依赖关系：

```c#
    UIPackage pkg;
    var dependencies = pkg.dependencies;
    foreach(var kv in dependencies)
    {
        Debug.Log(kv["id"]); //依赖包的id
        Debug.Log(kv["name"]); //依赖包的名称
    }
```



#### 划分包的规则：

有一个原则，就是不要建立交叉的引用关系，避免出现 A使用B的资源，B使用C的资源，可以创建一个通用common包，包里存放着其他包引用到的一些通用组件和资源，在游戏开始的时候先把common包加载进来，再到具体哪个ui时加载对应的包。



#### 资源URL地址：

##### 在FairyGUI中，每一个资源都有一个URL地址。选中一个资源，右键菜单，选择“复制URL”，就可以得到资源的URL地址。无论在编辑器中还是在代码里，都可以通过这个URL引用资源。例如设置一个按钮的图标，你可以直接从库中拖入，也可以手工粘贴这个URL地址。这个URL是一串编码，并不可读，在开发中使用会造成阅读困难，所以我们通常使用另外一种格式：ui://包名/资源名。两种URL格式是通用的，一种不可读，但不受包或资源重命名的影响；另一种则可读性较高。

```c#
 //对象的URL地址
  Debug.Log(aObject.resourceURL);

  //对象在资源库中的名称
  Debug.Log(aObject.packageItem.name);

  //对象所在包的名称
  Debug.Log(aObject.packageItem.owner.name);

  //根据URL获得资源名称
  Debug.Log(UIPackage.GetItemByURL(resourceURL).name);
```



#### FGUI分支的使用

![image-20210704165111412](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704165111412.png)

切换到对应的分支下右键包，给包创建分支，

![image-20210704165156940](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704165156940.png)

这样就为这个包创建了一个分支。

##### 分支简单的应用：

在组件内添加一个png，名字为1，当中文分支下的是笑脸图，英文分支下也保存一个名字为1的图片，是个哭脸图，当切换在中文分支下时就是笑脸，英文就是哭脸。

![image-20210704165404934](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704165404934.png)

![image-20210704165416266](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704165416266.png)

#### 发布分支

分支发布处理方式有两种：

- `主干包含所有分支` 发布结果包含主干以及所有分支的内容。发布的内容放置在“发布路径”，而非“分支发布路径”。使用这种处理方式，可以在运行时再决定切换到哪个分支。

  例如主干有一个face.png，分支en也有一个face.png，那么发布结果就含有两个face.png。运行时实际显示哪个图片，由代码设置的活跃分支名称决定。

- `主干合并活跃分支`  发布结果包含主干合并当前活跃分支后的内容。也就是说，无论当前分支是什么，发布结果首先都包含所有主干的内容，然后查看哪些资源有分支映射关系的，就用分支的代替主干的。当主工具栏上分支的设置为主干时，发布出来的结果放置在“发布路径”；当设置为某个分支时，发布出来的结果放置在“分支发布路径/分支名称”。

  例如主干有一个face.png，分支en也有一个face.png，如果主工具栏上分支的设置为主干，那么发布出来的结果放置在“发布路径”，包里的face.png是主干的face.png；如果主工具栏上分支的设置为en，那么发布出来的结果放置在“分支发布路径/en”，包里的face.png是分支en里的face.png。

  

![image-20210704170611620](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704170611620.png)

只有在这三种条件都满足的情况下才分支不同的部分才会被单独发步出来。

![image-20210704170704749](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704170704749.png)

#### 运行时使用分支：

```c#
 UIPackage.branch = "en";//需要在加载ui之前
```



#### 资源包的加载：

```c#
  UIPackage.AddPackage("Assets/FGUI/LoginUI");//路径根据具体的资源保存路径
  var com = UIPackage.CreateObject("LoginUI", "LoginUI").asCom;//包 / 组件名称
  GRoot.inst.AddChild(com);//添加到groot的舞台上才能看得到

```



#### 资源包打成ab的优化方案：

fgui发布后会生成2个文件，一个是二进制描述文件，一个是资源图文件，我们可以将这两个文件打成二个包，如果只涉及到ui的位置不修改资源的更新，可以重新导出后只更新二进制描述文件bundle就可以，不过这样的方式在AddPackage时只可以使用

```c#
 //desc_bundle和res_boundle的载入由开发者自行实现。
 UIPackage.AddPackage(desc_bundle, res_bundle);//描述文件bundle ，资源bundle ，资源bundle是旧的
```



#### 包内存管理：

1. AddPackage只有用到才会载入贴图、声音等资源。如果你需要提前全部载入，调用`UIPackage.LoadAllAssets`。
2. 如果UIPackage是从AssetBundle中载入的，在RemovePackage时AssetBundle才会被Unload(true)。如果你确认所有资源都已经载入了（例如调用了LoadAllAssets），也可以自行卸载AssetBundle。如果你的ab是自行管理，不希望FairyGUI做任何处理的，可以设置`UIPackage.unloadBundleByFGUI`为false。
3. 调用`UIPackage.UnloadAssets`可以只释放UI包的资源，而不移除包，也不需要卸载从UI包中创建的UI界面（这些界面你仍然可以调用显示，不会报错，但图片显示空白）。当包需要恢复使用时，调用`UIPackage.ReloadAssets`恢复资源，那些从这个UI包创建的UI界面能够自动恢复正常显示。如果包是从AssetBundle载入的，那么在UnloadAssets时AssetBundle会被Unload(true)，所以在ReloadAssets时，必须调用ReloadAssets一个带参数的重载并提供新的AssetBundle实例。





### DrawCall优化：

FairyGUI基于Unity的`Dynamic Batching`技术，提供了`深度调整技术`进行 Draw Call优化 。FairyGUI能在不改变最终显示效果的前提下，尽可能的把相同材质的物体调整到连续的RenderingOrder值上，以促使他们能够被Unity Dynamic Batching优化。Dynamic Batching是Unity提供的Draw Call  Batching技术之一。如果动态物体共用着相同的材质，那么Unity会自动对这些物体进行批处理。但Dynamic  Batching的一个重要的前提是这些动态物体是连续渲染的。

#### 用自己的理解就是FGUI将不相交的ui，进行优化处理，如果这些ui使用的是同一个材质和shader那么就将这些ui的RenderingOrder设置为同样的值。

FairyGUI提供了一个开关控制组件是否启用深度调整，它就是`fairyBatching`

```c#
aComponent.fairyBatching = true;
```

**永远不要在GRoot上开启fairyBatching.**







### 结合FGUI的UI框架

1.UIManager 管理UI的生成和消耗，UI资源的加载和卸载，ui的显示和影藏， 层级的管理，以及一些常驻界面的设置与管理。

**UI的创建 通过一开始初始化的ui数据，根据类名称 获取对应的数据，加载对应的资源包数据，再通过类名反射出对应UI类，再调用类的初始化方法，并返回这个类**

2.UIExtensionBase 所有的面板UI都继承自这个类，这个类拥有OnCreated,OnShow,OnHide,CreateModal,Dispose，等一些可以复写的方法。

3.ViewBase 一些非面板类型的界面继承这个类，这个类拥有OnCreated,OnShow,OnHide。





#### 资源包内存的占用

![image-20210704180221607](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704180221607.png)

![image-20210704180236732](D:\UnityTool\学习笔记\-\FGUI学习笔记.assets\image-20210704180236732.png)

start是加载资源包，通过包名加载6.6kb差不多和上面的图加二进制文件加起来一样多，应该是同时加载了。

start1是创建一个组件，这个组件只带了一张贴图，需要23kb，23kb是因为这张图片大小为87x76，计算 87x76x4（png rgba） 得下来差不多，所以再使用图片时尽量使用分辨率小的图片（再效果达到的情况下），像那些可以重复平铺拉升的单像素的图可以直接出一个几像素的小图就行。