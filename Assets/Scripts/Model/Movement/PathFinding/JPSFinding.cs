using System;
using System.Collections;
using System.Collections.Generic;
using Movement;
using TrueSync;
namespace Pathfinding
{
    public enum Direct
    {
        UP = 0,
        RIGHT_UP=1,
        RIGHT = 2,
        RIGHT_DOWN = 3,
        DOWN =4,
        LEFT_DOWN =5,
        LEFT = 6,
        LEFT_UP = 7
    }

    public enum ListStatus
    {
        ON_NONE,
        ON_OPEN,
        ON_CLOSED
    }

    public struct JPoint : FPoint, IEquatable<JPoint>
    {
        TSVector2 point;
        public JPoint(int x,int y)
        {
            point = new TSVector2(x, y);
        }

        public int x
        {
            get => point.x.AsInt();
            set => point.x = value;
        }

        public int y
        {
            get => point.y.AsInt();
            set => point.y = value;
        }

        public bool Equals(JPoint other)
        {
            return x == other.x && y == other.y;
        }

        /// <summary>
        /// 取横纵坐标距离中大的
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Diff(JPoint a,JPoint b)
        {
            int diff_x = Math.Abs(b.x - a.x);
            int diff_y = Math.Abs(b.y - a.y);

            return Math.Max(diff_x, diff_y);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }

    public class PathfindingNode
    {
        public PathfindingNode parent;
        public JPoint pos;
        public int givenCost;
        public int finalCost;
        public Direct directionFromParent;
        public ListStatus listStatus = ListStatus.ON_NONE;

        public void Reset()
        {
            this.parent = null;
            this.givenCost = 0;
            this.finalCost = 0;
            this.listStatus = ListStatus.ON_NONE;
        }
    }

    public class Node
    {
        public JPoint pos;

        public bool isObstacle = false;
        public int[] jpDistance = new int[8];

        public bool isJumpPoint = false;

        public bool[] jumpPointDirection = new bool[8];

        public bool IsJumpPointComingFrom(Direct dir)
        {
            return this.isJumpPoint && this.jumpPointDirection[(int)dir];
        }
    }

    public class JPSFinding
    {
        /// <summary>
        /// 地图格子
        /// </summary>
        public Node[] gridNodes = new Node[0];
        /// <summary>
        /// 搜索点
        /// </summary>
        public PathfindingNode[] pathfindingNodes = new PathfindingNode[0];

        //地图宽高
        private int width;
        private int height;

        //用于设置哪些格子是阻挡格
        Func<int, int, bool> checkBlock;

