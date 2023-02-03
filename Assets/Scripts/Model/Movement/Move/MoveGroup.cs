using System;
using System.Collections;
using System.Collections.Generic;
using TrueSync;
namespace Movement
{
    public enum GroupState
    {
        FreeStand = 0,
        FormationStand,
        GroupReached,
        DisplaceWait,
        Displacement,
        DisplacementByTime,
        LineUp,
        FormationMove,
        FreeAttack
    }
    public class MoveGroup : MoveUniform<MoveGroup>
    {
        public UnitFlag flag { get; set; }

        public GroupState groupState { get; protected set; }

        int _captainUid = MoveMgr.InvalidUid;

        public int captainUid
        {
            get => _captainUid;
            protected set
            {
                _captainUid = value;
                if (_captainUid != MoveMgr.InvalidUid)
                {
                    if (moveUnits.TryGetValue(_captainUid, out var leader))
                    {
                        leaderUnit = leader;
                        leader.flag = UnitFlag.Captain;
                    }
                }
            }
        }

        public override FP moveSpeed
        {
            get
            {
                if (leaderUnit != null)
                    return leaderUnit.moveSpeed;
                foreach (var unit in moveUnits.Values)
                    return unit.moveSpeed;
                return 0;
            }
            set
            {
                foreach (var unit in moveUnits.Values)
                    unit.moveSpeed = value;
            }
        }

        public Formation formation { get; private set; } = null;

        public int radius { get; private set; } = 0;
        private int groupRadius = 0;

        public static float atkDisRate { get; set; } = 1.2f;

        

        public void ChangeFormation(Formation newFormation)
        {
            if(formation != newFormation)
            {
                formation = newFormation;
                radius = MoveUnit.radius + formationRadius;
                groupRadius = radius;
                if (agentSid >= 0)
                    RVO.Simulator.Instance.setAgentRadius(agentSid, radius);
            }
        }

        public bool isLeader(int uid)
        {
            if (leaderUnit != null) return uid == leaderUnit.uid;
            return false;
        }

        public override void Dispose()
        {
            if (agentSid >= 0)
                RVO.Simulator.Instance.delAgent(agentSid);
            agentSid = -1;
        }

        public int camp { get; protected set; }

        public int gid { get; protected set; }

        public int formationRadius => (formation == null || unitsNum <= 0) ? 0 : formation.radius[unitsNum - 1];

        public bool keepFormation { get; set; } = false;

        public Dictionary<int, MoveUnit> moveUnits { get; private set; } = new Dictionary<int, MoveUnit>();

        public int unitsNum => moveUnits.Count;

        MoveUnit leaderUnit;
        MoveParty groupPivot;

        public override Action<StopCause,MoveGroup> moveStopCB { get; set; }

#if !SERVER
        public UnityEngine.GameObject gameObject => groupPivot.gameObject;
#endif

#if UNITY_EDITOR
        public List<TSVector2> showPoints;

        public UnityEngine.Vector3 showCenter
        {
            get
            {
                if (isChargeAttack)
                    return enemyCenter.ToVector();
                return position.ToVector();
            }
        }
#endif
        public int goalDir;
        public TSVector2 moveGoal = MoveMgr.InvalidPos;

        public override MoveGroup targetPrey { get ; protected set ; }

        public override TSVector2 position
        {
            get => groupPivot.position;
            set {
                groupPivot.position = value;
                foreach (var unit in moveUnits.Values)
                {
                    unit.position = unit.formationOffset.pos + groupPivot.position;
                    if (MapMgr.Inst.IsCollisionBlock(unit.position))
                    {
                        unit.position = groupPivot.position;
                    }
                    unit.direction = unit.formationOffset.dir;
                    unit.ScatterFromGoal();
                }
                SetGroupState(GroupState.FormationStand);
            }
        }

        public override int direction { get => groupPivot.dir; set =>groupPivot.dir = value % 8; }

        private void SetGroupState(GroupState state)
        {
            groupState = state;
            isMoving = groupState == GroupState.FormationMove;
        }

