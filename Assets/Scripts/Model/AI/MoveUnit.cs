using System.Collections;
using UnityEngine;
using TrueSync;
using System.Collections.Generic;
namespace MyAi
{
    public enum MoveState
    {
        Stand,
        LineUp,
        IncrementWayPoint,
        MoveReached,
    }

    /// <summary>
    /// 士兵
    /// </summary>
    public class MoveUnit : UnitEntity
    {
        public MoveGroup moveGroup;

        private int _uid;

        public int Uid {
            get
            {
                return _uid;
            }
        }

        private int _camp;

        public int Camp
        {
            get
            {
                return _camp;
            }
        }

        public int formationIndex = -1;
        public MoveUnit(int uid,int camp,MoveGroup moveGroup,GameObject prefab)
        {
            this._uid = uid;
            this._camp = camp;

            this.moveGroup = moveGroup;

            this.obj = GameObject.Instantiate(prefab);
            formationIndex = uid % 100;
            if (uid < 0)
                formationIndex = 100 + uid;
            var pos = MoveManager.Instance.GetOffsetTest(moveGroup.Position, formationIndex);
            obj.transform.position = pos.ToVector();
            this.Position = pos;
        }

        private MoveState moveState;
        private int warPointIndex = -1;
        private TSVector2 nextPosition = MoveManager.MoveInvaild;
        private TSVector2 moveV;

        public override TSVector2 Position
        {
            get => base.Position; set
            {
                base.Position = value;
                obj.transform.position = Position.ToVector();
            }
        }
        public void UpdateStep()
        {
            //寻路位移
            if(moveState == MoveState.IncrementWayPoint)
            {
                bool Reached = false;
                if (nextPosition != MoveManager.MoveInvaild)
                {
                    var lastPos = nextPosition - Position;
                    var nextPos = Position + TSVector2.ClampMagnitude(lastPos, 0.2f * 0.033f);
                 
                    //点乘判断两个向量的前后 小于0 代表在目标向量后
                    if (TSVector2.Dot(lastPos, nextPosition - nextPos) <= 0)
                    {
                        Position = nextPos;
                        Reached = true;
                    }
                    else
                        Position = nextPos;
                }
                if (wayPointPaths != null && wayPointPaths.Count > 0)
                {
                    if (warPointIndex == -1 || Reached)
                    {
                        if (warPointIndex >= wayPointPaths.Count - 1)
                        {
                            warPointIndex = -1;
                            moveState = MoveState.MoveReached;
                            Position = wayPointPaths[wayPointPaths.Count - 1];
                            nextPosition = MoveManager.MoveInvaild;
                        }
                        warPointIndex++;
                        nextPosition = wayPointPaths[warPointIndex];
                    }
                }
            }
        }

        private List<TSVector2> wayPointPaths;
        private TSVector2 TargerPos = MoveManager.MoveInvaild;
        public void MoveToPosition(TSVector2 targetPos,bool isManual)
        {
            if (this.TargerPos.Equals(targetPos)) return;
            this.TargerPos = MoveManager.Instance.GetOffsetTest(targetPos, formationIndex);
            wayPointPaths = AStarManager.Instance.astar.GetAStar(MoveManager.Instance.PosToCell(Position), MoveManager.Instance.PosToCell(TargerPos));
            warPointIndex = -1;
            moveState = MoveState.IncrementWayPoint;
        }
    }
}