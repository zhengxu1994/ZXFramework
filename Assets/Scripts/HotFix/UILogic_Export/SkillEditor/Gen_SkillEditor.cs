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

        public GButton btn_Import { get; private set; }
        public GButton btn_createHero { get; private set; }
        public GButton btn_next { get; private set; }
        public GButton btn_pause { get; private set; }
        public GButton btn_play { get; private set; }
        public GTextInput input_heroId { get; private set; }
        public GTextInput input_skillId { get; private set; }
        public GTextInput input_totalFrame { get; private set; }
        public GSlider slider_frame_Slider { get; private set; }

        public float btn_Import_x { get; private set; } = 0;
        public float btn_Import_y { get; private set; } = 0;

        public float btn_createHero_x { get; private set; } = 0;
        public float btn_createHero_y { get; private set; } = 0;

        public float btn_next_x { get; private set; } = 0;
        public float btn_next_y { get; private set; } = 0;

        public float btn_pause_x { get; private set; } = 0;
        public float btn_pause_y { get; private set; } = 0;

        public float btn_play_x { get; private set; } = 0;
        public float btn_play_y { get; private set; } = 0;

        public float input_heroId_x { get; private set; } = 0;
        public float input_heroId_y { get; private set; } = 0;

        public float input_skillId_x { get; private set; } = 0;
        public float input_skillId_y { get; private set; } = 0;

        public float input_totalFrame_x { get; private set; } = 0;
        public float input_totalFrame_y { get; private set; } = 0;

        public float slider_frame_Slider_x { get; private set; } = 0;
        public float slider_frame_Slider_y { get; private set; } = 0;

        public override void AutoBinderUI()
        {
            btn_Import = UIObj.GetChildAt<GButton>(5);
            btn_createHero = UIObj.GetChildAt<GButton>(8);
            btn_next = UIObj.GetChildAt<GButton>(2);
            btn_pause = UIObj.GetChildAt<GButton>(1);
            btn_play = UIObj.GetChildAt<GButton>(0);
            input_heroId = UIObj.GetChildAt<GTextInput>(7);
            input_skillId = UIObj.GetChildAt<GTextInput>(4);
            input_totalFrame = UIObj.GetChildAt<GTextInput>(6);
            slider_frame_Slider = UIObj.GetChildAt<GSlider>(3);
        }

        public override void InitObjPosition()
        {
            btn_Import_x = btn_Import.x;
            btn_Import_y = btn_Import.y;
            btn_createHero_x = btn_createHero.x;
            btn_createHero_y = btn_createHero.y;
            btn_next_x = btn_next.x;
            btn_next_y = btn_next.y;
            btn_pause_x = btn_pause.x;
            btn_pause_y = btn_pause.y;
            btn_play_x = btn_play.x;
            btn_play_y = btn_play.y;
            input_heroId_x = input_heroId.x;
            input_heroId_y = input_heroId.y;
            input_skillId_x = input_skillId.x;
            input_skillId_y = input_skillId.y;
            input_totalFrame_x = input_totalFrame.x;
            input_totalFrame_y = input_totalFrame.y;
            slider_frame_Slider_x = slider_frame_Slider.x;
            slider_frame_Slider_y = slider_frame_Slider.y;
        }
    }
}