namespace Nuthouse.Core.StateMachine
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Tick(float dt);
        void FixedTick(float fdt);
    }
}
/*
интерфейс для состояния
тик для обновлений через время после предыдущего кадра
фиксед тик для точного обновления через время 
(физика и всякие точности)
интер для активации состояния
экзит для выхода
*/