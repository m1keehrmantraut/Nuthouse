02-StateMachine — иерархическая стейт-машина игрока
Версия: 3.1 · Зависит от: 01-Architecture
Приоритет: ЭТО ЯДРО. Реализуется в первую очередь.
Изменения 3.1: исправлены порядок инициализации, добавлены привязка Hurtbox, groundCheck,
interactableLayer; явный ReturnAxe.

1. Назначение
StateMachine управляет поведением игрока: что он делает прямо сейчас, какие переходы допустимы,
когда читать Input Buffer. Это не «божественная FSM на всё» и не Ability Scheduler:
машина отвечает за режимы поведения, а исполнение физики делегирует CharacterMotor,
исполнение атак — PlayerCombat.

Что НЕ является стейтом: глобальные режимы игры (пауза, диалог, катсцена). Они управляются
флагом Control (PlayerFacade.SetControlEnabled) и событиями GameEvents, FSM при этом
продолжает тикать физику, но игнорирует ввод.

2. Иерархия стейтов
text
PlayerStateMachine
├── GroundedState            (родительский: общая логика «на земле»)
│   ├── IdleState
│   ├── RunState
│   └── CrouchState
├── AirborneState            (родительский: общая логика «в воздухе»)
│   ├── JumpState            (пока verticalVelocity > 0)
│   └── FallState            (пока verticalVelocity <= 0)
├── AttackState              (ближняя атака, исполняет AttackRunner)
├── AxeThrowState            (бросок топора, см. 05-Combat.md §7)
├── HurtState                (получение урона, откидывание)
└── DeadState
Родительский стейт — класс с общей логикой (например GroundedState проверяет, что земля
исчезла → переход в FallState; AirborneState проверяет приземление → GroundedState).
Дочерние стейты наследуются и дополняют. Это убирает дублирование и взрыв переходов.

3. Базовые классы (полная реализация)
csharp
// Nuthouse.Core.StateMachine
public interface IState
{
    void Enter();
    void Exit();
    void Tick(float dt);         // Update: решения, чтение буфера, таймеры
    void FixedTick(float fdt);   // FixedUpdate: физика через Motor
}

public abstract class StateBase : IState
{
    protected readonly PlayerContext Ctx;
    protected StateBase(PlayerContext ctx) => Ctx = ctx;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Tick(float dt) { }
    public virtual void FixedTick(float fdt) { }

    /// Переход через реестр; единственная точка смены стейта.
    protected void To<T>() where T : IState => Ctx.StateMachine.ChangeState(Ctx.States.Get<T>());
}

public sealed class PlayerStateMachine
{
    public IState Current { get; private set; }
    public event System.Action<IState, IState> StateChanged; // (old, new) — для AnimatorDriver/звука

    public void ChangeState(IState next)
    {
        var prev = Current;
        Current?.Exit();
        Current = next;
        Current.Enter();
        StateChanged?.Invoke(prev, next);
    }

    public void Tick(float dt) => Current?.Tick(dt);
    public void FixedTick(float fdt) => Current?.FixedTick(fdt);
}
Правила переходов:

Переход только через StateMachine.ChangeState. Стейт не вызывает Enter/Exit другого стейта.

Решение о переходе принимается в Tick по данным контекста (Ground, Buffer, Health) —
никаких событий для переходов.

Exit обязан сбрасывать всё, что стейт включал (таймеры, хитбоксы, модификаторы мотора).

4. PlayerContext и сборка
PlayerFacade.Awake() создаёт объекты в таком порядке (полный код — ниже, это канон):

csharp
// Nuthouse.Player
public sealed class PlayerFacade : MonoBehaviour
{
    [Header("Конфиги (SO)")]
    [SerializeField] private MovementConfig movementConfig;
    [SerializeField] private HealthConfig healthConfig;
    [SerializeField] private StaminaConfig staminaConfig;
    [SerializeField] private InputConfig inputConfig;

