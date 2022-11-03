# Unity 热更新

### 热更流程：

### 1.资源打包：

**我理解资源分为两类，一种是assets资源（**贴图，材质，预设，shader，fbx等等**），一种脚本文件资源**

##### Unity资源打包设置：

1.设置资源的AssetBundleName

2.设置资源的assetBundleVariant

```c#
       /// <summary>
        /// 设置单个AssetBundle的Name
        /// </summary>
        /// <param name="filePath"></param>
        static void SetABName(string assetPath)
        {
            string importerPath = "Assets" + assetPath.Substring(Application.dataPath.Length);  //这个路径必须是以Assets开始的路径
            AssetImporter assetImporter = AssetImporter.GetAtPath(importerPath);  //得到Asset
            string tempName = assetPath.Substring(assetPath.LastIndexOf(@"/") + 1);
            string assetName = tempName.Remove(tempName.LastIndexOf(".")); //获取asset的文件名称
            assetImporter.assetBundleName = assetName;    //最终设置assetBundleName
            assetImporter.assetBundleVariant = ".bundle";
        }
```

**unity的c#脚本可以先打包成dll文件，在将dll打包成Assetbundle去加载，android可以直接通过动态加载dll去运行逻辑，但是苹果禁止ios下的程序热更新，JIT 在ios下无效，所有需要用lua方案去做一些需要热更的逻辑，又或者一些热更库如ilruntime**



##### Unity打包策略（个人理解）:

1.按照类型文件夹去打包，如Effect下可以分别打包Texture/Animation/Material/Model/Prefab/Shader，都可以打成一个AssetBundle

如Prefab.assetbundle,Texture.assetBundle.（为什么可以全都打包到一个assetbundle中，因为unity在读取上做了优化，使读取时不会将所有的数据都读取到内存中，但这个读取方式也跟assetbundle的压缩格式有关）

spine，场景，地图，ui，等资源，可以将相关的数据按照文件一个个的打包成对应的assetbundle，如一个spine动画是由材质，贴图，数据等组成，那么就可以将一个spine所用的对象全都打到一个assetbundle中使用。

2.文件压缩格式，unity assetbundle打包有三种压缩格式，LZMA, LZ4, 以及不压缩。LZMA代表完全压缩，LZ4 部分压缩，性能上完全压缩加载最慢，因为它需要在加载时把所有的数据都加载到内存中，LZ4 性能和不压缩差不多，因为 LZ4在加载时只会先加载头部数据，然后在用的时候需要什么资源加载对应的资源。所以一般使用LZ4格式，除非那些不需要特别关心加载时间但是又很大的资源时可以使用LZMA. 

**BuildAssetBundleOptions.None 是一种默认的压缩形式，这种标准压缩格式是一个单一LZMA流序列化数据文件，并且在使用前需要解压缩整个包体。LZMA压缩是比较流行的压缩格式，能使压缩后文件达到最小，但是解压相对缓慢，导致加载时需要较长的解压时间**

**BuildAssetBundleOptions.ChunkBasedCompression   Unity支持LZ4压缩，能使得压缩量更大，而且在使用资源包前不需要解压整个包体。LZ4压缩是一种“Chunk-based”算法，因此当对象从LZ4压缩包中加载时，只有这个对象的对应模块被解压即可，这速度更快，意味着不需要等待解压整个包体。LZ4压缩格式是在Unity5.3版本中开始引入的，之前的版本不可用**

**BuildAssetBundleOptions.UncompressedAssetBundle  不压缩的方式打包后包体会很大，，导致很占用空间，但是一旦下载Assetbundle，访问非常快。不推荐这种方式打包，因为现在的加载功能做的很友好了，完全可以用加载界面来进行后台加载资源，而且时间也不长**

3.导出文件夹，一般发布包内只会带有少量不需要热更的ab资源，放在streamingassts文件夹，会随包一同打出去。

**Unity特殊文件夹**

有哪些文件夹:Application.data / Application.StreamingAssets/Resources/Application.persistenDataPath/Application.temporaryCachePath

ApplicationData:只读，放一些资源文件 

Application.StreamingAssets:移动平台只读，资源不会被压缩，一般放一些二进制文件

```c#
#if UNITY_EDITOR
string filepath = Application.dataPath +"/StreamingAssets"+"/my.xml";
#elif UNITY_IPHONE
 string filepath = Application.dataPath +"/Raw"+"/my.xml";
#elif UNITY_ANDROID
 string filepath = "jar:file://" + Application.dataPath + "!/assets/"+"/my.xml;
#endif

```

