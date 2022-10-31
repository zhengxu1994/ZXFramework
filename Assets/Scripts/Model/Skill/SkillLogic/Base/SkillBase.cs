using System;
using System.Collections.Generic;
using Bepop.Core;
using Notifaction;
using Skill.SkillTrigger;
using Skill.UseSkillCondition;

namespace Skill
{
    public struct SkillVector
    {
        public int x;

        public int y;

        public static SkillVector One { get { return new SkillVector(1,1); } }

        public static SkillVector Zero { get { return new SkillVector(0, 0); } }

        public SkillVector(int x,int y)
        {
            this.x = x;
            this.y = y;
        }

        //public int z;
    }
    public class UniStr
    {
        private UniStr() { }

        public static string sender = "sender";

        public static string trigger = "trigger";

        public static string skillId = "skillId";

        public static string buffId = "buffId";

        public const string PreLoadAssets = "PreLoadAssets";

        public static string cancelSkill = "cancelSkill";

        public static string cancelBuff = "cancelBuff";
    }

    public class SkillParam
    {
        public static SkillParam Create()
        {
            return new SkillParam();//后面要从对象池中获取
        }

        public Dictionary<string, int> intValues;
        public Dictionary<string, string> stringValues;
        public Dictionary<string, object> objValues;
        public Dictionary<string, ObjectEntity> entitiesValues;

        public int Int(string key)
        {
            if (intValues == null || !intValues.ContainsKey(key))
                return -1;
            return intValues[key];
        }

        public SkillParam Int(string key, int value)
        {
            if (intValues == null) intValues = new Dictionary<string, int>();
            intValues[key] = value;
            return this;
        }

        public string nullStr = "";
        public string Str(string key)
        {
            if (stringValues == null || !stringValues.ContainsKey(key))
                return nullStr;
            return stringValues[key];
        }

        public SkillParam Str(string key, string value)
        {
            if (stringValues == null) stringValues = new Dictionary<string, string>();
            stringValues[key] = value;
            return this;
        }

        public object Obj(string key)
        {
            if (objValues == null || !objValues.ContainsKey(key))
                return null;
            return objValues[key];
        }

        public SkillParam Obj(string key, int value)
        {
            if (objValues == null) objValues = new Dictionary<string, object>();
            objValues[key] = value;
            return this;
        }

        public ObjectEntity Entity(string key)
        {
            if (entitiesValues == null || !entitiesValues.ContainsKey(key))
                return null;
            return entitiesValues[key];
        }

        public SkillParam Entity(string key, ObjectEntity value)
        {
            if (entitiesValues == null) entitiesValues = new Dictionary<string, ObjectEntity>();
            entitiesValues[key] = value;
            return this;
        }
    }

    public class SkillData
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        public string BaseClass { get; private set; }

        /// <summary>
        /// 技能使用条件检测
        /// </summary>
        public CheckUseSkillConditionType[] Skill_UseCondition { get; private set; }
        /// <summary>
        /// 技能行为
        /// </summary>
        public int[] SkillBehavior { get; private set; }

        public (Skill_ChooseTargetType, Skill_ChooseTargetArea) Skill_ChooseTargetTypeAndArea { get; private set; }

        /// <summary>
        /// 技能筛选阵营类型
        /// </summary>
        public Skill_ChooseTargetTream[] Skill_ChooseTargetTreams { get; private set; }

        /// <summary>
        /// 技能排除种类类型
        /// </summary>
        public Skill_ChooseTargetExcludeTypes[] Skill_ChooseTargetExcludesTypes { get; private set; }

        /// <summary>
        /// 状态排除类型
        /// </summary>
        public Skill_ChooseTargetExcludeFlags[] Skill_ChooseTargetExcludesFlags { get; private set; }

        public int maxLevel { get; private set; }

        /// <summary>
        /// 施法距离
        /// </summary>
        public int SkillRange { get; private set; }

        /// <summary>
        /// 冷却时间
        /// </summary>
        public int SkillCoolDown { get; private set; }

        /// <summary>
        /// 耗蓝
        /// </summary>
        public int SkillCostValue { get; private set; }

        public bool IsPassive { get; private set; }

        public bool IsHidden { get; private set; }

        public ModifiersData Stand_Modifiers { get; private set; }

        public ModifiersData Sing_Modifiers { get; private set; }

