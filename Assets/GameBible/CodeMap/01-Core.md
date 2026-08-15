# 01 — Core (ядро)

Папка `Assets/Scripts/Core/`. Базовые механизмы, на которых держится всё остальное.

## StateMachine/ — стейт-машина игрока
- `IState.cs` `[новый]` — интерфейс состояния: `Enter/Exit/Tick/FixedTick`. Любой стейт игрока его реализует.
- `StateBase.cs` `[новый]` — базовый класс стейта. Хранит ссылку на контекст игрока (`Ctx`) и даёт метод `To<T>()` для перехода в другой стейт.
- `PlayerStateMachine.cs` `[новый]` — сама машина: хранит текущий стейт, переключает его, зовёт Tick/FixedTick, кидает событие `StateChanged`.
- `StateRegistry.cs` `[новый]` — словарь «тип стейта → его инстанс». Стейты один раз регистрируются и берутся по типу, без пересоздания.

## Events/ — каналы событий
Единой шины нет: маленькие статические классы, по одному на тему. Геймплей публикует, презентация слушает.
- `MovementEvents.cs` `[новый]` — Jumped, Landed, RunChanged, CrouchChanged (прыжок, приземление, бег, присед).
- `CombatEvents.cs` `[новый]` — AttackStarted, AttackHit, ThrowStarted, ProjectileThrown, AxeStuck, AxePickedUp, AxeCountChanged (атаки, бросок топора, боезапас).
- `DamageEvents.cs` `[новый]` — DamageTaken, Died (получен урон, умерли).
- `GameEvents.cs` `[новый]` — PauseRequested, PauseChanged, CheckpointReached (пауза, чекпоинт).

## ServiceLocator/ — реестр сервисов приложения
- `ServiceLocator.cs` `[новый]` — статический словарь сервисов. `Register` — добавить (только GameBootstrap), `Get` — взять. Для приложения (сцены/прогресс), НЕ для геймплея.
- `Singleton.cs` / `PersistentSingleton.cs` `[новый]` — базовые классы для глобальных объектов в единственном экземпляре (PersistentSingleton сохраняется между сценами).
- `GameBootstrap.cs` `[новый]` — точка старта: в Awake регистрирует сервисы (Scene, Progress) в ServiceLocator. Должен лежать в каждой сцене. NOTE: пока ни в одной сцене не размещён — см. `08-Починка-в-редакторе.md`.