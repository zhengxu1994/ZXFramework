# Unity资源管理

## 1.1什么是Assets

#### 一层是Unity默认工程的Assets资源，一层是Unity打包系统AssetBundles

#### 1.1.1 Assets目录

- Assets Unity工程实际的资源目录，所有项目用到的资源、代码、配置、库等等原始资源只有放置在这个文件夹才会被Unity认可和处理。
- Library 存放Unity处理完毕的资源，大部分的资源导入到Assets目录之后，还需要通过Unity转化成Unity认可的文件，转化后的文件会存储在这个目录。
- Packages 这个是2018以后新增的目录，用于管理Unity分离的packages组件。这个我在之前的知乎文章里有过阐述：https://zhuanlan.zhihu.com/p/77058380
- ProjectSettings 这个目录用于存放Unity的各种项目设定。



#### 1.1.2 AssetBundles

##### 这是一种捆绑包，是对Asset进行归档的格式，概念更趋向于我们使用zip或者RAR等格式对资源或者目录进行压缩、加密、归档、存储等等。而区别就在于zip等压缩格式是针对文件的，而AssetBundles则是针对Unity的Asset。但如果再转换一下概念来理解的话，zip其实操作归档的是操作系统能识别的文件，而AssetBundles操作归档的则是Unity能识别的文件，这么理解的话二者的作用几乎是一致的。

**AssetBundles是一个包含了特殊平台、非代码形式Assets的归档文件。**



#### 1.2 Assets的识别和引用

#####  作为资产文件，Assets有非常多的类型。比如材质球、纹理贴图、音频文件、FBX文件、各种动画、配置或者Clip文件等等。我们通常习惯于在Unity里进行拖拽、新增、修改、重命名甚至变更目录等等各式各样的操作，但不管你在Unity引擎里如何操作（不包括删除），那些相关的引用都不会丢失。这是为什么呢？

## 

#### 1.2.1 Assets和Objects

##### 在进行后面的阐述之前，先统一一下概念，包括如果在后面章节里提到的话，都会遵循这里统一的概念。Assets这里以及后续的内容都指Unity的资产，可以意指为Unity的Projects窗口里看到的单个文件（或者文件夹）。而Objects这里我们指的是从UnityEngine.Object继承的对象，它其实是一个可以序列化的数据，用来描述一个特定的资源的 **实例** 。它可以代表任何Unity引擎所支持的类型，比如mesh,sprite, AudioClip or AnimationClip等等。

大多数的Objects都是Unity内置支持的，但有两种除外：

- ScriptableObject  用来提供给开发者进行自定义数据格式的类型。从该类继承的格式，都可以想Unity的原生类型一样进行序列和反序列化，并且可以从Unity的Inspector窗口进行操作。
- MonoBehaviour 提供了一个指向MonoScript的转换器。MonoScript是一个Unity内部的数据类型，它不是可执行代码，但是会在特定的命名空间和程序集下，保持对某个或者特殊脚本的引用。

Assets和Objects之间是一对多的关系，比如一个Prefab我们可以认为是一个Asset，但是这个Prefab里可以包含很多个Objects，比如 ：如果是一个UGUI的Prefab，就可能里面会有很多个Text、Button、Image等组件。



#### 1.2.2 File GUIDs 和 Local IDs

熟悉Unity的人都知道，UnityEngine.Objects之间是可以互相引用的。这就会存在一个问题，这些互相引用的Objects有可能是在同一个Asset里，也有可能是在不同的Assets里。比如UGUI的一个Image需要引用一张Sprite  Atlas里的Sprite。这就要求Unity必须有健壮的资源标识，能稳定的处理不同资源的引用关系。除此之外的话，Unity还必须考虑这些资源标识应该与平台无关，不能让开发者在切换平台的时候还需要关注资源的引用关系，毕竟它自己是一个跨平台部署的引擎。

基于这些特定的需求，Unity把序列化拆分成两个表达部分，第一部分叫做File GUID。标识这个资产的位置，这个GUID是由Unity根据内部算法自动生成的，并且存放在原始文件的同目录、同名但是后缀为.meta的文件里。

这里需要注意几个点：

- 第一次导入资源的时候Unity会自动生成。
- 在Unity的面板里移动位置，Unity会自动帮你同步.meta文件。
- 在Unity打开的情况下，单独删除.meta，Unity可以确保重新生成的GUID和现有的一样。
- 在Unity关闭的情况下，移动或者删除.meta文件，Unity无法恢复到原有的GUID，也就是说引用会丢失。