        public ModifiersData End_Modifiers { get; private set; }

        public void TestSkill()
        {
            Name = "TestSkill";
            ID = 1;
            BaseClass = "SkillBase";
            Skill_UseCondition = new CheckUseSkillConditionType[] { CheckUseSkillConditionType.CD, CheckUseSkillConditionType.Magic };
            Skill_ChooseTargetTypeAndArea = (Skill_ChooseTargetType.ATTACKER, Skill_ChooseTargetArea.CENTER);
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
    public class ModifiersData
    {

        public string Name { get; private set; }

        public ModifiersParticleData effectData { get; private set; } = null;

        public SkillEffectLogicData[] eventDatas { get; private set; }

        public SkillBuffLogicData[] buffDatas { get; private set; }

        public void Test()
        {
            Name = "Damage";
            effectData = new ModifiersParticleData();
            effectData.Test();

            eventDatas = new SkillEffectLogicData[1];
            eventDatas[0] = new SkillEffectLogicData();//伤害
            eventDatas[0].EffectData = new ModifiersParticleData() {
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
                Name = "DamageEffect",
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
                     Name = "SlowDownEffect",
                }
            };

            buffDatas[0].TotalTick = 60;
            buffDatas[0].IntervalTick = 30;
        }
    }

    /// <summary>
    /// 特效数据
    /// </summary>
    public class ModifiersParticleData
    {
        public string Name { get; set; }

        /// <summary>
        /// 特效路径
        /// </summary>
        public string EffectName { get; set; }

        /// <summary>
        /// 选择目标 （Skill_ChooseTargetType）
        /// </summary>
        public Skill_ChooseTargetType Target { get; set; }

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
    public class SkillEffectLogicData
    {

        public ModifiersChooseTargetData ChooseData { get; set; }

        public ModifiersParticleData EffectData { get; set; }

        public ModifiersEffectData effect { get; set; }
    }

    public class SkillBuffLogicData
    {
        public int BuffId { get; set; }

        public int TotalTick { get; set; }
        /// <summary>
        /// 生效间隔 单位帧
        /// </summary>
        public int IntervalTick { get; set; }

        /// <summary>
        /// 技能结束是否删除
        /// </summary>
        public bool destroyWithSkill { get; set; }

        public ModifiersChooseTargetData ChooseData { get; set; }

        public ModifiersParticleData EffectData { get; set; }

        public ModifiersEffectData[] onceEffects { get; set; }

        public ModifiersEffectData[] effects { get; set; }
    }

    /// <summary>
    /// 技能效果数据
    /// 所有的效果的数据都从这个类继承，然后加载配置时根据名称分别处理
    /// </summary>
    public class ModifiersEffectData
    {
        /// <summary>
        /// 通过名称创建具体的效果类
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 目标选择数据
    /// </summary>
    public class ModifiersChooseTargetData
    {
        public Skill_ChooseTargetType TargetType { get; set; } = Skill_ChooseTargetType.NONE;
        public Skill_ChooseTargetArea TargetArea { get; set; } = Skill_ChooseTargetArea.NONE;
        public Skill_ChooseTargetTream TargetTream { get; set; } = Skill_ChooseTargetTream.UNIT_TARGET_TEAM_NONE;
        public Skill_ChooseTargetExcludeTypes TargetExcludeTypes { get; set; } = Skill_ChooseTargetExcludeTypes.UNIT_TARGET_ALL;
        public Skill_ChooseTargetExcludeFlags TargetExcludeFlags { get; set; } = Skill_ChooseTargetExcludeFlags.NONE;
    }
    

    public class ModifiersEffectDataFactory
    {
        public ModifiersEffectData CreateEffectData(string effectDataType,string data)
        {
            switch (effectDataType)
            {
                default:
                    break;
            }

            return new ModifiersEffectData();
        }
    }

