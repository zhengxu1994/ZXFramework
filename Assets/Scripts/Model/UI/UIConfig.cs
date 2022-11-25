using System.Collections.Generic;
using Config;

namespace Bepop.Core.UI
{
    public partial class UIManager
    {
        private Dictionary<string, UIData> _uiDatas = new Dictionary<string, UIData>();

        private void InitUIConfigWithCode()
        {
            AddUI("Login", "LoginUI").SetRoot(true);

            AddUI("Loading", "LoadingUI").SetLayer(UILayerEnum.Waiting).SetFullScreen(true);

            AddUI("Main", "MainUI", "", CommonTopState.Main).SetRoot(true);

            AddUI("Setting", "SettingUI").SetModal(true);

            AddUI("Shop", "ShopUI", "shop_title").SetFullScreen(true).SetHideBlow(true);

            AddUI("SkillEditor", "SkillEditor").SetRoot(true);
        }

        private  void InitUIConfigWithData()
        {

        }
       

        private UIData AddUI(string package,string name,string title = null, CommonTopState topState = CommonTopState.Normal)
        {
            UIData data = new UIData(name, package);

            if (title != null)
            {
                data.SetTopinfo(new TopUIInfo(name, title, topState));
            }

            _uiDatas.Add(name, data);
            return data;
        }

        private UIData GetUIData(string name)
        {
            if (_uiDatas.ContainsKey(name))
                return _uiDatas[name];
            return null;
        }
    }

    public class TopUIInfo
    {
        public string PanelName { get; private set; }

        public string Title => string.IsNullOrEmpty(_titleKey) ? "" : _titleKey.KeyToI18n();
        private string _titleKey;

        public CommonTopState Ctr_State { private set; get; } = CommonTopState.Normal;

        public TopUIInfo(string panel,string title,CommonTopState state)
        {
            PanelName = panel;
            _titleKey = title;
            Ctr_State = state;
        }
    }

    public class UIData
    {
        public bool IsRoot { private set; get; } = false;

        public bool IsModal { private set; get; } = false;

        public bool IsFullScreen { private set; get; } = false;

        public bool IsHideBlow { private set; get; } = true;

        public bool IsBatching { private set; get; } = false;

        public UILayerEnum UILayer { private set; get; } = UILayerEnum.UI;

        //UI类名称
        public string Name { private set; get; } = "";
        //UI资源包名
        public string Package { private set; get; } = "";
        //UI资源名
        public string Resource { private set; get; } = "";

        public string OpenSound { private set; get; } = "";

        public string CloseSound { private set; get; } = "";

        public TopUIInfo TopInfo { private set; get; } = null;

        public UIData(string name,string package)
        {
            Name = Resource = name;
            Package  = package;
        }

        public UIData SetFullScreen(bool value)
        {
            IsFullScreen = value;
            return this;
        }

        public UIData SetModal(bool value)
        {
            if (value)
                IsHideBlow = false;
            IsModal = value;
            UILayer = UILayerEnum.Cover;
            return this;
        }

        public UIData SetLayer(UILayerEnum value)
        {
            if (this.UILayer == UILayerEnum.Cover && value != UILayerEnum.Cover)
                Log.UI.LogWarning($"UI Have Modal State,But Set Layer is Not Cover,Layer Is {value}");
            UILayer = value;
            return this;
        }

        public UIData SetHideBlow(bool value)
        {
            IsHideBlow = value;
            return this;
        }

        public UIData SetRoot(bool value)
        {
            IsRoot = value;

            if (IsRoot)
            {
                IsFullScreen = true;
                IsModal = false;
            }

            return this;
        }

        public void SetTopinfo(TopUIInfo info)
        {
            TopInfo = info;
        }
    }
}