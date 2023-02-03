using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using TrueSync;

namespace Movement
{
    public enum StopCause
    {
        ReachedGoal,
        ManualReach,
        GroupReach,
        FormationReach,
        FormationObstruct,
        ChangeTarget,
        HasNearlyEnemy,
        EnemySpotted,
        EnemyDispose,
        AttackEnemy,
        WaitForFriend,
        TargetPeryYawed,
        YawedFromPath,
        CannotReach,
        Displacement,
    }
    public abstract class MoveUniform<T>
    {
        protected HashSet<int> sights = new HashSet<int>();

        public int numOfSight { get => sights.Count; }

        public void AddSight(int uid)
         => sights.Add(uid);

        public void SubSight(int uid)
            => sights.Remove(uid);

        public virtual T targetPrey { get; protected set; }

        public abstract void Dispose();

        public abstract Action<StopCause,T> moveStopCB { get; set; }

        public TSVector2 curTargetPos { get; set; } = MoveMgr.InvalidPos;

        public bool moveEnable { get; set; } = true;

        public abstract FP moveSpeed { get; set; }

        public abstract int direction { get; set; }

        public bool isMoving { get; protected set; } = false;

        public abstract TSVector2 position { get; set; }

        public abstract void DisplacementTo(TSVector2 target, FP speed);

        public abstract void LogicUpdate();

        public abstract void UpdateStep();

        public abstract void TargetDispose(bool targetDead);

        public TSVector2 CrossesPoint(TSVector2 a ,TSVector2 b,TSVector2 p)
        {
            var r = p - a;
            var k = b - a;
            var z = (r.x * k.x + r.y * k.y) / (k.x * k.x + k.y * k.y);
            var q = new TSVector2(a.x + z * k.x, a.y + z * k.y);
            return q;
        }
    }
}
