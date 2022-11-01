using System;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;

namespace Skill
{
    public class CheckData
    {
        public int skillRange = 10;
        /// <summary>
        /// 已这个对象为目标去判断范围
        /// </summary>
        public ObjectEntity target;

        public SkillVector vector;
    }


    /// <summary>
    /// 以某个条件开始选择 唯一
    /// 一级选择
    /// </summary>
    public enum Skill_ChooseTargetType
    {
        NONE,
        CASTER,//施放者
        TARGET,//目标
        POINT,//点
        ATTACKER,//攻击者
        UNIT,//单位
        PROJECTILE,//抛射物
    }


    /// <summary>
    /// 以唯一的开始的范围开始筛选
    /// 二级
    /// </summary>
    public enum Skill_ChooseTargetArea
    {
        NONE,
        CENTER,//范围内索搜
        RECTANGLE,//范围矩形内
        SECTOR,//范围扇形内
    }

    /// <summary>
    /// 筛选类型 可多组合
    /// 三级
    /// </summary>
    public enum Skill_ChooseTargetTream
    {
        UNIT_TARGET_TEAM_NONE,//无
        UNIT_TARGET_TEAM_BOTH,//双方队伍
        UNIT_TARGET_TEAM_CUSTOM,//普通队伍
        UNIT_TARGET_TEAM_ENEMY,//敌方队伍
        UNIT_TARGET_TEAM_FRIENDLY,//友方队伍
    }

    /// <summary>
    /// 指定类型 排除对应的类型 可多组合
    /// 四级
    /// </summary>
    public enum Skill_ChooseTargetExcludeTypes
    {
        UNIT_TARGET_ALL,//所有 就是无指定
        UNIT_TARGET_BUILDING,//排除建筑
        UNIT_TARGET_HERO,//排除英雄
        UNIT_TARGET_CUSTOM,// 排除普通 比如小兵
        //...
    }

    /// <summary>
    ///  指定目标的状态 排除对应状态的目标
    /// </summary>
    public enum Skill_ChooseTargetExcludeFlags
    {
        NONE,
        UNIT_TARGET_FLAG_DEAD,//排除死亡的
        UNIT_TARGET_FLAG_INVULNERABLE,//排除无敌的
        UNIT_TARGET_FLAG_MAGIC_IMMUNE_ENEMIES,//排除魔法免疫的
        UNIT_TARGET_FLAG_MAXHP,//只选择最大血量 排除其他
        //...
    }




    public class SkillTargetGetter
    {
        private List<ICheckCondition> conditions = new List<ICheckCondition>();


        public SkillTargetGetter(SkillData skillData)
        {
            InitCheckCondition(skillData);
        }

        public void InitCheckCondition(SkillData skillData)
        {
            conditions.Clear();

            //属性
            var condTream = skillData.Skill_ChooseTargetTreams;
            for (int i = 0; i < condTream.Length; i++)
            {
                var cond = GetCondBySkill_ChooseTargetTream(condTream[i]);
                if (cond != null)
                    conditions.Add(cond);
            }

            var condExcludeTypes = skillData.Skill_ChooseTargetExcludesTypes;
            for (int i = 0; i < condExcludeTypes.Length; i++)
            {
                var cond = GetCondBySkill_ChooseTargetExcludeTypes(condExcludeTypes[i]);
                if (cond != null)
                    conditions.Add(cond);
            }

            var condExcludeFlags = skillData.Skill_ChooseTargetExcludesFlags;
            for (int i = 0; i < condExcludeTypes.Length; i++)
            {
                var cond = GetCondBySkill_ChooseTargetExcludeFlags(condExcludeFlags[i]);
                if (cond != null)
                    conditions.Add(cond);
            }


            //范围内目标
            var cond1 = GetCondBySkill_ChooseTargetType(skillData.Skill_ChooseTargetType);
            var cond2 = GetCondBySkill_ChooseTargetArea(skillData.Skill_ChooseTargetArea);
            if (cond1 != null)
                conditions.Add(cond1);
            if (cond2 != null)
                conditions.Add(cond2);
        }

        HashSet<SoldierEntity> tempSoldiers = new HashSet<SoldierEntity>();
        public HashSet<SoldierEntity> GetTargets(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData data)
        {
            tempSoldiers.Clear();
            tempSoldiers.UnionWith(targets);

            for (int i = 0; i < conditions.Count; i++)
            {
                var con = conditions[i];
                if (tempSoldiers.Count <= 0) break;
                con.CheckTarget(user, tempSoldiers, data);
            }

            return tempSoldiers;
        }

        #region 静态方法获取目标

        public static Dictionary<Skill_ChooseTargetTream, ICheckCondition> chooseTreams = new Dictionary<Skill_ChooseTargetTream, ICheckCondition>();
        public static Dictionary<Skill_ChooseTargetExcludeTypes, ICheckCondition> chooseTargetExcludeTypes = new Dictionary<Skill_ChooseTargetExcludeTypes, ICheckCondition>();
        public static Dictionary<Skill_ChooseTargetExcludeFlags, ICheckCondition> chooseTargetExcludeFlags = new Dictionary<Skill_ChooseTargetExcludeFlags, ICheckCondition>();
        public static Dictionary<Skill_ChooseTargetType, ICheckCondition> chooseTargetTypes = new Dictionary<Skill_ChooseTargetType, ICheckCondition>();
        public static Dictionary<Skill_ChooseTargetArea, ICheckCondition> chooseTargetArea = new Dictionary<Skill_ChooseTargetArea, ICheckCondition>();

