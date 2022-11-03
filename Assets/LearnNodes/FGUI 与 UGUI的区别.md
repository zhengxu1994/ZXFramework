## FGUI 与 UGUI的区别

- ### 资源打包

  **FGUI**

  FGUI里以包package为资源单位，发布时会将包里所有设置为导出资源和组件，以及组件依赖的组件和资源进行导出。

  FGUI发布时可以设置包名称，以及发布路径，图集的最大尺寸，是否分离alpha通道（为什么分离原因是为了可以在unity里将原纹理设置为不支持alpha通道的格式（比如etc1）以减少内存占用）

  * alpha 分离的好处 ：兼容旧机型，将rgba 分离成2张rgb的贴图，这样在使用etc1（不支持带alpha通道的）和pvrtc压缩时就可以完美支持（fgui的shader对两张图进行了合并），有一些只支持ect1压缩格式的机器也能跑起来。
  * alpha 分离的缺点 ：很明显一张图便成了两张图，资源所占空间变大了，占用的内存也变大了，原来只需要加载一张图，现在要加载两张。

  #### Tip: Unity支持的几种纹理压缩格式

  | 压缩格式                                                     | 压缩比例 | 是否支持alpha通道                                |                                                              |
  | ------------------------------------------------------------ | -------- | ------------------------------------------------ | ------------------------------------------------------------ |
  | RGB ETC1 : 4bit/pixel                                        | 8:1      | 否                                               | ![image-20210807141716213](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807141716213.png)![image-20210807141809334](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807141809334.png) |
  | RGB ETC2:  4bit/pixel                                        | 8:1      | 否                                               | ![image-20210807141846544](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807141846544.png)![image-20210807141906449](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807141906449.png) |
  | RGB ETC2:  8bit/pixel                                        | 4:1      | 是                                               | ![image-20210807142008242](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142008242.png)![image-20210807141950296](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807141950296.png) |
  | RGB ETC2:1bit alpha 4bit RGB                                 | 8:1      | 是，支持简单的透明与不透明，更复杂的可以使用ASTC | ![image-20210807142111629](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142111629.png)![image-20210807142130257](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142130257.png) |
  | ASTC (iphone6以上基本都支持，在安卓上兼容性没有ect好)        | 4:1      | 是，可以做一些更复杂的功能                       | ![image-20210807142221862](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142221862.png)（4x4个像素一起压缩)也有更大8x8 12x12 但是肯定会有质量损失。 |
  | PVRTC4(iphone上使用，带有透明通道的图片压缩后会图片质量会降低如右图出现了锯齿) | 8:1      | 是                                               | ![image-20210807142542725](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142542725.png)![image-20210807142521069](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210807142521069.png) |

  

