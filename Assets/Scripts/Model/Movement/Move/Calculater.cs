using System;
using System.Collections.Generic;
using TrueSync;
namespace Movement
{
    public class Calculater
    {
        private static int[] tmpArr = { 2, 1, 0, 7, 6, 5, 4, 3, 2 };


        public static int GetDirByVector(TSVector2 vector)
        {
            return tmpArr[(int)Math.Floor((Math.Atan2((float)vector.y, (float)vector.x) + Math.PI) / (Math.PI * 0.25f) + 0.5f)];
        }

        public static int GetDirByPos(TSVector2 start, TSVector2 tarPos)
        {
            float x = (float)tarPos.x - (float)start.x;
            float y = (float)tarPos.y - (float)start.y;
            return tmpArr[(int)Math.Floor((Math.Atan2(y, x) + Math.PI) / (Math.PI * 0.25f) + 0.5f)];
        }

        public static TSVector2[] GetPathByBezier(TSVector2 start, TSVector2 control, TSVector2 end, int pointNum)
        {
            TSVector2[] path = new TSVector2[pointNum];
            for (int i = 0; i < pointNum; i++)
            {
                var t = (i + 1) / (FP)pointNum;
                path[i] = GetBezierPoint(t, start, control, end);
            }
            return path;
        }

        public static TSVector2 GetBezierPoint(FP t, TSVector2 start, TSVector2 control, TSVector2 end)
        {
            return (1 - t) * (1 - t) * start + 2 * t * (1 - t) * control + t * t * end;
        }

        public static int GetDirOffset(int dir1, int dir2)
        {
            int offset = dir1 > dir2 ? dir1 - dir2 : dir2 - dir1;
            return offset % 8;
        }

        private static readonly double eps = 1e-6;
        public static void GetCircle(List<TSVector2> p, out TSVector2 center, out FP radius)
        {
            center = p[0];
            radius = 0;
            int i, j, k;
            for (i = 0; i < p.Count; i++)
            {
                if (Distance(center, p[i]) < radius + eps)
                {
                    continue;
                }
                center = p[i];
                radius = 0;
                for (j = 0; j < i; j++)
                {
                    if (Distance(center, p[j]) < radius + eps)
                    {
                        continue;
                    }
                    Circle2Point(p[i], p[j], out center, out radius);
                    for (k = 0; k < j; k++)
                    {
                        if (Distance(center, p[k]) < radius + eps)
                            continue;
                        Circle3Point(p[i], p[j], p[k], out center, out radius);
                    }
                }
            }
        }

        private static FP Distance(TSVector2 p1, TSVector2 p2)
        {
            return TSVector2.Distance(p1, p2);
        }

        /// <summary>
        /// 求出以两点为直径的圆
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        private static void Circle2Point(TSVector2 p1, TSVector2 p2, out TSVector2 center, out FP radius)
        {
            center = new TSVector2((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
            radius = Distance(p1, center);
        }

        /// <summary>
        /// 求出过三个点的圆
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        private static void Circle3Point(TSVector2 p1, TSVector2 p2, TSVector2 p3, out TSVector2 center, out FP radius)
        {
            if (FP.Abs((p3.y - p1.y) * (p2.x - p1.x) - (p2.y - p1.y) * (p3.y - p1.x)) < eps)
            {
                FP d1 = Distance(p1, p2);
                FP d2 = Distance(p2, p3);
                FP d3 = Distance(p1, p3);

                if (d1 > d2 && d1 > d3)
                {
                    Circle2Point(p1, p2, out center, out radius);
                }
                else if (d2 > d1 && d2 > d3)
                {
                    Circle2Point(p2, p3, out center, out radius);
                }
                else
                {
                    Circle2Point(p1, p3, out center, out radius);
                }
            }
            else
            {
                PerpendicularBisector(p1, p2, out FP A1, out FP B1, out FP C1);
                PerpendicularBisector(p1, p3, out FP A2, out FP B2, out FP C2);

                FP a = A1 * B2 - A2 * B1;
                center = new TSVector2((B2 * C1 - B1 * C2) / a, (A1 * C2 - A2 * C1) / a);
                radius = Distance(center, p1);
            }
        }

        /// <summary>
        /// 求两点的中垂线方程
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        private static void PerpendicularBisector(TSVector2 p1,TSVector2 p2,out FP A,out FP B,out FP C)
        {
            A = p2.x - p1.x;
            B = p2.y - p1.y;
            FP a = p2.x + p1.x;
            FP b = p2.y + p1.y;
            C = (A * a + B * b) / 2;
        }

        public static bool IsPointInSector(TSVector2 ownerPos,TSVector2 targetPos,TSVector2 forward,FP angle,FP dis)
        {
            if (TSVector2.Distance(ownerPos, targetPos) > dis) return false;
            return IsPointInSectorAngle(ownerPos, targetPos, forward, angle, dis);
        }

        public static bool IsPointInSectorAngle(TSVector2 ownerPos,TSVector2 targetPos,TSVector2 forward,FP angle,FP dis)
        {
            FP dot = TSVector2.Dot(forward.normalized, (targetPos - ownerPos).normalized);
            dot = TSMath.Clamp(dot, -1, 1);
            return TSMath.Acos(dot) * TSMath.Rad2Deg < angle * 0.5f;
        }

        public static bool IsPointInMatrix(TSVector2 p1,TSVector2 p2,TSVector2 p3,TSVector2 p4,TSVector2 p)
        {
            return TSVector2.Dot(p - p1, p4 - p1) >= 0 && TSVector2.Dot(p - p1, p2 - p1) >= 0 &&
                TSVector2.Dot(p - p3, p4 - p3) >= 0 && TSVector2.Dot(p - p3, p2 - p3) >= 0;
        }

        public static bool IsPointInRectangle(int rectWidth,int skillRange,TSVector2 toNormalized,TSVector2 ownerPixel,TSVector2 targetPixel)
        {
            RotateRect(rectWidth, skillRange, toNormalized, out TSVector2 a, out TSVector2 b, out TSVector2 c, out TSVector2 d);
            a += ownerPixel;
            b += ownerPixel;
            c += ownerPixel;
            d += ownerPixel;

            if (!Calculater.IsPointInMatrix(a, b, c, d, targetPixel))
                return true;
            return false;
        }

        public static void RotateRect(int width,int length,TSVector2 to,out TSVector2 a,out TSVector2 b,out TSVector2 c,out TSVector2 d)
        {
            RotateRect(width, length, TSVector2.down, to, out a, out b, out c, out d);
        }

        public static void RotateRect(int width,int length,TSVector2 from,TSVector2 to, out TSVector2 a, out TSVector2 b, out TSVector2 c, out TSVector2 d)
        {
            var aa = new TSVector(-width / 2, 0, 0);
            var bb = new TSVector(width / 2, 0, 0);
            var cc = new TSVector(width / 2, -length, 0);
            var dd = new TSVector(-width / 2, -length, 0);
            TSQuaternion angle = TSQuaternion.FromToRotation(from.ToTSVector(), to.ToTSVector());
            aa = angle * aa;
            bb = angle * bb;
            cc = angle * cc;
            dd = angle * dd;

            a = aa.ToTSVector2();
            b = bb.ToTSVector2();
            c = cc.ToTSVector2();
            d = dd.ToTSVector2();
        }
    }
}