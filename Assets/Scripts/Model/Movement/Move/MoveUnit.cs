using System;
using System.Collections.Generic;
using TrueSync;
using UnityEngine;

namespace Movement
{
    public enum MoveState
    {
        Stand = 0,
        KeepOffFailed,
        ReachGoal,
        LineUpEnd,
        WaitForPath,
        Displacement,
        DisplacementByTime,
        IncrementWayPoint,
        MoveToLineUp,
        CloseToTarget
    }

    public enum CollisionUnit
    {
        NotCollision = 0,
        NearByCollision ,
        BlockCollision,
        HardCollision
    }

    public enum UnitFlag
    {
        None = 0,
        Soldier =1,
        Captain = 2,
        Group = 4,
        Obstruct = 8
    }
    public class MoveUnit : MoveUniform<MoveUnit>
    {

        public MoveState moveState { get; protected set; } = MoveState.Stand;

        public UnitFlag flag { get; set; }

        public int camp { get; protected set; }

        private FP _moveSpeed;

        public override FP moveSpeed
        {
            get
            {
                return _moveSpeed;
            }
            set
            {
                _moveSpeed = value;
                moveParty.speed = _moveSpeed;
                moveStep = _moveSpeed == 0 ? 0 : moveDelta * _moveSpeed;
                stepTick = _moveSpeed == 0 ? 0 : (radius / moveStep).AsInt();
            }

        }

        public override TSVector2 position { get => moveParty.position; set => moveParty.position = value; }

#if !SERVER
        public GameObject gameobject => moveParty.gameObject;
#endif

        public MoveGroup moveGroup { get; protected set; }

        public MoveParty moveParty { get; protected set; }

        public int uid { get; protected set; }

        public bool isLeader {
            get
            {
                if (moveGroup != null)
                    return moveGroup.isLeader(uid);
                return false;
            }
        }

        public int gid
        {
            get
            {
                if (moveGroup != null)
                    return moveGroup.gid;
                return MoveMgr.InvalidUid;
            }
        }

        public override int direction { get => moveParty.dir; set => moveParty.dir = value; }

        public FP moveDelta { get; set; } = 1f / 30f;
        public FP moveStep;

        public int stepTick;

        public const int collisionSQ = 576;
        public const int radius = 12;
        public const int radiusSQ = 144;

        public int atkRangeSQ { get; private set; } = 1225;
        const int nearbySQ = 900;
        const int slowDownSQ = 1225;

        int _atkRange = 35;
        public int atkRange
        {
            get => _atkRange;
            set
            {
                _atkRange = value;
                atkRangeSQ = _atkRange * _atkRange;
            }
        }

        public DirPos formationOffset{ get; set; }
        public int formationIdx { get; set; }

        public List<TSVector2> waypoints;
        int waypointIdx;
        public TSVector2 moveGoal;
        TSVector2 nextPoint;
        int nextDir;

        bool moveToGoalBySelf = false;
        bool scatterFromGoal = false;

        TSVector2 displacementPos;
        FP displaceSpeed;

        TSVector2 displaceVector;
        int displaceTime = 0;
        int hasDisplaceTime = 0;
        TSVector2[] displacePath;
        private int startTime = 0;
        private Action deleyCB;

        TSVector2 dropVector;
        int dropDis = 20;
        int dropTime = 5;
        bool hasBezier = false;

        public static readonly FP SOLDIER_ARROW_HIGHT_FACTOR = 0.8f;

        public override Action<StopCause, MoveUnit> moveStopCB { get; set; }

        public MoveUnit(MoveParty moveParty,int uid,int camp,MoveGroup moveGroup = null)
        {
            this.uid = uid;
            this.camp = camp;
            this.moveParty = moveParty;
            this.moveGroup = moveGroup;
            moveParty.onSetReached = () => { isMoving = !moveParty.reached; };
#if UNITY_EDITOR
            this.moveParty.id = uid;

#endif
        }

        public override void DisplacementTo(TSVector2 target, FP speed)
        {
            displacementPos = target;
            displaceSpeed = speed;
            direction = Calculater.GetDirByPos(position, displacementPos);
        }

