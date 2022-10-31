using System;
using UnityEngine;

namespace Skill
{
    public class SkillChooseRectangle : SkillChooseBase
    {
        public Transform arrow;
        public Material arrowMat;

        public float rectangleWidth;

        public SkillChooseRectangle(SkillChooseType chooseType) : base(chooseType)
        {
        }

        public virtual void SetArrowMat(int dis)
        {
            arrow.localScale = new Vector3(rectangleWidth, dis, 1);
            if(arrow != null)
            {
                if(rectangleWidth > dis)
                {
                    arrowMat.SetTextureOffset("_MainTex", new Vector2(0, 0));
                    arrowMat.SetTextureScale("_MainTex", new Vector2(1, 1));
                }
                else
                {
                    arrowMat.SetTextureOffset("_MainTex", new Vector2(0, -dis / rectangleWidth + 1));
                    arrowMat.SetTextureScale("_MainTex", new Vector2(1, dis / rectangleWidth));
                }    
            }
            arrow.transform.localPosition = new Vector3(0, dis / 2, 0);
        }
    }
}
