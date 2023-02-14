using System.Collections;
using UnityEngine;
using TrueSync;
using System.Collections.Generic;
using System;

namespace MyAi
{
    public enum MoveState
    {
        Stand,
        MoveToLineUp,
        IncrementWayPoint,
        MoveReached,
        CloseToTarget,
        KeepOffFailed,
        LineUpEnd,
    }

    public enum StopCause
    {
        WayPathMoveReached,
        GroupReached,
        WaitForFriend,
        GroupAttack,
        EnemyStop,
        CannotReach
    }

    /// <summary>
    /// 士兵
    /// </summary>
    public class MoveUnit : UnitEntity
    {
        public MoveGroup moveGroup;

        private int _uid;

        public Action<StopCause> moveCallback = null;

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
            moveParty = new MoveParty();
            moveParty.gameObject = this.obj;
            formationIndex = uid % 100;
            if (uid < 0)
                formationIndex = Mathf.Abs(uid % 100);
            this.obj.name = uid.ToString();
            var pos = MoveManager.Instance.GetOffsetTest(moveGroup.Position, formationIndex);
            obj.transform.position = pos.ToVector();
            this.Position = pos;
        }

        private MoveState moveState;
        private int warPointIndex = -1;
        private TSVector2 nextPosition = MoveManager.MoveInvaild;
        private TSVector2 moveV;

        private MoveUnit targetUnit;

        public override TSVector2 Position
        {
            get => moveParty.position; set
            {
                MoveManager.Instance.UpdatePostion(Position, value,this);
                moveParty.position = value;
            }
        }
        public void UpdateStep()
        {
            if(moveState == MoveState.Stand)
            {

            }
            //跟随军团移动
            else if(moveState == MoveState.MoveToLineUp)
            {
                var tpos = moveGroup.Position;
                int dir = MoveManager.GetDirByPos(Position, tpos);
                //方向上下一个格子的坐标
                var nextPos = MoveManager.Instance.NextPos(Position, dir);
                //如果是阻挡格
                if(MoveManager.Instance.IsPixelBlock(nextPos))
                {
                    //那么检测朝向阵型中的点的路径上是否有可以移动的点
                    var goal = MoveManager.Instance.GetOffsetTest(moveGroup.Position, formationIndex);
                    FP dis = TSVector2.Distance(Position,goal).AsInt();
                    FP step = 0;
                    bool allBlock = true;
                    while (step < dis)
                    {
                        step += 0.1f;
                        var next = goal;
                        if (step < dis)
                            next = Position + TSVector2.ClampMagnitude(goal - Position, step);
                        if (!MoveManager.Instance.IsPixelBlock(next))
                        {
                            goal = next;
                            allBlock = false;
                            break;
                        }
                    }

                    //如果路径中没有可以移动的点 那么朝路径反方向上寻找
                    if(allBlock)
                    {
                        if (moveGroup.targetPos == MoveManager.MoveInvaild)
                        {
                            StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                            return;
                        }
                        int goalDir = MoveManager.GetDirByPos(moveGroup.Position, moveGroup.targetPos);
                        var diro = MoveManager.Instance.diros[goalDir];
                        var addDiro = MoveManager.Instance.CellToPosSize(diro.Item1, diro.Item2);
                        goal -= addDiro;
                        //goal.y -= diro[moveGroup.goalDir, 1] * moveGroup.formationRadius;
                        goal = MoveManager.Instance.PosToCenter(goal);
                    }

                    //如果是阻挡点 或者 距离军团中心点更近 那么移动位置设置为军团中心点
                    var center = MoveManager.Instance.PosToCenter(moveGroup.Position);
                    if (MoveManager.Instance.IsPixelBlock(goal) || TSVector2.DistanceSquared(Position, center)
                             < TSVector2.DistanceSquared(Position, goal))
                        goal = center;

                    if(Position.Equals(goal))
                    {
                        StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                        return;
                    }

                    //寻路移动 避开阻挡点
                    if (MoveManager.Instance.IsSameCell(Position, goal))
                    {
                        wayPointPaths = new List<TSVector2>() { goal };
                    }
                    else
                        WayPointMove(goal);

                    if(wayPointPaths == null)
                    {
                        StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                        //ScatterFromGoal();
                        return;
                    }
                }
                else
                {
                    //朝目标点移动
                    var mypos = MoveManager.Instance.GetOffsetTest(moveGroup.Position, formationIndex);
                    moveParty.target = mypos;

                    if (moveParty.reached && !moveGroup.isMoving)
                    {
                        StopMove(StopCause.GroupReached, MoveState.LineUpEnd);
                        //ScatterFromGoal();s
                        return;
                    }

                    //优化 判断与目标点距离进行加速 减速

                }

            }
            //寻路位移 是在跟随移动时发现目标点周围存在阻挡 则重新寻路
             else if(moveState == MoveState.IncrementWayPoint)
            {
                bool Reached = false;
                if (nextPosition != MoveManager.MoveInvaild)
                {
                    var lastPos = nextPosition - Position;
                    var nextPos = Position + TSVector2.ClampMagnitude(lastPos, 0.2f * 0.033f);
                 
                    //点乘判断两个向量的前后 小于0 代表在目标向量后
                    if (TSVector2.Dot(lastPos, nextPosition - nextPos) <= 0)
                    {
                        Position = nextPosition;
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
                            wayPointPaths.Clear();
                            wayPointPaths = null;
                            StopMove(StopCause.WayPathMoveReached,MoveState.IncrementWayPoint);
                        }
                        warPointIndex++;
                        nextPosition = wayPointPaths[warPointIndex];
                    }
                }
            }
            else if(moveState == MoveState.CloseToTarget)
            {
                //靠近目标
                if(moveParty.reached)
                {
                    CheckMoveNext();
                }
            }


        }

        private List<TSVector2> wayPointPaths;

        public void MoveToPosition(TSVector2 targetPos,bool isManual)
        {
            if (moveParty.target.Equals(targetPos)) return;
            moveParty.target = targetPos;
            moveState = MoveState.MoveToLineUp;
        }

        private void WayPointMove(TSVector2 targetPos)
        {
            wayPointPaths = AStarManager.Instance.astar.GetAStar(MoveManager.Instance.PosToCell(Position), MoveManager.Instance.PosToCell(targetPos));
            warPointIndex = -1;
            moveState = MoveState.IncrementWayPoint;
        }

        public void StopMove(StopCause cause,MoveState state)
        {
            if(cause == StopCause.WayPathMoveReached && moveGroup.state == GroupState.FormationMove)
            {
                moveState = MoveState.MoveToLineUp;
            }
            else if(cause == StopCause.WaitForFriend  || cause == StopCause.GroupReached)
            {
                moveState = MoveState.Stand;
            }
            else if(cause == StopCause.GroupAttack)
            {
                
            }
            else if(cause == StopCause.GroupReached && state == MoveState.LineUpEnd)
            {
                moveState = MoveState.CloseToTarget;
            }

            moveCallback?.Invoke(cause);
        }

        private void CheckMoveNext()
        {

        }

        public void UpdateState()
        {

        }
    }
}