    /// <summary>
    /// 数据驱动技能类基类
    /// </summary>
    public class SkillBase
    {
        enum SkillPhase
        {
            None,
            Stand,
            Sing,
            End
        }
        /*
        组合技能行为（显示 技能的形状，技能是否在面板上显示，是否可以被中断 是否有施法前腰等），
        https://developer.valvesoftware.com/wiki/Dota_2_Workshop_Tools:zh-cn/Scripting:zh-cn/Abilities_Data_Driven:zh-cn 技能行为列表
        技能是否可以释放条件判断
        技能触发事件（与技能本体有关 比如技能施法开始也叫抬手，施法结束，持续施法，施法中断，拥有者死亡，拥有者出生，技能命中单位） 事件
        技能操作（触发与操作时一起的，当触发什么事件时进行什么技能操作  比如播放特效 播放声音 播放具体动作）
        技能目标选择（选取范围，中心目标，筛选条件，指定类型，指定目标状态，最多数量，是否额外随机一个）
        技能附加特征 (buff debuff )(两种作用类型 一种直接修改目标属性 一种添加状态) 
        技能附加特征事件 （一些触发事件 比如当攻击异常状态时 ，拥有特征目标死亡时，攻击时 ，攻击后）
        */

        /*
         * 先制作一个简单的技能，给某个对象造成一定伤害，并且给对象减速，如果对方死亡我恢复气血
         * 释放条件 cd 蓝量　(显示部分) 结合第二步(正式释放)
         * 技能选择 范围内 敌方 单体
         * 技能生命周期（抬手 生效 收招）
         * 技能效果 给对方直接造成伤害 并且添加减速buff (死亡回血可以直接做成一个被动 在初始化的时候就添加上 如果是自己 并且是技能 杀死敌人 则回复气血)
         */

        protected SkillData skillData = null;

        protected List<CheckCanUseCondition> checkUseSkillConditions = new List<CheckCanUseCondition>();

        protected SkillTargetGetter targetGetter = null;

        protected CheckData checkData = null;

        protected SkillVector chooseVecotr;


        NotifyParam param = NotifyParam.Create();

        private HashSet<SoldierEntity> soldiers = SoldierEntityMgr.Instance.soldiers;
        #region 帧阶段
        /// <summary>
        /// 抬手帧率
        /// </summary>
        public int TotalStandFrame { get; protected set; }
        /// <summary>
        /// 抬手关键帧
        /// </summary>
        public int StandFrame { get; protected set; }
        /// <summary>
        /// 吟唱
        /// </summary>
        public int TotalSingFrame { get; protected set; }
        /// <summary>
        /// 吟唱间隔
        /// </summary>
        public int SingFrameInterval { get; protected set; }
        /// <summary>
        /// 吟唱生效帧
        /// </summary>
        public int SingFrame { get; protected set; }
        /// <summary>
        /// 收招
        /// </summary>
        public int TotalEndFrame { get; protected set; }
        /// <summary>
        /// 收招关键帧
        /// </summary>
        public int EndFrame { get; protected set; }

        SkillPhase phase = SkillPhase.None;
        #endregion

        private HashSet<Buff> buffList = new HashSet<Buff>();
        /// <summary>
        /// 创建条件检测类
        /// </summary>
        /// <param name="data"></param>
        private void CreateCheckUseCondition(SkillData data)
        {
            var conditions = data.Skill_UseCondition;
            if (conditions == null || conditions.Length <= 0)
            {
                Log.BASE.LogWarning($"CheckUseCondition is null :{data.ID}");
                return;
            }
           
            for (int i = 0; i < conditions.Length; i++)
            {
                var cond = conditions[i];
                var condition = CreateCanUseCondition(cond);
                checkUseSkillConditions.Add(condition);
            }
        }

        private void CreateTargetGetter(SkillData data)
        {
            targetGetter = new SkillTargetGetter(data);
            checkData = new CheckData();
            checkData.skillRange = data.SkillRange;
        }

        public SoldierEntity Owner { get; private set; }

        public virtual void Init(SkillData skillData,SoldierEntity owner)
        {
            this.skillData = skillData;
            this.Owner = owner;
            CreateCheckUseCondition(skillData);
            CreateTargetGetter(skillData);
            //Notifaction.NotifactionCenter.Instance.PostNotification()
        }

        public virtual bool CheckCanUse()
        {
            bool canUse = true;
            if (checkUseSkillConditions.Count > 0)
            {
                for (int i = 0; i < checkUseSkillConditions.Count; i++)
                {
                    canUse = checkUseSkillConditions[i].CheckCondition(Owner);
                    if (!canUse)
                        break;
                }
            }
            return canUse;
        }

        protected HashSet<SoldierEntity> targets { get; set; } = new HashSet<SoldierEntity>();

