using System;
using System.Collections.Generic;
using KdTree;
using Pathfinding;
using TrueSync;

namespace Movement
{
    public class MoveMgr
    {
        public static MoveMgr Inst { get; } = new MoveMgr();

        public static int rvoTick { get; set; } = 1;

        static bool _enable = false;

        public static bool enable
        {
            get => _enable;
            set
            {
                if(_enable != value)
                {
                    _enable = value;
                    if(_enable)
                    {
                        //设置rvo update时间 1秒钟30次
                        RVO.Simulator.Instance.setTimeStep(1f / 30f);
                        // 设置新的寻路对象 默认值
                        RVO.Simulator.Instance.setAgentDefaults(300, 10, 2, 5f, 50f, 50f, new TSVector2(0, 0));
                        // 创建阻挡点信息
                        RVO.Simulator.Instance.processObstacles();
                    }
                    else
                    {
                        //清除rvo寻路信息
                        RVO.Simulator.Instance.Clear();
                    }
                }
            }
        }

        //private Dictionary<int,>

        private byte[,] moveBlock;
        private float multSize;

        public const int logicSize = 24;
        public const int halfSize = 12;
        public const int searchRadius = 20;

        public int logicWidth { get; private set; }
        public int logicHeight { get; private set; }

        public const int InvalidUid = int.MinValue;
        public static readonly TSVector2 InvalidPos = new TSVector2(-1, -1);

        private List<int> searchUids = new List<int>();


        private Dictionary<int, MoveUnit> moveUnits = new Dictionary<int, MoveUnit>();
        public Dictionary<int, MoveGroup> moveGroups = new Dictionary<int, MoveGroup>();
        private TSVector2KdTree<MoveUnit> unitKdTree = new TSVector2KdTree<MoveUnit>(2, KdTree.AddDuplicateBehavior.Skip);

