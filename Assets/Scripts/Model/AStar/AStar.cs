using Bepop.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyAi;
public class AStarManager : Singleton<AStarManager>
{
    public AStar astar;
    private AStarManager()
    {
        
    }

    public void Init(int x, int y, HashSet<(int, int)> blockIndexs)
    {
        astar = new AStar(); 
        astar.Init(x, y,blockIndexs);
    }
}

public class AStarNode
{
    public int x;
    public int y;
    public bool isObstacle;

    public AStarNode parent = null;

    public int  g, h;

    public int f { get { return g + h; } }
    public AStarNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Reset()
    {
        parent = null;
        g = h = 0;
    }
}


public class AStar 
{
    //f = g + h
    //开放列表
    public HashSet<AStarNode> openList = new HashSet<AStarNode>();
    //关闭列表
    public HashSet<AStarNode> closeList = new HashSet<AStarNode>();

    private AStarNode[,] Map = null;

    private int MinX,MaxX,MinY,MaxY;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Init(int x, int y, HashSet<(int, int)> blockIndex)
    {
        Map = new AStarNode[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                AStarNode node = new AStarNode(i, j);
                Map[i, j] = node;
                if (blockIndex.Contains((i, j)))
                    node.isObstacle = true;
            }
        }
        MinX = MinY = 0;
        MaxX = x;
        MaxY = y;
    }
    #region A星部分
    //步骤
    //1.首先将根节点加入到开放列表中
    //2.在开发列表中找出总消耗最低的点 优先级 f -> h 如果当前点为终点则搜索完毕
    //3.搜索当前点八方向上的点 计算g行动消耗 并加入开放列表 如果计算出的行动消耗比原来的消耗低 则更新g
    //4.将八方向上的点（排除不能行走的部分）的父对象设置为当前点
    //5.重复循环2 - 3 - 4 直到找到目标点 或者 没有目标了
    #endregion

    #region JPS跳点
    //1.首先将根节点加入到开放列表中
    //2.在开发列表中找出总消耗最低的点 优先级 f -> h 如果当前点为终点则搜索完毕
    //3.搜索上下左右方向依次往前搜索 再其他八方向上搜索
    //3.1 搜索到阻挡格或者是边界时返回
    //3.2 搜索到终点时直接返回结果
    //3.3 搜索到跳点时将跳点的父节点加入到开放列表中（拥有强迫邻居的点为跳点）
    //3.4 更新父节点的行动消耗
    //4. 
    //重复循环2-3-4
    #endregion


    #region A* 和 jps的区别
    //A ∗算法的邻居节点为几何意义上的邻居，而JPS算法的邻居节点为跳跃所得的邻居。
    #endregion
    public AStarNode IndexToNode((int,int) index)
    {
        return Map[index.Item1, index.Item2];
    }

    public List<TrueSync.TSVector2> GetAStar((int, int) start, (int, int) end)
    {
        var startPos = IndexToNode(start);
        var endPos = IndexToNode(end);
        return GetAStarByTS(startPos, endPos);
    }

    private void ResetNode()
    {
        for (int i = 0; i < MaxX; i++)
        {
            for (int j = 0; j < MaxY; j++)
            {
                Map[i, j].Reset();
            }
        }
    }

    public List<AStarNode> GetAStar(AStarNode start, AStarNode end)
    {
        AStarNode node = GetAStarLine(start, end);
        if (node != null)
        {
            List<AStarNode> nodes = new List<AStarNode>();
            nodes.Add(node);
            while(node.parent != null)
            {
                nodes.Add(node.parent);
                node = node.parent;
            }
            nodes.Reverse();
            return nodes;
        }
        return null;
    }

    public List<TrueSync.TSVector2> GetAStarByTS(AStarNode start, AStarNode end)
    {
        AStarNode node = GetAStarLine(start, end);
        if (node != null)
        {
            List<TrueSync.TSVector2> nodes = new List<TrueSync.TSVector2>();

            nodes.Add(MoveManager.Instance.CellToPos(node.x, node.y));
            while (node.parent != null)
            {
                nodes.Add(MoveManager.Instance.CellToPos(node.parent.x, node.parent.y));
                node = node.parent;
            }
            nodes.Reverse();
            return nodes;
        }
        return null;
    }

    private AStarNode GetAStarLine(AStarNode start, AStarNode end)
    {
        ResetNode();
        openList.Clear();
        closeList.Clear();
        openList.Add(start);

        AStarNode current = null;
        while (openList.Count > 0)
        {
            current = null;
            openList.ForEach((k) =>
            {
                if (current == null) current = k;
                //花费相同的情况下 选择距离终近的
                if (k.f < current.f || (k.f == current.f && k.h < current.h))
                    current = k;
            });

            openList.Remove(current);
            closeList.Add(current);

            if (current == end)
                break;

            int x = current.x;
            int y = current.y;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    if (CheckOutSide(i, j))
                        continue;
                
                    var checkNode = Map[i, j];

                    if (checkNode.isObstacle || closeList.Contains(checkNode))
                        continue;


                    //更新当前点周围的点的行动消耗
                    int newgCost = current.f + GetDistance(current, checkNode);
                    bool inOpenList = openList.Contains(checkNode);
                    //不在开放列表中加入开放列表 如果有更优的行动方式 则更新行动方式
                    if (newgCost < checkNode.g || !inOpenList)
                    {
                        checkNode.g = newgCost;
                        checkNode.h = GetDistance(checkNode, end);
                        checkNode.parent = current;

                        if (!inOpenList)
                            openList.Add(checkNode);
                    }
                }
            }
           
        }

        return current;
    }

    private int GetDistance(AStarNode current,AStarNode point)
    {
        int distanceX = (int)Mathf.Abs(current.x - point.x);
        int distanceY = (int)Mathf.Abs(current.y - point.y);
        if (distanceX > distanceY)
        {
            return 14 * (distanceY) + 10 * (distanceX - distanceY);
        }
        else
            return 14 * (distanceX) + 10 * (distanceY - distanceX);
    }

    private bool CheckOutSide(int x,int y)
    {
        if (x < MinX || x >= MaxX || y < MinY || y >= MaxY)
            return true;
        return false; 
    }

}
