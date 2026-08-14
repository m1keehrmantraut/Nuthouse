01-Architecture — общая архитектура
Версия: 3.1 · Статус: базовая линия. Остальные документы не могут противоречить этому.
Изменения 3.1: добавлено поле Interact в PlayerContext, уточнён состав PlayerContext,
исправлены противоречия сигнатур.

1. Цели
Масштабируемость: новая механика добавляется новым кодом, а не переписыванием старого.

Минимальная связанность: системы общаются через контекст, интерфейсы и раздельные события.

Без оверинжиниринга: абстракция появляется только там, где уже есть 2+ реальных потребителя.

2. Слои
text
Presentation   (анимация, звук, камера, VFX, UI)
      ▲ читает события, НИКОГДА не управляет геймплеем
Gameplay       (StateMachine, Combat, Health, Interact)
      ▲
Services       (ServiceLocator: Scene, Progress, позже Time/Audio/Save — 08-ServiceLocator)
      ▲ инфраструктурные сервисы приложения, не геймплей
Infrastructure (Input System, Rigidbody2D, Cinemachine, сцены, сохранения)
Правило направления: зависимости идут вниз (Gameplay знает Infrastructure,
Presentation читает Gameplay через события). Обратные ссылки запрещены.

Сервисный слой (Services) стоит рядом с геймплеем, а не под ним как зависимость
геймплейных систем: ядро игрока (StateMachine/Motor/Combat/States) сервисы не читает
(зависимости — через PlayerContext). Сервисы читают меню, UI, World, презентация и
PlayerFacade (только для сервисов приложения). Подробности — 08-ServiceLocator.md §6.

3. Состав игрока (композиция)
Игрок собирается на одном GameObject-префабе. Композиционный корень — PlayerFacade.

text
Player (prefab)
└── PlayerFacade            — собирает и инициализирует всё в Awake
    ├── PlayerContext       — контейнер ссылок (без логики)
    ├── InputReader         — чтение устройств → команды
    ├── InputBuffer         — очередь намерений с TTL
    ├── PlayerStateMachine  — иерархическая FSM (02-StateMachine.md)
    ├── CharacterMotor      — единственный владелец Rigidbody2D (04-Movement.md)
    ├── GroundDetector      — земля/стены/потолок, coyote time
    ├── FacingController    — направление взгляда (flip спрайта)
    ├── PlayerCombat        — исполнение атак (05-Combat.md)
    ├── Health              — HP, получение урона
    ├── Stamina             — стамина, реген
    ├── InteractController  — подбор/взаимодействие по буферу Interact (05 §7.5)
    └── PlayerAnimatorDriver— единственный, кто трогает Animator (06)
Внешний мир (диалоги, катсцены, сохранения) общается с игроком только через публичный API
PlayerFacade (SetControlEnabled(bool), GetHealth(), ReturnAxe(int) и т.п.), не через внутренние компоненты.

PlayerContext
Простой класс-контейнер, создаётся PlayerFacade и передаётся во все подсистемы:

csharp
public sealed class PlayerContext
{
    public PlayerFacade Facade;
    public CharacterMotor Motor;
    public GroundDetector Ground;
    public FacingController Facing;
    public InputReader Input;
    public InputBuffer Buffer;
    public PlayerStateMachine StateMachine;
    public StateRegistry States;            // реестр инстансов стейтов (02 §7)
    public PlayerCombat Combat;
    public Health Health;
    public Stamina Stamina;
    public InteractController Interact;     // подсистема интеракций (05 §7.5)
    public PlayerAnimatorDriver Animator;   // MonoBehaviour на префабе, facade делает GetComponent + Init(ctx)
    public Transform HitboxRoot;            // HitboxHolder из префаба (05 §5)
    public MovementConfig MovementConfig;

    // Односторонние запросы, которые выставляет Health и потребляет фасад/стейты:
    public HurtRequest? PendingHurt;        // урон с откидыванием → HurtState (02 §6, 05 §2)

    // Срез управления (01 §8): фасад пишет, стейты читают перед чтением буфера.
    public bool ControlEnabled = true;
}
HurtRequest — маленький readonly-struct в Nuthouse.Combat:

csharp
public readonly struct HurtRequest
{
    public readonly DamageInfo Info;
    public readonly Vector2 Direction;   // от источника к игроку
    public readonly float Force;         // сила откидывания для Motor.Knockback
    public HurtRequest(DamageInfo info, Vector2 direction, float force)
    { Info = info; Direction = direction; Force = force; }
}
Контекст — только предоставление зависимостей. Он не вызывает методы систем и не содержит логики.
FindFirstObjectByType в геймплее запрещён. Singleton и ServiceLocator — только для
системных сервисов приложения (08-ServiceLocator.md), в геймплейных системах запрещены.

4. Порядок обновления (фиксированный)
text
InputReader (события Unity Input System)
    ↓
StateMachine.Tick(deltaTime)          — Update: решения, таймеры, буфер
    ↓
StateMachine.FixedTick(fixedDelta)    — FixedUpdate: физика через Motor
    ↓
Health/Stamina.Tick                   — реген, таймеры неуязвимости
    ↓
Presentation                          — AnimatorDriver, камера, звук (LateUpdate/события)
PlayerFacade вызывает Tick/FixedTick подсистем в этом порядке и только он.
Подсистемы не обновляют друг друга.