Resources:只可以通过unity api 读取,资源会被压缩成二进制

```c#
Resources.Load("xx/xx");//path 按照resources文件夹下的路径结构
```

Application.persistenDataPath：持久化路径，可读可写，存放下载下来的资源包等文件

4.自定义数据文件，用于资源校验，如md5文件，版本号文件，其他依赖数据文件（根据项目需要）

### 2.资源更新

**资源更新流程**

##### 1.连接服务器，下载版本号数据文件，与本地版本号文件对比，如果版本号一致则不更新，不一致进入更新流程。

##### 2.进入更新流程后，遍历服务器资源，将新的资源或者md5不一致的资源加入更新队列。

###### 2.1.获取本地文件的大小（没有则创建），和请求服务器返回的数据流大小对比，如果不一致则从当前流位置进行重新请求下载，HttpWebRequest.Range(length),reqeust.GetResponse().GetResponeseStream(),如果本地该资源的md5还是不一致，那么需要重新加入队列下载（有可能这个文件是很旧很旧的但是没有下载完全）

##### 3.如果下载下的文件中包含需要重启游戏的key（一般热更的东西更新不需要重启，除非主工程用到的资源进行的更新），则进行重启游戏，如果不需要则直接进入游戏。

##### 4.将下载下来的资源写入到持久化路径下保存。

### 3.资源加载

**Unity API 支持几种加载方式，包含了同步异步的方法**

**加载assetbundle之前需要先加载所有ab的依赖信息文件，也就是{StreamingAssets}.manifest前面的名字不固定,可以加载自定义的依赖信息，其实最终目标只是想知道当前资源所依赖的所有依赖并将这些资源全部加载出来即可。**

```c#
//第一种同步
AssetBundle.LoadFromFile(path);
//第二种异步
AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
yield return request;
//网络异步下载
UnityWebRequest request1 = UnityWebRequestAssetBundle.GetAssetBundle(path);
yield return request1.SendWebRequest();
//资源加载 异步 
AssetBundleRequest q1 = request.assetBundle.LoadAssetAsync("lobby");
yield return q1;
//同步
var obj = request.assetBundle.LoadAsset("lobby");
```

### 4.资源管理

**AssetBundle.Unload(True),会将ab从场景移除，并销毁和删除，那么所有引用了这个包内的资源的对象引用都将缺失**

**AssetBundle.Unload(false),会将ab的包头信息卸载，将资源与对象之间的引用关系断开。**

### 方案：

1.在应用程序的生命周期内定义一个合适的节点，并在此期间卸载不需要的AssetBundle，例如在关卡切换或加载屏幕期间。这是最简单和最常见的选择。

2.、维护单个对象的引用计数，并仅当所有组成对象都未使用时才卸载AssetBundles。这允许应用程序在不重复内存的情况下卸载和重新加载单个对象。

**如果应用程序必须使用AssetBundle.Unload(False)，那么只能通过两种方式卸载各个对象：**

1、在场景和代码中消除对不需要的对象的所有引用。完成后，调用Resources.UnloadUnusedAssets。

2、非附加加载场景。这将销毁当前场景中的所有对象并调用Resources.UnloadUnusedAssets。

最简单的方法是将项目的离散块打包到场景中，然后将这些场景与所有依赖项一起构建到AssetBundles中。然后，应用程序可以切换到“loading”场景，从而完全卸载包含旧场景的AssetBundles，然后加载包含新场景的AssetBundles。

**ChunBasedCompression(LZ4压缩模式下)**

![image-20210705163820718](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705163820718.png)

![image-20210705163900554](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705163900554.png)

**None(完全压缩LZMA)**

![image-20210705164005414](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705164005414.png)

![image-20210705164030816](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705164030816.png)

**Uncompressed Asset Bundle**

![image-20210705164136292](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705164136292.png)

![image-20210705164205789](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Unity 热更新.assets/image-20210705164205789.png)

#### 总结：

##### 资源加载时间对比：LZMA(10ms) < LZ4(13ms) < 不压缩(24ms)，LZMA 与 LZ4 加载时间相差不大，与不压缩加载时间相差有2倍所有。

**AB包加载时间对比：LZ4(0.48ms)  < LZMA (2.23ms) < 不压缩（3.52ms）,包加载速度是数量级的减少** 

所以综合考虑，可以使用LZ4压缩模式，这样包即得到了压缩，包也加载最快，资源也可以快速加载。

