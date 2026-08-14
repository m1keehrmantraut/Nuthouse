04-Movement — движение и физика
Версия: 2.1 · Зависит от: 01-Architecture, 02-StateMachine
Приоритет: реализуется третьим (вместе со стейтами Idle/Run/Jump/Fall/Crouch).
Изменения 2.1: мотор не зависит от GroundDetector (accel передаёт стейт), конструктор
GroundDetector, API Stamina (SetSprinting), jump-cut через InputReader.JumpHeld.

(Документ 04 не требует правок, кроме уточнения, что GroundDetector использует точку ног.)

1. Назначение
Movement — исполнительный слой физики. Он не знает, почему персонаж движется:
решения принимает StateMachine, а CharacterMotor превращает запросы в поведение Rigidbody2D.

Входящие CharacterMotor: Rigidbody2D (Dynamic, freezeRotation Z, gravityScale = 0 —
гравитация своя) и MovementConfig. Мотор не владеет GroundDetector: землю читает
стейт и передаёт мотору уже выбранные параметры (ускорение, флаг). Это сохраняет запрет
«Motor не знает Ground» (01 §5).

2. Компоненты
Класс	Ответственность
CharacterMotor	скорости, ускорение, трение, прыжок, гравитация, внешние импульсы
GroundDetector	земля/потолок/стены, coyote-таймер
FacingController	направление взгляда, flip спрайта (scale.x)
MovementConfig (SO)	все числовые параметры
Никто кроме CharacterMotor не пишет в Rigidbody2D.

3. CharacterMotor — API
csharp
// Nuthouse.Player.Movement
public sealed class CharacterMotor
{
    private readonly Rigidbody2D body;      // linearVelocity — Unity 6!
    private readonly MovementConfig cfg;
    private float gravityScale = 1f;        // множитель гравитации (fall/jump-cut)

    public Vector2 Velocity => body.linearVelocity;
    public float VerticalVelocity => body.linearVelocity.y;

    // Горизонтальное движение. accel выбирает стейт (на земле/в воздухе разные, 02 §8);
    // при inputX == 0 мотор сам тормозит с cfg.decel.
    public void MoveHorizontal(float inputX, float targetSpeed, float accel, float fdt);

    // Трение, когда ввода нет (вызывается из Grounded-стейтов)
    public void ApplyGroundFrictionIfNoInput(float inputX, float fdt);

    // Прыжок: задать вертикальную скорость
    public void Jump(float force);                  // velocity.y = force
    public void CutJump();                          // отпустили кнопку: velocity.y *= jumpCutMultiplier

    // Гравитация: вызывается каждый FixedUpdate из Airborne-стейтов
    public void ApplyGravity(float fdt);

    // Внешние импульсы (HurtState, отдача, взрывы)
    public void ApplyImpulse(Vector2 impulse);
    public void Knockback(Vector2 direction, float force);

    public void ResetVerticalVelocity();            // при приземлении
    public void SetGravityMultiplier(float m);      // для fall-multiplier / jump-cut
}
Формулы (канон)
Горизонталь (accel приходит от стейта; стейт берёт cfg.accel на земле и cfg.airAccel
в воздухе, 02 §8):

text
float a = inputX != 0 ? accel : cfg.decel;
velocity.x = Mathf.MoveTowards(velocity.x, inputX * targetSpeed, a * fdt)
Гравитация (в Airborne, вызывается стейтом каждый FixedUpdate):

text
float g = Physics2D.gravity.y * cfg.gravityMultiplier;
if (falling) g *= cfg.fallMultiplier;          // падение тяжелее подъёма
body.linearVelocity += Vector2.up * (g * fdt);
velocity.y = Mathf.Max(velocity.y, -cfg.maxFallSpeed);
Jump-cut (переменная высота прыжка): в JumpState.Tick, если !Ctx.Input.JumpHeld
(кнопка отпущена, 03 §5) и velocity.y > 0 → CutJump() (velocity.y *= cfg.jumpCutMultiplier, ~0.4–0.5).

4. GroundDetector
csharp
public sealed class GroundDetector
{
    // feet — точка отсчёта (ноги), cfg — coyoteTime/радиус, groundLayer — что считать землёй
    public GroundDetector(Transform feet, MovementConfig cfg, LayerMask groundLayer) { }

    public bool IsGrounded { get; private set; }
    public bool CeilingBlocked { get; private set; }   // нужно для приседа (legacy-логика)
    public Vector2 GroundNormal { get; private set; }

