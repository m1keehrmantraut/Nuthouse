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
/*
стейт машина
sealed — запрещает наследование от PlayerStateMachine. Это финальная реализация, её не нужно расширять.

public IState Current { get; private set; } — свойство,
доступное для чтения всем, но изменять его можно только 
внутри этого класса (через ChangeState). 
Это защищает от случайной смены состояния извне.

? это проверка на null и если да то возвращает null вместо метода
StateChanged?.Invoke вызов события только если на него кто-то подписан

класс меняет состояние и проводит тики

*/