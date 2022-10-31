using System;
using System.Collections.Generic;
using System.IO;
using Bepop.Core;
using FairyGUI.Utils;
using UnityEditor;

namespace Config
{
    internal static class I18nExt
    {
        public static string KeyToI18n(this string key)
        {
            if (I18nMgr.Instance.I18nDic.ContainsKey(key))
                return I18nMgr.Instance.I18nDic[key];
            return I18n.Instance.GetStr(key);
        }
    }

    public enum LanguageType
    {
        Chinese,
        English
    }

    internal class I18n : Singleton<I18n>
    {
        public string GetStr(string key)
        {
            var value = Instance.GetField<string>(key);
            return string.IsNullOrEmpty(value) ? key : value;
        }

        private I18n() { }
        public readonly static string shop_title = "商店";
    }

    public class I18nMgr : Singleton<I18nMgr>
    {
        private static string[] languageTags = new string[] { "Chinese", "Chinese", "English" };

        public Dictionary<string, string> I18nDic = new Dictionary<string, string>();

        private LanguageType _language = LanguageType.Chinese;

        public LanguageType Langeage
        {
            get {
                return _language;
            }
            set
            {
                if(_language != value)
                {
                    _language = value;
                    //本地存储下当前语言
                }
            }
        }

        private I18nMgr() { }

        public void ChangeLanguage(LanguageType type)
        {

            if (_language != type)
            {
                LoadFairyGuiI18n(type);
                switch (type)
                {
                    //切换fgui分之
                    case LanguageType.Chinese:
                        LoadStringI18n(LanguageType.Chinese);
                        FairyGUI.UIPackage.branch = "";
                        break;
                    case LanguageType.English:
                        LoadStringI18n(LanguageType.English);
                        FairyGUI.UIPackage.branch = "en";
                        break;
                }
            }
        }

        private string GetLanguageTag(LanguageType type)
        {
            switch (type)
            {
                case LanguageType.Chinese:
                    return "Chinese";
                case LanguageType.English:
                    return "English";
                default:
                    return "";
            }
        }

        private void LoadFairyGuiI18n(LanguageType type)
        {
            var suffix = GetLanguageTag(type);
            string url = string.Format($"Assets/Fgui/fgui_{suffix}.xml");
            if(File.Exists(url))
            {
                string xmlContent = File.ReadAllText(url);
                FairyGUI.Utils.XML xml = new FairyGUI.Utils.XML(xmlContent);
                FairyGUI.UIPackage.SetStringsSource(xml);
            }
            else
            {
                Log.UI.LogError($"File load Error:{url}");
            }
        }

        private void LoadStringI18n(LanguageType type)
        {
            string xmlContext = "";

            string xmlPath = string.Format($"Assets/Config/string_{GetLanguageTag(type)}.xml");
            if (File.Exists(xmlPath))
            {
                xmlContext = File.ReadAllText(xmlPath);
            }
            else
                Log.UI.LogError($"File Load Error:{xmlPath}");

            if(xmlContext.Length > 0)
            {
                XML xml = new XML(xmlContext);
                XMLList.Enumerator et = xml.GetEnumerator("string");
                while (et.MoveNext())
                {
                    XML ml = et.Current;
                    string key = ml.GetAttribute("key");
                    string value = ml.text;
                    if(!string.IsNullOrEmpty(value))
                    {
                        I18nDic[key] = value;
                        var i18n = I18n.Instance;
                        i18n.SetField(key, value);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void CheckRepetString()
        {
            var t = typeof(I18n);
            var fields = t.GetFields();
            Dictionary<string, string> StrKey = new Dictionary<string, string>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(I18n.Instance);
                if(value is string str)
                {
                    if (StrKey.ContainsKey(str))
                        Log.UI.LogWarning($"I18n 重复字符串:{str} key:{StrKey[str]},{field.Name}");
                    else
                        StrKey[str] = field.Name;
                }
            }
        }

        private Dictionary<string,string> GetTranslate(string suffix)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string xmlUrl = string.Format("Assets/Config/string_{0}.xml", suffix);
            if(File.Exists(xmlUrl))
            {
                string xmlContent = File.ReadAllText(xmlUrl);
                if(xmlContent.Length >0)
                {
                    XML xml = new XML(xmlContent);
                    XMLList.Enumerator et = xml.GetEnumerator("string");
                    while (et.MoveNext())
                    {
                        XML ml = et.Current;
                        string key = ml.GetAttribute("key");
                        string zh = ml.GetAttribute("zh");
                        string value = ml.text;
                        if (!string.IsNullOrEmpty(value))
                            dic[zh] = value;
                    }
                }
            }
            return dic;
        }

        public void SaveStringToXml()
        {
            if(_language != LanguageType.Chinese)
            {
                return;
            }
            int count = 0;
            var t = typeof(I18n);
            var fields = t.GetFields();
            Dictionary<string, string> keyStr = new Dictionary<string, string>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var value = field.GetValue(I18n.Instance);
                if(value is string str)
                {
                    count++;
                    keyStr[field.Name] = str;
                }
            }
            Log.UI.LogInfo($"i18n strs count :{count}");

            for (int i = 0; i < languageTags.Length; i++)
            {
                int unTrans = 0;
                string tag = languageTags[i];
                Dictionary<string, string> translate = GetTranslate(tag);
                XML xml = XML.Create("translate");
                var pair = keyStr.GetEnumerator();
                while (pair.MoveNext())
                {
                    var key = pair.Current.Key;
                    var zhStr = pair.Current.Value;
                    var node = XML.Create("string");
                    node.SetAttribute("key", key);
                    node.SetAttribute("zh", zhStr);

                    if (translate.ContainsKey(zhStr) && tag != "Chinese")
                        node.text = translate[zhStr];
                    else
                        node.text = zhStr;

                    if (zhStr == node.text)
                        unTrans++;
                    xml.AppendChild(node);
                }

                var strs = xml.ToXMLString(true);
                string path = $"Assets/Config/string_{tag}.xml";
                if (!File.Exists(path))
                    File.Create(path);
                File.WriteAllText(path, strs);
                Log.UI.LogInfo($"save file :{path}");
                if (tag != "Chinese")
                    Log.UI.LogInfo($"string_{tag} 未翻译数量:{unTrans}");
            }
        }

#endif
    }
}