using Bepop.Core;
using RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using TrueSync;
using UnityEngine;
using Random = System.Random;

namespace MyAi
{
    public enum GroupState
    {
        Stand,
        FormationMove,
        Attack,
        GroupReach,
        LineUp,
        WaitForFriend,
        CloseToTarget
    }
    public class MoveGroup  : UnitEntity
    {
        public int agentSid = -1;

        public int groupId = -1;
        private Random m_random = new Random();

        public Dictionary<int, MoveUnit> moveUnits;

        public FP NormalMoveSpeed = 0.2f;

        public GroupState state { get; private set; }

        public override FP Speed
        {
            get => base.Speed;
            set
            {
                base.Speed = value;
                if (agentSid > 0)
                    RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, value);
            }
        }

        public MoveGroup(int groupId,FP radius,TSVector2 postion,GameObject unitObj,GameObject groupArea)
        {
            Speed = NormalMoveSpeed;
            if (agentSid >= 0)
                RVO.Simulator.Instance.delAgent(agentSid);
            agentSid = RVO.Simulator.Instance.addAgent(postion);
            this.groupId = groupId;
            Simulator.Instance.setAgentPrefVelocity(agentSid, new TSVector2(0,0));
            this.obj = GameObject.Instantiate(groupArea);
            this.obj.transform.position = postion.ToVector();
            this.Position = postion;
            CreateUnit(9, unitObj);
        }

        private void CreateUnit(int unitNum,GameObject prefab)
        {
            moveUnits = new Dictionary<int, MoveUnit>();
            int uid = groupId * 100;
            for (int i = 0; i < unitNum; i++)
            {
                int unitUid = groupId > 0 ? uid + i : uid - i;
                var unit = new MoveUnit(unitUid , groupId >= 0 ? 1 : 0, this, prefab);
                moveUnits.Add(i, unit);
            }
        }

        private int waitingTick;

        static int[] perturbNums = { 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61 };
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
        public void UpdateStep()
        {
            if (targetPos == MoveManager.MoveInvaild)
            {
                Speed = 0;
                state = GroupState.Stand;
            }
            else if(agentSid >= 0 && moveEnable && state != GroupState.Attack)
            {
                if (Speed <= 0) Speed = NormalMoveSpeed;
                var playerPos = RVO.Simulator.Instance.getAgentPosition(agentSid);
                Position = playerPos;
                obj.transform.position = playerPos.ToVector();
      
                var goalVector = targetPos - playerPos;

                goalVector = RVO.RVOMath.normalize(goalVector) * 0.2f;
                /* Perturb a little to avoid deadlocks due to perfect symmetry. */
                /* 因为完美对称，所以需要加入些许抖动用来避免死锁
                 * 这里的完美对称，测试出是指两个完全一样的单位，不抖动=中心点一样=无法把其他排斥出去
                 */
                FP angle = perturb * 2.0f * TSMath.Pi;
                FP dist = perturb * 0.0001f;
                goalVector += dist * new TSVector2(TSMath.Cos(angle), TSMath.Sin(angle));
                //rvo前进
                RVO.Simulator.Instance.setAgentPrefVelocity(agentSid, goalVector); ;
            }

            if(moveUnits!= null && moveUnits.Count > 0)
            {
                moveUnits.ForEach((index, unit) => {
                    unit.UpdateStep();
                    unit.moveParty.UpdateStep();
                });
            }
        }

        private FP closeToTargetRadiusSquared = 16f;
       
        private FP closeToTargetSpeed = 0.1f;

        private FP freeAttackRadiusSquared = 2f;
        public void UpdateState()
        {
            //更新group状态
            //var dis = TSVector2.DistanceSquared(Position, targetPos);
            //if(targetGroup == null)
            //{
            //    //无目标的情况下 无需减速处理
            //    if(dis <= 0.05f)
            //    {
            //        //到达目的地
            //        state = GroupState.Stand;
            //        if (Speed != 0)
            //        {
            //            Speed = 0;
            //            isMoving = false;
            //        }

            //        if (moveUnits != null && moveUnits.Count > 0)
            //        {
            //            moveUnits.ForEach((index, unit) => {
            //                unit.StopMove(StopCause.GroupReached, MoveState.Stand);
            //            });
            //        }
            //    }
            //    else
            //    {
            //        state = GroupState.FormationMove;
            //        if (Speed != NormalMoveSpeed)
            //        {
            //            Speed = NormalMoveSpeed;
            //            isMoving = true;
            //        }
            //    }
            //}
            //else
            //{
            //    if (dis > closeToTargetRadiusSquared)
            //    {
            //        state = GroupState.FormationMove;
            //        if (Speed != NormalMoveSpeed)
            //            Speed = NormalMoveSpeed;
            //    }
            //    else if (dis <= closeToTargetRadiusSquared && dis > freeAttackRadiusSquared)
            //    {
            //        state = GroupState.CloseToTarget;
            //        if (Speed != closeToTargetSpeed)
            //            Speed = closeToTargetSpeed;
            //    }
            //    else if (dis <= freeAttackRadiusSquared)
            //    {
            //        //到达目的地
            //        state = GroupState.Attack;
            //        if (Speed != 0)
            //        {
            //            Speed = 0;
            //            isMoving = false;
            //        }

            //        moveUnits.ForEach((index, unit) => {
            //            unit.StopMove(StopCause.GroupAttack, MoveState.CloseToTarget);
            //        });
            //    }
            //}
            

            if (moveUnits != null && moveUnits.Count > 0)
            {
                moveUnits.ForEach((index, unit) =>
                {
                    unit.UpdateState();
                });
            }
        }

        public TSVector2 targetPos = MoveManager.MoveInvaild;
        public TSVector2 goal;
        public MoveGroup targetGroup = null;
        public GroupState groupState;

        private void CheckWaitForFriend()
        {
            //判断是否因为其他队伍阻挡 进入等待
            if (RVO.Simulator.Instance.getAgentMaxSpeed(agentSid) > 0)
            {
                var velocity = RVO.Simulator.Instance.getAgentVelocity(agentSid);
                if (RVO.RVOMath.absSq(velocity) < 1f)
                {
                    waitingTick = 30;
                    RVO.Simulator.Instance.setAgentMaxSpeed(agentSid, 0);

                    foreach (var unit in moveUnits.Values)
                        unit.StopMove(StopCause.WaitForFriend, MoveState.Stand);
                    RVO.Simulator.Instance.setAgentPosition(agentSid, Position);
                    return;
                }
            }
        }
        public void MoveToPosition(TSVector2 targetPos,bool isManual)
        {
            this.targetPos = targetPos;
            state = GroupState.FormationMove;
            isMoving = true;
            Log.BASE.LogInfo("MoveToPosition" + targetPos);
            if(moveUnits != null && moveUnits.Count > 0)
            {
                moveUnits.ForEach((index, unit) => {
                    unit.MoveToPosition(targetPos,isManual);
                });
            }
        }

        public void AttachTarget(MoveGroup group,bool isManual)
        { 
            if(group != null)
            {
                targetGroup = group;
                targetPos = group.Position;
                state = GroupState.FormationMove;
                isMoving = true;
                if (moveUnits != null && moveUnits.Count > 0)
                {
                    moveUnits.ForEach((index, unit) => {
                        unit.MoveToPosition(targetPos, isManual);
                    });
                }
            }
        }

    }
}