        public JPSFinding(Func<int, int, bool> checkFunc)
        {
            checkBlock = checkFunc;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;

            gridNodes = new Node[width * height];
            pathfindingNodes = new PathfindingNode[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int i = y * width + x;
                    gridNodes[i] = new Node();
                    gridNodes[i].pos = new JPoint(x, y);

                    pathfindingNodes[i] = new PathfindingNode();
                    pathfindingNodes[i].pos = new JPoint(x, y);
                    if (checkBlock(x, y))
                    {
                        //设置阻挡格信息 比如 可以读取地图编辑器中的地图信息
                        gridNodes[i].isObstacle = true;
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前格子右下格
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetDownRightIndex(int x, int y)
        {
            if (x + 1 >= width || y - 1 < 0) return -1;
            return (x + 1) + (y - 1) * width;
        }
        /// <summary>
        /// 获取当前格子右上格
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetUpRightIndex(int x, int y)
        {
            if (x + 1 >= width || y + 1 >= height) return -1;
            return (x + 1) + (y + 1) * width;
        }
        /// <summary>
        /// 获取当前格子左上
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetUpLeftIndex(int x, int y)
        {
            if (x - 1 < 0 || y + 1 >= height) return -1;
            return (x - 1) + (y + 1) * width;
        }
        /// <summary>
        /// 获取当前格子左下格
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetDownLeftIndex(int x, int y)
        {
            if (x - 1 < 0 || y - 1 < 0) return -1;
            return (x - 1) + (y - 1) * width;
        }
        /// <summary>
        /// 是否可以继续往下走（既不是阻挡格也不是边界）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsWalkable(int index)
        {
            if (index < 0) return false;

            int x, y;
            y = index / width;
            x = index % width;
            return IsWalkable(x, y);
        }

        private bool IsObstacleOrWall(int index)
        {
            if (index < 0) return true;
            int x, y;
            y = index / width;
            x = index % width;
            return IsObstacleOrWall(x, y);
        }

        private bool IsWalkable(int x, int y)
        {
            return !IsObstacleOrWall(x, y);
        }

        private bool IsObstacleOrWall(int x, int y)
        {
            return IsInBounds(x, y) && gridNodes[x + (y * width)].isObstacle;
        }

        private bool IsInBounds(int index)
        {
            if (index < 0 || index >= gridNodes.Length) return false;

            int x, y;
            y = index / width;
            x = index % width;

            return IsInBounds(x, y);
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && y < height && x >= 0 && x < width;
        }
        //直接定义变量 这样不用反复获取
        private static readonly FP SQRT_2 = Math.Sqrt(2);
        private static readonly FP SQRT_2_MINUS_1 = Math.Sqrt(2) - 1.0f;

        /// <summary>
        /// 行进方向上对应不需要检测的其他方向
        /// </summary>
        private static Dictionary<Direct, Direct[]> validDirLookUpTable = new Dictionary<Direct, Direct[]>()
        {
            {Direct.DOWN,new []{ Direct.LEFT,Direct.LEFT_DOWN,Direct.DOWN,Direct.RIGHT_DOWN,Direct.RIGHT } },
            { Direct.RIGHT_DOWN,new []{ Direct.DOWN,Direct.RIGHT_DOWN,Direct.RIGHT} },
            { Direct.RIGHT,new []{ Direct.DOWN,Direct.RIGHT_DOWN,Direct.RIGHT,Direct.RIGHT_UP,Direct.UP} },
            { Direct.RIGHT_UP,new []{ Direct.RIGHT,Direct.RIGHT_UP,Direct.UP} },
            { Direct.UP,new []{ Direct.RIGHT,Direct.RIGHT_UP,Direct.UP,Direct.LEFT_UP,Direct.LEFT} },
            { Direct.LEFT_UP,new []{ Direct.UP,Direct.LEFT_UP,Direct.LEFT} },
            { Direct.LEFT,new []{ Direct.UP,Direct.LEFT_UP,Direct.LEFT,Direct.LEFT_DOWN,Direct.DOWN} },
            { Direct.LEFT_DOWN,new []{Direct.LEFT,Direct.LEFT_DOWN,Direct.DOWN } }
        };
        /// <summary>
        /// 八方向
        /// </summary>
        private static Direct[] allDirections = { Direct.UP,Direct.RIGHT_UP,Direct.RIGHT,Direct.RIGHT_DOWN,Direct.DOWN
        ,Direct.LEFT_DOWN,Direct.LEFT,Direct.LEFT_UP};

        private Direct[] GetAllValidDirections(PathfindingNode curr_node)
        {
            return curr_node.parent == null ? allDirections : validDirLookUpTable[curr_node.directionFromParent];
        }

        /// <summary>
        /// 获取当前格子八方向上的其他格子索引
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private int GetIndexOfNodeTowardsDirection(int index,Direct direction)
        {
            int x, y;
            x = index % width;
            y = index / width;

            int change_x = 0;
            int change_y = 0;

            switch (direction)
            {
                case Direct.RIGHT_DOWN:
                case Direct.DOWN:
                case Direct.LEFT_DOWN:
                    change_y = -1;
                    break;

                case Direct.RIGHT_UP:
                case Direct.UP:
                case Direct.LEFT_UP:
                    change_y = 1;
                    break;
            }

            switch (direction)
            {
                case Direct.LEFT:
                case Direct.LEFT_UP:
                case Direct.LEFT_DOWN:
                    change_x = -1;
                    break;


                case Direct.RIGHT:
                case Direct.RIGHT_UP:
                case Direct.RIGHT_DOWN:
                    change_x = 1;
                    break;
            }

            int new_y = y + change_y;
            int new_x = x + change_x;

            if(IsInBounds(new_x,new_y))
            {
                return new_x + (new_y * width);
            }
            return -1;
        }

        /// <summary>
        /// 初始化可以跳跃的点信息
        /// </summary>
        public void BuildPrimaryJumpPoints()
        {
            for (int i = 0; i < gridNodes.Length; i++)
            {
                Node current_node = gridNodes[i];
                if (current_node.isObstacle)
                {
                    int x = i % width;
                    int y = i / width;

                    int up_right_node, down_right_node, down_left_node, up_left_node;

                    up_right_node = GetUpRightIndex(x,y);

                    if(up_right_node != -1)
                    {
                        Node node = gridNodes[up_right_node];

                        if (!node.isObstacle)
                        {
                            //向右上行进时判断左下是否可以行走 但出现障碍物时就需要根据行进方向去判断这个点是否可以作为跳点
                            if (IsWalkable(GetIndexOfNodeTowardsDirection(up_right_node,Direct.DOWN))&& IsWalkable(GetIndexOfNodeTowardsDirection(up_right_node,Direct.LEFT)))
                            {
                                node.isJumpPoint = true;
                                node.jumpPointDirection[(int)Direct.DOWN] = true;
                                node.jumpPointDirection[(int)Direct.LEFT] = true;
                            }
                        }
                    }

                    down_right_node = GetDownRightIndex(x, y);
                    if (down_right_node != -1)
                    {
                        Node node = gridNodes[down_right_node];
                        if (!node.isObstacle)
                        {
                            //向右下行进时判断左上是否可以行走
                            if (IsWalkable(GetIndexOfNodeTowardsDirection(down_right_node, Direct.UP)) &&
                                IsWalkable(GetIndexOfNodeTowardsDirection(down_right_node, Direct.LEFT)))
                            {
                                node.isJumpPoint = true;
                                node.jumpPointDirection[(int)Direct.UP] = true;
                                node.jumpPointDirection[(int)Direct.LEFT] = true;
                            }
                        }
                    }

                    down_left_node = GetDownLeftIndex(x, y);
                    if (down_left_node != -1)
                    {
                        Node node = gridNodes[down_left_node];
                        if (!node.isObstacle)
                        {
                            //向左下行进时判断右上是否可以行走
                            if (IsWalkable(GetIndexOfNodeTowardsDirection(down_left_node,Direct.UP))&&
                                IsWalkable(GetIndexOfNodeTowardsDirection(down_left_node,Direct.RIGHT)))
                            {
                                node.isJumpPoint = true;
                                node.jumpPointDirection[(int)Direct.UP] = true;
                                node.jumpPointDirection[(int)Direct.RIGHT] = true;
                            }
                        }
                    }

                    up_left_node = GetUpLeftIndex(x, y);
                    if (up_left_node != -1)
                    {
                        Node node = gridNodes[up_left_node];
                        if (!node.isObstacle)
                        {
                            //向左上行进时判断右下是否可以行走
                            if (IsWalkable(GetIndexOfNodeTowardsDirection(up_left_node, Direct.DOWN)) &&
                                IsWalkable(GetIndexOfNodeTowardsDirection(up_left_node, Direct.RIGHT)))
                            {
                                node.isJumpPoint = true;
                                node.jumpPointDirection[(int)Direct.DOWN] = true;
                                node.jumpPointDirection[(int)Direct.RIGHT] = true;
                            }
                        }
                    }
                }
            }
        }

        private int RowColumnToIndex(int x,int y)
        {
            return x + (y * width);
        }

        /// <summary>
        /// 第二步 算出每个可走点（无论自身是不是跳点）距离纵横正方向每个跳点的距离
        /// </summary>
        public void BuildStraightJumpPoints()
        {
            for (int y = 0; y < height; y++)
            {
                int jumpDistanceSoFar = -1;
                bool jumpPointSeen = false;


                //从左往右遍历
                for (int x = 0; x < width; x++)
                {
                    Node node = GetNode(x, y);
                    //如果遇到正向阻挡 重新开始 前一个跳点不再有用，阻挡格等同左侧墙
                    if(node.isObstacle)
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.jpDistance[(int)Direct.LEFT] = 0;
                        continue;
                    }

                    ++jumpDistanceSoFar;

                    if (jumpPointSeen)
                        node.jpDistance[(int)Direct.LEFT] = jumpDistanceSoFar;
                    else
                        node.jpDistance[(int)Direct.LEFT] = -jumpDistanceSoFar;

                    if(node.IsJumpPointComingFrom(Direct.RIGHT))
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }    
                }

                jumpDistanceSoFar = -1;
                jumpPointSeen = false;

                for (int x = width -1; x >= 0; x--)
                {
                    Node node = GetNode(x, y);

                    if(node.isObstacle)
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.jpDistance[(int)Direct.RIGHT] = 0;
                        continue;
                    }

                    ++jumpDistanceSoFar;
                    if (jumpPointSeen)
                        node.jpDistance[(int)Direct.RIGHT] = jumpDistanceSoFar;
                    else
                        node.jpDistance[(int)Direct.RIGHT] = -jumpDistanceSoFar;

                    if(node.IsJumpPointComingFrom(Direct.LEFT))
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                int jumpDistanceSoFar = -1;
                bool jumpPointSeen = false;

                for (int y = height - 1; y >= 0; y--)
                {
                    Node node = GetNode(x, y);

                    if (node.isObstacle)
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.jpDistance[(int)Direct.UP] = 0;
                        continue;
                    }

                    ++jumpDistanceSoFar;

                    if (jumpPointSeen)
                        node.jpDistance[(int)Direct.UP] = jumpDistanceSoFar;
                    else
                        node.jpDistance[(int)Direct.UP] = -jumpDistanceSoFar;

                    if (node.IsJumpPointComingFrom(Direct.DOWN))
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }

                jumpDistanceSoFar = -1;
                jumpPointSeen = false;

                for (int y = 0; y < height; y++)
                {
                    Node node = GetNode(x, y);

                    if(node.isObstacle)
                    {
                        jumpDistanceSoFar = -1;
                        jumpPointSeen = false;
                        node.jpDistance[(int)Direct.DOWN] = 0;
                        continue;
                    }

                    ++jumpDistanceSoFar;

                    if (jumpPointSeen)
                        node.jpDistance[(int)Direct.DOWN] = jumpDistanceSoFar;
                    else
                        node.jpDistance[(int)Direct.DOWN] = -jumpDistanceSoFar;

                    if(node.IsJumpPointComingFrom(Direct.UP))
                    {
                        jumpDistanceSoFar = 0;
                        jumpPointSeen = true;
                    }
                }
            }
        }