        TSVector2 enemyCenter;
        bool manualMove = false;

        bool isChargeAttack = false;
        int agentSid = -1;
        public void AttackTarget(MoveGroup enemy,bool chargeAttack)
        {
            isChargeAttack = chargeAttack;
            if (isChargeAttack)
            {
                enemyCenter = enemy.position;
                if (agentSid >= 0)
                {
                    RVO.Simulator.Instance.setAgentRelationGroup(agentSid, 0);
                }
            }
        }

        int manualTargetGid = MoveMgr.InvalidUid;
        public override void TargetDispose(bool targetDead)
        {
            targetPrey = null;
            checkPeryYawTick = 0;

            if (isChargeAttack && agentSid >= 0 && targetDead)
            {
                RVO.Simulator.Instance.setAgentRelationGroup(agentSid, camp + 10);
                var normalVec = (enemyCenter - position).normalized;
                var fightCenter = position + normalVec * radius;
                groupPivot.target = fightCenter;
                var speed = TSVector2.Distance(fightCenter, position) * 30;
                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, speed);
                RVO.Simulator.Instance.setAgentPrefVelocity(agentSid, normalVec * speed);
            }

            isChargeAttack = false;
            manualTargetGid = MoveMgr.InvalidUid;

            if (isMoving)
                StopMove(StopCause.EnemyDispose, GroupState.FreeStand);
        }

        public void ApproachTarget(int gid,bool isManual = false)
        {
            targetPrey = MoveMgr.Inst.GetMoveGroup(gid);
            if (isManual)
                manualTargetGid = gid;
        }

        TSVector2 displacementPos;
        FP displaceSpeed;
        TSVector2 displaceVector;
        int displaceTime = 0;
        int hasDisplaceTime = 0;
        bool waitingManualMove = false;
        int waitingTick = 0;
        int checkPathYawTick = 0;
        int checkPeryYawTick = 0;
        int stopUpTick = 0;

        public override void DisplacementTo(TSVector2 target, FP speed)
        {
            displacementPos = target;
            displaceSpeed = speed;
            if (!OnFromtaion())
            {
                SetGroupState(GroupState.DisplaceWait);
                foreach (var unit in moveUnits.Values)
                {
                    var lineUpPos = unit.formationOffset.pos + displacementPos;
                    unit.DisplacementTo(lineUpPos,speed);
                }
            }
            else
            {
                direction = Calculater.GetDirByPos(position, displacementPos);
                SetGroupState(GroupState.Displacement);
                foreach (var unit in moveUnits.Values)
                    unit.DisplacementTo(target, speed);
            }
        }

        private List<TSVector2> _points = new List<TSVector2>();

        public void ReCalculateRadius()
        {
            _points.Clear();
            foreach (var unit in moveUnits.Values)
            {
                _points.Add(unit.position);
            }
            Calculater.GetCircle(_points, out TSVector2 center, out FP r);
            groupPivot.position = center;
            radius = (int)r + MoveUnit.radius / 2;
            if(agentSid >= 0)
            {
                RVO.Simulator.Instance.setAgentRadius(agentSid, radius);
                RVO.Simulator.Instance.setAgentPosition(agentSid, groupPivot.position);
            }
        }

        public void DisplacementByTime(TSVector2 target,int time,Formation formation)
        {
            displacementPos = target;
            displaceVector = (target - position) / time;
            hasDisplaceTime = 0;
            displaceTime = time;
            direction = Calculater.GetDirByPos(position, displacementPos);
            SetGroupState(GroupState.DisplacementByTime);
            foreach (var unit in moveUnits.Values)
            {
                var unitTarget = target + formation.GetDirOffset(direction, unit.formationIdx, unitsNum).pos;
                unit.DisplacementTo(time, unitTarget);
            }
        }