        public void DisplacementTo(int time,TSVector2 endPoint,int startTime,Action cb = null)
        {
            var dirVector = endPoint - position;
            displaceVector = dirVector / time;
            displacementPos = endPoint;
            displaceTime = time;
            hasDisplaceTime = 0;

            dropVector = dirVector.normalized * dropDis;
            var dropPoint = endPoint - dropVector;
            deleyCB = cb;
            InitPathByBezier(dropPoint, startTime);

            if (displacePath != null && displacePath.Length > 0)
                moveState = MoveState.DisplacementByTime;
            else
                position = endPoint;
        }

        public void DisplacementTo(int time,TSVector2 endPoint,Action cb = null)
        {
            var dirVector = endPoint - position;
            displaceVector = dirVector / time;
            displacementPos = endPoint;
            displaceTime = time;
            direction = Calculater.GetDirByPos(position, endPoint);
            hasDisplaceTime = 0;
            moveState = MoveState.DisplacementByTime;
            hasBezier = false;
            deleyCB = cb;
        }

        private void InitPathByBezier(TSVector2 endPos,int startTime)
        {
            this.startTime = startTime;
            var controlPos = new TSVector2((position.x + endPos.x) / 2, (position.y + endPos.y) / 2 + TSMath.Abs(position.x - endPos.x) * SOLDIER_ARROW_HIGHT_FACTOR);
            displacePath = Calculater.GetPathByBezier(position, controlPos, endPos, displaceTime - dropTime - startTime);
            dropVector /= dropTime;
            hasBezier = true;
        }

        public bool IsOnFormation()
        {
            if (moveToGoalBySelf)
                return true;
            if (moveState == MoveState.KeepOffFailed || moveState == MoveState.ReachGoal)
                return true;
            if (position != (formationOffset.pos + moveGroup.position))
                return false;
            return true;
        }

        void MoveByPath(List<TSVector2> path)
        {
            if(waypoints != null)
            {
                waypoints = path;
                moveGoal = waypoints[waypoints.Count - 1];
                moveParty.reached = true;
                waypointIdx = 0;
                nextPoint = waypoints[waypointIdx];
                if (MapMgr.Inst.IsCollisionBlock(nextPoint))
                    nextPoint = MapMgr.Inst.PosToCenter(nextPoint);
                MoveToNextPoint();
            }
        }

        void MoveToNextPoint()
        {
            if (waypoints == null) return;
            moveParty.target = nextPoint;
            moveState = MoveState.IncrementWayPoint;
        }

        public void MoveToPosition(TSVector2 target)
        {
            moveGoal = MapMgr.Inst.PosToCenter(target);
            direction = Calculater.GetDirByPos(position, moveGoal);
            moveState = MoveState.CloseToTarget;
            //CheckMoveToNext();
        }

        public void ApproachTarget(int uid)
        {
            if (targetPrey != null && targetPrey.uid == uid)
                return;
            var unit = MoveMgr.Inst.GetMoveUnit(uid);
            if (unit != null)
            {
                targetPrey = null;
                direction = Calculater.GetDirByPos(position, targetPrey.position);
                moveState = MoveState.CloseToTarget;
                CheckMoveToNext();
            }
        }

