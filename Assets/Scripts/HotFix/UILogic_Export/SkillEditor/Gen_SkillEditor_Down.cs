/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;

namespace Bepop.Core.UI
{
    partial class ExtensionList
    {
        public ExtInfo e_SkillEditor_Down = new ExtInfo() { ExtClass = typeof(SkillEditor_Down), ExtUrl = "ui://SkillEditor/SkillEditor_Down", ExtType = ObjectType.Component };
    }
    partial class SkillEditor_Down : ViewBase
    {
        public static readonly string Url = "ui://SkillEditor/SkillEditor_Down";

        public override void OnCreate() { }
        public GTextField txt_name { get; private set; }

        public override void AutoBinderUI()
        {
            txt_name = UIObj.GetChildAt<GTextField>(0);
        }

    }
}