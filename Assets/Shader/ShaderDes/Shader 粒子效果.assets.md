### Shader 基础知识篇

**渲染管线流程**

- Application cpu将模型的 纹理 材质 transform 等信息存到gpu中

- 模型空间   建模时的自身空间坐标
- 世界空间  导入到unity中 在世界坐标系下的信息（旋转/移动）
- 视图空间（相机空间） 物体在相机视野内的坐标
- 裁剪空间 将相机视野以外的顶点全部才剪掉
- 屏幕空间 将3d下的坐标转换为屏幕上的2d坐标 并归一化
- 经过顶点着色器 返回顶点信息 包含顶点 法线 坐标等信息
- 光栅化 将三角形进行光栅化 将颜色值进行差值 得到flagment
- 片元着色器 flagment shader 计算得到的flagment 返回像素点颜色
- blend 混合阶段
  - flagment是否被其他物体挡住 如果被挡住则会被剔除不被渲染
  - 深度测试  深度 指距离相机的距离，离相机越远 深度越大 ，ZTest Less | Greater | LEqual | GEqual | Equal | NotEqual | Always。 如果被深度小的对象给挡住（并且这个对象不透明）那么深度大的对象就会被舍弃
  - 模版测试  板缓冲区可用作一般目的的每像素遮罩，以便保存或丢弃像素  满足模版条件后才会被渲染
  - alpha测试 满足设置的alpha值条件后才会被渲染
  - 最终根据混合模式得到最终的颜色值 设置的模式有 add 和 mult乘 。