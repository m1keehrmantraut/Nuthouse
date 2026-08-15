using System;

namespace Nuthouse.Core.StateMachine
{
    public sealed class PlayerStateMachine
    {
        public IState Current { get; private set; }
        public event Action<IState, IState> StateChanged;

        public void ChangeState(IState next)
        {
            var prev = Current;
            Current?.Exit();
            Current = next;
            Current?.Enter();
            StateChanged?.Invoke(prev, next);
        }

        public void Tick(float dt) => Current?.Tick(dt);
        public void FixedTick(float fdt) => Current?.FixedTick(fdt);
    }
}