        public void MoveToPosition(TSVector2 point,TSVector2 goal,bool isManual)
        {
            if (curTargetPos.Equals(point))
                return;
            ClearAgentVelocity();
            foreach (var unit in moveUnits.Values)
                unit.StopMove(StopCause.ChangeTarget, MoveState.WaitForPath);
            manualMove = isManual;

            curTargetPos = point;
            moveGoal = goal;
            goalDir = Calculater.GetDirByVector(moveGoal - position);

            SetGroupState(GroupState.FormationMove);

            groupPivot.reached = false;
            groupPivot.target = point;

            ReorderLineUp();
            SetDefaultVelocity();
        }

        public MoveGroup(MoveParty moveParty,int gid,int camp)
        {
            groupPivot = moveParty;
            groupPivot.dirChange = (dir) => {
                if(groupState == GroupState.FormationStand)
                {
                    ReorderLineUp();
                    SetGroupState(GroupState.LineUp);
                }
            };
            this.gid = gid;
            this.camp = camp;

            checkPeryYawTick = Math.Abs(gid);
        }

        public int AddMoveUnit(MoveUnit unit,bool isCaptain= false)
        {
            moveUnits.Add(unit.uid, unit);
            unit.JoinGroup(this);
            if (isCaptain)
            {
                captainUid = unit.uid;
                leaderUnit = unit;
            }
            return moveUnits.Count;
        }

        public int RemoveUnit(MoveUnit unit)
        {
            if (moveUnits.ContainsKey(unit.uid))
                moveUnits.Remove(unit.uid);
            if (captainUid == unit.uid)
                captainUid = MoveMgr.InvalidUid;
            if (leaderUnit != null && unit.uid == leaderUnit.uid)
                leaderUnit = null;
            if (unitsNum > 0)
                ReorderLineUp();
            return unitsNum;
        }

        public void StartLocation(int direction,TSVector2 position,Formation newFormation)
        {
            formation = newFormation;
            if (agentSid >= 0)
                RVO.Simulator.Instance.delAgent(agentSid);
            agentSid = RVO.Simulator.Instance.addAgent(position, formationRadius, 0);

            if (agentSid >= 0)
                RVO.Simulator.Instance.setAgentRelationGroup(agentSid, camp + 10);
            if (captainUid != MoveMgr.InvalidUid)
            {
                int index = 0;
                if (moveUnits.TryGetValue(captainUid,out leaderUnit))
                {
                    leaderUnit.formationIdx = index;
                    leaderUnit.formationOffset = formation.GetDirOffset(direction, index, unitsNum);
                    index++;
                }
                foreach (var unit in moveUnits.Values)
                {
                    if (unit.uid == captainUid) continue;
                    unit.formationIdx = index;
                    unit.formationOffset = formation.GetDirOffset(direction, index, unitsNum);
                    index++;
                }
            }
            else
            {
                int index = 0;
                foreach (var unit in moveUnits.Values)
                {
                    if (index == 0) leaderUnit = unit;
                    unit.formationIdx = index;
                    unit.formationOffset = formation.GetDirOffset(direction, index, unitsNum);
                    index++;
                }
            }
            this.direction = direction;
            this.position = position;
        }

        HashSet<int> ordered = new HashSet<int>();
        int lastLineUpDir = -1;
        int lastLineUpNum = 0;

        private void ReorderLineUp()
        {
            if (formation != null && unitsNum > 0)
            {
                if (lastLineUpNum == unitsNum && lastLineUpDir == direction) return;

                lastLineUpDir = direction;
                lastLineUpNum = unitsNum;
                ordered.Clear();

                if (captainUid != MoveMgr.InvalidUid)
                {
                    if(moveUnits.TryGetValue(captainUid,out leaderUnit))
                    {
                        leaderUnit.formationIdx = 0;
                        leaderUnit.formationOffset = formation.GetDirOffset(direction, 0, unitsNum);
                        ordered.Add(leaderUnit.uid);
                    }
                }
                for (int i = ordered.Count; i < moveUnits.Count; i++)
                {
                    int ii = i;
                    var offset = formation.GetDirOffset(direction, i, unitsNum);
                    var lineupPos = position;
                    if (isMoving)
                        lineupPos = position + (curTargetPos - position).normalized * formationRadius * 2;
                    lineupPos = lineupPos + offset.pos;
                    var nearests = MoveMgr.Inst.GetNearestMoveUnit(lineupPos, (n) =>
                    {
                        if (n.gid != gid) return false;
                        if (ordered.Contains(n.uid)) return false;
                        return true;
                    });
                    if (nearests != null)
                    {
                        if (i == 0 && captainUid == MoveMgr.InvalidUid)
                            leaderUnit = nearests;
                        nearests.formationIdx = i;
                        nearests.formationOffset = offset;
                        ordered.Add(nearests.uid);
                    }
                    else
                        break;
                }
                radius = MoveUnit.radius + formationRadius;
                groupRadius = radius;
                if (agentSid >= 0)
                    RVO.Simulator.Instance.setAgentRadius(agentSid, radius);
            }
        }

