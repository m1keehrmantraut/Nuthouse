using Nuthouse.Player;

namespace Nuthouse.Core.StateMachine
{
    public abstract class StateBase : IState
    {
        protected readonly PlayerContext Ctx;

        protected StateBase(PlayerContext ctx) => Ctx = ctx;

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick(float dt) { }
        public virtual void FixedTick(float fdt) { }

        protected void To<T>() where T : IState => Ctx.StateMachine.ChangeState(Ctx.States.Get<T>());
    }
}