5. Правила зависимостей (обязательны)
Система	Может знать	Запрещено знать
InputReader	Input Actions asset	Combat, Movement, Animator
InputBuffer	только команды и время	всё игровое
StateMachine	Context, конфиги	Animator, Audio, Camera, ServiceLocator
CharacterMotor	Rigidbody2D, конфиг	Combat, Animator, Input, Ground
PlayerCombat	Context, AttackDefinition, Hitbox	Animator напрямую, Input напрямую
Health/Stamina	конфиг, DamageInfo, Context (только PendingHurt)	Combat, Weapon, Animator
AnimatorDriver	Animator, события, Context (read-only)	логику боя/движения
Weapon/Projectile	свой конфиг, DamageInfo	Player, Input
GameBootstrap	все сервисы	геймплейные объекты
Сервис (ServiceLocator)	свой конфиг, другие сервисы, события (как слушатель)	Player, Enemy, Combat, Motor
Меню/UI/Presentation	ServiceLocator, события	прямые вызовы геймплейных систем
Ключевые запреты:

Combat не двигает персонажа сам — запрашивает у Motor (через стейт).

Movement не наносит урон и не знает про Combat.

Input никогда не вызывает геймплей напрямую — только кладёт команды в Buffer.

Ни одна система не вызывает Animator.* кроме PlayerAnimatorDriver.

ScriptableObject хранит только конфигурацию, никогда runtime-состояние
(HP, таймеры, текущее комбо — только в рантайм-классах).

6. События: раздельные каналы вместо единой шины
Единого EventBus нет. Есть маленькие статические классы-каналы, по одному на домен
(подробности и список событий — 06-EventsAndPresentation.md):

text
MovementEvents   — Jumped, Landed, RunChanged, CrouchChanged
CombatEvents     — AttackStarted, AttackHit, ThrowStarted, ProjectileThrown, AxeStuck, ...
DamageEvents     — DamageTaken, Died, ...
GameEvents       — PauseRequested, PauseChanged, CheckpointReached, ...
Правила:

Геймплей публикует события; Presentation подписывается. Обратное запрещено.

Запрещено строить игровую логику на событиях («начать атаку по событию X»). События —
только уведомление факта. Управление идёт через StateMachine/Motor напрямую.

Подписка — в OnEnable, отписка — в OnDisable. Статические подписки без отписки = баг.

7. Data Driven
Поведение настраивается ScriptableObject-ассетами в Assets/Data/:

SO	Что хранит
MovementConfig	скорости, ускорения, гравитация, coyote, буфер прыжка
InputConfig	окна буферизации действий (мс)
AttackDefinition	фазы атаки, урон, хитбокс, отдача, аним-триггер
AxeThrowDefinition	сила броска, вращение, поведение при попадании
HealthConfig	макс. HP, время неуязвимости после удара
StaminaConfig	макс., расход, реген, порог восстановления
Баланс меняется в инспекторе без правки кода.

8. Слои поведения вместо одного «Player State»
Одновременно существуют независимые срезы:

text
Control:    Enabled / Disabled (диалог, катсцена, пауза)
Locomotion: Idle / Run / Crouch / Jump / Fall  ← StateMachine
Combat:     None / Attacking / Throwing        ← CombatState внутри FSM
Life:       Alive / Dead                       ← флаг Health
Отключение управления (Control) не ломает FSM: PlayerFacade каждый кадр пишет
ctx.ControlEnabled, StateMachine.Tick выполняется всегда (переходы по земле/смерти,
таймеры), но стейты читают Input Buffer только при ControlEnabled == true. Физика
(FixedTick) не останавливается никогда. Это нужно для диалогов, катсцен, паузы.

9. Законы проекта
Любое действие игрока исполняется стейтом или системой, но инициируется только из Input Buffer.

Rigidbody2D трогает только CharacterMotor.

Animator трогает только PlayerAnimatorDriver.

Input не вызывает геймплей напрямую.

Combat не знает Animator; Movement не знает Combat.

Оружие/снаряд не знает Player (только DamageInfo и конфиг).

SO — только данные.

Новая механика расширяет архитектуру (новый стейт/класс/конфиг), а не переписывает ядро.

Отложенные системы (FutureComponents/) не реализуются и не получают заготовок заранее.

ServiceLocator — только для сервисов приложения (Scene, Progress, позже Time/Audio);
регистрирует их единственный GameBootstrap. Ядро геймплея ServiceLocator не читает.

Singleton (Singleton<T>/PersistentSingleton<T>) — только для системных объектов
(GameBootstrap, системные префабы). Игрок, враги, стейты — не синглтоны.

10. Критерий корректности архитектуры
Добавление нового движения/атаки/врага не требует правки мотора, буфера, других стейтов
или сервисов: только новый класс стейта/конфиг + одна строка регистрации в
PlayerFacade.Awake() (реестр стейтов) и, при необходимости, условие входа в таблице
переходов существующего стейта.

Presentation можно целиком отключить — геймплей продолжит работать.

Нет циклических зависимостей: граф зависимостей игрока — дерево с корнем в PlayerFacade;
сервисы приложения — отдельный плоский реестр (ServiceLocator), не связанный с этим деревом.

