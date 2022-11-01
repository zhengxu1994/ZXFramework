using System;
using System.Collections.Generic;
using Bepop.Core;
using Notifaction;
using Skill.SkillTrigger;

namespace Skill
{
    public class Buff : IDisposable
    {

        public int buffId { get; protected set; }

        private SkillBuffLogicData data;

        private SoldierEntity adder;

        private SoldierEntity owner;

        #region 生命周期
        //所有tick
        private int totalTick;
        //当前tick
        private int tempTick;
        //生效tick
        private int triggerTick;
        #endregion


        #region 添加效果
        private ModifiersEffectData[] updateEffects;

        private ModifiersEffectData[] onceEffects;

        private SkillEffectBase[] onceEffectLogics;

        private SkillEffectBase[] updateEffectLogics;

        #endregion


        #region 处理光环
        private HashSet<SoldierEntity> addedSoldiers;
        private HashSet<SoldierEntity> removeSoldiers;
        #endregion

        private CheckData checkData;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="adder">添加者</param>
        /// <param name="owner">拥有者</param>
        public Buff(SkillBuffLogicData data, SoldierEntity adder,SoldierEntity owner)
        {
            this.data = data;
            this.buffId = data.BuffId;
            tempTick = 0;
            totalTick = data.TotalTick;
            triggerTick = data.IntervalTick;
            updateEffects = data.effects;
            onceEffects = data.onceEffects;

            if (updateEffects != null && updateEffects.Length > 0)
            {
                updateEffectLogics = new SkillEffectBase[updateEffects.Length];
                for (int i = 0; i < updateEffects.Length; i++)
                {
                    var effect = SkillBase.CreateEffect(updateEffects[i]);
                    if (effect != null)
                    {
                        updateEffectLogics[i] = effect;
                        effect.OnEffectOn(owner, owner);
                    }
                }
            }

            if (onceEffects != null)
            {
                addedSoldiers = new HashSet<SoldierEntity>();
                removeSoldiers = new HashSet<SoldierEntity>();
                onceEffectLogics = new SkillEffectBase[onceEffects.Length];
                for (int i = 0; i < onceEffects.Length; i++)
                {
                    var onceEffectData = onceEffects[i];
                    var effect = SkillBase.CreateEffect(onceEffectData);
                    if (effect != null)
                    {
                        onceEffectLogics[i] = effect;
                    }
                }
            }

            checkData = new CheckData();
            checkData.skillRange = 100;//test

            this.adder = adder;
            this.owner = owner;

            NotifactionCenter.Instance.RegistNotification(SkillEvent.Trigger_BuffUpdate, this, (evt) => {
               var isOver = OnBuffUpdate();
                if (isOver)
                    owner.RemoveBuff(this);
            });
        }

        public static Buff CreateBuff(SkillBuffLogicData data, SoldierEntity adder, SoldierEntity target)
        {
            //判断是否免疫之类的
            var param = NotifyParam.Create();
            param.Int(UniStr.sender, adder.entityId);
            param.Int(UniStr.trigger, target.entityId);
            param.Int(UniStr.buffId, data.BuffId);
            NotifactionCenter.Instance.PostNotification(SkillEvent.Trigger_AddBuffBefore, param);
            bool cancel = param.Bool(UniStr.cancelBuff);
            if (cancel) return null;
            return new Buff(data,adder,target);
        }

        /// <summary>
        /// 增加叠加次数
        /// </summary>
        public void UpdateTimes()
        {

        }

        /// <summary>
        /// 光环update
        /// </summary>
        public void OnHaloUpdate()
        {
            if (onceEffects != null)
            {
                var targets = SkillTargetGetter.GetTargetsWithData(owner, SoldierEntityMgr.Instance.soldiers, checkData, data.ChooseData);
                removeSoldiers.Clear();
                //移除部分
                addedSoldiers.ForEach((soldier) => {
                    if (!targets.Contains(soldier))
                        removeSoldiers.Add(soldier);
                });

                removeSoldiers.ForEach((soldier) => {
                    addedSoldiers.Remove(soldier);
                    for (int i = 0; i < onceEffectLogics.Length; i++)
                    {
                        if (onceEffectLogics[i] != null)
                            onceEffectLogics[i].OnEffectOut(owner, soldier);
                    }
                });


                //添加部分
                targets.ForEach((soldier) => {
                   if(!addedSoldiers.Contains(soldier))
                    {
                        for (int i = 0; i < onceEffectLogics.Length; i++)
                        {
                            if (onceEffectLogics[i] != null)
                                onceEffectLogics[i].OnEffectOn(owner, soldier);
                        }
                        addedSoldiers.Add(soldier);
                    }
                });
            }
        }

  
        public bool OnBuffUpdate()
        {
            tempTick++;
            if (tempTick % triggerTick == 0)
            {
                //buff 效果更新
                if (updateEffectLogics != null && updateEffectLogics.Length > 0)
                {
                    for (int i = 0; i < updateEffectLogics.Length; i++)
                    {
                        //一般都是给自己的 如有特殊后面处理
                        if (updateEffectLogics[i] != null)
                            updateEffectLogics[i].Exce(owner, owner);
                    }
                }
            }

            OnHaloUpdate();
            return tempTick >= totalTick;
        }

        public void Dispose()
        {
            if(updateEffectLogics != null && updateEffectLogics.Length > 0)
            {
                for (int i = 0; i < updateEffectLogics.Length; i++)
                {
                    updateEffectLogics[i].OnEffectOut(owner,owner);
                    updateEffectLogics[i].Dispose();
                }
            }

            if((onceEffectLogics != null && onceEffectLogics.Length > 0) &&(addedSoldiers != null && addedSoldiers.Count > 0))
            {
                addedSoldiers.ForEach((soldier) =>
                {
                    for (int i = 0; i < onceEffectLogics.Length; i++)
                    {
                        onceEffectLogics[i].OnEffectOut(owner, soldier);
                    }
                });
                onceEffectLogics.ForEach((effect) => {
                    effect.Dispose();
                });

                addedSoldiers.Clear();
            }

            onceEffectLogics = null;
            updateEffectLogics = null;
            NotifactionCenter.Instance.RemoveNotifacation(this);
        }
    }
}