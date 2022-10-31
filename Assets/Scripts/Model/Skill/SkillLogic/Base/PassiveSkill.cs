using System;
namespace Skill
{
    public class PassiveSkill : SkillBase
    {
        public override void Init(SkillData skillData, SoldierEntity owner)
        {
            base.Init(skillData, owner);
            //Add Trigger
        }

        public override void Use(SkillVector vector)
        {
            base.Use(vector);
            //GetSkillTargets(SkillVector.Zero)
        }
    }
}