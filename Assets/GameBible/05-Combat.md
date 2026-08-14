05-Combat — атака, урон и бросок топора
Версия: 2.2 · Зависит от: 01, 02, 03, 04
Приоритет: ближняя атака — этап 4; бросок топора — главная цель текущего спринта (этап 5).
Изменения 2.2: исправлена сигнатура InteractController (LayerMask), уточнён Health.TakeDamage,
AttackState использует runner.Current, добавлен ReturnAxe в фасад.

1. Общие принципы
Атака — данные (AttackDefinition), исполнение — код (AttackRunner). Новая атака =
новый SO-ассет (+ опционально свой стейт/подкласс), без правки ядра.

Урон наносится только через Hitbox → IDamageable. Physics2D.OverlapCircle «на месте»
(как в текущем MeleeCombat) не используется: только коллайдерные хитбоксы, управляемые фазами.

Combat не знает Animator и Input. Анимацию запускает событие: стейт/PlayerCombat
публикует CombatEvents.AttackStarted(def), PlayerAnimatorDriver слушает и ставит
триггер из def.animTrigger (06 §2). Прямой вызов драйвера из боя запрещён.
Ввод приходит через Buffer → стейт.

2. Урон: DamageInfo, IDamageable, Health
csharp
// Nuthouse.Combat
public enum DamageType { Physical, Ranged }   // расширяется без правки потребителей

public readonly struct DamageInfo
{
    public readonly float Amount;
    public readonly DamageType Type;
    public readonly Vector2 Direction;     // от атакующего к цели (для knockback/анимации)
    public readonly float Knockback;
    public readonly GameObject Source;

    public DamageInfo(float amount, DamageType type, Vector2 direction, float knockback, GameObject source)
    { Amount = amount; Type = type; Direction = direction; Knockback = knockback; Source = source; }
}

public interface IDamageable
{
    void TakeDamage(in DamageInfo info);
}
Health (игрок) — чистый класс
csharp
// Nuthouse.Health
public sealed class Health : IDamageable
{
    public Health(HealthConfig cfg, PlayerContext ctx);   // ctx — чтобы писать PendingHurt

    public float Current { get; }
    public bool IsDead { get; }
    public bool Invulnerable { get; }                     // окно i-frames после удара

    public void TakeDamage(in DamageInfo info);
    public void Tick(float dt);                           // таймер неуязвимости
}
Получив урон (и не будучи в неуязвимости): вычитает HP, публикует
DamageEvents.DamageTaken, при HP ≤ 0 — DamageEvents.Died и флаг IsDead.
Если HP > 0 — пишет:

csharp
Ctx.PendingHurt = new HurtRequest(info, info.Direction, info.Knockback);
PlayerFacade.Update видит запрос и переводит в HurtState (02 §6). Неуязвимость
после удара — HealthConfig.invulnTime (~0.5 с); в прототипе её не было, это новое
поведение, а не перенос старого.

Враги: DamageReceiver (MonoBehaviour)
Враги используют DamageReceiver — тонкую MonoBehaviour-обёртку над IDamageable
(лежит в Scripts/Health): HP из SO-конфига, публикация DamageEvents.Died(gameObject).
EnemyHealth : DamageReceiver (см. ForDeveloper/01 §6). У игрока DamageReceiver нет:
его Hurtbox привязывается к чистому Health через Hurtbox.Bind(...) в фасаде (§5).

3. AttackDefinition (SO)
csharp
[CreateAssetMenu(menuName = "Nuthouse/Attack Definition")]
public sealed class AttackDefinition : ScriptableObject
{
    [Header("Фазы, сек")]
    public float startup  = 0.08f;   // замаха нет хитбокса
    public float active   = 0.10f;   // хитбокс активен
    public float recovery = 0.22f;   // возврат, уязвимы

    [Header("Урон")]
    public float damage = 25f;
    public DamageType damageType = DamageType.Physical;
    public float knockback = 4f;

    [Header("Хитбокс (локально, относительно центра персонажа)")]
    public Vector2 hitboxOffset = new(0.9f, 0f);
    public Vector2 hitboxSize   = new(1.2f, 1.0f);

    [Header("Движение")]
    public float lungeImpulse = 0f;        // импульс вперёд в Enter
    public bool lockMovement  = true;

    [Header("Комбо")]
    public AttackDefinition nextInCombo;   // null = конец цепочки
    public float cancelWindowStart = 0.5f; // доля recovery, с которой можно продолжить комбо

    [Header("Анимация")]
    public string animTrigger = "Attack1";
}
Комбо-цепочка задаётся связкой ассетов (nextInCombo). Полный Combo Graph —
FutureComponents/ComboGraph.md, пока достаточно цепочки.