        private Node GetNode(int x, int y)
        {
            Node node = null;
            if (IsInBounds(x, y))
                node = gridNodes[RowColumnToIndex(x, y)];
            return node;
        }

        /// <summary>
        /// 第三步 计算斜角方向的跳点
        /// </summary>
        public void BuildDiagonalJumpPoints()
        {
            //从上往下遍历所有非墙 非阻挡的点
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsObstacleOrWall(x, y))
                        continue;

                    Node node = GetNode(x, y);

                    if (x == 0 || y == height - 1 ||
                        IsObstacleOrWall(x, y + 1) ||
                        IsObstacleOrWall(x - 1, y) ||
                        IsObstacleOrWall(x - 1, y + 1))
                    {
                        node.jpDistance[(int)Direct.LEFT_UP] = 0;
                    }
                    else if(IsWalkable(x,y+1) && IsWalkable(x-1,y)
                        &&(GetNode(x-1,y+1).jpDistance[(int)Direct.UP] > 0 ||
                        GetNode(x-1,y+1).jpDistance[(int)Direct.LEFT] > 0))
                    {
                        node.jpDistance[(int)Direct.LEFT_UP] = 1;
                    }
                    else
                    {
                        int jumpDistance = GetNode(x - 1, y + 1).jpDistance[(int)Direct.LEFT_UP];

                        if (jumpDistance > 0)
                            node.jpDistance[(int)Direct.LEFT_UP] = 1 + jumpDistance;
                        else
                            node.jpDistance[(int)Direct.LEFT_UP] = -1 + jumpDistance;
                    }



