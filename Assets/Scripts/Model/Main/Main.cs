using System;
using UnityEngine;
using Bepop.Core;
using System.Text.RegularExpressions;

public class Main : MonoBehaviour
{
    private MainUpdate mainUpdate;
    private void Start()
    {
        mainUpdate = new MainUpdate();
        mainUpdate.Start();
        //Test
        //RedDotMgr.Inst.InitRedDotTreeNode();
        UnityEngine.Profiling.Profiler.BeginSample("===============1");
        int a = 100;
        var obj = Boxer<int>.Box(ref a);
        Debug.Log(obj);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("===============2");
        int a1 = 10000;
        var obj1 = Boxer<int>.Box(ref a1);
        Debug.Log(obj1);
        Debug.Log(obj);
        UnityEngine.Profiling.Profiler.EndSample();

        string str = "<hero>123</hero>xxxxx2123<hero>32122</hero>";
        int id = 1;
        var mat = Regex.Match(str, "<hero>(.*?)</hero>");

        str = Regex.Replace(str, "<hero>(.*?)</hero>", (match) => {
            Debug.Log(match);
            int index = match.Value.IndexOf("<hero>");
            int endIndex = match.Value.IndexOf("</hero>");
            string str1 = match.Value.Substring(6, endIndex - index -6);
            return "";
        });
    }

    private void Update()
    {
        mainUpdate.OtherUpdate();
        mainUpdate.LogicUpdate();
    }
}