    [Header("Бой (SO)")]
    [SerializeField] private AttackDefinition primaryAttack;   // первая атака цепочки
    [SerializeField] private AxeThrowDefinition axeThrow;

    [Header("Слои/точки префаба")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask interactableLayer;      // слой Interactable
    [SerializeField] private Transform groundCheck;            // точка ног (GroundCheck)
    [SerializeField] private Transform hitboxRoot;             // HitboxHolder
    [SerializeField] private Hurtbox hurtbox;                  // Hurtbox игрока (для Bind)

    private PlayerContext ctx;
    private bool controlEnabled = true;

    private void Awake()
    {
        ctx = new PlayerContext { Facade = this, MovementConfig = movementConfig };

        // Сначала чистая логика
        ctx.Ground   = new GroundDetector(groundCheck, movementConfig, groundLayer);
        ctx.Facing   = new FacingController(transform);
        ctx.Motor    = new CharacterMotor(GetComponent<Rigidbody2D>(), movementConfig);
        ctx.Input    = new InputReader(inputConfig);   // inputConfig — ради heavyAttackHold (03 §5)
        ctx.Buffer   = new InputBuffer(inputConfig);
        ctx.Health   = new Health(healthConfig, ctx);          // ctx нужен для PendingHurt
        ctx.Stamina  = new Stamina(staminaConfig);
        ctx.Combat   = new PlayerCombat(ctx, primaryAttack, axeThrow);
        ctx.Interact = new InteractController(ctx, interactableLayer);
        ctx.HitboxRoot = hitboxRoot;

        // Привязываем Hurtbox к Health (05 §5)
        if (hurtbox != null) hurtbox.Bind(ctx.Health);

        // Реестр стейтов и машина (до Animator.Init!)
        ctx.States = new StateRegistry();
        RegisterStates();
        ctx.StateMachine = new PlayerStateMachine();

        // AnimatorDriver — MonoBehaviour на префабе; GetComponent + Init.
        ctx.Animator = GetComponent<PlayerAnimatorDriver>();
        ctx.Animator.Init(ctx);   // теперь ctx.StateMachine не null

        // Ввод → буфер (с фильтрами, см. 03 §7).
        ctx.Input.ActionPressed += OnActionPressed;

        ctx.StateMachine.ChangeState(ctx.States.Get<IdleState>());
    }

    private void RegisterStates()
    {
        ctx.States.Register(new IdleState(ctx));
        ctx.States.Register(new RunState(ctx));
        ctx.States.Register(new CrouchState(ctx));
        ctx.States.Register(new JumpState(ctx));
        ctx.States.Register(new FallState(ctx));
        ctx.States.Register(new AttackState(ctx));
        ctx.States.Register(new AxeThrowState(ctx));
        ctx.States.Register(new HurtState(ctx));
        ctx.States.Register(new DeadState(ctx));
    }

    private void OnActionPressed(InputAction action)
    {
        if (action == InputAction.Pause)          // пауза не буферизуется (03 §7)
        {
            GameEvents.PublishPauseRequested();
            return;
        }
        if (!controlEnabled) return;
        if (action == InputAction.Throw && !ctx.Combat.HasAxe) return; // недоступно
        ctx.Buffer.Add(action);
    }

    private void Update()
    {
        ctx.ControlEnabled = controlEnabled;      // стейты читают перед чтением буфера
        ctx.Buffer.Tick();                      // экспирация команд

        // Единственный «внешний» переход: откидывающий урон (см. §6).
        if (ctx.PendingHurt.HasValue)
        {
            ctx.StateMachine.ChangeState(ctx.States.Get<HurtState>());
        }
        else
        {
            ctx.StateMachine.Tick(Time.deltaTime); // всегда: физика/таймеры не стопорятся
        }

        ctx.Interact.Tick();
        ctx.Stamina.Tick(Time.deltaTime);
        ctx.Health.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        ctx.Ground.UpdateGround();
        ctx.StateMachine.FixedTick(Time.fixedDeltaTime);
    }

    public void SetControlEnabled(bool enabled) // диалоги, катсцены
    {
        controlEnabled = enabled;
        if (!enabled) ctx.Buffer.Clear();
    }

    public void ReturnAxe(int count) => ctx.Combat.ReturnAxe(count);   // внешний мир → API фасада

    private void OnDestroy()
    {
        ctx.Input.ActionPressed -= OnActionPressed;
        ctx.Input.Dispose();
    }
}
Подсистемы могут быть и MonoBehaviour (если нужен inspector/префаб), но их игровая логика
не должна полагаться на порядок MonoBehaviour.Update — тикает их только PlayerFacade.
Исключение — PlayerAnimatorDriver: это presentation, живёт на префабе, подписывается на
события в OnEnable/Init и сам читает контекст в LateUpdate (06 §2).

Сервисы приложения (Scene, Progress, позже Time/Audio) фасад при необходимости берёт из
ServiceLocator (08-ServiceLocator.md), но ядро игрока (стейты/мотор/комбат) — нет.

5. Логика родительских стейтов
csharp
public abstract class GroundedState : StateBase
{
    protected GroundedState(PlayerContext ctx) : base(ctx) { }

