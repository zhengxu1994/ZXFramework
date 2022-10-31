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
