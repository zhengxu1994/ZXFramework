using System;
using Pathfinding;
using TrueSync;
using System.Collections.Generic;
using UnityEngine;
namespace Movement
{
    public class MapMgr
    {
        public static MapMgr Inst { get; } = new MapMgr();

        public JPSFinding jPSFinding { get; private set; }

        public Pathmodifier pathmodifier { get; private set; }

        private float multTileSize;
        public const int tileSize = 24;
        public const int halfSize = 12;
        public const int halfSizeSQ = 144;

        private int tileWidth = 0;
        private int tileHight = 0;

        public int mapWidth { get; private set; } = 0;
        public int mapHight { get; private set; } = 0;
        private byte[,] blockData;

        public const byte TYPE_BLOCK = 1;
        public const byte TYPE_NOBLOCK = 0;
        public bool IsCollisionBlock(TSVector2 pos)
        {
            var cell = PosToCell(pos);
            return false;
        }

        public bool IsBlockPixel(TSVector2 pixel,bool allowEdge = false)
        {
            int x = pixel.x.AsInt();
            int y = pixel.y.AsInt();
            return IsBlockPixel(x, y, allowEdge);
        }

        public bool IsBlockPixel(int x,int y,bool allowEdge =false)
        {
            if(allowEdge)
            {
                if (x % tileSize == 0)
                    x = Math.Max(0, x - 1);
                if (y % tileSize == 0)
                    y = Math.Max(0, y - 1);
            }
            x = (int)Math.Floor(x * multTileSize);
            y = (int)Math.Floor(y * multTileSize);
            return IsBlockTile(x, y);
        }


        public bool IsBlockTile(int x,int y)
        {
            if (x < 0 || y < 0 || x >= tileWidth || y >= tileHight)
                return true;
            return blockData[x, y] == TYPE_BLOCK;
        }

        public JPoint PosToCell(TSVector2 pos)
        {
            return new JPoint((int)(pos.x * multTileSize), (int)(pos.y * multTileSize));
        }

        public TSVector2 PosToCenter(TSVector2 pos)
        {

            return new TSVector2(((int)(pos.x * multTileSize) * tileSize + halfSize),((int)(pos.y * multTileSize) * tileSize + halfSize));
        }

        public bool IsSameCell(TSVector2 pos1,TSVector2 pos2)
        {
            return (int)(pos1.x * multTileSize) == (int)(pos2.x * multTileSize) &&
                (int)(pos1.y * multTileSize) == (int)(pos2.y * multTileSize);
        }

        public List<TSVector2> FindCellPath(TSVector2 startPos,TSVector2 targetPos,string tag="")
        {
            if (blockData == null) return null;
            if (IsBlockPixel(targetPos)) return null;
            List<TSVector2> paths = null;

            var finder = jPSFinding.GetPathAsync(PosToCell(startPos), PosToCell(targetPos));
            while (finder.MoveNext())
            {
                PathfindReturn ret = (PathfindReturn)finder.Current;
                if (ret.status == PathfindReturn.PathfindStatus.FOUND)
                {
                    if (ret.path.Count > 1)
                        paths = pathmodifier.ApplyIntersection(ret.path, startPos, targetPos);
                    break;
                }
                else if (ret.status == PathfindReturn.PathfindStatus.NOT_FOUND)
                {
                    Debug.Log("not find Path");
                    break;
                }
            }
            return paths;
        }
    }
}