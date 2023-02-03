using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bepop.Core.UI;
using Sirenix;
using Resource;
using Bepop.Core;
using YooAsset;
using FairyGUI;

public class SkillEditorMgr : MonoSingleton<SkillEditorMgr>
{
    [SerializeField]
    private SkillEditor skillEditor;

    public void Start()
    {
        StartCoroutine(ResUpdateManager.Instance.StartUpdate(this, () => {
            Debug.Log("ÏÂÔØ»Øµ÷");
            FguiExtension.SetExtension("ui://SkillEditor/SkillEditor_Down", ObjectType.Component,(com)=> {
                ViewBase ret = ViewBase.Create(typeof(SkillEditor_Down), com);
            });
            LoadSkillEditor();
        }));
    }
    public void LoadSkillEditor()
    {
        skillEditor = UIManager.Instance.OpenPanel<SkillEditor>() as SkillEditor;
        skillEditor.Init();
    }
}
