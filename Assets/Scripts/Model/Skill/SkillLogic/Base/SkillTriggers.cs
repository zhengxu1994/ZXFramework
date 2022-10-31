using System;
using System.Collections.Generic;
using Bepop.Core;
using Notifaction;

namespace Skill.SkillTrigger
{

    public static class SkillEvent
    {
        public static string Trigger_SkillStand = "Trigger_SkillStand";
        public static string Trigger_SkillSinging = "Trigger_SkillSinging";
        public static string Trigger_SkillEnd = "Trigger_SkillEnd";
        public static string Trigger_AddBuffBefore = "Trigger_AddBuffBefore";
        public static string Trigger_BuffUpdate = "Trigger_BuffUpdate";
    }

    public enum SkillTriggerType
    {
        OnSkillInit,//技能初始化时
        OnOwnerDeath,
        OnSkillStart,
        OnSkillFinish
    }

    /// <summary>
    /// trigger集中处理中心
    /// 不在每个trigger内处理 便于调试 统计 统一管理
    /// </summary>
    public class SkillTriggerCenter : Singleton<SkillTriggerCenter> 
    {
        public Dictionary<SkillTriggerType, HashSet<SkillTriggerBase>> triggers = new Dictionary<SkillTriggerType, HashSet<SkillTriggerBase>>();

        public void AddTrigger(SkillTriggerBase trigger)
        {
            var type = trigger.triggerType;
            if (!triggers.ContainsKey(type))
            {
                triggers.Add(type, new HashSet<SkillTriggerBase>());
            }
            triggers[trigger.triggerType].Add(trigger);
#if ShowLog
            var senderId = trigger.sender == null ? -1 : trigger.sender.entityId;
            var triggerId = trigger.trigger == null ? -1 : trigger.trigger.entityId;
            Log.BASE.LogInfo($"add trigger type:{type},senderId :{senderId},triggerId :{triggerId}");
#endif
        }


        public void RemoveTrigger(SkillTriggerBase trigger)
        {
            if (!triggers.ContainsKey(trigger.triggerType))
            {
                Log.EX.LogError($"dnot have triggertype :{trigger.triggerType}");
                return;
            }

            if (!triggers[trigger.triggerType].Contains(trigger))
            {
                Log.EX.LogError($"dnot have trigger :{trigger.triggerType}");
                return;
            }

            triggers[trigger.triggerType].Remove(trigger);

#if ShowLog
            var senderId = trigger.sender == null ? -1 : trigger.sender.entityId;
            var triggerId = trigger.trigger == null ? -1 : trigger.trigger.entityId;
            Log.BASE.LogInfo($"remove trigger type:{trigger.triggerType},senderId :{senderId},triggerId :{triggerId}");
#endif

        }

        /// <summary>
        /// 触发对应类型的trigger
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        public void Trigger(SkillTriggerType type,SkillParam param)
        {
            if (!triggers.ContainsKey(type))
            {
                Log.BASE.LogWarning($"trigger type :{type} is null");
                return;
            }

            var hash = triggers[type];
            hash.ForEach((v) =>
            {
                if(v.TriggerCondition(param))
                v.Trigger(param);
            });
        }



        public override void Dispose()
        {
            triggers.ForEach((k, v) => {
                v.ForEach((e) => {
                    e.Dispose();
                });
            });
            triggers.Clear();
        }
    }
    //技能触发事件
    //与技能本体有关
    //比如技能施法开始也叫抬手，施法结束，持续施法，施法中断，拥有者死亡，拥有者出生，技能命中单位
    public class SkillTriggerBase : IDisposable
    {
        /// <summary>
        /// trigger类型
        /// </summary>
        public SkillTriggerType triggerType { get; protected set; }

        /// <summary>
        /// 事件触发后操作数据
        /// </summary>
        public SkillEffectLogicData triggerData { get; protected set; }

        /// <summary>
        /// 触发者
        /// </summary>
        public ObjectEntity trigger { get; protected set; }

        /// <summary>
        /// 事件发送者
        /// 发送者 可能与触发者一致 
        /// </summary>
        public ObjectEntity sender { get; protected set; }

        public virtual void Init(SkillParam param, SkillTriggerType triggerType)
        {
            this.triggerType = triggerType;
            if (param.Entity(UniStr.sender) != null)
                sender = param.Entity(UniStr.sender);
            if (param.Entity(UniStr.trigger) != null)
                trigger = param.Entity(UniStr.trigger);
        }

        public virtual bool TriggerCondition(SkillParam param)
        {
            return true;
        }

        public virtual void Trigger(SkillParam param)
        {
            Log.BASE.LogInfo($"Trigger Type:{triggerType}");
        }

        public virtual void Dispose()
        {
            trigger = null;
            sender = null;
            triggerData = null;
        }

    }

    /// <summary>
    /// 技能初始化时
    /// </summary>
    public class OnSkillInit : SkillTriggerBase
    {
        public override void Init(SkillParam param, SkillTriggerType triggerType)
        {
            base.Init(param, triggerType);

        }

        public override void Trigger(SkillParam param)
        {
        }
    }

    ///// <summary>
    ///// 当施法开始时
    ///// </summary>
    //public class OnSpellStart : SkillTriggerBase, ISkillTrigger
    //{
    //    public void Init(ObjectEntity entity, SkillTriggerType triggerType)
    //    {

    //    }

    //    public void Remove()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Trigger(SkillParam param)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}