    public float CoyoteTimeLeft { get; private set; }  // сек
    public void UpdateGround();                        // FixedUpdate, до StateMachine.FixedTick
    public void ConsumeCoyote();                       // при прыжке
}
Реализация: Physics2D.OverlapCapsule / пара кругов под ногами, слой Ground
(текущая whatIsGround LayerMask переезжает сюда, в фасад приходит [SerializeField]).
Радиус ~0.1–0.2, как сейчас (k_GroundedRadius). Coyote: пока персонаж на земле,
CoyoteTimeLeft держится равным cfg.coyoteTime; при потере земли начинает убывать.
Прыжок возможен, пока CoyoteTimeLeft > 0 (проверка в GroundedState.Tick, 02 §5),
и JumpState.Enter вызывает ConsumeCoyote() (обнуляет, чтобы не прыгнуть дважды).

5. FacingController
Меняет transform.localScale.x на ±1 (текущий подход с eulerAngles 180° — убрать,
он ломает коллайдеры/дочерние объекты; flip только через scale либо поворот спрайт-рендера).

Обновляется по вводу в Grounded-стейтах; в воздухе и во время атаки не переворачивается
(персонаж смотрит туда, куда атакует). Атака смотрит в сторону Facing.Direction на момент
входа в стейт.

API: Direction (int ±1), UpdateByInput(float x), Lock()/Unlock() (на время атаки).

6. MovementConfig — стартовые значения
SO: Assets/Data/MovementConfig.asset. Значения подобраны от текущего геймплея
(legacy скорости 300/500 при множителе ×10×fixedDeltaTime ≈ 6/10 юнитов/с):

text
walkSpeed            6
runSpeed             10
crouchSpeed          2.5
accel                60
airAccel             35
decel                80
jumpForce            16
jumpCutMultiplier    0.45
gravityMultiplier    3.0
fallMultiplier       1.5
maxFallSpeed         28
coyoteTime           0.10
Все значения меняются в инспекторе; код их не хардкодит. После этапа 3 значения
доводятся геймдизайнером.

7. Бег и стамина
csharp
// Nuthouse.Health
public sealed class Stamina
{
    public Stamina(StaminaConfig cfg);

    public bool CanSprint { get; }          // стамины > 0 и не в «восстановлении»
    public float Normalized { get; }        // 0..1 — для UI (06 §5)

    public void SetSprinting(bool active);  // вызывает RunState каждый Tick
    public void Tick(float dt);             // расход/реген/порог восстановления

    public event System.Action<bool> SprintBlockedChanged;  // для UI/звука (опц.)
}
Бег: RunState вычисляет wantRun = Ctx.Input.RunHeld && inputX != 0 && Ctx.Stamina.CanSprint,
затем Ctx.Stamina.SetSprinting(wantRun) и targetSpeed = wantRun ? runSpeed : walkSpeed.

Расход идёт только когда SetSprinting(true); реген — в противном случае. Порог
восстановления после обнуления — из StaminaConfig (сейчас 20), реализуется таймером
в Tick (вместо корутины WaitUntil из текущего PlayerHealth).

Stamina не читает Motor и не знает про стейты — состояние передаёт RunState
через SetSprinting. Это сохраняет направление зависимостей (01 §5).

8. Присед
Пока зажат Crouch и персонаж на земле — CrouchState: скорость crouchSpeed,
флаг для анимации, при выходе проверка CeilingBlocked (не вставать под потолком,
как в текущем коде).

Бег отменяет присед (текущее поведение сохраняем).

9. Ограничения движения (locks)
Во время атаки/броска стейт не двигает персонажа по вводу; опционально атака может
задавать малое смещение (lunge) через Motor.ApplyImpulse в Enter.
API локов минимальный: стейт просто не вызывает MoveHorizontal. Отдельная система
«Movement Constraints» пока не нужна.

10. События движения
CharacterMotor/стейты публикуют в MovementEvents (06): Jumped, Landed(float impact),
RunChanged(bool), CrouchChanged(bool). Звук шагов, пыль при приземлении и т.п.
подписываются на них. Текущий «walk/run loop на AudioSource» переезжает в presentation-слушатель
MovementAudioListener.

11. Антипаттерны
Rigidbody2D/transform двигается из стейта, атаки, оружия напрямую.

Гравитация через Rigidbody2D.gravityScale одновременно со своей — выбрать что-то одно
(у нас своя, gravityScale = 0).

Направление через eulerAngles.y = 180 — только scale/sprite flip.

Параметры движения в [SerializeField] на куче разных MonoBehaviours — только в SO.

Движение в Update вместо FixedUpdate.

12. Чек-лист готовности
□ Motor двигает по формулам §3, гравитация своя, maxFallSpeed ограничен.
□ Jump: буфер + coyote + переменная высота (cut) работают одновременно.
□ Бег/стамина/присед ведут себя как в текущем прототипе (не хуже).
□ Flip через scale; во время атаки facing заперт.
□ Все числа в MovementConfig.asset; в коде нет магических констант.