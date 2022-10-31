using System;
namespace Bepop.Core.UI
{
    /// <summary>
    /// 玩家Top信息界面
    /// </summary>
    public abstract class CommonTopBase : ViewBase
    {

        public abstract void AddPlayerInfoChangeEvent();

        public abstract void RemovePlayerInfoChangeEvent();
        
        public virtual void SetCommonTopInfo(UIData uIData,bool show,bool showAnim = true)
        {
            if(!show)
            {
                this.Visible = false;
                OnHide();
            }
            else
            {
                if(this.Visible == false)
                {
                    this.Visible = true;
                    OnShow();
                }
            }
        }


        public virtual void OnHide()
        {
            RemovePlayerInfoChangeEvent();
        }

        public virtual void OnShow()
        {
            AddPlayerInfoChangeEvent();
        }
    }
}