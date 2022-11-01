using System;
using Sirenix.OdinInspector;
using Skill.UseSkillCondition;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(fileName = "技能配置",menuName = "配置/技能配置")]
    public class SkillData : ScriptableObject
    {
        [Title("技能名称")]
        public string Name;
        public int ID;
        [Title("基类名称")]
        public string BaseClass;

        /// <summary>
        /// 技能使用条件检测
        /// </summary>
        [Title("技能使用条件检测")]
        public CheckUseSkillConditionType[] Skill_UseCondition;
        /// <summary>
        /// 技能行为
        /// </summary>
        [Title("技能行为")]
        public int[] SkillBehavior;

        [Title("目标类型")]
        public Skill_ChooseTargetType Skill_ChooseTargetType;

        [Title("选择范围类型")]
        public Skill_ChooseTargetArea Skill_ChooseTargetArea;

        /// <summary>
        /// 技能筛选阵营类型
        /// </summary>
        [Title("技能筛选阵营类型")]
        public Skill_ChooseTargetTream[] Skill_ChooseTargetTreams;

        /// <summary>
        /// 技能排除种类类型
        /// </summary>
        [Title("技能排除种类类型")]
        public Skill_ChooseTargetExcludeTypes[] Skill_ChooseTargetExcludesTypes;

        /// <summary>
        /// 状态排除类型
        /// </summary>
        [Title("状态排除类型")]
        public Skill_ChooseTargetExcludeFlags[] Skill_ChooseTargetExcludesFlags;

        public int maxLevel;

        /// <summary>
        /// 施法距离
        /// </summary>
        [Title("施法距离")]
        public int SkillRange;

        /// <summary>
        /// 冷却时间
        /// </summary>
        [Title("冷却时间")]
        public int SkillCoolDown;

        /// <summary>
        /// 耗蓝
        /// </summary>
        [Title("耗蓝")]
        public int SkillCostValue;
        [Title("是否是被动")]
        public bool IsPassive;
        [Title("icon是否隐藏")]
        public bool IsHidden;

        [Title("抬手效果配置")]
        public ModifiersData Stand_Modifiers;
        [Title("吟唱效果配置")]
        public ModifiersData Sing_Modifiers;
        [Title("收招效果配置")]
        public ModifiersData End_Modifiers;

        public void TestSkill()
        {
            Name = "TestSkill";
            ID = 1;
            BaseClass = "SkillBase";
            Skill_UseCondition = new CheckUseSkillConditionType[] { CheckUseSkillConditionType.CD, CheckUseSkillConditionType.Magic };
            Skill_ChooseTargetType = Skill_ChooseTargetType.ATTACKER;
            Skill_ChooseTargetArea = Skill_ChooseTargetArea.CENTER;
            Skill_ChooseTargetTreams = new Skill_ChooseTargetTream[] { Skill_ChooseTargetTream.UNIT_TARGET_TEAM_ENEMY };
            Skill_ChooseTargetExcludesTypes = new Skill_ChooseTargetExcludeTypes[] { Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL };
            Skill_ChooseTargetExcludesFlags = new Skill_ChooseTargetExcludeFlags[] { Skill_ChooseTargetExcludeFlags.UNIT_TARGET_FLAG_DEAD };
            maxLevel = 1;
            SkillRange = 100;
            SkillCoolDown = 10;
            SkillCostValue = 100;

            Stand_Modifiers = new ModifiersData();
            Stand_Modifiers.Test();
        }
    }

    /// <summary>
    /// buff/debuff数据
    /// </summary>
    [Serializable]
    public class ModifiersData
    {
        [Title("效果名称")]
        public string Name;

        [Title("特效信息")]
        public ModifiersParticleData effectData;
        [Title("直接生效的逻辑")]
        public SkillEffectLogicData[] eventDatas;
        [Title("产生的buff")]
        public SkillBuffLogicData[] buffDatas;

        public void Test()
        {
            Name = "Damage";
            effectData = new ModifiersParticleData();
            effectData.Test();

            eventDatas = new SkillEffectLogicData[1];
            eventDatas[0] = new SkillEffectLogicData();//伤害
            eventDatas[0].EffectData = new ModifiersParticleData()
            {
                Name = "伤害",
                EffectName = "Effect/Damage",
                Target = Skill_ChooseTargetType.TARGET,
            };

            eventDatas[0].ChooseData = new ModifiersChooseTargetData()
            {
                TargetTream = Skill_ChooseTargetTream.UNIT_TARGET_TEAM_ENEMY,
                TargetExcludeTypes = Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL,
                TargetType = Skill_ChooseTargetType.NONE,
                TargetArea = Skill_ChooseTargetArea.NONE,
                TargetExcludeFlags = Skill_ChooseTargetExcludeFlags.UNIT_TARGET_FLAG_DEAD
            };

            eventDatas[0].effect = new ModifiersEffectData()
            {
                Name = SkillEffectType.DamageEffect
            };

            //eventDatas[0].TriggerEffect = new ModifiersEffectData[] {
            //    new ModifiersEffectData(){
            //        Name ="Cure",
            //        ChooseData = new ModifiersChooseTargetData(){
            //            TargetType = Skill_ChooseTargetType.ATTACKER
            //        }
            //    }
            //};
            buffDatas = new SkillBuffLogicData[1];
            buffDatas[0] = new SkillBuffLogicData();//buff
            buffDatas[0].EffectData = new ModifiersParticleData()
            {
                Name = "减速buff",
                EffectName = "Effect/SlowDown",
                Target = Skill_ChooseTargetType.TARGET,
            };

            buffDatas[0].ChooseData = new ModifiersChooseTargetData()
            {
                TargetTream = Skill_ChooseTargetTream.UNIT_TARGET_TEAM_ENEMY,
                TargetExcludeTypes = Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL,
                TargetExcludeFlags = Skill_ChooseTargetExcludeFlags.UNIT_TARGET_FLAG_DEAD
            };

            buffDatas[0].effects = new ModifiersEffectData[] {
                new ModifiersEffectData(){
                     Name =  SkillEffectType.SlowDownEffect
                }
            };

            buffDatas[0].TotalTick = 60;
            buffDatas[0].IntervalTick = 30;
        }
    }

    /// <summary>
    /// 特效数据
    /// </summary>
    [Serializable]
    public class ModifiersParticleData
    {
        [Title("特效名称")]
        public string Name;

        /// <summary>
        /// 特效路径
        /// </summary>
        [Title("特效路径")]
        public string EffectName;

        /// <summary>
        /// 选择目标 （Skill_ChooseTargetType）
        /// </summary>
        [Title("特效挂在目标")]
        public Skill_ChooseTargetType Target;

        public void Test()
        {
            Name = "起手特效";
            EffectName = "Effect/Stand";
            Target = Skill_ChooseTargetType.ATTACKER;
        }
    }

    /// <summary>
    /// buff debuff  
    /// </summary>
    [Serializable]
    public class SkillEffectLogicData
    {
        [Title("目标选择数据")]
        public ModifiersChooseTargetData ChooseData;

        [Title("效果产生的特效")]
        public ModifiersParticleData EffectData;

        [Title("直接生效的逻辑")]
        public ModifiersEffectData effect;
    }

    [Serializable]
    public class SkillBuffLogicData
    {
        public int BuffId;

        [Title("总时长(帧)")]
        public int TotalTick;
        /// <summary>
        /// 生效间隔 单位帧
        /// </summary>
        [Title("生效间隔(帧)")]
        public int IntervalTick;

        /// <summary>
        /// 技能结束是否删除
        /// </summary>
        [Title("技能结束是否删除")]
        public bool destroyWithSkill;

        [Title("buff目标选择")]
        public ModifiersChooseTargetData ChooseData;

        [Title("特效数据")]
        public ModifiersParticleData EffectData;

        [Title("只触发一次的逻辑")]
        public ModifiersEffectData[] onceEffects;

        [Title("每次帧事件触发的逻辑")]
        public ModifiersEffectData[] effects;
    }

    /// <summary>
    /// 技能效果数据
    /// 所有的效果的数据都从这个类继承，然后加载配置时根据名称分别处理
    /// </summary>
    [Serializable]
    public class ModifiersEffectData
    {
        /// <summary>
        /// 通过名称创建具体的效果类
        /// </summary>
        [Title("通过名称创建具体的效果类")]
        public SkillEffectType Name;
    }

    /// <summary>
    /// 目标选择数据
    /// </summary>
    [Serializable]
    public class ModifiersChooseTargetData
    {
        public Skill_ChooseTargetType TargetType  = Skill_ChooseTargetType.NONE;
        public Skill_ChooseTargetArea TargetArea  = Skill_ChooseTargetArea.NONE;
        public Skill_ChooseTargetTream TargetTream  = Skill_ChooseTargetTream.UNIT_TARGET_TEAM_NONE;
        public Skill_ChooseTargetExcludeTypes TargetExcludeTypes = Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL;
        public Skill_ChooseTargetExcludeFlags TargetExcludeFlags  = Skill_ChooseTargetExcludeFlags.NONE;
    }


    public class ModifiersEffectDataFactory
    {
        public ModifiersEffectData CreateEffectData(string effectDataType, string data)
        {
            switch (effectDataType)
            {
                default:
                    break;
            }

            return new ModifiersEffectData();
        }
    }

}