确定了资产文件之后，还需要一个Local IDs来表示当前的Objects在资产里的唯一标识。File GUID确保了资产在整个Unity工程里唯一，Local ID确保Objects在资产里唯一，这样就可以通过二者的组合去快速找到对应的引用。

Unity还在内部维护了一张资产GUID和路径的映射表，每当有新的资源进入工程，或者删除了某些资源。又或者调整了资源路径，Unity的编辑器都会自动修改这张映射表以便正确的记录资产位置。所以如果.meta文件丢失或者重新生成了不一样的GUID的话，Unity就会丢失引用，在工程内的表现就是某个脚本显示“Missing”,或者某些贴图材质的丢失导致场景出现粉红色。



#### 1.2.4 Instance Id

File GUID和Local ID确实已经能够在编辑器模式下帮助Unity完成它的规划了，与平台无关、快速定位和维护资源位置以及引用关系。但若投入到运行时，则还有比较大的性能问题。也就是说运行时还是需要一个表现更好的系统。

于是Unity又弄了一套缓存（还记得前面那套缓存嘛，是用来记录GUID和文件的路径关系的）PersistentManager，用来把File GUIDs 和 Local IDs转化为一个简单的、Session唯一的整数。这些整数就是Instance Id。Instance  Id很简单，就是一个递增的整数，每当有新对象需要在缓存里注册的时候，简单的递增就行。



#### 1.3资源生命周期

到现在为止我们已经搞清楚了Unity的Asset在编辑器和运行时的关联和引用关系。那么接下来我们还要关注一下这些资源的生命周期，以及在内存中的管理方式，以便大家能更好的管理加载时长和内存占用。



#### 1.3.1 Object加载

当Unity的应用程序启动的时候，PersistentManager的缓存系统会对项目立刻要用到的数据（比如启动场景里的这些或者它的依赖项），以及所有包含在Resources  目录的Objects进行初始化。如果在运行时导入了Asset或者从AssetBundles（比如远程下载下来的）加载Object都会产生新的Instance ID。

另外Object在满足下列条件的情况时会自动加载，比如：

1、某个Object的Instance ID被间接引用了。

2、Object当前没有被加载进内存。

3、可以定位到Object的源位置（File GUID 和 Local ID）。

另外，如果File GUID和LocalID没有Instance ID，或者有Instance  ID，但是对应的Objects已经被卸载了，并且这个Instance ID引用了无效的File  GUID和LocalID，那么这个Objects的引用会被保留，但是实际Objects不会被加载。在Unity的编辑器里会显示为：“(Missing)”引用，而在运行时，根据Objects类型不一样，有可能会是空指针，有可能会丢失网格或者纹理贴图导致场景或者物体显示粉红色。



```c#
 //加载一个texture 
var txt = Resources.Load("1") as Texture2D;
 var sprite = Sprite.Create(txt, new Rect(0, 0, txt.width,txt.height), Vector2.zero);
 sp.sprite = sprite;

 Resources.UnloadAsset(txt);//图片会消失 因为资源被卸载掉了
```



#### 1.3.2 Object卸载

除了加载之外，Objects会在一些特定情况下被卸载。

1、当没有再使用的Asset在执行清理的时候，会自动卸载对应的Object。一般是由切场景或者手动调用了Resources.UnloadUnusedAssets的 API时候触发的。但是这个过程只会卸载那些没有任何引用的Objects。

2、从Resources目录下加载的Objects可以通过调用Resources.UnloadAsset  API进行显式的卸载。但这些Objects的 Instance ID会保持有效，并且仍然会包含有效的File GUID 和 LocalID  。当任何Mono的变量或者其他Objects持有了被Resources.UnloadAsset卸载的Objects的引用之后，这个Object在被直接或者间接引用之后马上被加载。

3、从AssetBundles里得到的Objects在执行了AssetBundle.Unload(true) API之后，会立刻自动的被卸载。并且这会立刻让这些Objects的File GUID 、 Local ID以及Instance  ID立马失效。任何试图访问它的操作都会触发一个NullReferenceException。但如果调用的是AssetBundle.Unload(false)API的话，那么生命周期内的Objects不会随着AssetBundle一起被销毁，但是Unity会中断 File GUID 、Local ID和对应Object的Instance  IDs之间的联系，也就是说，如果这些Objects在未来的某些时候被销毁了，那么当再次对这些Objects进行引用的时候，是没法再自动进行重加载的。