                    if(y == height -1 || x == width- 1 ||
                        IsObstacleOrWall(x,y+1)||
                        IsObstacleOrWall(x+1,y)||
                        IsObstacleOrWall(x+1,y+1))
                    {
                        node.jpDistance[(int)Direct.RIGHT_UP] = 0;
                    }
                    else if(IsWalkable(x,y +1)&&
                        IsWalkable(x+1,y)&&
                        (GetNode(x+1,y+1).jpDistance[(int)Direct.UP]>0 ||
                        GetNode(x+1,y+1).jpDistance[(int)Direct.RIGHT] >0))
                    {
                        node.jpDistance[(int)Direct.RIGHT_UP] = 1;
                    }
                    else
                    {
                        int jumpDistance = GetNode(x + 1, y + 1).jpDistance[(int)Direct.RIGHT_UP];

                        if (jumpDistance > 0)
                            node.jpDistance[(int)Direct.RIGHT_UP] = 1 + jumpDistance;
                        else
                            node.jpDistance[(int)Direct.RIGHT_UP] = -1 + jumpDistance;
                    }
                }
            }

            //从下往上遍历所有非墙 非阻挡的点
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsObstacleOrWall(x, y)) continue;

                    Node node = GetNode(x, y);

                    if(y == 0 || x== 0||
                        IsObstacleOrWall(x,y-1) ||
                        IsObstacleOrWall(x-1,y)||
                        IsObstacleOrWall(x-1,y-1))
                    {
                        node.jpDistance[(int)Direct.LEFT_DOWN] = 0;
                    }
                    else if(IsWalkable(x,y-1) &&
                        IsWalkable(x-1,y) &&
                        (GetNode(x-1,y-1).jpDistance[(int)Direct.DOWN]>0 ||
                        GetNode(x-1,y-1).jpDistance[(int)Direct.LEFT] > 0))
                    {
                        node.jpDistance[(int)Direct.LEFT_DOWN] = 1;
                    }
                    else
                    {
                        int jumpDistance = GetNode(x - 1, y - 1).jpDistance[(int)Direct.LEFT_DOWN];

                        if (jumpDistance > 0)
                            node.jpDistance[(int)Direct.LEFT_DOWN] = 1 + jumpDistance;
                        else
                            node.jpDistance[(int)Direct.LEFT_DOWN] = -1 + jumpDistance;
                    }

                    if (y == 0 || x == width - 1 ||
                        IsObstacleOrWall(x, y - 1) ||
                        IsObstacleOrWall(x + 1, y) ||
                        IsObstacleOrWall(x + 1, y - 1))
                    {
                        node.jpDistance[(int)Direct.RIGHT_DOWN] = 0;
                    }
                    else if(IsWalkable(x,y-1) &&
                        IsWalkable(x+1,y) &&
                        (GetNode(x+1,y-1).jpDistance[(int)Direct.DOWN]> 0 ||
                        GetNode(x+1,y-1).jpDistance[(int)Direct.RIGHT]>0))
                    {
                        node.jpDistance[(int)Direct.RIGHT_DOWN] = 1;
                    }
                    else
                    {
                        int jumpDistance = GetNode(x + 1, y - 1).jpDistance[(int)Direct.RIGHT_DOWN];

                        if (jumpDistance > 0)
                            node.jpDistance[(int)Direct.RIGHT_DOWN] = 1 + jumpDistance;
                        else
                            node.jpDistance[(int)Direct.RIGHT_DOWN] = -1 + jumpDistance;
                    }
                }
            }
        }


        public void ResetPathfindingNodeData()
        {
            foreach (var node in pathfindingNodes)
            {
                node.Reset();
            }
        }

        /// <summary>
        //  处理路径 用于外部使用
        /// </summary>
        /// <param name="endpos"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public List<FPoint> ReconstructPath(PathfindingNode endpos,JPoint start)
        {
            List<FPoint> path = new List<FPoint>();
            PathfindingNode curr_node = endpos;

            while (curr_node.parent != null)
            {
                path.Add(curr_node.pos);
                curr_node = curr_node.parent;
            }

            path.Add(start);
            path.Reverse();
            return path;
        }

        private bool IsDiagonal(Direct dir)
        {
            switch (dir)
            {
                case Direct.RIGHT_DOWN:
                case Direct.LEFT_DOWN:
                case Direct.RIGHT_UP:
                case Direct.LEFT_UP:
                    return true;
            }
            return false;
        }

        private bool EndPosIsInExactDirection(JPoint curr,Direct dir,JPoint endpos)
        {
            int diff_x = endpos.x - curr.x;
            int diff_y = endpos.y - curr.y;

            switch (dir)
            {
                case Direct.UP:
                    return diff_y > 0 && diff_x == 0;
                case Direct.RIGHT_UP:
                    return diff_y > 0 && diff_x > 0 && Math.Abs(diff_y) == Math.Abs(diff_x);
                case Direct.RIGHT:
                    return diff_y == 0 && diff_x > 0;
                case Direct.RIGHT_DOWN:
                    return diff_y < 0 && diff_x > 0 && Math.Abs(diff_y) == Math.Abs(diff_x);
                case Direct.DOWN:
                    return diff_y < 0 && diff_x == 0;
                case Direct.LEFT_DOWN:
                    return diff_y < 0 && diff_x < 0 && Math.Abs(diff_y) == Math.Abs(diff_x);
                case Direct.LEFT:
                    return diff_y == 0 && diff_x < 0;
                case Direct.LEFT_UP:
                    return diff_y > 0 && diff_x < 0 && Math.Abs(diff_y) == Math.Abs(diff_x);
            }
            return false;
        }

        private bool EndPosIsInGeneralDirection(JPoint curr, Direct dir, JPoint endpos)
        {
            int diff_x = endpos.x - curr.x;
            int diff_y = endpos.y - curr.y;

            switch (dir)
            {
                case Direct.UP:
                    return diff_y > 0 && diff_x == 0;
                case Direct.RIGHT_UP:
                    return diff_y > 0 && diff_x > 0;
                case Direct.RIGHT:
                    return diff_y == 0 && diff_x > 0;
                case Direct.RIGHT_DOWN:
                    return diff_y < 0 && diff_x > 0;
                case Direct.DOWN:
                    return diff_y < 0 && diff_x == 0;
                case Direct.LEFT_DOWN:
                    return diff_y < 0 && diff_x < 0;
                case Direct.LEFT:
                    return diff_y == 0 && diff_x < 0;
                case Direct.LEFT_UP:
                    return diff_y > 0 && diff_x < 0;
            }
            return false;
        }

        private PathfindingNode GetNodeDist(int x,int y,Direct direction,int dist)
        {
            PathfindingNode new_node = null;

            int new_y = y, new_x = x;
            switch (direction)
            {
                case Direct.UP:
                    new_y += dist;
                    break;
                case Direct.RIGHT_UP:
                    new_x += dist;
                    new_y += dist;
                    break;
                case Direct.RIGHT:
                    new_x += dist;
                    break;
                case Direct.RIGHT_DOWN:
                    new_x += dist;
                    new_y -= dist;
                    break;
                case Direct.DOWN:
                    new_y -= dist;
                    break;
                case Direct.LEFT_DOWN:
                    new_x -= dist;
                    new_y -= dist;
                    break;
                case Direct.LEFT:
                    new_x -= dist;
                    break;
                case Direct.LEFT_UP:
                    new_x -= dist;
                    new_y += dist;
                    break;
            }
            if (IsInBounds(new_x, new_y))
                new_node = pathfindingNodes[RowColumnToIndex(new_x, new_y)];
            return new_node;
        }

        public IEnumerator GetPathAsync(JPoint start,JPoint end)
        {
            bool found_path = false;
            PathfindReturn return_state = new PathfindReturn();

            if (start.Equals(end))
            {
                return_state.path = new List<FPoint> { start };
                return_state.status = PathfindReturn.PathfindStatus.FOUND;
                yield return found_path;
            }

            PriorityQueue<PathfindingNode, int> open_set = new PriorityQueue<PathfindingNode, int>();
            //初始化openset
            ResetPathfindingNodeData();
            PathfindingNode starting_node = pathfindingNodes[PointToIndex(start)];
            starting_node.pos = start;
            starting_node.parent = null;
            starting_node.givenCost = 0;
            starting_node.finalCost = 0;
            starting_node.listStatus = ListStatus.ON_OPEN;

            open_set.Push(starting_node, 0);

            while (!open_set.IsEmpty())
            {
                PathfindingNode curr_node = open_set.Pop();

                Node jp_node = gridNodes[PointToIndex(curr_node.pos)];

                return_state.current = curr_node;
                //如果到达目标点了 结束搜索
                if (curr_node.pos.Equals(end))
                {
                    return_state.path = ReconstructPath(curr_node, start);
                    return_state.status = PathfindReturn.PathfindStatus.FOUND;
                    found_path = true;
                    yield return return_state;
                    break;
                }

                yield return return_state;

                //根据四周有效的点 计算花费最小的点
                foreach (Direct dir in GetAllValidDirections(curr_node))
                {
                    PathfindingNode new_successor = null;
                    int given_cost = 0;
                    //endpos is closer than wall distance or closer than or equal to jump point distance 
                    if (IsCardinal(dir)
                        && EndPosIsInExactDirection(curr_node.pos, dir, end)
                        && JPoint.Diff(curr_node.pos, end) <= Math.Abs(jp_node.jpDistance[(int)dir]))
                    {
                        new_successor = pathfindingNodes[PointToIndex(end)];
                        //距离花费
                        given_cost = curr_node.givenCost + JPoint.Diff(curr_node.pos, end);
                    }
                    // Goal is closer or equal in either row or column than wall or jump point distance
                    else if (IsCardinal(dir)
                        && EndPosIsInGeneralDirection(curr_node.pos, dir, end)
                        && (Math.Abs(end.x - curr_node.pos.x) <= Math.Abs(jp_node.jpDistance[(int)dir]) ||
                        Math.Abs(end.y - curr_node.pos.y) <= Math.Abs(jp_node.jpDistance[(int)dir])))
                    {
                        //create  a target jump point
                        int min_diff = Math.Min(Math.Abs(end.x - curr_node.pos.x), Math.Abs(end.y - curr_node.pos.y));
                        new_successor = GetNodeDist(curr_node.pos.x, curr_node.pos.y, dir, min_diff);
                        given_cost = curr_node.givenCost + (int)(SQRT_2 * JPoint.Diff(curr_node.pos, new_successor.pos));
                    }
                    else if(jp_node.jpDistance[(int)dir] > 0)
                    {
                        //Jump Jpoint in this direction
                        new_successor = GetNodeDist(curr_node.pos.x, curr_node.pos.y, dir, jp_node.jpDistance[(int)dir]);

                        given_cost = JPoint.Diff(curr_node.pos, new_successor.pos);

                        if (IsDiagonal(dir))
                            given_cost = (int)(given_cost * SQRT_2);
                        given_cost += curr_node.givenCost;
                    }

                    //Traditional A* from this point
                    if(new_successor != null)
                    {
                        if(new_successor.listStatus!= ListStatus.ON_OPEN)
                        {
                            new_successor.parent = curr_node;
                            new_successor.givenCost = given_cost;
                            new_successor.directionFromParent = dir;
                            new_successor.finalCost = given_cost + OctileHeuristic(new_successor.pos.x, new_successor.pos.y, end.x, end.y);
                            new_successor.listStatus = ListStatus.ON_OPEN;
                            open_set.Push(new_successor, new_successor.finalCost);
                        }
                        else if(given_cost < new_successor.givenCost)
                        {
                            new_successor.parent = curr_node;
                            new_successor.givenCost = given_cost;
                            new_successor.directionFromParent = dir;
                            new_successor.finalCost = given_cost + OctileHeuristic(new_successor.pos.x, new_successor.pos.y , end.x, end.y);
                            new_successor.listStatus = ListStatus.ON_OPEN;
                            open_set.Push(new_successor, new_successor.finalCost);
                        }
                    }
                }
            }
            if(!found_path)
            {
                return_state.status = PathfindReturn.PathfindStatus.NOT_FOUND;
                yield return return_state;
            }
        }

        private int PointToIndex(JPoint pos)
        {
            return RowColumnToIndex(pos.x, pos.y);
        }

        /// <summary>
        /// 是否是正方向
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool IsCardinal(Direct dir)
        {
            switch (dir)
            {
                case Direct.DOWN:
                case Direct.RIGHT:
                case Direct.UP:
                case Direct.LEFT:
                    return true;
            }
            return false;
        }

        private int OctileHeuristic(int curr_x,int curr_y,int end_x,int end_y)
        {
            int heuristic;
            int x_dist = end_x - curr_x;
            int y_dist = end_y - curr_y;

            heuristic = (int)(Math.Max(x_dist, y_dist) + SQRT_2_MINUS_1 * Math.Min(x_dist, y_dist));
            return heuristic;
        }
    }
}
