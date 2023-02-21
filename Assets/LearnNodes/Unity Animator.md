###                                                       Unity 动画系统

**Unity 动画系统可以分为四个组成部分:**

- 动画片段 AnimationClip 对物体变化的一种展示
- 动画状态机 Animator 跟踪当前动画的播放状态并根据设置来决定何时以及如何切换动画片段
- 动画组件 Animator Component 用于代码访问控制动画数据
- 人形动画  替身 人物动画的复用



### AniationClip

可以使用Animation 编辑器去对一个游戏对象编辑动画 或者 从外部导入一个动画对象。

![image-20230216143432091](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216143432091.png)

动画复用，核心原理就是通过在生成的动画配置文件中找到对应的对象并根据数据播放相应的动画化，如果没有找到对应的对象那么播放就会播放失败，而人形动画的复用与这个思路一致，只要所有的骨骼名称一致那么即可复用。但是手动修改名称太繁琐所以unity使用了替身Avatar（大致就是将A的骨骼信息转换成unity的骨骼信息，B通过读取unity的骨骼信息而不是直接读A的信息， 适配器模式），如下面的文本中Path就是记录的对象信息。

```yaml
%不是全部数据
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: AnimationTest
   serializedVersion: 7
  m_Legacy: 1
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves:
  - curve:
      serializedVersion: 2
      m_Curve:
      - serializedVersion: 3
        time: 0
        value: {x: 0, y: 0, z: 0}
        inSlope: {x: 0, y: 0, z: 0}
        outSlope: {x: 0, y: 0, z: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}
        outWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}
      - serializedVersion: 3
        time: 1
        value: {x: 180, y: 180, z: 180}
        inSlope: {x: 0, y: 0, z: 0}
        outSlope: {x: 0, y: 0, z: 0}
        tangentMode: 0
        weightedMode: 0
        inWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}
        outWeight: {x: 0.33333334, y: 0.33333334, z: 0.33333334}
      m_PreInfinity: 2
      m_PostInfinity: 2
      m_RotationOrder: 4
    path: Anim2 
```

#### AnimationType

- None 没有动画
- Legacy旧版动画 基本不用了
- Generic 通过的动画 除人形动画外
- Humanoid 人形动画

#### Avatar definiton 

- Create From this model 从当前对象上读取avatar信息
- Copy from Other avatar 从其他对象上去的avatar信息

### Animator 组件

- Controller 动画状态机 组织管理动画的工具
- Avatar 替身
- Apply Root Motion 是否允许动画进行位移
- UpdateMode  动画刷新模式-重新计算每个骨骼节点的位置，转向和缩放的数值
  - Normal 和帧率同步 也就是update
  - Animate Physic 和物理同步 也就是fixupdate
  - unscaled time 不受unity的timescale 影响 上面2个都受影响
- Culling Mode 剔除模式 在相机外的对象是否进行计算
  - Always animation 不剔除
  - Cull update transforms 不剔除 但是会停止ik（方向动力学）的计算
  - Cull compeltely 剔除 

### Animation Controller 动画状态机

**Des：控制多个动画文件播放和切换的工具**

![image-20230216164601248](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216164601248.png)

- Layers 组合动画，比如我想让上身播放持枪动作 下身播放行走动作 就可以使用layers去组合
- Parameters 控制动画状态的参数 参数类型有Float Int Bool Trigger

### Avatar 设置

![image-20230216170005219](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216170005219.png)![image-20230216170147146](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216170147146.png)

一个人形骨骼至少需要15个骨骼节点，这里unity已经自动设置好了，如果出现鬼畜效果则需要在这个面板内调整。

Mapping 设置骨骼信息 ，Muscies&setting 设置肌肉拉伸等信息。

#### Animation State 动画状态，动画状态分为三种

![image-20230216164916393](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216164916393.png)

- 单独的动画片段
- 多个动画片段组成的混合树 Bleed Tree
- 其他一个状态机

![image-20230216170459769](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216170459769.png)