4. AttackRunner — исполнение фаз
csharp
// Nuthouse.Combat
public enum AttackPhase { None, Startup, Active, Recovery }

public sealed class AttackRunner
{
    public AttackPhase Phase { get; private set; }
    public AttackDefinition Current { get; private set; }
    public bool InCancelWindow { get; private set; }   // для комбо-продолжения

    // ctx нужен для HitboxRoot, Facing и публикации событий.
    public AttackRunner(PlayerContext ctx);

    public void Start(AttackDefinition def);   // сам создаёт/переиспользует Hitbox
    public void Tick(float dt);                // таймер фаз, вкл/выкл Hitbox
    public void Cancel();                      // выход из атаки (Hurt, конец)
    public event System.Action<AttackDefinition> AttackFinished;
}
Правила:

AttackRunner владеет одним переиспользуемым Hitbox-объектом: создаёт его лениво
как ребёнка Ctx.HitboxRoot (пустышка в префабе, ForDeveloper/01 §1). В Start —
позиционирует/сайзит по def (offset.x зеркалится от Ctx.Facing.Direction), вызывает
hitbox.Setup(damageInfo, ownerRoot) и hitbox.ResetHits().

Хитбокс включается (SetActive(true)) строго на фазе Active, выключается на Recovery.

Дедупликация урона: каждый враг за одну Active-фазу получает урон максимум один раз.
Это обеспечивает сам Hitbox через TryHit (§5), а не внешний список — так
дедупликацию нельзя обойти из Hurtbox. ResetHits() в Start очищает список.

Start публикует CombatEvents.AttackStarted(def) (анимацию ставит AnimatorDriver, §6).

AttackState.Tick: если runner.InCancelWindow && Buffer.TryConsume(Attack) и есть
runner.Current.nextInCombo → runner.Start(runner.Current.nextInCombo) без выхода из стейта.

5. Hitbox / Hurtbox
Hitbox: GameObject с BoxCollider2D(isTrigger) + Hitbox.cs. Слой/матрица коллизий:
Hitbox против Hurtbox (ForDeveloper/01 §1). У снарядов (топор) — тот же Hitbox
на префабе. Хитбокс знает текущий DamageInfo, владельца и список уже поражённых целей.
НЕ знает Player, Animator.

Hurtbox: collider на теле персонажа/врага + Hurtbox.cs. Получателя урона задают
двумя путями: враги — [SerializeField] DamageReceiver в инспекторе; игрок — runtime
Hurtbox.Bind(ctx.Health) в PlayerFacade.Awake (т.к. Health — чистый класс).

Дедупликация: Hurtbox наносит урон, только если Hitbox.TryHit(receiver) вернул
true (цель ещё не бита этим хитбоксом в текущей фазе/полёте).

Свой урон: Hurtbox игнорирует хитбоксы своего владельца (IsOwnedBy), чтобы
топор/меч не били хозяина.

csharp
// Nuthouse.Combat
public sealed class Hitbox : MonoBehaviour
{
    public DamageInfo CurrentDamageInfo { get; private set; }
    private Transform ownerRoot;
    private readonly HashSet<IDamageable> alreadyHit = new();

    public void Setup(in DamageInfo info, Transform ownerRoot)
    {
        CurrentDamageInfo = info;
        this.ownerRoot = ownerRoot;
    }

    public void ResetHits() => alreadyHit.Clear();          // в Start атаки / Launch снаряда

    public bool TryHit(IDamageable target) => alreadyHit.Add(target); // false, если уже били

    public bool IsOwnedBy(Transform root) => ownerRoot == root;
}

public sealed class Hurtbox : MonoBehaviour
{
    [SerializeField] private DamageReceiver receiver;       // враги: через инспектор
    private IDamageable target;                             // игрок: через Bind

    private Transform Root => transform.root;

    public void Bind(IDamageable damageable) => target = damageable;   // PlayerFacade.Awake

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Hitbox>(out var hb)) return;
        if (hb.IsOwnedBy(Root)) return;                     // свой хитбокс/снаряд
        var who = target ?? (IDamageable)receiver;
        if (who == null || !hb.TryHit(who)) return;         // дедупликация
        who.TakeDamage(hb.CurrentDamageInfo);
    }
}
6. AttackState (ближняя атака)
Стейт из 02-StateMachine. Логика:

text
Enter:    runner.Start(первый AttackDefinition цепочки)   // сам публикует AttackStarted;
          Facing.Lock(); Motor.ApplyImpulse(lunge)
