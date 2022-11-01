using System;
using System.Collections.Generic;
using Bepop.Core;
using Notifaction;

namespace Skill
{
    public enum SkillEffectType
    {
        None,
        DamageEffect,
        SlowDownEffect
    }


    public class SkillEffectBase  
    {
        public int effectId;

        ModifiersEffectData data;

        public virtual void Init(ModifiersEffectData  data)
        {
            this.data = data;
        }

        public virtual void OnEffectOn(SoldierEntity adder, SoldierEntity target, NotifyParam param = null)
        {

        }

        public virtual void OnEffectOut(SoldierEntity adder, SoldierEntity target, NotifyParam param = null)
        {

        }

        public virtual void Exce(SoldierEntity adder,HashSet<SoldierEntity> targets,NotifyParam param = null)
        {
            
        }

        public virtual void Exce(SoldierEntity adder,SoldierEntity target, NotifyParam param = null)
        {

        }

        public virtual void Dispose()
        {

        }
    }

    public class DamageEffect : SkillEffectBase
    {
        public override void Exce(SoldierEntity adder, HashSet<SoldierEntity> targets, NotifyParam param = null)
        {
            if (targets == null || targets.Count <= 0) return;
            int atk = adder.atk;
            targets.ForEach((soldier) => {
                //test 计算公式要单独拿出来
                int damage = atk > soldier.def ? atk - soldier.def : 0;
                soldier.Hurt(damage, adder);
            });
        }
    }

    /// <summary>
    /// 减速
    /// </summary>
    public class SlowDownEffect : SkillEffectBase
    {
        private int changeValue = -20;

        public override void Init(ModifiersEffectData data)
        {
            base.Init(data);
        }

        public override void OnEffectOn(SoldierEntity adder, SoldierEntity target, NotifyParam param = null)
        {
            target.ChangeSpeed(changeValue);
        }

        public override void OnEffectOut(SoldierEntity adder, SoldierEntity target, NotifyParam param = null)
        {
            target.ChangeSpeed(-changeValue);
        }
    }
}