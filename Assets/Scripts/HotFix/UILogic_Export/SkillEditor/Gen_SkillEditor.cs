/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;

namespace Bepop.Core.UI
{
    partial class ExtensionList
    {
        public ExtInfo e_SkillEditor = new ExtInfo() { ExtClass = typeof(SkillEditor), ExtUrl = "ui://SkillEditor/SkillEditor", ExtType = ObjectType.Component };
    }
    partial class SkillEditor
    {
        public static readonly string Url = "ui://SkillEditor/SkillEditor";

        public GButton btn_next { get; private set; }
        public GButton btn_pause { get; private set; }
        public GButton btn_play { get; private set; }
        public GSlider slider_frame_Slider { get; private set; }

        public float btn_next_x { get; private set; } = 0;
        public float btn_next_y { get; private set; } = 0;

        public float btn_pause_x { get; private set; } = 0;
        public float btn_pause_y { get; private set; } = 0;

        public float btn_play_x { get; private set; } = 0;
        public float btn_play_y { get; private set; } = 0;

        public float slider_frame_Slider_x { get; private set; } = 0;
        public float slider_frame_Slider_y { get; private set; } = 0;

        public override void AutoBinderUI()
        {
            btn_next = UIObj.GetChildAt<GButton>(2);
            btn_pause = UIObj.GetChildAt<GButton>(1);
            btn_play = UIObj.GetChildAt<GButton>(0);
            slider_frame_Slider = UIObj.GetChildAt<GSlider>(3);
        }

        public override void InitObjPosition()
        {
            btn_next_x = btn_next.x;
            btn_next_y = btn_next.y;
            btn_pause_x = btn_pause.x;
            btn_pause_y = btn_pause.y;
            btn_play_x = btn_play.x;
            btn_play_y = btn_play.y;
            slider_frame_Slider_x = slider_frame_Slider.x;
            slider_frame_Slider_y = slider_frame_Slider.y;
        }
    }
}