        static int[] perturbNums = { 17,19,23,29,31,37,41,43,47,53,59,61};
        static int perturbIdx = 0;

        static int perturb
        {
            get
            {
                var val = perturbNums[perturbIdx++];
                if (perturbIdx >= perturbNums.Length)
                    perturbIdx = 0;
                return val;
            }
        }

        bool isYawed = false;
        bool yawEnd = true;

        public override void UpdateStep()
        {
            if (groupState == GroupState.Displacement)
            {
                var lastDPos = displacementPos - groupPivot.position;
                var nextDPos = groupPivot.position + TSVector2.ClampMagnitude(lastDPos, displaceSpeed * groupPivot.delta);
                if (TSVector2.Dot(lastDPos,displacementPos - nextDPos)<= 0)
                {
                    groupPivot.position = displacementPos;
                    SetGroupState(GroupState.FormationStand);
                    moveStopCB?.Invoke(StopCause.Displacement, null);
                }
                else
                {
                    groupPivot.position = nextDPos;
                }
                RVO.Simulator.Instance.setAgentPosition(agentSid, groupPivot.position);
                return;
            }
            else if(groupState == GroupState.DisplacementByTime)
            {
                if (hasDisplaceTime <= displaceTime)
                    groupPivot.position += displaceVector;
                else
                {
                    groupPivot.position = displacementPos;
                    SetGroupState(GroupState.FormationStand);
                    moveStopCB?.Invoke(StopCause.Displacement, null);
                }
                RVO.Simulator.Instance.setAgentPosition(agentSid, groupPivot.position);
                hasDisplaceTime++;
                return;
            }

            if (agentSid < 0) return;
            if (waitingTick > 0) return;
            if (isChargeAttack) return;
            if (waitingManualMove) return;

            var lastPos = groupPivot.position;
            var targetPos = groupPivot.target;
            var newPos = RVO.Simulator.Instance.getAgentPosition(agentSid);
            if(stopUpTick > 0)
            {
                stopUpTick--;
                if (stopUpTick <= 0)
                    StopMoveCallBack(StopCause.CannotReach, GroupState.FreeStand);
                return;
            }

            if (!moveEnable || !groupPivot.enable || groupPivot.reached)
            {
                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);
                return;
            }

