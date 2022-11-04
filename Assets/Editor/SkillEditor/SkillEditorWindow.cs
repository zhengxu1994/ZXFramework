using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

#if UNITY_EDITOR
public class SkillEditorWindow : OdinEditorWindow
{
    [MenuItem("Tools/SkillEditor")]
    private static void OpenWindow()
    {
        var window = GetWindow<SkillEditorWindow>();

        // Nifty little trick to quickly position the window in the middle of the editor.
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1136, 640);
    }
    [Sirenix.OdinInspector.FilePath]
    public string skillDataPath;


}
#endif