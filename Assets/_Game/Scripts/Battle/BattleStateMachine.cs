using System;
using Game.Data;

namespace Game.Battle
{
    public class BattleStateMachine
    {
        public BattleState CurrentState { get; private set; } = BattleState.Intro;

        public event Action<BattleState> OnStateChanged;

        public void SetState(BattleState next)
        {
            CurrentState = next;
            OnStateChanged?.Invoke(next);
        }
    }
}