            if (MapMgr.Inst.IsBlockPixel(newPos))
            {
                stopUpTick = 10;
                RVO.Simulator.Instance.setAgentTimeHorizon(agentSid, 1);
                RVO.Simulator.Instance.setAgentPosition(agentSid, lastPos);
                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);
                StopMove(StopCause.FormationObstruct, GroupState.FreeStand);
                return;
            }
            RVO.Simulator.Instance.setAgentTimeHorizon(agentSid, 2);

            //防止走过了
            if (targetPos == lastPos || TSVector2.DistanceSquared(targetPos,lastPos)<4)
            {
                groupPivot.position = targetPos;
                groupPivot.reached = true;
                RVO.Simulator.Instance.setAgentPosition(agentSid, targetPos);
                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);
                return;
            }
            else if (RVO.Simulator.Instance.getAgentMaxSpeed(agentSid)>0)
            {
                //已经到达目标点
                var velocity = RVO.Simulator.Instance.getAgentVelocity(agentSid);
                if (RVO.RVOMath.absSq(velocity) <1f)
                {
                    waitingTick = 30;
                    RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);

                    foreach (var unit in moveUnits.Values)
                        unit.StopMove(StopCause.WaitForFriend, MoveState.Stand);
                    RVO.Simulator.Instance.setAgentPosition(agentSid, lastPos);
                    return;
                }
                //检测路径是否偏移
                if (checkPathYawTick > 10)
                {
                    checkPathYawTick = 0;
                    var forward = targetPos - newPos;
                    //是否偏航
                    float dot = TSVector2.Dot(velocity.normalized, forward.normalized).AsFloat();
                    var yaw = dot < 0.9f;
                    if (!isYawed && yaw)
                    {
                        isYawed = true;
                        yawEnd = false;
                    }
                    else if (isYawed && !yaw)
                    {
                        StopMoveCallBack(StopCause.YawedFromPath, GroupState.FreeStand);
                        RVO.Simulator.Instance.setAgentPosition(agentSid, lastPos);
                        return;
                    }
                }
                checkPeryYawTick++;
            }
            groupPivot.position = newPos;
            SetDefaultVelocity();
        }

        FP lastLineUpX = 0;
        FP lastLineUpY = 0;
        bool onlyEnemy = true;
        bool collision = false;
        MoveGroup other = null;
        FP distance = 0;

        public void AddIgnoreGroup(MoveGroup group)
        {
            if (group == null) return;
            RVO.Simulator.Instance.addIgnoreAgent(agentSid, group.agentSid);
        }

        public void RemoveIgnoreGroup(MoveGroup group)
        {
            if (group == null) return;
            RVO.Simulator.Instance.removeIgnoreAgent(agentSid, group.agentSid);
        }

        public override void LogicUpdate()
        {
            if (!moveEnable) return;
            switch (groupState)
            {
                case GroupState.LineUp:
                    if (OnFromtaion())
                        SetGroupState(GroupState.FormationStand);
                    else
                        foreach (var unit in moveUnits.Values)
                        {
                            unit.MoveToLineUp();
                        }
                    break;
                case GroupState.FormationMove:
                    collision = false;
                    other = null;
                    //如果存在手动目标，先判断是否接触，接触则直接攻击
                    if (manualTargetGid != MoveMgr.InvalidUid)
                    {
                        if (MoveMgr.Inst.moveGroups.TryGetValue(manualTargetGid,out other))
                        {
                            distance = TSVector2.Distance(position, other.position);
                            if(distance <= (int)((radius + other.radius) * atkDisRate))
                            {
                                StopMoveCallBack(StopCause.EnemySpotted, GroupState.FreeAttack, other);
                                break;
                            }
                        }
                    }

                    foreach (var group in MoveMgr.Inst.moveGroups.Values)
                    {
                        if (group.gid == gid) continue;
                        if (onlyEnemy && group.camp == camp)
                            continue;
                        distance = TSVector2.Distance(position, group.position);
                        if (distance <= (int)((radius + group.radius)* atkDisRate))
                        {
                            other = group;
                            collision = true;
                            break;
                        }
                    }

                    if (collision)
                    {
                        if (!manualMove && manualTargetGid == MoveMgr.InvalidUid)
                        {
                            StopMoveCallBack(StopCause.EnemySpotted, GroupState.FreeAttack, other);
                            break;
                        }
                        else
                        {
                            waitingManualMove = Calculater.GetDirOffset(direction, Calculater.GetDirByPos(position, other.position)) < 2;
                            if (waitingManualMove && agentSid >= 0)
                                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);
                        }
                    }
                    else
                        waitingManualMove = false;
                    if (targetPrey != null)
                    {
                        if (checkPeryYawTick > 30)
                        {
                            checkPeryYawTick = 0;
                            float dot = TSVector2.Dot((targetPrey.position - position).normalized,
                                (moveGoal - position).normalized).AsFloat();
                            var yawDis = TSVector2.DistanceSquared(targetPrey.position, moveGoal);
                            if (dot < 0.9f || yawDis > 40000f)
                            {
                                StopMoveCallBack(StopCause.TargetPeryYawed, GroupState.FormationStand, targetPrey);
                                break;
                            }
                        }
                        checkPeryYawTick++;
                    }

                    if (waitingTick > 0)
                    {
                        waitingTick--;
                    }
                    else if (stopUpTick <= 0)
                    {
                        if (FP.FastAbs(lastLineUpX - position.x) > 2f || FP.FastAbs(lastLineUpY - position.y) > 2f)
                        {
                            lastLineUpX = position.x;
                            lastLineUpY = position.y;
                            foreach (var unit in moveUnits.Values)
                            {
                                unit.MoveToLineUp();
                            }
                        }
                    }

                    if (groupPivot.reached)
                    {
                        if (targetPrey != null)
                        {
                            if (TSVector2.Distance(position,targetPrey.position)>radius + targetPrey.radius)
                            {
                                StopMoveCallBack(StopCause.TargetPeryYawed, GroupState.FormationStand, targetPrey);
                                break;
                            }
                        }
                        if (OnFromtaion())
                        {
                            foreach (var unit in moveUnits.Values)
                            {
                                unit.direction = unit.formationOffset.dir;
                                StopMove(StopCause.FormationReach, GroupState.FormationStand);
                            }
                        }
                        else
                        {
                            ReorderLineUp();
                            if (manualMove)
                            {
                                StopMoveCallBack(StopCause.ManualReach, GroupState.LineUp);
                            }
                            else
                                StopMoveCallBack(StopCause.GroupReach, GroupState.LineUp);
                        }
                    }
                    break;
            }
        }

      

        private void StopMoveCallBack(StopCause cause,GroupState endState,MoveGroup relate = null)
        {
            StopMove(cause, endState);
            moveStopCB?.Invoke(cause, relate);
        }

        private void SetDefaultVelocity()
        {
            RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, moveSpeed);
            var goalVector = groupPivot.target - RVO.Simulator.Instance.getAgentPosition(agentSid);

            goalVector = RVO.RVOMath.normalize(goalVector) * moveSpeed;

            FP angle = perturb * 2.0f * TSMath.Pi;
            FP dist = perturb * 0.0001f;
            goalVector += dist * new TSVector2(TSMath.Cos(angle), TSMath.Sin(angle));
            //rvo前进
            RVO.Simulator.Instance.setAgentPrefVelocity(agentSid, goalVector);
        }

        public void ClearAgentVelocity()
        {
            if (agentSid >= 0)
                RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);
            checkPathYawTick = 0;
        }

        bool OnFromtaion()
        {
            foreach (var unit in moveUnits.Values)
            {
                if (!unit.IsOnFormation())
                    return false;
            }
            return true;
        }
        public void StopMove(StopCause cause,GroupState endState = GroupState.FreeStand)
        {
            SetGroupState(endState);
            groupPivot.reached = true;
            curTargetPos = position;
            moveGoal = MoveMgr.InvalidPos;

            if (cause == StopCause.ManualReach)
                manualMove = false;
            isYawed = false;
            yawEnd = true;

            targetPrey = null;
            checkPeryYawTick = 0;

            ClearAgentVelocity();

            foreach (var unit in moveUnits.Values)
            {
                if (cause != StopCause.FormationObstruct)
                {
                    unit.StopMove(StopCause.GroupReach, MoveState.MoveToLineUp);
                    unit.direction = direction;
                }
                else
                    unit.StopMove(StopCause.GroupReach, MoveState.WaitForPath);
            }
        }

        public int GetNearIndex(int index)
        {
            int ret = 0;
            foreach (var unit in moveUnits.Values)
                ret = unit.formationIdx;
            return ret;
        }

        public void SetPosition(TSVector2 position)
        {
            if (agentSid >= 0)
                RVO.Simulator.Instance.setAgentPosition(agentSid, position);
            groupPivot.position = position;
        }
    }
}
