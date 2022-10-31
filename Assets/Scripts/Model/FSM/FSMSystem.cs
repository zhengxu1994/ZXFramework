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
                CurrentState = states[stateId];
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
