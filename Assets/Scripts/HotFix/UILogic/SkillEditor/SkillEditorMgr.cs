using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bepop.Core.UI;
using Sirenix;
using Resource;
using Bepop.Core;

public class SkillEditorMgr : Singleton<SkillEditorMgr>
{
    [SerializeField]
    private SkillEditor skillEditor;


    public void LoadSkillEditor()
    {
        skillEditor = UIManager.Instance.OpenPanel<SkillEditor>() as SkillEditor;
        skillEditor.Init();
    }
}
