using System;
using System.Collections.Generic;
using System.Reflection;
/*
红点系统描述及规则
1.支持多层级树结构,支持显示数量
2.可动态注册节点事件或撤销节点事件
3.需要显示红点的UI会注册一个节点的Key，可以是叶子结点也可以是非叶子结点
4.在合适的时候会标记结点的状态（只能编辑叶子结点，非叶子结点状态由子节点决定）
5.非叶子结点：当有一个或多个子节点处于激活状态时，该节点被激活

红点管理：
1.注册红点
2.注销红点
3.检测红点
4.设置红点
红点节点：
红点名称
1.红底类型 标记
2.子节点信息
3.值 已读/值控制
4.添加删除子节点
5.是否是子节点
6.通知 父节点 设置节点信息

//https://zhuanlan.zhihu.com/p/86069641
红点系统分为3层 结构层 ， 驱动层 ，表现层
驱动层 ：RedDotMgr
结构层 ：RedTreeNode
表现层 ：RedDotItem
红点定义: RedDotConst
*/


/// <summary>
/// 红点管理器
/// </summary>
public class RedDotMgr  
{
    public static RedDotMgr Inst = new RedDotMgr();

    public delegate void OnRedDotNumChange(RedDotNode node);//红点变化通知

    public RedDotNode root = new RedDotNode();//红点root节点

    private Dictionary<string, string[]> nodeSpiltStr = new Dictionary<string, string[]>();

    /// <summary>
    /// 初始化树结构
    /// </summary>
    public void InitRedDotTreeNode()
    {
        //利用反射获取变量
        FieldInfo[] fields = typeof(RedDotConst).GetFields(BindingFlags.Static | BindingFlags.Public);
        RedDotConst consts = new RedDotConst();
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if(field.GetValue(consts) is string dot)
            {
                //根据定义的变量初始化红点树
                RedDotType type = RedDotType.Normal;
                var dotStr = dot.Split('_');
                if (!nodeSpiltStr.ContainsKey(dot)) nodeSpiltStr.Add(dot,dotStr);
                BuildTree(dotStr, type);
            }
        }
    }

    private void BuildTree(string[] flag, RedDotType type = RedDotType.Normal)
    {
        var node = root;
        //遍历标记 创建对应标记的红点
        for (int i = 0; i < flag.Length; i++)
        {
            if (!node.ContainNode(flag[i]))
            {
                //获取当前索引位置的标记 并创建对应的红点类 如果没有则使用默认的
                //通过类名 动态去创建
                var fullClassName = flag.CutArrayJointToString(i + 1, '_');
                fullClassName = $"RedDot_{fullClassName}";
                if (string.IsNullOrEmpty(fullClassName))
                    continue;
                var clsType = fullClassName.GetClassType("");
                RedDotNode childNode = null;
                if (clsType == null)
                {
                    //log
                    //创建默认cls
                    childNode = new RedDotNode(flag[i], flag.CutArrayJointToStringArray(i + 1));
                }
                else
                {
                    childNode = clsType.GetClassInstance() as RedDotNode;
                    childNode.SetRedDotNode(flag[i], flag.CutArrayJointToStringArray(i + 1));

                }
                node.AddChildNode(childNode, type);
            }
            node = node.GetChildNode(flag[i]);
        }
        node.dotType = type;
    }
    /// <summary>
    /// 注册红点数量监听事件
    /// </summary>
    /// <param name="strNode"></param>
    /// <param name="callback"></param>
    public void SetRedDotNodeCallBack(string strNode,OnRedDotNumChange callback)
    {
        string[] nodeList = null;
        if (nodeSpiltStr.ContainsKey(strNode))
            nodeList = nodeSpiltStr[strNode];
        else
        {
            nodeList = strNode.Split('_');
            nodeSpiltStr.Add(strNode, nodeList);
        }
        var node = root;
        for (int i = 0; i < nodeList.Length; i++)
        {
            var childNode = node.GetChildNode(nodeList[i]);
            if(childNode == null)
            {
                //log error
                return;
            }
            node = childNode;
            if (i == nodeList.Length - 1)
                node.onRedDotNumChange = callback;
        }
    }
    /// <summary>
    /// 设置红点值
    /// </summary>
    /// <param name="strNode"></param>
    /// <param name="num"></param>
    public void SetRedDotValue(string strNode,int num)
    {
        string[] nodeList = null;
        if (nodeSpiltStr.ContainsKey(strNode))
            nodeList = nodeSpiltStr[strNode];
        else
        {
            nodeList = strNode.Split('_');
            nodeSpiltStr.Add(strNode, nodeList);
        }

        var node = root;
        for (int i = 0; i < nodeList.Length; i++)
        {
            if (!node.ContainNode(nodeList[i]))
            {
                //log
                return;
            }
            node = node.GetChildNode(nodeList[i]);
            //只能设置叶子节点值 然后通过叶子节点递归通知父节点
            if (i == nodeList.Length - 1)
                node.SetRedDotValue(num);
        }
    }

    public void OnDispose()
    {
        nodeSpiltStr.Clear();
    }
}
