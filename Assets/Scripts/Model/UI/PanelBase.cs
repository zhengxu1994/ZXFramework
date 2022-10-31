using System;
using FairyGUI;
using Notifaction;

namespace Bepop.Core.UI
{
    /// <summary>
    /// UI界面的基类
    /// </summary>
    public class PanelBase : UIBase
    {
        public UILayerEnum Layer = UILayerEnum.UI;

        private GGraph _modal = null;
        public GGraph Modal => _modal;
        public int SortingOrder
        {
            get => UIObj.sortingOrder;
            protected set
            {
                UIObj.sortingOrder = value;
                OnChangeSortingOrder();
            }
        }

        public bool IsPause { get; private set; }

        public virtual void OnUpdate() { }

        public virtual void OnShow(NotifyParam param)
        {
            Visible = true;
            Param = param;
            if (_modal != null)
                _modal.visible = true;
        }

        public virtual void OnHide()
        {
            Visible = false;
            NotifyParam.UINotifyParamPool.Recycle(Param);
            Param = null;
            if (_modal != null)
                _modal.visible = false;
        }

        public virtual void OnPause()
        {
            if (!IsPause)
                IsPause = true;
        }

        public virtual void OnResume()
        {
            if (IsPause)
                IsPause = false;
        }

        public virtual void OnSafeAreaChange(int safeSpace)
        {

        }

        public virtual void OnChangeSortingOrder() { }


        public override void OnCreate()
        {
           
        }

        public override void Dispose()
        {
            _modal?.Dispose();
            _modal = null;
            base.Dispose();
        }

        public GGraph CreateModal()
        {
            if (_modal != null) return _modal;
            _modal = new GGraph();
            _modal.SetSize(GRoot.inst.width, GRoot.inst.height);
            _modal.DrawRect(GRoot.inst.width, GRoot.inst.height, 1, new UnityEngine.Color(0, 0, 0),
                new UnityEngine.Color(0, 0, 0, 0.7f));
            _modal.onClick.Set((EventCallback0)(() =>
            {
                UIManager.Instance.RemovePanel(this);
            })
            );
            return _modal;
        }
    }
}
