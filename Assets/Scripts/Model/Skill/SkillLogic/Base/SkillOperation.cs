using System;
namespace Skill.SkillOperation
{
    //技能操作（触发与操作时一起的，当触发什么事件时进行什么技能操作  比如播放特效 播放声音 播放具体动作）
    public interface ISkillOperation
    {
        void Operation(ObjectEntity target, SkillParam param);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public class PlaySoundOperation : ISkillOperation
    {
        public void Operation(ObjectEntity target, SkillParam param)
        {
            target.PlaySound(param.Str("Sound"));//test
        }
    }

    public class PlayActionOperation : ISkillOperation
    {
        public void Operation(ObjectEntity target, SkillParam param)
        {
            target.PlayAction(param.Str("Action"));
        }
    }
}