上面图内就是Animation State的设置界面

- Tag 标签 ，可以给不同的状态做分类方便管理，比如在播放Tag为TagTest标签的动画时 打印一句话 然后返回

  ```c#
  using Bepop.Core;
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  public class AnimatorStateTest : MonoBehaviour
  {
      private Animator animator;
      private AnimatorStateInfo stateInfo;
      // Start is called before the first frame update
      void Start()
      {
          animator = GetComponent<Animator>();
      }
  
      // Update is called once per frame
      void Update()
      {
          if(Input.GetKeyDown(KeyCode.Space))
          {
              //获取名称为base layer的层级 并通过层级去获取animation state info
              stateInfo = animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer"));
              if(stateInfo.IsTag("TagTest"))
              {
                  Log.BASE.LogInfo("Tag is Trigger");
                  return;
              }
          }
      }
  }
  ```

- Motion 当前state管理的动画片段

- Speed 动画的播放速度 负值为倒放

- Multiplier  可以关联一个Parameters 动画控制参数然后通过代码获取设置参数大小 因为speed是无法直接访问设置的  如果开启Multiplier  那么最终speed = speed * Multiplier  ，通过   animator.SetFloat("PlaySpeed", speed); //PlaySpeed为参数名称 speed为设置的值

  ![image-20230216171843072](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216171843072.png)![image-20230216171852033](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216171852033.png)

- Motion Time 动画播放的时间点 从0-1 也可以像Multiplier  一样管理一个浮点型变量

- Mirror 是否镜像

- Cycle Offset 动画偏移 ，就是一开始从动画片段的哪个部分播放 与Motion Time不同 cycle offset 并不是切割 它会把所有动画播放完毕

- Foot IK IK动画矫正机制 unity会有几个默认的ik root位置，如果发现人物关节运动异常可以使用代码将关节像对应的ik root靠拢， 如果需要使用ik 那么需要将layer里面的ik pass 开启![image-20230216173134710](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20230216173134710.png)

  ```c#
      public float rightRootWeight;
  
      private void OnAnimatorIK(int layerIndex)
      {
          //将右脚像0，0，0的位置靠拢
          animator.SetIKPosition(AvatarIKGoal.RightFoot, Vector3.zero);
          //设置权重 1为做大 
          animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightRootWeight);
      }
  ```

- Write Default 需要注意!!!  当前属性与记录属性进行比对，当前属性与记录属性不一致时将当前属性设置为记录属性。

  https://gwb.tencent.com/community/detail/123970 具体解决方案

  - 记录属性是在object onenable方法内设置 所以但对象被setactive true后就会重新设置一次值

  - 存在问题：如果当前动画状态内属性并不需要被修改时，又打开了write default 那么但属性不一致时将会被修改，这时需要检查是否需要打开此功能。

    ```c#
    * animator.Crossfade("DefaultPose", 0f); //Force switch to Default Pose State, instant transition
    * animator.Update(0f); //force transition completion, your object should now be in default pose
    * go.SetActive(false); //disabled in default pose
    ```
  
  **Animator Transition** 动画过渡
  
  ![image-20230216190242910](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230216190242910.png)![image-20230216190326864](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230216190326864.png)![image-20230216190444892](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230216190444892.png)
  
  这里的Transitions可以有多个，多个Transition执行的优先级为：solo-->条件先达到--->Transitions中的排序先后 第一个执行。
  
  **勾选了Mute的转换永远不会被执行**。
  
  面板参数：
  
  - Has Exit Time 默认被勾选 当前动画状态播放到一个时间点时才会执行这个转换，不勾选那么动画会立即跳转。
  - exit time 动画开始转换的时间 比例值 
  - fixed during 转换的持续时间是用秒还是用当前动画状态的百分比
  - transtion offset 进入下一个动画的偏移量 0代表从下一个状态的开始进行播放 0.5代表从下一个状态的中间点进行播放 
  - interruption source 有哪些转换可以打断当前这个转换
  
  **Conditions 过渡条件**
  
  过渡条件有四个类型是从上面的parameters中获取
  
  通过animation.SetFloat("name","value") 等等方法
  
  **Interruption Source 动画状态过渡中断/转换打断**
  
  *ordered interruption 是否按照过渡的顺序执行，如果a可以到b c d ，那么只有顺序中的第一位可以被执行 a-c ，a-d就无法执行，可以手动调整顺序来控制*
  
  下面的条件是已a为current state处理
  
  - current state 当a-b时 可以被a-c状态打断 直接过去到c
  - next state a - b的转换可以由全部从b出发的转换打断，比如b-c b-d  注意是全部 但是当转换条件都满足时 b-c b-d还是根据执行顺序来判断优先执行
  - current state than next state ,无论是从a出发 还是 b出发的转换都可以打断转换，但是a出发的优先级高于b
  - next state than current state 无论是从a出发 还是 b出发的转换都可以打断转换，但是b出发的优先级高于c

