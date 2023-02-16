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

