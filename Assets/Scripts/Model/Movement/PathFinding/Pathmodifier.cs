using System;
using System.Collections.Generic;
using Pathfinding;
using TrueSync;
namespace Movement
{
    public class Pathmodifier
    {
        List<TSVector2> nodesByPos = new List<TSVector2>();
        Func<int, int, bool> checkBlock;
        FP gridSize;
        FP multGridSize;
        FP gridHalf;
        FP gridQuarter;

        public Pathmodifier(int gridSize, Func<int, int, bool> checkFunc)
        {
            this.gridSize = gridSize;
            multGridSize = 1f / gridSize;
            gridHalf = gridSize / 2f;
            gridQuarter = gridSize / 4f;
            checkBlock = checkFunc;
        }

        public TSVector2 CellToPos(FPoint cell, FP offsetx, FP offsety)
        {
            return new TSVector2(cell.x * gridSize + offsetx, cell.y * gridSize + offsety);
        }

        /// <summary>
        /// 传入的路点是格子坐标，返回的新路点是像素坐标
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public List<TSVector2> ApplyCenter(List<FPoint> paths, TSVector2 startPos, TSVector2 endPos)
        {
            List<TSVector2> newPath = new List<TSVector2>();
            if (paths.Count < 2 || paths[0].Equals(paths[1]))
            {
                return null;
            }

            TSVector2 point1;
            TSVector2 point2;

            int startIdx = 0;
            int endIdx = paths.Count - 1;
            int checkIdx = endIdx;
            //起点格 偏移点
            FP ox = gridHalf;
            FP oy = gridHalf;

            while (checkIdx > 0 && startIdx < endIdx - 1)
            {
                point1 = CellToPos(paths[startIdx], ox, oy);
                point2 = CellToPos(paths[checkIdx], ox, oy);

                if (checkIdx == startIdx + 1 || !HasBarrier(point1, point2))
                {
                    newPath.Add(point2);
                    startIdx = checkIdx;
                    checkIdx = endIdx;
                }
                else
                    checkIdx--;
            }
            if (newPath.Count < 1)
                newPath.Add(CellToPos(paths[endIdx], ox, oy));
            else
            {
                var p1 = newPath[newPath.Count - 1];
                var p2 = paths[endIdx];
                if (p1.x != p2.x || p1.y != p2.y)
                    newPath.Add(CellToPos(paths[endIdx], ox, oy));
            }

            AddIntersectionToPath(ref newPath, startPos, endPos);
            return newPath;
        }

        public List<TSVector2> ApplyPos(List<FPoint> paths, TSVector2 startPos, TSVector2 endPos)
        {
            List<TSVector2> newPath = new List<TSVector2>();
            if (paths.Count < 2 || paths[0].Equals(paths[1]))
                return null;

            TSVector2 point1, point2;

            int startIdx = 0;
            int endIdx = paths.Count - 1;

            //看看是不是先用终点格偏移点计算
            if (endPos.x % gridSize != gridHalf || endPos.y % gridSize != gridHalf)
                endIdx = paths.Count;
            int checkIdx = endIdx;

            FP ox = startPos.x % gridSize;
            FP oy = startPos.y % gridSize;

            while (checkIdx > 0 && startIdx < endIdx - 1)
            {
                point1 = CellToPos(paths[startIdx], ox, oy);
                if (checkIdx == paths.Count)
                    point2 = endPos;
                else
                    point2 = CellToPos(paths[checkIdx], ox, oy);

                // 如果没有阻挡，再从当前起点开始查找又没有找到终点的捷径，没有的话把目标往前
                if (checkIdx == startIdx + 1 || !HasBarrier(point1, point2))
                {
                    newPath.Add(point2);
                    startIdx = checkIdx;
                    checkIdx = endIdx;
                }
                else
                    checkIdx--;
            }
            if (newPath.Count < 1 || newPath[newPath.Count - 1] != endPos)
                newPath.Add(endPos);
            return newPath;
        }

        public List<TSVector2> ApplyIntersection(List<FPoint> paths, TSVector2 startPos, TSVector2 endPos)
        {
            List<TSVector2> newPath = new List<TSVector2>();
            if (paths.Count < 2 || paths[0].Equals(paths[1]))
                return null;

            FP ox = gridHalf;
            FP oy = gridHalf;

            for (int i = 0; i < paths.Count; i++)
                newPath.Add(CellToPos(paths[i], ox, oy));
            AddIntersectionToPath(ref newPath, startPos, endPos);
            return newPath;
        }

