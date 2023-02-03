using System;
using System.Collections.Generic;
using TrueSync;
namespace Pathfinding
{
    public interface FPoint
    {
        int x { get; set; }
        int y { get; set; }
    }

    public class ANode
    {
        public bool canWalk;

        public TSVector2 point;

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

        public int gCost;
        public int hCost;

        public int fCost => gCost + hCost;

        public ANode parent;

        public ANode(int x,int y)
        {
            point = new TSVector2(x, y);
        }

        public bool Equals(JPoint other)
        {
            return this.x == other.x && this.y == other.y;
        }
    }

    public struct APoint : FPoint
    {
        TSVector2 point;
        public APoint(int x,int y)
        {
            point = new TSVector2(x, y);
        }

        public APoint(ANode node)
        {
            point = node.point;
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

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }

    public class PathfindReturn
    {
        public enum PathfindStatus
        {
            SEARCHING,
            FOUND,
            NOT_FOUND
        }

        public PathfindingNode current;
        public PathfindStatus status = PathfindStatus.SEARCHING;
        public List<FPoint> path = new List<FPoint>();
    }
}