        //接受ui层传递过来的信息 同时要兼容自动释放逻辑
        public virtual HashSet<SoldierEntity> GetSkillTargets(SkillVector vector,HashSet<SoldierEntity> checkTargets)
        {
            if (targetGetter == null) return null;
            checkData.vector = vector;
            var getTargets = targetGetter.GetTargets(Owner, checkTargets, checkData);
            targets.Clear();
            targets.UnionWith(getTargets);
            return targets;
        }

        public virtual void Use(SkillVector vector)
        {
            //分为表现层(人物抬手动作 释放特效等逻辑) 和逻辑层(技能关键帧生成技能效果 如直接给目标添加buff 或者生成一个弹道等逻辑)
            //这些都需要用到关键帧逻辑 那么我们把逻辑帧跑的逻辑单独拿出来
            param.Clear();
            buffList.Clear();
            phase = SkillPhase.Stand;
            chooseVecotr = vector;
        }



        private int tempFrame = 0;
        /// <summary>
        /// 技能逻辑帧 
        /// </summary>
        /// <returns>返回true 代表技能被打断或者结束</returns>
        public bool UpdateFrame()
        {
            switch (phase)
            {
                case SkillPhase.Stand:
                    SkillStand();
                    break;

                case SkillPhase.Sing:
                    SkillSinging();
                    break;

                case SkillPhase.End:
                    SkillEnd();
                    break;
            }


            CheckPhase();
            return phase == SkillPhase.None;
        }

        void CheckPhase()
        {
            if (phase == SkillPhase.None) return;
            if (phase == SkillPhase.Stand)
            {
                if (tempFrame > TotalStandFrame)
                {
                    if (TotalSingFrame > 0)
                    {
                        phase = SkillPhase.Sing;
                        tempFrame = 0;
                    }
                    else if (TotalEndFrame > 0)
                    {
                        phase = SkillPhase.End;
                        tempFrame = 0;
                    }
                    else
                        phase = SkillPhase.None;
                }
            }
            else if (phase == SkillPhase.Sing)
            {
                if (tempFrame > TotalSingFrame)
                {
                    if (TotalEndFrame > 0)
                    {
                        phase = SkillPhase.End;
                        tempFrame = 0;
                    }
                    else
                        phase = SkillPhase.None;
                }
            }
            else if (phase == SkillPhase.End)
            {
                if (tempFrame > TotalEndFrame)
                {
                    phase = SkillPhase.None;
                    tempFrame = 0;
                }
            }
            tempFrame++;
        }

        /// <summary>
        /// 打断技能
        /// </summary>
        public void BreakSkill()
        {
            phase = SkillPhase.None;
            tempFrame = 0;
            RemoveTriggerEvent();
            param.Clear();
        }



        public virtual void SkillStand()
        {
            if(tempFrame == StandFrame)
            {
                //触发抬手事件
                Notifaction.NotifactionCenter.Instance.PostNotification(SkillEvent.Trigger_SkillStand,param
                    .Int(UniStr.skillId, skillData.ID)
                    .Int(UniStr.sender, Owner.entityId));
                Log.BASE.LogInfo($"SkillStand ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");

                bool cancel = param.Bool(UniStr.cancelSkill);

                if(cancel)
                {
                    //打断技能
                    BreakSkill();
                    Log.BASE.LogInfo($"Skill Has Be Break On Stand ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");
                    return;
                }
                //技能抬手效果生效
                //比如创建buff
                if (skillData == null || skillData.Stand_Modifiers == null) return;
                var info = skillData.Stand_Modifiers;
                ExceLogic(info,new CheckData());
            }
        }


        private List<SkillEffectBase> singingEffects = new List<SkillEffectBase>();

