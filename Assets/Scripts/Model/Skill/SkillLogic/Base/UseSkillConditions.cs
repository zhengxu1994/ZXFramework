using System;
namespace Skill.UseSkillCondition
{
    public enum CheckUseSkillConditionType
    {
        None,
        CD,
        Magic
    }
    //技能是否可以释放条件判断

    public interface ICheckUseSkillCondition
    {
        bool CheckCondition(ObjectEntity entity);
    }

    public class CheckCanUseCondition : ICheckUseSkillCondition
    {
        public CheckUseSkillConditionType conditionType;
        public virtual bool CheckCondition(ObjectEntity entity)
        {
            return true;
        }
    }

    public class CDCondition : CheckCanUseCondition
    {
        public CDCondition()
        {
            conditionType = CheckUseSkillConditionType.CD;
        }
        public override bool CheckCondition(ObjectEntity entity)
        {
            return true;//cd 计算
        }
    }

    public class MagicCondition : CheckCanUseCondition
    {
        public MagicCondition()
        {
            conditionType = CheckUseSkillConditionType.Magic;
        }

        public override bool CheckCondition(ObjectEntity entity)
        {
            return true;//蓝量计算
        }
    }
}