using System;
using TrueSync;

namespace Movement
{
    public enum Dir
    {
        Down = 0,
        LeftDown = 1,
        Left = 2,
        LeftUp = 3,
        Up = 4,
        RightUp = 5,
        Right = 6,
        RightDown = 7
    }

    public struct DirPos
    {
        public TSVector2 pos;

        public int x
        {
            get => pos.x.AsInt();
            set => pos.x = value;
        }

        public int y
        {
            get => pos.y.AsInt();
            set => pos.y = value;
        }

        public int dir;

        public static readonly DirPos zero = new DirPos(0, 0, 0);

        public DirPos(int x,int y,int dir)
        {
            pos = new TSVector2(x, y);
            this.dir = dir;
        }

        public static DirPos operator +(DirPos value1,DirPos value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            value1.dir += value2.dir;
            return value1;
        }
    }

    public class Formation
    {
        public string name { get; private set; }

        public int count { get; private set; }

        public DirPos[] grids { get; private set; }

        public DirPos[] offsets { get; private set; }

        public DirPos[,] diroffsets { get; private set; }

        public TSVector2[,] cores { get; private set; }

        public int[] radius { get; private set; }

        public Formation(string name,int count)
        {
            this.name = name;
            this.count = count;
            grids = new DirPos[count];
            offsets = new DirPos[count];
            radius = new int[count];
            cores = new TSVector2[8, count];
        }

        public void GenerateOffset()
        {
            if (diroffsets != null) return;
            diroffsets = new DirPos[8, count];
            FP radiusSQ = 0;
            int coreX = 0;
            int coreY = 0;

            TSVector[] coreP = new TSVector[count];
            TSVector2 core = TSVector2.zero;
            for (int i = 0; i < count; i++)
            {
                coreX += offsets[i].x;
                coreY += offsets[i].y;

                core.x = coreX / (i + 1);
                core.y = coreY / (i + 1);

                coreP[i] = core.ToTSVector();

                radiusSQ = TSMath.Max(radiusSQ, TSVector2.DistanceSquared(offsets[i].pos, core));
                radius[i] = FP.Sqrt(radiusSQ).AsInt();
            }

            for (int groupDir = 0; groupDir < 8; groupDir++)
            {
                for (int i = 0; i < count; i++)
                {
                    cores[groupDir, i] = (TSQuaternion.AngleAxis((groupDir + 8 - 6) % 8 * 45, TSVector.back) * coreP[i]).ToTSVector2();
                    var offset = new TSVector(offsets[i].x, offsets[i].y, 0);
                    var ret = TSQuaternion.AngleAxis((groupDir + 8 - 6) % 8 * 45, TSVector.back) * offset;
                    diroffsets[groupDir, i] = new DirPos(ret.x.AsInt(), ret.y.AsInt(), GetSingleDir(groupDir, i));
                }
            }
        }

        public DirPos GetDirOffset(int groupDir,int index,int curName)
        {
            if(groupDir >= 0 && groupDir < 8 && index >= 0 && index < count)
            {
                var ret = diroffsets[groupDir, index];
                if (curName > 0)
                    ret.pos = ret.pos - cores[groupDir, curName - 1];
                return ret;
            }
            return DirPos.zero;
        }

        public int GetSingleDir(int groupDir,int index)
        {
            if(index >= 0 && index < count)
            {
                var off = (grids[index].dir + 8 - 6) % 8;
                return (groupDir + off) % 8;
            }
            return 0;
        }
    }
}