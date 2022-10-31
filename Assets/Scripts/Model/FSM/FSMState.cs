using System;
using System.Collections.Generic;
using Bepop.Core;

namespace FSM
{
    public abstract class FSMState
    {
        public StateId id;
        /// <summary>
        /// 所有的状态都写在一个枚举里面的 定义最小和最大防止加入错误的transid
        /// </summary>
        protected TransId minTrans, maxTrans;
        protected StateId minState, maxState;

        private bool IsTransIdValid(TransId trans)
        {
            return trans >= minTrans && trans <= maxTrans;
        }

        private bool IsStateIdValid(StateId stateId)
        {
            return stateId >= minState && stateId <= maxState;
        }

        /// <summary>
        /// 可以跳转的状态
        /// </summary>
        public Dictionary<TransId, StateId> map = new Dictionary<TransId, StateId>();

        public virtual void AddTrans(TransId transId,StateId stateId)
        {
            if(transId == TransId.NullTransId)
            {
                Log.BASE.LogError("add transId is null");
                return;
            }

            if(stateId == StateId.NullStateId)
            {
                Log.BASE.LogError("add stateId is null");
                return;
            }

            //超出状态
            if(!IsTransIdValid(transId))
            {
                Log.BASE.LogError("add transId is valid");
                return;
            }

            if (!IsStateIdValid(stateId))
            {
                Log.BASE.LogError("add stateId is valid");
                return;
            }

            if(map.ContainsKey(transId))
            {
                Log.BASE.LogError($" transId has be add:{transId}");
                return;
            }

            map[transId] = stateId;
        }

        public virtual void RemoveTransId(TransId transId)
        {
            if (transId == TransId.NullTransId)
            {
                Log.BASE.LogError("remove transId is null");
                return;
            }

            if (!IsTransIdValid(transId))
            {
                Log.BASE.LogError("remove transId is valid");
                return;
            }

            if(map.ContainsKey(transId))
            {
                map.Remove(transId);
                return;
            }
        }

        public StateId GetOutputState(TransId transId)
        {
            if(map.ContainsKey(transId))
            {
                return map[transId];
            }
            return StateId.NullStateId;
        }

        public virtual void DoBeforeEnter()
        {
        }

        public virtual void DoBeforeLeave()
        {
        }

        public virtual bool Reason() {
            return true;
        }
        public abstract void Action();

        public virtual void Dispose() { }


    }
}