### Bleed Tree的使用

右键状态机空白部分，Create State - From New Bleed Tree,双击打开bleed tree 设置面板。

![image-20230217134639612](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217134639612.png)![image-20230217134712953](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217134712953.png)![image-20230217144039813](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217144039813.png)![image-20230217134735165](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217134735165.png)

可以给bleed tree 添加motion状态。

**BleedType**

- 1D 只有一个变量控制转换
- 2D Simple Directional
- 2D Freeform Directional
- 2D Freeform Cartesian
- Direct

**Automate Thresholds 是否自动设置动画权重**

### Apply  Root Motion

apply root motion 开启与未开启的区别：

- 未开启 每帧会根据动画配置中的数据去读取对应的位置以及旋转 并做差值运算赋值，所以当动画回到第一帧时对象的状态也会回到第一帧
- 开启后 unity会通过动画文件里记录的绝对坐标和绝对方向以及当前游戏对象的缩放比例计算出游戏对象在上一帧的相对位移和相对转角，然后在根据相对位移相对转角来操作对象。
- 总结：**动画位移会直接修改每一帧里游戏对象的坐标值和角度，而root motion则通过相对位移和转角来移动游戏对象。比如初始位置（0，0）一共10帧， 每次位移（1，1）。 未开启在回到0帧时会回到（0，0），而开启后会在（10，10）的位置继续前进**

**Apply Root motion的控制可以通过代码去操作**

![image-20230217151721186](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217151721186.png)

```c#
    public void OnAnimatorMove()
    {
        transform.position += animator.deltaPosition;//一样能达到开启后位移的效果
    }
```



### Generic 类型的模型也可以创建自定的avatar骨骼并运动root motion

#### Des: Root Motion在Generic 动画中指的就是将角色的根骨骼的运动应用到游戏对象上。



### Tip:在使用apply root motion 去控制带有位移动画的游戏对象时可能出现一种情况，那就是对象在动画最后一帧时又卡回到了远点，导致这个结果的原因是在这个动画设置中勾选了Bake Into Pose 看下图，不勾选就解决了这个问题，核心思路就是我想让游戏对象受动画的控制。

![image-20230217163805048](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230217163805048.png)

-  Bake into Pose ，是否需要动画来驱动游戏对象旋转 ， 移动 xz y。
- Based Upon 游戏对象在动画开始时对准根骨骼节点的指向方向，开启代表不需要
- Offset 当方向不准确可以调整offset来修改

#### Humhanoid 下 Root Motion的使用

**Des:Generic模式下Root motion是通过把动画文件描述中的根骨骼坐标值和角度值转换为相对应的位移和相对转角，并以此来移动对象，而Humhanoid由于使用avatar系统动画文件不在包含具体的骨骼描述，自然无法通过指定根骨骼来解决这个问题，unity通过分析humhanoid动画中的骨骼结构计算出模型的重心center of mass如下图1可以看到人物的重心,也可以通过代码去访问到重心坐标和方向，最终将这个重心的位置当作根骨骼的节点来对待。**

