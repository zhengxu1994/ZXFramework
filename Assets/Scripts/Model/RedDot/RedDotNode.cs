using System;
using System.Collections.Generic;
using static RedDotMgr;
/// <summary>
/// 红点结构层
/// 节点
/// </summary>
public class RedDotNode
{
    public string Name { get; private set; }

    /// <summary>
    /// 红点标记
    /// </summary>
    public string[] flag;

    public RedDotNode() { }

    public RedDotNode(string name,string[] flag)
    {
        this.Name = name;
        this.flag = flag;
    }

    public void SetRedDotNode(string name, string[] flag)
    {
        this.Name = name;
        this.flag = flag;
    }
    /// <summary>
    /// 红点数量
    /// </summary>
    public int pointNum { get; protected set; } = 0;

    public RedDotType dotType;

    /// <summary>
    /// 父节点
    /// </summary>
    public RedDotNode parent = null;
    /// <summary>
    /// 子节点
    /// </summary>
    public Dictionary<string, RedDotNode> childs = null;

    public OnRedDotNumChange onRedDotNumChange;

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public bool IsLeaf => childs == null || childs.Count <= 0;
    /// <summary>
    /// 判断是否有改类型的子节点
    /// </summary>
    /// <param name="dotType"></param>
    /// <returns></returns>
    public bool ContainNode(string dotType)
    {
        return !IsLeaf ? childs.ContainsKey(dotType) : false;
    }

    public virtual void CheckRedDot()
    {

    }

    public void AddChildNode(RedDotNode node,RedDotType type)
    {
        if (childs == null) childs = new Dictionary<string, RedDotNode>();
        if(!childs.ContainsKey(node.Name))
        {
            childs[node.Name] = node;
            node.parent = this;
            node.dotType = type;
        }
    }

    public void RemoveChildNode(RedDotNode node)
    {
        if (childs == null || childs.Count <= 0) return;
        if (childs.ContainsKey(node.Name))
            childs.Remove(node.Name);
        node.parent = null;
    }

    public RedDotNode GetChildNode(string name)
    {
        if (childs == null) return null;
        return childs.ContainsKey(name) ? childs[name] : null;
    }

    public void SetRedDotValue(int num)
    {
        //只有叶子节点可以设置
        if (!IsLeaf) return;
        pointNum = num;
        
        NotifyRedDotNumChange();
        //递归通知
        if (parent != null)
            parent.ChangeRedDotNum();
    }

    public void ChangeRedDotNum()
    {
        int num = 0;
        foreach (var child in childs)
        {
            num += child.Value.pointNum;
        }
        //红点数量发生变化处理自身红点情况并通知父节点
        if (num != pointNum)
        {
            pointNum = num;
            NotifyRedDotNumChange();
        }
        if (parent != null)
            parent.ChangeRedDotNum();
    }

    public void NotifyRedDotNumChange()
    {
        onRedDotNumChange?.Invoke(this);
    }
}


public class RedDot_Mail_System : RedDotNode
{
    public RedDot_Mail_System() { }
    public RedDot_Mail_System(string name, string[] flag) : base(name, flag)
    {
    }
}

public class RedDot_Mail_Tream : RedDotNode
{
    public RedDot_Mail_Tream() { }
    public RedDot_Mail_Tream(string name, string[] flag) : base(name, flag)
    {
    }
}