        public static HashSet<SoldierEntity> GetTargetsWithData(SoldierEntity owner, HashSet<SoldierEntity> targets, CheckData data, ModifiersChooseTargetData chooseData)
        {
            HashSet<SoldierEntity> tempSoldiers = new HashSet<SoldierEntity>();
            tempSoldiers.UnionWith(targets);

            //部队类型
            if(!chooseTreams.ContainsKey(chooseData.TargetTream))
            {
                chooseTreams.Add(chooseData.TargetTream, GetCondBySkill_ChooseTargetTream(chooseData.TargetTream));
            }
            if (chooseTreams[chooseData.TargetTream] != null)
                chooseTreams[chooseData.TargetTream].CheckTarget(owner, tempSoldiers, data);

            //排除对应的类型
            if (!chooseTargetExcludeTypes.ContainsKey(chooseData.TargetExcludeTypes))
            {
                chooseTargetExcludeTypes.Add(chooseData.TargetExcludeTypes, GetCondBySkill_ChooseTargetExcludeTypes(chooseData.TargetExcludeTypes));
            }
            if (chooseTargetExcludeTypes[chooseData.TargetExcludeTypes] != null)
                chooseTargetExcludeTypes[chooseData.TargetExcludeTypes].CheckTarget(owner, tempSoldiers, data);

            //排除对应状态的目标
            if (!chooseTargetExcludeFlags.ContainsKey(chooseData.TargetExcludeFlags))
            {
                chooseTargetExcludeFlags.Add(chooseData.TargetExcludeFlags, GetCondBySkill_ChooseTargetExcludeFlags(chooseData.TargetExcludeFlags));
            }
            if (chooseTargetExcludeFlags[chooseData.TargetExcludeFlags] != null)
                chooseTargetExcludeFlags[chooseData.TargetExcludeFlags].CheckTarget(owner, tempSoldiers, data);

            //以什么为基础目标选择
            if (!chooseTargetTypes.ContainsKey(chooseData.TargetType))
            {
                chooseTargetTypes.Add(chooseData.TargetType, GetCondBySkill_ChooseTargetType(chooseData.TargetType));
            }
            if (chooseTargetTypes[chooseData.TargetType] != null)
                chooseTargetTypes[chooseData.TargetType].CheckTarget(owner, tempSoldiers, data);

            //范围内选择
            if (!chooseTargetArea.ContainsKey(chooseData.TargetArea))
            {
                chooseTargetArea.Add(chooseData.TargetArea, GetCondBySkill_ChooseTargetArea(chooseData.TargetArea));
            }
            if (chooseTargetArea[chooseData.TargetArea] != null)
                chooseTargetArea[chooseData.TargetArea].CheckTarget(owner, tempSoldiers, data);


            return tempSoldiers;
        }
        #endregion

        private static ICheckCondition GetCondBySkill_ChooseTargetExcludeFlags(Skill_ChooseTargetExcludeFlags conditionType)
        {
            ICheckCondition condition = null;
            switch (conditionType)
            {
                case Skill_ChooseTargetExcludeFlags.UNIT_TARGET_FLAG_MAXHP:
                    condition = new CheckMaxHpCondition();
                    break;
                case Skill_ChooseTargetExcludeFlags.UNIT_TARGET_FLAG_DEAD:
                    condition = new Check_UNIT_TARGET_FLAG_DEAD_Condition();
                    break;
                default:
                    break;
            }
            return condition;
        }

        private static ICheckCondition GetCondBySkill_ChooseTargetExcludeTypes(Skill_ChooseTargetExcludeTypes conditionType)
        {
            ICheckCondition condition = null;
            switch (conditionType)
            {
                case Skill_ChooseTargetExcludeTypes.UNIT_TARGET_HERO:
                   
                    break;
                case Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL:
                    return new Check_UNIT_TARGET_ALL();
                case Skill_ChooseTargetExcludeTypes.UNIT_TARGET_CUSTOM:
                    return new Check_UNIT_TARGET_CUSTOM_Condition();
                default:
                    break;
            }
            return condition;
        }


        private static ICheckCondition GetCondBySkill_ChooseTargetTream(Skill_ChooseTargetTream conditionType)
        {
            ICheckCondition condition = null;
            switch (conditionType)
            {
                case Skill_ChooseTargetTream.UNIT_TARGET_TEAM_ENEMY:
                    return new Check_UNIT_TARGET_TEAM_ENEMY_Condition();
                default:
                    break;
            }
            return condition;
        }

        private static ICheckCondition GetCondBySkill_ChooseTargetArea(Skill_ChooseTargetArea conditionType)
        {
            ICheckCondition condition = null;
            switch (conditionType)
            {
                case Skill_ChooseTargetArea.CENTER:
                    condition = new Check_Skill_ChooseTargetArea();
                    break;
                default:
                    break;
            }
            return condition;
        }


        private static ICheckCondition GetCondBySkill_ChooseTargetType(Skill_ChooseTargetType conditionType)
        {
            ICheckCondition condition = null;
            switch (conditionType)
            {
                case Skill_ChooseTargetType.ATTACKER:
                    condition = new Check_Skill_ChooseTargetType_CASTER();
                    break;
                default:
                    break;
            }
            return condition;
        }
    }
}