        bool CheckStandSpace()
        {
            var uids = MoveMgr.Inst.GetUnitsInCell(position, -1, uid);
            if (uids.Count > 0)
            {
                var emptyPos = MoveMgr.Inst.GetStandPosAround(position, position);
                if(emptyPos != position)
                {
                    for (int i = 0; i < uids.Count; i++)
                    {
                        var unit = MoveMgr.Inst.GetMoveUnit(uids[i]);
                        if(unit != null && !unit.isMoving)
                        {
                            moveParty.target = emptyPos;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void CheckMoveToNext()
        {
            if(targetPrey == null)
            {
                if(moveGoal.Equals(position) || moveGoal == MoveMgr.InvalidPos)
                {
                    StopMove(StopCause.ReachedGoal, MoveState.Stand);
                    return;
                }
            }
            var colRet = HasCollision(out var other, out var disSQ, position, true);
            bool canContinueMove = false;
            if (other == null)
                canContinueMove = true;
            else
            {
                if ((!IsGetInTheWay(other) || colRet != CollisionUnit.HardCollision) && other.camp == camp)
                    canContinueMove = true;
            }

            if (!canContinueMove)
            {
                if ((colRet == CollisionUnit.HardCollision || colRet == CollisionUnit.NearByCollision) && other.camp != camp)
                {
                    StopMoveCallBack(StopCause.EnemySpotted, MoveState.Stand, other);
                    return;
                }

                other = MoveMgr.Inst.GetCollisionMoveUnit(position, uid, atkRange, camp);
                if(other != null)
                {
                    StopMoveCallBack(StopCause.EnemySpotted, MoveState.Stand, other);
                    return;
                }

                var center = MoveMgr.Inst.PosToCenter(position);
                if(center != position)
                {
                    moveParty.target = center;
                    return;
                }
            }
            else
            {
                if(targetPrey != null && TSVector2.DistanceSquared(position,targetPrey.position) < atkRangeSQ)
                {
                    StopMoveCallBack(StopCause.EnemySpotted, MoveState.Stand, targetPrey);
                    return;
                }
                var tpos = targetPrey == null ? moveGoal : targetPrey.position;
                nextDir = Calculater.GetDirByPos(position, tpos);
                var nextCenter = MoveMgr.Inst.NextCenter(position, nextDir);
                MoveUnit nextUnit = null;
                var nunit = MoveMgr.Inst.GetUnitInCell(nextCenter);
                if (nunit != null && nunit.camp == camp)
                {
                    if (formationIdx > 0)
                    {
                        var offdir = formationIdx % 2 == 0 ? -1 : 1;
                        nextCenter = MoveMgr.Inst.NextCenter(position, (nextDir + 8 + offdir) % 8);
                        nunit = MoveMgr.Inst.GetUnitInCell(nextCenter);
                        if (nunit != null && nunit.camp == camp) { }
                        else
                            nextUnit = nunit;
                    }
                }
                else
                    nextUnit = nunit;

                if(nextUnit != nunit)
                {
                    if(TSVector2.DistanceSquared(position,nextUnit.position) < atkRangeSQ)
                    {
                        StopMoveCallBack(StopCause.EnemySpotted, MoveState.Stand, nextUnit);
                        return;
                    }
                    else
                    {
                        StopMoveCallBack(StopCause.HasNearlyEnemy, MoveState.CloseToTarget, nextUnit);
                        var nearPos = nextUnit.position + TSVector2.ClampMagnitude(position - nextUnit.position, radius * 2);
                        moveParty.target = nearPos;
                        moveParty.reached = false;
                        return;
                    }
                }
                else
                {
                    nextPoint = nextCenter;
                    if (!MoveMgr.Inst.IsBlockPixel(nextPoint))
                    {
                        moveParty.target = nextPoint;
                        moveParty.reached = false;
                        return;
                    }
                    else
                    {
                        //我正在往目标方向走，但是我的八方向前进格是阻挡格 ，此时是否寻路
                    }
                }
            }
        }

        public void JoinGroup(MoveGroup group)
        {
            camp = group.camp;
            moveGroup = group;
        }

        public override void TargetDispose(bool targetDead)
        {
            targetPrey = null;
            if (isMoving)
                StopMove(StopCause.EnemyDispose, MoveState.Stand);
        }
        
        public void StopMove(StopCause cause, MoveState endState = MoveState.Stand)
        {
            targetPrey = null;
            moveToGoalBySelf = false;
            waypoints = null;
            moveGoal = MoveMgr.InvalidPos;

            if (endState != MoveState.MoveToLineUp && cause != StopCause.ChangeTarget)
                moveParty.reached = true;
            moveState = endState;
        }


        public void StopMoveCallBack(StopCause cause,MoveState endState = MoveState.Stand,MoveUnit relate =null)
        {
            StopMove(cause, endState);
            moveStopCB?.Invoke(cause, relate);
        }

        int JudgeMovePriority()
        {
            var baseIdx = 50 - formationIdx;
            if (moveState == MoveState.IncrementWayPoint)
                return baseIdx;
            if (IsOnFormation() || moveState == MoveState.Stand) return 0;
            if (moveState == MoveState.MoveToLineUp) return 100;
            return baseIdx;
        }

        bool IsGetInTheWay(MoveUnit other)
        {
            if (other == null) return false;
            var dirForMe = Calculater.GetDirByPos(position, other.position);
            return Calculater.GetDirOffset(direction, dirForMe) < 2;
        }

        private CollisionUnit HasCollision(out MoveUnit other,out FP distance,TSVector2 pos,bool checkAtk)
        {
            other = null;
            distance = 0;
            if (MapMgr.Inst.IsBlockPixel(pos) || MapMgr.Inst.IsCollisionBlock(pos))
                return CollisionUnit.BlockCollision;
            other = MoveMgr.Inst.GetCollisionMoveUnit(pos, uid, checkAtk ? atkRange : radius);
            if(other != null)
            {
                var dis = TSVector2.DistanceSquared(pos, other.position);
                distance = dis.AsInt();
                if (dis <= collisionSQ)
                    return CollisionUnit.HardCollision;
                else if (dis <= atkRangeSQ)
                    return CollisionUnit.NearByCollision;
            }
            return CollisionUnit.NotCollision;
        }

        private CollisionUnit HasCollision(out MoveUnit other,out FP distance)
        {
            return HasCollision(out other, out distance, position, false);
        }

        public void ScatterFromGoal()
        {
            if(!scatterFromGoal)
            {
                scatterFromGoal = true;
                if (HasCollision(out var other,out var disSQ) != CollisionUnit.NotCollision)
                {
                    if (other!= null)
                    {
                        if (formationIdx > other.formationIdx)
                        {
                            var pos = MoveMgr.Inst.GetStandPosAround(position, position);
                            if (!pos.Equals(position))
                            {
                                scatterFromGoal = true;
                                moveParty.target = pos;
                            }
                        }
                    }
                }
            }
        }

        public bool MoveToLineUp()
        {
            if (moveState == MoveState.ReachGoal || moveState == MoveState.IncrementWayPoint ||
                moveState == MoveState.KeepOffFailed) return false;
            if (IsOnFormation()) return false;
            moveState = MoveState.MoveToLineUp;
            return true;
        }

        static readonly int[,] diro = { { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        int checkTick = 0;

        public override void LogicUpdate()
        {
            if (!moveEnable) return;
            switch (moveState)
            {
                case MoveState.Stand:
                    CheckStandSpace();
                    break;
                case MoveState.IncrementWayPoint:
                    if (waypoints != null)
                    {
                        if (moveParty.reached)
                        {
                            moveParty.speed = moveSpeed;
                            if (waypointIdx < waypoints.Count - 1)
                            {
                                waypointIdx++;
                                nextPoint = waypoints[waypointIdx];
                                if (MapMgr.Inst.IsCollisionBlock(nextPoint))
                                    nextPoint = MapMgr.Inst.PosToCenter(nextPoint);
                                MoveToNextPoint();
                            }
                            else
                            {
                                if (!moveToGoalBySelf)
                                    StopMove(StopCause.ReachedGoal);
                                else
                                    StopMove(StopCause.ReachedGoal, MoveState.MoveToLineUp);
                            }
                        }
                        if (!moveParty.reached)
                        {
                            if (moveToGoalBySelf)
                            {
                                var t = moveGroup.formationRadius;
                                if (TSVector2.DistanceSquared(position, moveGroup.position) > t * t)
                                    moveParty.speed = moveSpeed * 1.5;
                            }
                            HasCollision(out MoveUnit other, out FP disSQ);
                            if (other != null)
                            {
                                if (IsGetInTheWay(other))
                                {
                                    if (other.gid == gid && JudgeMovePriority() < other.JudgeMovePriority())
                                    {
                                        if (disSQ < slowDownSQ)
                                            moveParty.speed = moveSpeed * 0.5f;
                                        else if (disSQ < nearbySQ)
                                            moveParty.speed = 0;
                                    }
                                    break;
                                }
                            }
                            moveParty.speed = moveSpeed;
                        }
                    }
                    break;
                case MoveState.CloseToTarget:
                    if (moveParty.reached)
                    {
                        if (checkTick > 0)
                            checkTick--;
                        else
                        {
                            CheckMoveToNext();
                            checkTick = 30;
                        }
                    }
                    break;
                case MoveState.KeepOffFailed:
                case MoveState.ReachGoal:
                    if (scatterFromGoal && moveParty.reached)
                    {
                        scatterFromGoal = false;
                        SetReached(true);
                        direction = formationOffset.dir;
                        moveStopCB?.Invoke(StopCause.FormationReach, null);
                    }
                    break;
                case MoveState.MoveToLineUp:
                    var nextPos = MoveMgr.Inst.NextPos(position, direction);
                    bool nextBlock = MapMgr.Inst.IsCollisionBlock(nextPos);
                    if (moveGroup.keepFormation && nextBlock && direction == moveGroup.direction)
                    {
                        moveGroup.StopMove(StopCause.FormationObstruct, GroupState.FormationStand);
                        break;
                    }

                    if (nextBlock)
                    {
                        var goal = moveGroup.curTargetPos + formationOffset.pos;
                        int dis = TSVector2.Distance(position, goal).AsInt();
                        int step = 0;
                        bool allBlock = true;
                        while (step < dis)
                        {
                            step += moveGroup.formationRadius * 2;
                            var next = goal;
                            if (step < dis)
                                next = position + TSVector2.ClampMagnitude(goal - position, step);
                            if (!MapMgr.Inst.IsCollisionBlock(next))
                            {
                                goal = next;
                                allBlock = false;
                                break;
                            }
                        }
                        if (allBlock)
                        {
                            if (moveGroup.moveGoal == MoveMgr.InvalidPos)
                            {
                                StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                                break;
                            }
                            goal = moveGroup.moveGoal;
                            goal.x -= diro[moveGroup.goalDir, 0] * moveGroup.formationRadius;
                            goal.y -= diro[moveGroup.goalDir, 1] * moveGroup.formationRadius;
                            goal = MapMgr.Inst.PosToCenter(goal);
                        }
                        var center = MapMgr.Inst.PosToCenter(moveGroup.moveGoal);
                        if (MapMgr.Inst.IsBlockPixel(goal) || TSVector2.DistanceSquared(position, center)
                            < TSVector2.DistanceSquared(position, goal))
                            goal = center;
                        if (position == goal)
                        {
                            StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                            break;
                        }
                        if (MapMgr.Inst.IsSameCell(position, goal))
                            waypoints = new List<TSVector2>() { goal };
                        else
                            waypoints = MapMgr.Inst.FindCellPath(position, goal);
                        if (waypoints != null)
                        {
                            MoveByPath(waypoints);
                            moveToGoalBySelf = true;
                        }
                        else
                        {
                            StopMove(StopCause.CannotReach, MoveState.KeepOffFailed);
                            ScatterFromGoal();
                        }
                    }
                    else
                    {
                        var mypos = moveGroup.position + formationOffset.pos;
                        moveParty.target = mypos;
                        if (moveParty.reached && !moveGroup.isMoving)
                        {
                            StopMove(StopCause.ReachedGoal, MoveState.LineUpEnd);
                            ScatterFromGoal();
                            break;
                        }
                        if (TSVector2.DistanceSquared(position, mypos) > radiusSQ)
                            moveParty.speed = moveSpeed * 1.5f;
                        else
                            moveParty.speed = moveSpeed;
                    }
                    break;
            }
        }


        private void SetReached(bool reached)
        {
            moveParty.reached = reached;
            isMoving = !reached;
        }

        public override void UpdateStep()
        {
            if(moveState == MoveState.Displacement)
            {
                var lastPos = displacementPos - position;
                var nextPOs = position + TSVector2.ClampMagnitude(lastPos, displaceSpeed * moveParty.speed);
                if (TSVector2.Dot(lastPos, displacementPos - nextPOs) <= 0)
                {
                    position = displacementPos;
                    StopMove(StopCause.Displacement);
                }
                else
                    position = nextPOs;
            }
            else if (moveState == MoveState.DisplacementByTime)
            {
                if(hasDisplaceTime >= startTime)
                {
                    if(hasBezier)
                    {
                        if(deleyCB != null)
                        {
                            deleyCB.Invoke();
                            deleyCB = null;
                        }
                        int displacePathCount = hasDisplaceTime - startTime;
                        if (displacePathCount < displacePath.Length)
                        {
                            var nextPos = displacePath[displacePathCount];
                            position = nextPos;
                        }
                        else
                        {
                            if (hasDisplaceTime >= displaceTime)
                            {
                                position = displacementPos;
                                StopMove(StopCause.Displacement);
                                return;
                            }
                            else
                                position += dropVector;
                        }
                    }
                    else
                    {
                        position += displaceVector;
                        if (hasDisplaceTime >= displaceTime)
                        {
                            position = displacementPos;
                            deleyCB?.Invoke();
                            deleyCB = null;
                            StopMove(StopCause.Displacement);
                            return;
                        }
                    }
                }
                hasDisplaceTime++;
            }
            else if (moveEnable)
            {
                if (!moveEnable) return;
                moveParty.UpdateStep();
            }
        }
    }
}