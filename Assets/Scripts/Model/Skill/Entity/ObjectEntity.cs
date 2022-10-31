using System;
using UnityEngine;
namespace Skill
{
    public class ObjectEntity
    {
        public int entityId;

        public Vector2 pos;

        public float lastUseSkillTime = -1;

        public float skillCD = 10;//test 10

        public GameObject Obj { get; protected set; }

        public int campId;

        public void SetGameObject(GameObject obj)
        {
            this.Obj = obj;
            pos = obj.transform.position;
        }

        public virtual void PlaySound(string soundName)
        {

        }

        public virtual void PlayAction(string actionName)
        {

        }
    }
}