Tick:     runner.Tick(dt);
          если Ctx.PendingHurt.HasValue → выход (атака прерывается);
          если InCancelWindow && буфер Attack && runner.Current.nextInCombo → runner.Start(runner.Current.nextInCombo)
FixedTick: ничего (lockMovement) либо лёгкое торможение
Exit:     runner.Cancel(); Facing.Unlock();
Анимацию при входе ставит PlayerAnimatorDriver, подписанный на
CombatEvents.AttackStarted (06 §2) — стейт драйвер не вызывает.
Текущая логика MeleeCombat (кулдаун 0.5 c) заменяется фазами Recovery из SO.

7. Бросок топора — AxeThrow (целевая механика)
7.1. Дизайн
Топор — метательное оружие с ограниченным боезапасом и физическим полётом:

Замах (startup) — короткий, персонаж заперт.

Бросок (active) — спавн AxeProjectile в руке, импульс в сторону Facing.

Восстановление (recovery) — чуть дольше замаха: бросок наказуем вблизи.

Топор в полёте вращается. При попадании во врага — урон и застревание; в стену/пол —
втыкается и ждёт подбора; промах мимо всего — падает на землю под гравитацией.
Подбор топора — через Interact (возврат боезапаса).

7.2. AxeThrowDefinition (SO)
csharp
[CreateAssetMenu(menuName = "Nuthouse/Axe Throw Definition")]
public sealed class AxeThrowDefinition : ScriptableObject
{
    [Header("Фазы броска")]
    public float windup   = 0.12f;
    public float recovery = 0.30f;

    [Header("Полёт")]
    public float throwForce = 18f;        // горизонтальная скорость спавна
    public float upwardBias = 2f;         // небольшая добавка к velocity.y
    public bool inheritOwnerVelocity = true;
    public float spinSpeed = 720f;        // град/сек — вращение в полёте
    public bool physicalSpin = false;     // true = через Rigidbody2D.angularVelocity
                                          // false = визуальное вращение спрайта (базово)

    [Header("Урон")]
    public float damage = 35f;
    public float knockback = 6f;

    [Header("Поведение при контакте")]
    public bool stickToEnemy  = true;     // застревает в враге (парентинг)
    public bool stickToWorld  = true;     // втыкается в пол/стену
    public float bounceOnNoStick = 0.3f;  // упругость, если не воткнулся
    public float despawnTime = 0f;        // 0 = не исчезает, ждёт подбора

    [Header("Ссылки и боезапас")]
    public AxeProjectile axePrefab;
    public string animTrigger = "ThrowAxe";
    public int startAxeCount = 2;         // стартовый боезапас PlayerCombat
}
7.3. AxeProjectile
csharp
// Nuthouse.Combat.Projectiles
public sealed class AxeProjectile : MonoBehaviour
{
    private AxeThrowDefinition def;
    private Rigidbody2D body;
    private Hitbox hitbox;
    private State state;               // Flying, Stuck, Grounded

    public void Launch(AxeThrowDefinition def, Vector2 direction, Vector2 ownerVelocity);
    private void FixedUpdate();        // физика + вращение
    private void HandleHit(Collider2D other);
    public void PickUp();              // Interact → уничтожить/вернуть в пул, +боезапас
}
Механика полёта:

Спавн в точке руки (anchor на префабе игрока), body.linearVelocity = direction * throwForce + up * upwardBias (+ ownerVelocity).

Гравитация обычная (gravityScale ~1–1.5) — траектория навесная, не лазерная.

Вращение: базово визуальное — крутим дочерний спрайт spinSpeed°/с; флаг
physicalSpin переключает на body.angularVelocity (тогда freezeRotation = false
и нужен физический контакт, см. ниже). Вращение не влияет на урон — хитбокс покрывает
весь топор целиком.

Хитбокс активен всё время полёта (state == Flying), DamageInfo заполнен при спавне.
В Launch снаряд вызывает hitbox.Setup(info, ownerRoot) и не делает ResetHits
дальше: список поражённых целей живёт весь полёт — одна цель получает урон максимум
один раз за полёт (дедупликация Hitbox.TryHit, §5).

Контакты (OnTriggerEnter2D / OnCollisionEnter2D хитбокса):

Цель	Поведение
Враг (IDamageable)	урон; stickToEnemy → снаряд становится ребёнком коллайдера врага, state=Stuck, хитбокс выключен. Урон от застрявшего топора не тикает
Стена/пол (слой Ground)	stickToWorld → остановить тело (body.bodyType = Kinematic), развернуть спрайт «лезвием в поверхность» (по нормали контакта), state=Stuck
Ни то ни другое	отскок с bounceOnNoStick, дальше катится/лежит: state=Grounded, хитбокс выключен
Застрявший/лежащий топор — объект с Interactable-компонентом (подбор).

