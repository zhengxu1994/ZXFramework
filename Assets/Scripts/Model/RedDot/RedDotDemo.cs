using System;
using UnityEngine;
using UnityEngine.UI;
public class RedDotDemo : MonoBehaviour
{
    public Button[] buttons;
    public Text[] txts;
    private void Start()
    {
        RedDotMgr.Inst.InitRedDotTreeNode();

        RedDotMgr.Inst.SetRedDotNodeCallBack(RedDotConst.Mail, (dot) => {
            txts[4].enabled = dot.pointNum > 0;
            txts[4].text = dot.pointNum.ToString();
        });

        RedDotMgr.Inst.SetRedDotNodeCallBack(RedDotConst.Mail, (dot) => {
            txts[1].enabled = dot.pointNum > 0;
            txts[1].text = dot.pointNum.ToString();
        });
        RedDotMgr.Inst.SetRedDotNodeCallBack(RedDotConst.Mail_System, (dot) => {
            txts[2].enabled = dot.pointNum > 0;
            txts[2].text = dot.pointNum.ToString();
        });

        RedDotMgr.Inst.SetRedDotNodeCallBack(RedDotConst.Mail_Tream, (dot) => {
            txts[3].enabled = dot.pointNum > 0;
            txts[3].text = dot.pointNum.ToString();
        });

        RedDotMgr.Inst.SetRedDotValue(RedDotConst.Mail_System, 1);
        RedDotMgr.Inst.SetRedDotValue(RedDotConst.Mail_Tream, 1);
    }
}