        public virtual void SkillSinging()
        {
            var info = skillData.Sing_Modifiers;
            if (tempFrame == 0)
            {
                //第一帧把所有effect 创建出来
                singingEffects.Clear();
                var eventDatas = info.eventDatas;
                if (eventDatas != null)
                {
                    for (int i = 0; i < eventDatas.Length; i++)
                    {
                        var data = eventDatas[i];
                        if (data == null) continue;
                        var effect = CreateEffect(data.effect);
                        singingEffects.Add(effect);
                    }
                }
            }


            int _frame = tempFrame % SingFrameInterval;
            if (_frame == SingFrame)
            {
                //触发吟唱事件
                Notifaction.NotifactionCenter.Instance.PostNotification(SkillEvent.Trigger_SkillSinging, NotifyParam.Create()
                .Int(UniStr.skillId, skillData.ID)
                .Int(UniStr.sender, Owner.entityId));
                Log.BASE.LogInfo($"SkillSinging ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");

                bool cancel = param.Bool(UniStr.cancelSkill);

                if (cancel)
                {
                    //打断技能
                    BreakSkill();
                    Log.BASE.LogInfo($"Skill Has Be Break On Sing ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");
                    return;
                }

                //创建buff 或者效果
                GetSkillTargets(chooseVecotr, soldiers);
                if (singingEffects.Count > 0)
                {
                    singingEffects.ForEach((effect) => {
                        effect.Exce(Owner, targets, param);
                    });
                }

                CreateBuffs(info);
            }

            if (_frame == 0)
            {
                //吟唱间隔
                //重置动作等操作
            }
        }

        public virtual void SkillEnd()
        {
            if(tempFrame == EndFrame)
            {

                //触发收招事件
                Notifaction.NotifactionCenter.Instance.PostNotification(SkillEvent.Trigger_SkillEnd, NotifyParam.Create()
                .Int(UniStr.skillId, skillData.ID)
                .Int(UniStr.sender, Owner.entityId));
                Log.BASE.LogInfo($"SkillEnd ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");

                bool cancel = param.Bool(UniStr.cancelSkill);

                if (cancel)
                {
                    //打断技能
                    BreakSkill();
                    Log.BASE.LogInfo($"Skill Has Be Break On End ====SkillId:{skillData.ID}, OwnerId:{Owner.entityId}");
                    return;
                }

                //收招buff 或者其他逻辑
                ExceLogic(skillData.End_Modifiers, new CheckData());
            }
        }

        private void ExceLogic(ModifiersData info,CheckData checkData)
        {
            GetSkillTargets(chooseVecotr, soldiers);
            var eventDatas = info.eventDatas;
            if (eventDatas != null)
            {
                for (int i = 0; i < eventDatas.Length; i++)
                {
                    var data = eventDatas[i];
                    if (data == null) continue;
                    var effect = CreateEffect(data.effect);
                    if (effect != null)
                        effect.Exce(Owner, targets, param);
                }
            }

            CreateBuffs(info);
        }

        private void CreateBuffs(ModifiersData info)
        {
            var buffs = info.buffDatas;
            if (buffs != null)
            {
                for (int i = 0; i < buffs.Length; i++)
                {
                    var data = buffs[i];
                    if (data == null) continue;
                    var tempTargets = SkillTargetGetter.GetTargetsWithData(Owner, targets, checkData, data.ChooseData);
                    tempTargets.ForEach((soldier) =>
                    {
                        var buff = Buff.CreateBuff(data, Owner, soldier);
                        if (buff != null && data.destroyWithSkill)
                            buffList.Add(buff);
                        else
                            soldier.AddBuff(buff);
                    });
                }
            }
        }

        public static CheckCanUseCondition CreateCanUseCondition(CheckUseSkillConditionType type)
        {
            switch (type)
            {
                case CheckUseSkillConditionType.CD:
                    return new CDCondition();
                case CheckUseSkillConditionType.Magic:
                    return new MagicCondition();
                default:
                    return null;
            }
        }

        public static SkillEffectBase CreateEffect(ModifiersEffectData data)
        {
    
            SkillEffectBase logic = null;
            switch (data.Name)
            {
                case SkillEffectName.DamageEffect:
                    logic = new DamageEffect();
                    break;
                case SkillEffectName.SlowDownEffect:
                    logic = new SlowDownEffect();
                    break;
            }

            if (logic != null)
                logic.Init(data);
            else
            {
                logic = new SkillEffectBase();
                logic.Init(data);
            }
            return logic;
        }

        public virtual void RemoveTriggerEvent()
        {
            NotifactionCenter.Instance.RemoveNotifacation(this);
        }

        public void SkillOver()
        {
            BreakSkill();
            if(buffList.Count > 0)
            {
                buffList.ForEach((buff) => {
                    buff.Dispose();
                });
            }    
        }

        public virtual void Dispose()
        {

        }
    }
}