7.4. AxeThrowState
text
Enter:    Ctx.Combat.TakeAxe();  Facing.Lock(); CombatEvents.ThrowStarted (анимацию
          ставит AnimatorDriver по def.animTrigger — прямых вызовов нет)
          фаза Windup по таймеру
Момент броска (конец Windup): Ctx.Combat.SpawnAxe(def, Ctx.Facing.Direction)
          → CombatEvents.ProjectileThrown (звук свистка, лёгкая отдача камеры в будущем)
Recovery: таймер; буфер Attack в recovery → можно отменить в Idle (опц., по геймдизайну)
Exit:     Facing.Unlock()
7.5. Боезапас и подбор
PlayerCombat.AxesCount (int, старт = AxeThrowDefinition.startAxeCount), HasAxe,
TakeAxe(), ReturnAxe(int n). Конструктор PlayerCombat(ctx, primaryAttack, axeThrow)
получает оба SO из [SerializeField]-полей фасада (02 §4).

Подбор — через систему интеракций игрока:

csharp
// Nuthouse.World.Interact
public abstract class Interactable : MonoBehaviour
{
    public float radius = 1f;                 // радиус срабатывания
    public bool IsAvailable { get; protected set; } = true;
    public abstract void Interact(PlayerFacade player);   // внешний мир → API фасада
}

// Nuthouse.Player (подсистема игрока, знает World.Interact)
public sealed class InteractController
{
    public InteractController(PlayerContext ctx, LayerMask interactableLayer);
    public void Tick();   // PlayerFacade.Update: TryConsume(Interact) → ближайший доступный
}
InteractController.Tick при Ctx.ControlEnabled: потребляет InputAction.Interact
из буфера, ищет ближайший Interactable в радиусе (Physics2D.OverlapCircleAll,
слой Interactable из переданного LayerMask + проверка radius и
IsAvailable каждого), вызывает Interact(ctx.Facade).
Застрявший/лежащий топор включает на себе AxeInteractable : Interactable, который
делает player.ReturnAxe(1) и Destroy/возврат снаряда в пул. Внешний мир общается
с игроком только через публичный API фасада (PlayerFacade.ReturnAxe(int) →
ctx.Combat.ReturnAxe), не через внутренние компоненты (01 §3).

UI боезапаса подписывается на CombatEvents.AxeCountChanged (Presentation, см. 06).

8. Урон по игроку
Враги используют тот же пайплайн: их атаки — хитбоксы (или свои AttackRunner'ы),
игрок имеет Hurtbox. Health.TakeDamage → Ctx.PendingHurt → HurtState (knockback через
Motor.Knockback). Краткая неуязвимость после удара — HealthConfig.invulnTime (~0.5 c):
в прототипе неуязвимости не было, добавляем с нуля — таймер в Health.Tick, повторный
урон в окно неуязвимости игнорируется самим Health.TakeDamage (не Hurtbox).

9. События боя
В CombatEvents (06): AttackStarted(def), AttackHit(DamageInfo, target),
AttackWhiffed(def), ThrowStarted(axeDef) (момент входа в бросок — для анимации),
ProjectileThrown, AxeStuck(AxeProjectile, surface),
AxePickedUp, AxeCountChanged(int). Звук/камера/VFX подписываются.

10. Расширение
Новая ближняя атака → новый AttackDefinition (+ в цепочку комбо). Код не трогается.

Новый снаряд → подкласс/новый класс по образцу AxeProjectile + свой SO.

Новое поведение при попадании (например, рикошет) → стратегия в HandleHit/поле в SO.

11. Антипаттерны
Урон через OverlapCircle в момент нажатия (текущий код — переписать).

Атака сама включает анимацию (_animator.SetTrigger) или вызывает AnimatorDriver напрямую.

Снаряд знает Player/PlayerCombat — только DamageInfo, конфиг и события.

Хитбокс активен вне фазы Active (у снаряда — вне Flying).

Один снаряд/хитбокс бьёт одну цель несколько раз за фазу/полёт (обход TryHit).

Хитбокс бьёт своего владельца (должен фильтроваться Hurtbox.IsOwnedBy).

12. Чек-лист готовности
□ Melee: фазы из SO, хитбокс коллайдерный, один урон за фазу, комбо-цепочка по cancel-окну.
□ Урон игрока и врага идёт через IDamageable/Hurtbox.
□ AxeThrow: windup/throw/recovery, топор летит навесом и вращается.
□ Топор втыкается во врага и в мир, подбирается через Interact, боезапас сходится.
□ Все параметры в SO; в коде нет магических чисел урона/скоростей.