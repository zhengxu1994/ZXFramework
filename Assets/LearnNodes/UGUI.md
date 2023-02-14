# UGUI

## 组件

- ### Canvas

  Canvas有三种渲染模式

  - Screen Space - Overlay 屏幕空间-覆盖  

    在这种模式下，Canvas 被缩放以适合屏幕，然后直接渲染而不参考场景或相机（即使场景中根本没有相机也会渲染 UI）。如果屏幕的大小或分辨率发生变化，则 UI 将自动重新缩放以适应。UI 将绘制在任何其他图形（例如相机视图）之上。

    总结：ui永远渲染在最上层 没有前后的观念，canvas之间的排序通过sort order去排序。

  - Screen Space - Camera 屏幕空间-相机 

    在这种模式下，Canvas 的渲染就像是在给定相机前面一定距离的平面对象上绘制的一样。UI 的屏幕尺寸不随距离变化，因为它总是重新缩放以完全适合[相机视锥体](FrustumSizeAtDistance.html)。如果屏幕的大小或分辨率或相机视锥体发生变化，则 UI 将自动重新缩放以适应。任何**3D对象**
    在场景中，比 UI 平面更靠近相机的场景将渲染在 UI 的前面，而平面后面的对象将被遮挡。

    总结：ui通过相机渲染，ui与3d对象通过z轴进行排序渲染（可以通过plane distance去设置距离相机的距离1：100）， （canvas之间通过sortinglayer 以及 order in layer 去进行排序（层级相同时根据plane distance去排序）

  - World Space 世界空间  此模式将 UI 渲染为场景中的平面对象。*然而，与Screen Space - Camera*模式不同的是，飞机不需要面向相机并且可以按照您喜欢的方式进行定向。Canvas 的大小可以使用其 Rect Transform 进行设置，但其屏幕大小将取决于相机的视角和距离。其他场景对象可以从 Canvas 后面、穿过或前面经过。

    总结：ui可以当作3d对象去显示，排序与3d对象一致。

- ### Canvas Scale ui缩放工具

  - UI Scale Mode

    - Constant Pixel Size 恒定像素大小 无论屏幕大小如何，都使 UI 元素保持相同的像素大小。

    - Scale WIth Screen Size 随屏幕尺寸缩放  根据设计分辨率与屏幕大小比例进行缩放

      - 有三种屏幕匹配模式

        - Match Width or Height 使用宽度作为参考、高度作为参考或介于两者之间的值来缩放画布区域

        - expend  总结将  宽高比取小的进行扩大，这样能保证所有的ui都在屏幕内

          举例来说：Reference Resolution为1280X720，Screen Size为800X600

          ScaleFactor Width： 800/1280=0.625

          ScaleFactor Height：600/720=0.83333

          套用ScaleFactor公式：Canvas Size = Screen Size / Scale Factor

          Canvas Width：800 / 0.625 = 1280

          Canvas Height：600 / 0.625 = 960

          Canvas Size 为 1280*960，高度从720变成了960，最大程度的放大(显示所有元素)

        - Shrink   总结将  宽高比取大的进行收缩。

          举例来说，Reference Resolution为1280X720，Screen Size为800X600

          ScaleFactor Width： 800/1280=0.625

          ScaleFactor Height：600/720=0.83333

          套用ScaleFactor公式：Canvas Size = Screen Size / Scale Factor

          Canvas Width：800 / 0.83333 = 960

          Canvas Height：600 / 0.83333 = 720

          Canvas Size 为 960*720，宽度从1280变成了960，最大程度的缩小

          ```c#
          Vector2 screenSize = new Vector2(Screen.width, Screen.height);
          
          float scaleFactor = 0;
          switch (m_ScreenMatchMode)
          {
              case ScreenMatchMode.MatchWidthOrHeight:
              {
                  // We take the log of the relative width and height before taking the average.
                  // Then we transform it back in the original space.
                  // the reason to transform in and out of logarithmic space is to have better behavior.
                  // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                  // In normal space the average would be (0.5 + 2) / 2 = 1.25
                  // In logarithmic space the average is (-1 + 1) / 2 = 0
                  float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
                  float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
                  float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                  scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                  break;
              }
              case ScreenMatchMode.Expand:
              {
                  scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                  break;
              }
              case ScreenMatchMode.Shrink:
              {
                  scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                  break;
              }
          }
          ```

- Graphic Raycaster 用于管理ui的事件触发，主要使用了射线。一般用默认选项即可。

  - Ignore Reversed Graphic 是否应该考虑远离光线投射器的图形
  - Blocking Objects 将阻止图形光线投射的对象类型。
  - Blocking Mask 将阻止图形光线投射的对象类型。