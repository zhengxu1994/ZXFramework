using System;
using System.Collections.Generic;
using Bepop.Core;

namespace Skill
{
    public enum CampType
    {
        Self,
        Friend,
        Enemy
    }
    public class SoldierEntity : ObjectEntity
    {
        public bool sex;
        public int atk;
        public int def;
        public int hp;
        public bool isAlive;
        public int moveSpeed;

        public bool isSolider;

        protected Dictionary<int, Buff> buffs = new Dictionary<int, Buff>();

        public SkillBase AddSkill(SkillData skillData)
        {
            SkillBase skill = new SkillBase();
            skill.Init(skillData,this);
            return skill;
        }

        public void Hurt(int damage,SoldierEntity attack)
        {
            hp -= damage;
            if(hp <= 0)
            {
                hp = 0;
                Log.BASE.LogInfo("soldier is death");
            }
            Log.BASE.LogInfo($"soldier is hurt :{damage}");
        }


        public void ChangeSpeed(int changeValue)
        {
            moveSpeed += changeValue;
            Log.BASE.LogInfo($"change speed :{changeValue}");
        }

        public void UseSkill()
        {

        }

        public void AddBuff(Buff buff)
        {
            if(buffs.ContainsKey(buff.buffId))
            {
                Log.BASE.LogWarning($"has buff Id:{buff.buffId}");
                return;
            }

            buffs.Add(buff.buffId,buff);
        }

        public void RemoveBuff(Buff buff)
        {
            if (buffs.ContainsKey(buff.buffId))
                buffs.Remove(buff.buffId);

            buff.Dispose();
        }
    }
}
