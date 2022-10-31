using System;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;

namespace Skill
{
    //技能目标选择（选取范围，中心目标，筛选条件，指定类型，指定目标状态，最多数量，是否额外随机一个）
    public interface ICheckCondition
    {
        HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData);
    }


    #region Skill_ChooseTargetType 

    public class Check_Skill_ChooseTargetType_CASTER : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            checkData.target = user;
            return targets;
        }
    }

    #endregion

    #region Skill_ChooseTargetArea

    public class Check_Skill_ChooseTargetArea : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            var target = checkData.target;
            targets.RemoveWhere((soldier) => { return Vector2.Distance(target.pos, soldier.pos) > checkData.skillRange; });
            return targets;
        }
    }

    #endregion



    #region Skill_ChooseTargetTream
    /// <summary>
    /// 检测敌方部队
    /// </summary>
    public class Check_UNIT_TARGET_TEAM_ENEMY_Condition : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            int camp = user.campId;
            targets.RemoveWhere((entity) => { return entity.campId == camp; });
            return targets;
        }
    }

    #endregion

    #region Skill_ChooseTargetExcludeTypes
    /// <summary>
    /// 检测排除士兵
    /// </summary>
    public class Check_UNIT_TARGET_CUSTOM_Condition : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            targets.RemoveWhere((entity) => {
                return entity.isSolider;
            });
            return targets;
        }
    }

    public class Check_UNIT_TARGET_ALL: ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        { 
            return targets;
        }
    }
    #endregion

    #region Skill_ChooseTargetExcludeFlags
    public class CheckMaxHpCondition : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            SoldierEntity entity = null;
            int hp = -1;
            targets.ForEach((solider) =>
            {
                if (solider.hp > hp)
                {
                    entity = solider;
                    hp = solider.hp;
                }
            });
            targets.Clear();
            if (entity != null)
                targets.Add(entity);
            return targets;
        }
    }

    public class Check_UNIT_TARGET_FLAG_DEAD_Condition : ICheckCondition
    {
        public HashSet<SoldierEntity> CheckTarget(SoldierEntity user, HashSet<SoldierEntity> targets, CheckData checkData)
        {
            targets.RemoveWhere((soldier) => {
                return !soldier.isAlive;
            });
            return targets;
        }
    }
    #endregion


}
