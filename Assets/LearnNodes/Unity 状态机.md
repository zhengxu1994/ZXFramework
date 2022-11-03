### Unity 状态机

#### 用于解决什么问题？

*将状态与逻辑分离，区分状态，划分行为逻辑，下层负责设计每个状态下对应的行为逻辑，上层只通过条件判断状态的切换*

- 状态之间的切换，只有可以相互跳转的状态之间才能切换。
- 为了防止状态之间的跳转过于复杂，可以设计让所有的状态（死亡除外）都只能跳转至待机状态，在通过待机状态跳转至对应的状态。
- 分层，当一个指令影响到多个类型对象的状态时就需要将他们分层处理，如（部队/士兵），下达指令让部队移动，部队通知士兵往哪里移动，士兵负责具体怎么移动过去（上层影响下层，但最终逻辑取决于下层逻辑）。
- 唯一性，一个对象同时只能有一个状态在运行
- 所有的状态都应该由状态机内部条件判断切换（死亡除外）



**状态的生命周期**

- Enter 进入这个状态
- Update 状态更新
- Edit 离开这个状态
- 添加/删除跳转状态

**状态机**

- 判断条件，如某个状态条件满足则切换状态
- 当前状态的更新逻辑
- 添加/删除状态

#### *状态之间的切换，只有可以相互跳转的状态之间才能切换，所以我们需要定义哪些状态之前是可以跳转的，数据结构使用键值对 符合一对多的情况并且可以快速查找，过渡TransId - 状态StateId*



```c#
using System;
namespace FSM
{
    /// <summary>
    /// 过渡条件
    /// </summary>
    public enum TransId
    {
        NullTransId,

        DecisionIdle,
        DecisionMove,
        DecisionAtk,
    }
    /// <summary>
    /// 状态id
    /// </summary>
    public enum StateId
    {
        NullStateId,
        DecisionIdle,
        DecisionMove,
        DecisionAtk,
    }
}

```

```c#
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

```

```c#
using System;
using System.Collections.Generic;
using Bepop.Core;
namespace FSM
{
    public class FSMSystem : Singleton<FSMSystem>
    {
        private Dictionary<StateId,FSMState> states;
        public FSMState CurrentState;
        public StateId CurrentStateId;

        public FSMSystem()
        {
            states = new Dictionary<StateId, FSMState>();
        }

        public virtual void AddState(FSMState state)
        {
            if(state == null)
            {
                Log.BASE.LogError("add state is null");
                return;
            }
            if(states.ContainsKey(state.id))
            {
                Log.BASE.LogError($"state is added:{state.id}");
                return;
            }
            if(states.Count == 0)
            {
                CurrentState = state;
                CurrentStateId = state.id;
            }
            states[state.id] = state;
        }

        public virtual void DeleteState(StateId id)
        {
            if(states.ContainsKey(id))
                states.Remove(id);
             Log.BASE.LogError($"state is null:{id}");
        }

        public virtual void PerformStateId(TransId trans)
        {
            if(trans == TransId.NullTransId)
            {
                Log.BASE.LogError($"trans is null");
                return;
            }
            var stateId = CurrentState.GetOutputState(trans);
            if(stateId == StateId.NullStateId)
            {
                Log.BASE.LogError($"state is null");
                return;
            }

            if(states.ContainsKey(stateId))
            {
                CurrentState.DoBeforeLeave();
                CurrentState = states[transId];
                CurrentStateId = CurrentState.id;
                CurrentState.DoBeforeEnter();
            }
        }

        public void Run()
        {
            if(CurrentState.Reason())
                CurrentState.Action();
        }

        public void Clear()
        {

        }
    }
}

```