- ### 自适应

  * 屏幕缩放 

    | FGUI Render Mode。 FGUI 与 UGUI 一样                         |
    | ------------------------------------------------------------ |
    | 1.Screen Space Overlay(画布会被缩放适应屏幕，画布会被直接渲染不参与场景和相机的融合，ui永远绘制在最上层) 2.Screen Space Camera(画布会在给定相机（stage camera）前面一定距离的平面对象上绘制，一般为近裁剪平面，ui的屏幕尺寸不会随距离变化)                                                                                                                                                                 3.World Space（不需要相机，画布和世界对象一起渲染，可以旋转，移动，会和其他世界对象进行穿透 遮挡） |
    | UGUI Render Mode                                             |
    | 1.Screen Space Overlay(画布会被缩放适应屏幕，画布会被直接渲染不参与场景和相机的融合，ui永远绘制在最上层) 2.Screen Space Camera(画布会在给定相机（一般设置一个camera 或者使用main camera）前面一定距离的平面对象上绘制，一般为近裁剪平面，ui的屏幕尺寸不会随距离变化)                                                                                                                                                                 3.World Space（不需要相机，画布和世界对象一起渲染，可以旋转，移动，会和其他世界对象进行穿透 遮挡） |

    | ScaleMode | FGUI与UGUI一样                                               |                        |                                                              |
    | --------- | ------------------------------------------------------------ | ---------------------- | ------------------------------------------------------------ |
    | FGUI      | Constant Pixel Size                                          | Scale With Screen Size | Constant Physics Size                                        |
    |           | 固定像素大小显示（无论屏幕大小如何，UI 元素都保持相同的像素大小。） | 按照屏幕尺寸缩放显示   | 固定物理尺寸显示（使 UI 元素无论屏幕大小和分辨率如何都保持相同的物理大小） |
    | UGUI      | Constant Pixel Size                                          | Scale With Screen Size | Constant Physics Size                                        |
    |           | 固定像素大小显示（无论屏幕大小如何，UI 元素都保持相同的像素大小。） | 按照屏幕尺寸缩放显示   | 固定物理尺寸显示（使 UI 元素无论屏幕大小和分辨率如何都保持相同的物理大小） |

    | Screen Match Mode |                                                              |                                                              |                                                              |
    | ----------------- | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
    | FGUI              | Match Width or Height                                        | Match Width                                                  | Match Height                                                 |
    |                   | 取设计分辨率与设备分辨率宽比和高比中的小的比例作为画布整体的缩放比例 | 按照宽度比缩放（竖屏）                                       | 按照高度比缩放（横屏）                                       |
    | UGUI              | Match Width or Height                                        | Expand                                                       | Shrink                                                       |
    |                   | 0～1之间，如果是0则完全按照宽度比取缩放，如果是1则完全按照高度比缩放，如果在0-1之间那么就是比例在宽高比的大小之间。 | 此模式下会保证设计分辨率下的东西能够全部显示出来，及选择设备分辨率和设计分辨率的宽、高比中选择最小值作为缩放因子 | 此模式下会保证设计分辨率下的东西能够全部显示出来，及选择设备分辨率和设计分辨率的宽、高比中选择最大值作为缩放因子 |

    

  * 锚点关联

    都带有的关联：上 下 左 右 左右居中 水平居中 宽 高 比例缩放

    fgui独有的关联：上 下 左 右 之间的相互延展，意思就例如：![img](https://www.fairygui.com/docs_images/gaollg20.gif)

    **当白色方块向左移动时，绿色方块的左侧跟随白色方块移动，但绿色方块的右侧保持不动，效果就是绿色方块被拉长了。**

    UGUI只支持子UI与父UI之间的关联，FGUI支持任意两个UI之间的关联

  * 坐标系统

    UGUI以左下角为原点构建坐标系，FGUI已左上角为原点构建坐标系

    ```c#
      //Unity的屏幕坐标系，以左下角为原点
        Vector2 pos = Input.mousePosition;
    
        //转换为FairyGUI的屏幕坐标
        pos.y = Screen.height - pos.y;
    
        
    ```

    

- ### CPU优化

     批次合并

  - UGUI合并批次

    - 1.材质相同如果这些ui中间没有穿插（z轴不一样或者存在遮挡关系）使用的其他材质的ui那么unity就可以使用一个drawcall去渲染：例如：ABCDE顺序在hierarch面板中显示，C的材质与A B D E 不同，当他们的z一样或者 c没有与其他四个UI有遮挡关系时ABDE会合并 C单独渲染。如果C的z轴不一样那么则AB会一次 c会一次 DE会一次，破坏了合并顺序。

    - 2.使用同一个遮照的ui会合并（满足条件1）无法与未使用遮照的ui合并

    - ugui的文字渲染优先级高于图片，所以尽量不要把图片遮挡在文字上，这样会打断合并。

      | 文本与图片未遮挡 | ![image-20210808175149198](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210808175149198.png) |
      | ---------------- | ------------------------------------------------------------ |
      | 图片与图片遮挡   | ![image-20210808175247655](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210808175247655.png) |
      | 图片与文字遮挡   | ![image-20210808175345003](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/FGUI 与 UGUI的区别.assets/image-20210808175345003.png) |

      

  - FGUI合并批次

    - 1.package在导出时会自动将用的图片资源合并到一个图集中
    - 2.开启fairybatching 开关后，如果相同材质的ui并且它和其他不同材质的ui没有遮挡关系，那么fgui会将这些ui设置到同一渲染队列中，以达到合并的效果。

​          资源加载

​          Ugui：可以通过ab包或者resources等加载api加载ui  prefab并实例化。

​          Fgui：需要先通过ab或者resouces等加载fgui 对应的ui包资源，然后通过fgui api去load对应的ui并将它添加到画布或者其他ui上。

​         不管ugui还是fgui，可以将一些通用的资源比如常用icon小的贴图放到一个commom图集中，在内存中常驻，将一些大图比如背景人物原画等放到一个图集中（不够的话就多个图集），在开始的时候加载，尽量避免反复加载。其他界面的ui资源不使用时可以加它卸载掉。

​        

- ### GPU优化

  - UI重绘
    - 1.由于大量的半透明材质导致的重绘：多层叠加的UI中尽量不要使用半透明贴图（除非效果需要），因为使用了半透明那么就无法开启对象剔除。可以使用unity的scene中的overdraw界面查看overdraw情况，约红的代表重绘次数越多。
    - 2.由于UI属性发生了改变如UI位移界面需要重新布局绘制，将需要经常移动的ui放到一个canvas下，可以移动的部分只会重新绘制当前canvas的UI。FGUI也是将这些UI放在一个package中，通过package去加载这一个单独的界面。

  

  