        void AddIntersectionToPath(ref List<TSVector2> paths, TSVector2 startPos, TSVector2 endPos)
        {
            FP ox = startPos.x - endPos.x % gridSize;
            FP oy = startPos.y - endPos.y % gridSize;

            TSVector2 start1 = new TSVector2(ox, oy);
            TSVector2 start2 = paths[0];

            FP distX = FP.Abs(start2.x - start1.x);
            FP distY = FP.Abs(start2.y - start2.x);

            bool isHorizontal = distX > distY ? false : true;
            FP spx, spy;

            if (distX != 0 && distY != 0)
            {
                if (isHorizontal)
                {
                    if (start2.x - start1.x > 0)
                        spx = start1.x + gridHalf;
                    else
                        spx = start1.x - gridHalf;
                    spy = GetLineY(start1, start2, spx);
                }
                else
                {
                    if (start2.y - start1.y > 0)
                        spy = start1.y + gridHalf;
                    else
                        spy = start1.y - gridHalf;
                    spx = GetLineX(start1, start2, spy);
                }

                paths.RemoveAt(0);

                TSVector2 intersectionStart = new TSVector2(spx, spy);
                if (!startPos.Equals(intersectionStart))
                    paths.Insert(0, intersectionStart);
            }

            int endIdx = paths.Count - 1;
            TSVector2 preEnd = endIdx > 0 ? paths[endIdx - 1] : startPos;
            TSVector2 end = paths[endIdx];

            distX = FP.Abs(end.x - preEnd.x);
            distY = FP.Abs(end.y - preEnd.y);
            isHorizontal = distX > distY ? false : true;

            if (distX != 0 && distY != 0)
            {
                if (isHorizontal)
                {
                    if (end.x - preEnd.x > 0)
                        spx = end.x - gridHalf;
                    else
                        spx = end.x + gridHalf;
                    spy = GetLineY(preEnd, end, spx);
                }
                else
                {
                    if (end.y - preEnd.y > 0)
                        spy = end.y - gridHalf;
                    else
                        spy = end.y + gridHalf;
                    spx = GetLineX(preEnd, end, spy);
                }

                paths.RemoveAt(endIdx);
                TSVector2 intersectionEnd = new TSVector2(spx, spy);
                paths.Add(intersectionEnd);
                if (!endPos.Equals(intersectionEnd))
                    paths.Add(endPos);
            }
        }


        int GetNodesByPos(FP xPos, FP yPos)
        {
            nodesByPos.Clear();

            bool xBorder = xPos % gridSize == 0;
            bool yBorder = yPos % gridSize == 0;

            int x = (int)TSMath.Floor(xPos * multGridSize);
            int y = (int)TSMath.Floor(yPos * multGridSize);

            //点由四节点共享情况
            if (xBorder && yBorder)
            {
                nodesByPos.Add(new TSVector2(x - 1, y - 1));
                nodesByPos.Add(new TSVector2(x, y - 1));
                nodesByPos.Add(new TSVector2(x - 1, y));
                nodesByPos.Add(new TSVector2(x, y));
            }
            //点由2节点共享情况
            //点落在两节点左右临边上
            else if (xBorder && !yBorder)
            {
                nodesByPos.Add(new TSVector2(x, y - 1));
                nodesByPos.Add(new TSVector2(x, y));
            }
            // 点由一节点独享情况
            else
            {
                nodesByPos.Add(new TSVector2(x, y));
            }
            return nodesByPos.Count;
        }

        bool HasBarrier(TSVector2 startPos, TSVector2 endPos)
        {
            FP distX = FP.Abs(endPos.x - startPos.x);
            FP distY = FP.Abs(endPos.y - startPos.y);

            //如果是临近格 肯定无阻碍
            if (distX <= gridSize && distY <= gridSize)
                return false;

            bool isHorizontal = distX > distY ? true : false;

            FP loopStart, loopEnd;
            if (isHorizontal)
            {
                loopStart = TSMath.Min(startPos.x, endPos.x);
                loopEnd = TSMath.Max(startPos.x, endPos.x);

                int qtr = 0;
                //检验步长从小到大 尽量避免拐角卡住
                FP step = gridQuarter;

                for (FP xpos = loopStart; xpos <= loopEnd; xpos += step)
                {
                    FP ypos = GetLineY(startPos, endPos, xpos);

                    int num = GetNodesByPos(xpos, ypos);
                    foreach (var node in nodesByPos)
                    {
                        if (checkBlock(node.x.AsInt(), node.y.AsInt()))
                            return true;
                    }

                    if (xpos == loopStart)
                        xpos -= xpos % gridSize;

                    qtr++;
                    if (qtr >= 4)
                        step = gridHalf;
                    else if (qtr >= 8)
                        step = gridSize;
                }
            }
            else
            {
                loopStart = TSMath.Min(startPos.y, endPos.y);
                loopEnd = TSMath.Max(startPos.y, endPos.y);

                int qtr = 0;
                FP step = gridQuarter;

                //开始纵向遍历起点于终点的节点看是否存在障碍（不可移动点）
                for (FP ypos = loopStart; ypos <= loopEnd; ypos += step)
                {
                    //根据y得到直线上的x值
                    FP xpos = GetLineX(startPos, endPos, ypos);
                    //检查经过的节点是否由障碍物 若有则返回true
                    int num = GetNodesByPos(xpos, ypos);
                    foreach (var node in nodesByPos)
                    {
                        if (checkBlock(node.x.AsInt(), node.y.AsInt()))
                            return true;
                    }

                    if (ypos == loopStart)
                        ypos -= ypos % gridSize;

                    qtr++;
                    if (qtr >= 4)
                        step = gridHalf;
                    else if (qtr >= 8)
                        step = gridSize;
                }
            }
            return false;
        }

        FP GetLineX(TSVector2 startPoint,TSVector2 endPoint,FP y)
        {
            //y 相等 直线平行于x轴 方程式 y =a
            if (startPoint.y == endPoint.y)
                throw new Exception("can not get x from y");
            else if(startPoint.x == endPoint.x)
            {
                return startPoint.x;
            }
            else
            {
                FP a = (startPoint.y - endPoint.y) / (startPoint.x - endPoint.x);
                FP b = startPoint.y - a * startPoint.x;
                return (y - b) / a;
            }
        }

        FP GetLineY(TSVector2 startPoint,TSVector2 endPoint,FP x)
        {
            //y 相等 直线平行于x轴 方程式 y =a
            if (startPoint.x == endPoint.x)
                throw new Exception("can not get y from x");
            else if (startPoint.y == endPoint.y)
            {
                return startPoint.y;
            }
            else
            {
                FP a = (startPoint.y - endPoint.y) / (startPoint.x - endPoint.x);
                FP b = startPoint.y - a * startPoint.x;
                return a * x + b;
            }
        }
    }
}