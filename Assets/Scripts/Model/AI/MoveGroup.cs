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
        LineUp
    }
    public class MoveGroup  : UnitEntity
    {
        public int agentSid = -1;

        public int groupId = -1;
        private Random m_random = new Random();

        public Dictionary<int, MoveUnit> moveUnits;

        public FP NormalMoveSpeed = 0.2f;

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
            groupId = this.groupId;
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
                int unitUid = uid + i;
                var unit = new MoveUnit(unitUid, groupId >= 0 ? 1 : 0, this, prefab);
                moveUnits.Add(i, unit);
            }
        }
        public void AttachTarget(MoveGroup group)
        {
            if(group != null)
            {
                UpdateStep();
            }
        }

        public void UpdateStep()
        {
            if (targetPos == MoveManager.MoveInvaild)
            {
                Speed = 0;
            }
            else if(agentSid >= 0 && moveEnable)
            {
                if (Speed <= 0) Speed = NormalMoveSpeed;
                var playerPos = RVO.Simulator.Instance.getAgentPosition(agentSid);
                var playerVel = RVO.Simulator.Instance.getAgentPrefVelocity(agentSid);
                Position = playerPos;
                obj.transform.position = playerPos.ToVector();
                   
                
                var goalVector = targetPos - RVO.Simulator.Instance.getAgentPosition(agentSid);

                if(RVOMath.absSq(goalVector) > 1.0f)
                {
                    goalVector = RVOMath.normalize(goalVector);
                }
                //移动方向
                Simulator.Instance.setAgentPrefVelocity(agentSid, goalVector);

                /* Perturb a little to avoid deadlocks due to perfect symmetry. */
                /* 因为完美对称，所以需要加入些许抖动用来避免死锁
                 * 这里的完美对称，测试出是指两个完全一样的单位，不抖动=中心点一样=无法把其他排斥出去
                 */
                float angle = (float)m_random.NextDouble() * 2.0f * (float)Math.PI;
                float dist = (float)m_random.NextDouble() * 0.0001f;

                Simulator.Instance.setAgentPrefVelocity(agentSid, Simulator.Instance.getAgentPrefVelocity(agentSid) +
                                                    dist *
                                                    new TSVector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
            }

            if(moveUnits!= null && moveUnits.Count > 0)
            {
                moveUnits.ForEach((index, unit) => {
                    unit.UpdateStep();
                });
            }
        }

        public TSVector2 targetPos = MoveManager.MoveInvaild;
        public TSVector2 goal;

        public GroupState groupState;
        public void MoveToPosition(TSVector2 targetPos,bool isManual)
        {
            this.targetPos = targetPos;
            Log.BASE.LogInfo("MoveToPosition" + targetPos);
            if(moveUnits != null && moveUnits.Count > 0)
            {
                moveUnits.ForEach((index, unit) => {
                    unit.MoveToPosition(targetPos,isManual);
                });
            }
        }
    }
}