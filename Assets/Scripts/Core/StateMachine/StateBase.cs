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
/*
абстрактный класс состояний
который хранит контекст игрока (состояния которые есть у игрока
и прочую шнягу)
виртуальные методы чтобы дочерние состояния сами
определеля их если нужно
и шаблонный метод To, для перехода в новое состояние 
через стейт машину
*/