![image-20230218133053311](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230218133053311.png)

```c#
var pos = animator.bodyPosition;
var rotation = animator.bodyRotation;
```

#### 在Bleed Tree 下使用root motion

![image-20230218134337839](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230218134337839.png)![image-20230218134720194](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230218134720194.png)

compute thresholds:计算阈值的方式

- Speed 取root motion的绝对值
- Velocity X. Root motion x上的速度
- Velocity Y root motion y
- Velocity Z root motion z
- Angular Speed(rad) root mition的旋转速度 弧度每秒
- Angular Speed(Deg)。角度每秒

当我选择Velocity Z 时可以看到计算出来的阈值，那么将速度设置到这个值时理论上root motion就会已对应的速度前进和后退。

adjust time scale 计算让所有方向上的速度一致

homegeneous speed 自动计算。

#### 不同角色使用同一个avatar下的root motion 速度不一致问题解决方案：

导致不同的速度的根本原因是不同的模型在unity humanoid中的scale是不一样的，可以通过animator.humanSacle获取。

可以通过让动画播放的速度 = 播放速度 / humanscale 就可以让不同的模型保持同样的速度，但是我又不想让我的所有动画速度都改变那么可以通过动画状态设置面板中的Multiplier 属性去修改即可，开启选项并通过代码animator.SetFloat("name",speed/humanScale);

.![image-20230219131337436](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230219131337436.png)

#### 自定义控制人物动画移动速度

- 给人物添加rigdbody，锁定x 和z轴转动防止人物移动出错，调整collider 大小，注意通过要设置animator的update mode为animator physic  物理同步 因为使用到了ridybody。
- 添加脚本，复写OnAnimatorMove方法将ridibody的velocity设置为root motion的velocity
- 这里会遇到一个问题 人物的重力好像没有作用，是因为动画配置中的bake into pose没有勾选上，勾选上后就会让动画位移旋转不作用到对象上，这样就可以由重力控制。
- 通过又遇到一个问题 在复写了OnAnimatorMove方法后人物的下落速度很慢，原因是人物的重力加速是如每帧0.02秒 重力是9.8，那么每帧的加速是0.125，但是我们在方法内通过设置velocity的方法把下落速度设置为0了，解决方法就是我们只读取root motion的xz值y值使用ridybody的值即可。



![image-20230219134138790](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230219134138790.png)

### 角色控制器

为角色添加animator rigdbody collider ，通过input 传入的值去控制角色的移动和转向,可以通过添加cinemachine控制角色相机。

```c#
playerMovement.x = inputVector.x;
playerMovement.y = inputVector.z;

Quaternion targetQ = Quaternion.LookRotation(playerMovement,Vector.Up);
transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRotaion,0.5f);
```

####  角色动画分层

![image-20230220153756277](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230220153756277.png)![image-20230220154203594](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230220154203594.png)

比如可以控制手臂和身体播放两种动画，

- Weight 权重 0代表不起作用

- Avatar mask 骨骼蒙版 让身体的哪些骨骼受这个层级的动画控制,可以在资源面板内创建mask对象。如上图 绿色的代表受控制 红色不受控制

- blending 有两种 override 用当前层级的动画取代上面层级的动画 和 add 和上面的动画混合 比如可以处理人物疲劳效果

- sync 同步选择与哪个层级保持一致  ，当a与b保持一致时当b的动画状机发生改变后a也会自动改变,注意保持的只是状态机状态和转换关系，具体状态的动画和bleed tree是不同步的。![image-20230220162746410](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230220162746410.png)![image-20230220162953998](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/LearnNodes/Unity Animator.assets/image-20230220162953998.png)

- ik pass 是否开启ik动画 

  ```c#
  //获取动画层级
  var index = animator.GetLayerIndex("layer name");
  //设置层级权重
  animator.SetLayerWeight(index,num);
  ```

  

#### 使用Animation Rigging 插件来控制ik动画

