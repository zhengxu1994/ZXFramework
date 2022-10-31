using System;
using UnityEngine;
using Effect;
using System.Collections.Generic;
namespace Skill
{
    public class SkillChooseBase
    {
        public SkillChooseType chooseType;

        public int skillRange = 400;//攻击距离


        protected bool refresh = true;

        protected int refreshNum = 5;

        protected int nowRefrshNum = 0;

        protected ParticleNode skillAreaNode = null;

        protected List<ParticleNode> particles = new List<ParticleNode>();

        public SkillChooseBase(SkillChooseType chooseType)
        {
            this.chooseType = chooseType;
        }

        public virtual void OnDragStart(SkillVector logic)
        {

        }

        public virtual void OnDragMove(SkillVector logic)
        {

        }

        public virtual void OnDragEnd(SkillVector logic,bool cancel)
        {

        }

        public virtual void OnDispose()
        {
            if (skillAreaNode != null)
                EffectMgr.Instance.RemoveNode(skillAreaNode.nodeId);
            for (int i = 0; i < particles.Count; i++)
            {
                EffectMgr.Instance.RemoveNode(particles[i].nodeId);
            }
            particles.Clear();
        }
    }
}