        static readonly int[,] diro = { { 0, -1 }, { -1,- 1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };
        private MoveMgr()
        {
        }

        public ANode PosToCell(TSVector2 pos)
        {
            return new ANode((int)(pos.x * multSize), (int)(pos.y * multSize));
        }

        public TSVector2 CellToPos(int x,int y)
        {
            return CellToPos(x, y, halfSize, halfSize);
        }

        public TSVector2 CellToPos(int x,int y,FP offsetX,FP offsetY)
        {
            return new TSVector2(x * logicSize + offsetX, y * logicSize + offsetY);
        }

        public TSVector2 CellToPos(ANode cell,FP offsetX,FP offsetY)
        {
            return CellToPos(cell.x, cell.y, offsetX, offsetY);
        }


        public TSVector2 PosToCenter(TSVector2 pos)
        {
            return new TSVector2((int)(pos.x * multSize) * logicSize + halfSize, (int)(pos.y * multSize) * logicSize + halfSize);
        }

        public TSVector2 NextCenter(TSVector2 pos,int dir)
        {
            var x = (int)(pos.x * multSize) + diro[dir, 0];
            var y = (int)(pos.y * multSize) + diro[dir, 1];
            return new TSVector2(x * logicSize + halfSize, y * logicSize + halfSize);
        }

        public MoveUnit GetMoveUnit(int uid)
        {
            if (moveUnits.TryGetValue(uid, out var unit))
                return unit;
            return null;
        }

        public MoveUnit GetCollisionMoveUnit(TSVector2 pos,int skipUid = InvalidUid ,int radius = halfSize,int skipCamp = InvalidUid)
        {
            var units = unitKdTree.RadialSearch(pos, radius);
            if (units.Length > 0)
            {
                foreach (var pair in units)
                {
                    var unit = pair.Value;
                    if (skipUid != InvalidUid && unit.uid == skipUid)
                        continue;
                    if (skipCamp != InvalidUid && unit.camp == skipCamp)
                        continue;
                    return unit;
                }
            }
            return null;
        }

        public List<int> GetUnitsInCell(TSVector2 pos, int camp = -1, int skipUid = InvalidUid, UnitFlag flag = UnitFlag.None)
        {
            var cell = PosToCell(pos);
            return GetUnitsInCell(cell.x, cell.y, camp, skipUid, flag);
        }

        internal List<int> GetUnitsInCell(int x, int y, int camp = -1, int skipUid = InvalidUid, UnitFlag flag = UnitFlag.None)
        {
            searchUids.Clear();

            var center = CellToPos(x, y);
            if (MapMgr.Inst.IsBlockTile(x, y))
                return searchUids;
            var units = unitKdTree.RadialSearch(center, searchRadius);
            if(units.Length > 0)
            {
                foreach (var pair in units)
                {
                    var unit = pair.Value;
                    if (skipUid != InvalidUid && unit.uid == skipUid)
                        continue;
                    if (camp > 0 && unit.camp != camp)
                        continue;
                    if (flag != UnitFlag.None && (unit.flag & flag) == UnitFlag.None)
                        continue;
                    var cell = PosToCell(unit.position);
                    if (cell.x != x || cell.y != y)
                        continue;
                    searchUids.Add(unit.uid);
                }
            }
            return searchUids;
        }


        public MoveUnit GetUnitInCell(TSVector2 pos,int camp = -1,int skipUid = InvalidUid,UnitFlag flag = UnitFlag.None)
        {
            var ret = GetUnitsInCell(pos, camp, skipUid, flag);
            if (ret.Count > 0)
                return GetMoveUnit(ret[0]);
            return null;
        }

        public TSVector2 GetStandPosAround(TSVector2 pos,TSVector2 from)
        {
            var dir = Calculater.GetDirByVector(from - pos);

            var x = (int)TSMath.Floor(pos.x * multSize);
            var y = (int)TSMath.Floor(pos.y * multSize);

            if (!IsBlockLogic(x, y))
                return pos;

            int offdir, cx, cy;
            for (int i = 0; i < 8; i++)
            {
                offdir = (i + dir) % 8;
                cx = x + diro[offdir, 0];
                cy = y + diro[offdir, 1];
                if (!IsBlockLogic(cx, cy))
                    return new TSVector2(cx * logicSize + halfSize, cy * logicSize + halfSize);
            }
            return pos;
        }

        public bool IsBlockLogic(int x,int y)
        {
            if (MapMgr.Inst.IsBlockPixel(x * logicSize + halfSize, y * logicSize + halfSize))
                return true;
            if (x >= 0 && x < logicWidth && y >= 0 && y < logicHeight)
                return moveBlock[x, y] == 1;
            return true;
        }

        public bool IsBlockPixel(TSVector2 pos,bool allowEdge = false)
        {
            if (MapMgr.Inst.IsBlockPixel(pos)) return true;
            int x = pos.x.AsInt();
            int y = pos.y.AsInt();
            return IsBlockPixel(x, y, allowEdge);
        }

        public bool IsBlockPixel(int x,int y,bool allowEdge =false)
        {
            if(allowEdge)
            {
                if (x % logicSize == 0)
                    x = Math.Max(0, x - 1);
                if (y % logicSize == 0)
                    y = Math.Max(0, y - 1);
            }

            x = (int)Math.Floor(x * multSize);
            y = (int)Math.Floor(y * multSize);

            if (x >= 0 && x < logicWidth && y >= 0 && y < logicHeight)
                return moveBlock[x, y] == 1;
            return true;
        }

        public TSVector2 NextPos(TSVector2 pos,int dir)
        {
            var x = (int)(pos.x * multSize) + diro[dir, 0];
            var y = (int)(pos.y * multSize) + diro[dir, 1];
            return new TSVector2(x * logicSize + pos.x % logicSize, y * logicSize + pos.y % logicSize);
        }

        public MoveGroup GetMoveGroup(int gid)
        {
            if (moveGroups.TryGetValue(gid, out var group))
                return group;
            return null;
        }

        public MoveUnit GetNearestMoveUnit(TSVector2 pos,Func<MoveUnit,bool> fliter = null)
        {
            var units = unitKdTree.GetNearestNeighbours(pos, 1, fliter);
            if (units.Length > 0)
                return units[0].Value;
            return null;
        }
    }
}
