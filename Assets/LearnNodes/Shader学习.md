### Shader学习

#### 1.CD技能图标实现

  API:

- Atan2 通过x y计算出夹角角度（-pi ～ pi）
- SubStract 减去一个指定值
- Break To Components 将输入数据分解成单独的组件
- Length 获取一个向量的长度
-  Multiply 乘
- One minus 用1去减一个值，一般用于取反
-  Remap 重新映射 将一个范围 映射到一个新的范围 
- SmoothStep 插值[min,max]将小于min值的设置为0，将大于max的值设置为1
- Clip裁剪，alpha值大于阈值的值裁剪掉、
- Append 值混合 （v2 v3 v4 color）
- Template Param 当前shader自带的变量
- Texture Coordinates  纹理坐标
- Vertex TexCoord 顶点颜色
- Lerp 插值计算
- Saturate 防止溢出 强制将值控制在（0-1）之间
- Time Parametes 时间参数
- GrabPass  存储在GrabTexture内 获取屏幕颜色 作为一张贴图

实现步骤：

- Texture Coordinates获取图片uv （-1-1）

- subtract  减去（0.5,0.5）得到0.5，0.5

- Break To Components （将输入数据分解成单独的组件）得到x=0.5 y=0.5

- Multiply 将y值乘以-1 得到0.5,-0.5

- Atan2(x,y)得到角度

- Remap重新映射新的值，将（-pi-pi）映射成（0-1）

- One Minus 加 一个（0-1）的progress 得到一个取反的（1-0）值

- SubStact 使 Remap得到的值减去 One Minus得到的值

- SmoothStep插值[min,max]，小于min的为0，大于max的为1，即非黑即白

- Remap 将（0-1）映射成（0.3-1）不要非黑，变为0.3透明

- 制作一个圆形遮照步骤，在上面的第二步对（0.5，0.5）使用length，multiply*2，one minus取反，在smoothstep[0,0.05]得到一个非黑即白的圆形，在clip一下（0.5）让值大于0.5的部分被剔除。

- 将图片纹理MainTex与clip得到的数据相乘，在与Remap得到的值相乘进行输出。

  生成一个圆

![image-20211102160837773](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211102160837773.png)

创建一个uv顶点波动效果

![image-20211104144207822](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211104144207822.png)

创建一个sin波动效果，传入顶点uv信息

![image-20211104144327699](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211104144327699.png)

添加一个遮照，以及复用遮照

![image-20211104144449099](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211104144449099.png)![image-20211104144457048](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211104144457048.png)

实现一个扭曲效果

核心效果实现：

- GrabScreenPosition 将屏幕颜色作为一张贴图来使用
- 使用法线贴图的rg两个通道作为扭曲的偏移

![image-20211105153818893](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211105153818893.png)

![image-20211105154026916](/Users/zhengzhengxu/Desktop/zhengxu/Unity3d/Tool/LearnNote/Shader学习.assets/image-20211105154026916.png)

