using Resource;
using UnityEngine;
using System.Collections.Generic;

namespace Bepop.Core.UI
{
    partial class SkillEditor : PanelBase
    {
        
        public override void OnCreate()
        {
            base.OnCreate();
        }

        public void Init()
        {
            //先将人物创建 动画播放  特效播放 以及技能效果结合起来
            btn_createHero.onClick.Set(() => {
                
            });

            btn_play.onClick.Set(() => {

            });

            btn_pause.onClick.Set(() => { });

            btn_next.onClick.Set(() => { });

            btn_Import.onClick.Set(() => { });
        }
    }
}