    public override void Enter() => Ctx.Motor.ResetVerticalVelocity();

    public override void Tick(float dt)
    {
        if (!Ctx.Ground.IsGrounded)                       { To<FallState>(); return; }
        if (Ctx.Health.IsDead)                            { To<DeadState>(); return; }

        // При выключенном управлении (диалог/катсцена) буфер не читаем,
        // но физика/переходы по земле продолжают работать (01 §8).
        if (!Ctx.ControlEnabled) return;

        if (Ctx.Ground.CoyoteTimeLeft > 0f
            && Ctx.Buffer.TryConsume(InputAction.Jump))   { To<JumpState>(); return; }
        if (Ctx.Buffer.TryConsume(InputAction.Attack))    { To<AttackState>(); return; }
        if (Ctx.Combat.HasAxe
            && Ctx.Buffer.TryConsume(InputAction.Throw))  { To<AxeThrowState>(); return; }
    }

    public override void FixedTick(float fdt)
    {
        Ctx.Motor.ApplyGroundFrictionIfNoInput(Ctx.Input.MoveAxis.x, fdt);
        Ctx.Facing.UpdateByInput(Ctx.Input.MoveAxis.x);
    }
}
(To<T>() унаследован от StateBase, §3.)

AirborneState аналогично: проверяет Ctx.Ground.IsGrounded → переход в IdleState
(или CrouchState при зажатом Crouch). Буферизованный прыжок в воздухе недоступен (пока нет
double jump). Атаки в воздухе сейчас запрещены (атака только на земле; см. таблицу ниже).
JumpState.Enter вызывает Ctx.Ground.ConsumeCoyote() и Ctx.Motor.Jump(...); jump-cut
по отпусканию кнопки — Ctx.Motor.CutJump() (03 §5 даёт JumpHeld, 04 §3).

6. Таблица переходов (каноническая)
Из стейта	Условие	В стейт
Idle	MoveAxis.x != 0	Run
Idle	зажат Crouch	Crouch
Run	MoveAxis.x == 0	Idle
Run	зажат Crouch (опц.)	Crouch
Crouch	отпущен Crouch	Idle
любой Grounded	нет земли	Fall
любой Grounded	буфер Jump + есть coyote	Jump
любой Grounded	буфер Attack	Attack
любой Grounded	буфер Throw + есть топор	AxeThrow
Jump	velocity.y <= 0	Fall
Jump	отпущен Jump	Fall (с обрезкой скорости)
Fall	есть земля	Idle / Crouch
Attack	фаза Recovery завершена	Idle
Attack	буфер Attack в cancel-окне	Attack (след. стадия комбо)
AxeThrow	Recovery завершена	Idle
Hurt	таймер stagger истёк	Idle / Fall
любой	Health.IsDead	Dead
любой	внешний откидывающий урон	Hurt
Hurt прерывает любой стейт: Health.TakeDamage выставляет запрос (Ctx.PendingHurt —
HurtRequest с силой и направлением, см. 01 §3), PlayerFacade.Update проверяет его
до тика текущего стейта и переводит в HurtState. HurtState.Enter читает и обнуляет
Ctx.PendingHurt, применяя Ctx.Motor.Knockback(...). Это единственный «внешний» переход,
инициируемый не из текущего стейта.

7. Реестр стейтов
Чтобы не создавать стейты каждый раз, PlayerContext хранит словарь инстансов:

csharp
public sealed class StateRegistry   // часть PlayerContext (поле States)
{
    private readonly Dictionary<System.Type, IState> states = new();
    public void Register(IState state) => states[state.GetType()] = state;
    public T Get<T>() where T : IState => (T)states[typeof(T)];
}
Регистрация всех стейтов — в PlayerFacade.RegisterStates() (вызывается из Awake).
Стейты stateless между вызовами (всё изменяемое состояние — в контексте/таймерах стейта,
сбрасывается в Enter).

8. Взаимодействие с Motor
Стейты двигают персонажа только через CharacterMotor (API — в 04-Movement.md):

csharp
// RunState.FixedTick — ускорение берётся по месту: на земле/в воздухе разные
float accel = Ctx.Ground.IsGrounded ? Ctx.MovementConfig.accel : Ctx.MovementConfig.airAccel;
Ctx.Motor.MoveHorizontal(Ctx.Input.MoveAxis.x, targetSpeed, accel, fdt);

// JumpState.Enter — прыжок потребляет coyote-окно
Ctx.Ground.ConsumeCoyote();
Ctx.Motor.Jump(Ctx.MovementConfig.jumpForce);
Запрещено: Rigidbody2D напрямую из стейта, transform.Translate для игрового движения.
Команда Jump уже потреблена Buffer.TryConsume в родительском стейте до перехода —
повторно «чистить» буфер не нужно.

9. Анимация и стейты
StateMachine не знает Animator. PlayerAnimatorDriver подписывается на
StateMachine.StateChanged и на события (MovementEvents, CombatEvents) и выставляет
параметры (таблица параметров — 06-EventsAndPresentation.md §2).

10. Расширение (добавление нового стейта)
Создать класс XState : StateBase (или наследник родителя) в Scripts/Player/States/.

Реализовать Enter/Exit/Tick/FixedTick и переходы по таблице.

Зарегистрировать в PlayerFacade.RegisterStates() (ctx.States.Register(new XState(ctx))).

Добавить условие входа в таблицу переходов существующих стейтов (обычно чтение буфера).

Добавить параметр/переход в Animator через AnimatorDriver.

Ядро (StateMachine, PlayerContext, мотор) при этом не меняется. Пошагово —
ForDeveloper/01-HowToAdd.md §2.

11. Антипаттерны
Одна плоская FSM со всеми состояниями (Idle, Run, Jump, Attack, Hurt, Dead...) в одном
switch — использовать иерархию.

Переходы по событиям/строковым триггерам.

Хранение в стейте ссылок на Animator/UI/камеру.

Стейт сам двигает Rigidbody.

Переход в Attack из Input напрямую, минуя Buffer.

12. Чек-лист готовности
□ IState/StateBase/PlayerStateMachine реализованы, переходы только через ChangeState.
□ Иерархия Grounded/Airborne с дочерними Idle/Run/Crouch/Jump/Fall работает.
□ Hurt/Death прерывают любой стейт.
□ Смена стейта видна в анимации через AnimatorDriver.
□ SetControlEnabled(false) останавливает чтение буфера, но физика продолжается.