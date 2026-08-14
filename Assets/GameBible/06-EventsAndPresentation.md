06-EventsAndPresentation — события, анимация, звук, камера
Версия: 2.2 · Зависит от: 01-Architecture
Изменения 2.2: исправлено противоречие Time.timeScale, убрано несуществующее событие StaminaChanged.

1. События: раздельные каналы
Единого EventBus нет. Каждый домен имеет свой маленький статический класс
с типизированными C#-событиями. Плюсы: нет глобального состояния-магнита, подписки видны
по имени класса, удалённый домен уносит с собой и свой канал.

Не путать с ServiceLocator (08-ServiceLocator.md): локатор — реестр сервисов
приложения (Scene, Progress, позже Time/Audio), а каналы ниже — способ уведомлять
презентацию о фактах геймплея. Это разные механизмы, и один не заменяет другой.

csharp
// Nuthouse.Core.Events — по одному файлу на канал
public static class MovementEvents
{
    public static event System.Action Jumped;
    public static event System.Action<float> Landed;          // сила удара
    public static event System.Action<bool> RunChanged;       // начал/закончил бег
    public static event System.Action<bool> CrouchChanged;

    public static void PublishJumped() => Jumped?.Invoke();
    // ... такие же Publish-методы для остального
}

public static class CombatEvents
{
    public static event System.Action<AttackDefinition> AttackStarted;
    public static event System.Action<DamageInfo, GameObject> AttackHit;
    public static event System.Action<AttackDefinition> AttackWhiffed;
    public static event System.Action<AxeThrowDefinition> ThrowStarted;   // вход в бросок (анимация)
    public static event System.Action ProjectileThrown;                    // момент спавна снаряда
    public static event System.Action<AxeProjectile, GameObject> AxeStuck;
    public static event System.Action AxePickedUp;
    public static event System.Action<int> AxeCountChanged;
}

public static class DamageEvents
{
    public static event System.Action<DamageInfo, GameObject> DamageTaken; // цель
    public static event System.Action<GameObject> Died;
    // Healed появится вместе с механикой лечения (сейчас её нет — не плодить заранее).
}

public static class GameEvents
{
    public static event System.Action PauseRequested;
    public static event System.Action<bool> PauseChanged;      // true = пауза
    public static event System.Action CheckpointReached;
}
Правила
Публикует только геймплей; Presentation только слушает. Слушатель не публикует обратно
игровые команды.

Нельзя принимать геймплейные решения по событиям («по DamageTaken начать HurtState»).
Управление — прямыми вызовами (StateMachine/Motor). События = уведомление постфактум.

Подписка в OnEnable, отписка в OnDisable — всегда парно. Исключение:
PlayerAnimatorDriver подписывается в Init(ctx) и отписывается в OnDestroy,
т.к. ему нужен контекст (§2).

Событие с данными — struct/неизменяемые аргументы; не передавать изменяемые коллекции.

Новый домен = новый файл-канал. Не плодить события «на будущее».

2. AnimatorDriver
Единственное место в проекте, где разрешён вызов Animator.*. Это MonoBehaviour на
префабе игрока: фасад не создаёт его через new, а делает
GetComponent<PlayerAnimatorDriver>() и вызывает Init(ctx) (02 §4).

csharp
// Nuthouse.Player.Presentation
public sealed class PlayerAnimatorDriver : MonoBehaviour  // висит на префабе игрока
{
    private Animator animator;
    private PlayerContext ctx;

    public void Init(PlayerContext context)
    {
        ctx = context;
        animator = GetComponentInChildren<Animator>();
        ctx.StateMachine.StateChanged += OnStateChanged;
        MovementEvents.Jumped += OnJumped;
        MovementEvents.Landed += OnLanded;
        CombatEvents.AttackStarted += OnAttackStarted;
        CombatEvents.ThrowStarted += OnThrowStarted;
        DamageEvents.DamageTaken += OnDamageTaken;
    }

    private void OnDestroy()
    {
        if (ctx == null) return;
        ctx.StateMachine.StateChanged -= OnStateChanged;
        MovementEvents.Jumped -= OnJumped;
        MovementEvents.Landed -= OnLanded;
        CombatEvents.AttackStarted -= OnAttackStarted;
        CombatEvents.ThrowStarted -= OnThrowStarted;
        DamageEvents.DamageTaken -= OnDamageTaken;
    }

    private void OnAttackStarted(AttackDefinition def) => animator.SetTrigger(def.animTrigger);
    private void OnThrowStarted(AxeThrowDefinition def) => animator.SetTrigger(def.animTrigger);
    // ... остальные обработчики

    private void LateUpdate()
    {
        // непрерывные параметры — из контекста (read-only, Presentation читает Gameplay)
        animator.SetFloat("Speed", Mathf.Abs(ctx.Motor.Velocity.x));
        animator.SetFloat("VerticalVelocity", ctx.Motor.VerticalVelocity);
        animator.SetBool("Grounded", ctx.Ground.IsGrounded);
    }
}
Почему подписки в Init, а не в OnEnable: драйверу нужен PlayerContext, который
появляется только в Awake фасада; OnEnable у префаба срабатывает раньше. Отписка —
в OnDestroy (объект живёт вместе с префабом). Для слушателей-презентации без
контекста (аудио, VFX) остаётся классическая пара OnEnable/OnDisable.

Параметры Animator (Player.controller)
Текущие параметры прототипа переименовываются по единой схеме:

Параметр	Тип	Заменяет
Speed	float	Speed (есть)
VerticalVelocity	float	— (Jump/Fall, новое)
Grounded	bool	— (новое)
IsRunning	bool	Running (есть)
IsCrouching	bool	Crouching (есть)
Attack1 / Attack2 / Attack3	trigger	Fight (разбить на стадии комбо)
ThrowAxe	trigger	— (новое)
Hurt	trigger	Hit (есть)
Dead	trigger	— (новое)
Анимации прыжка/падения пока нет — до их появления Jump/Fall отображаются через
Speed/VerticalVelocity на существующих клипах (или Idle в воздухе). Не блокировать
этап 3 отсутствием анимаций.

Animation Events (например «включить хитбокс на кадре N») сейчас не используются:
фазы атак таймерные (05 §4). Переход на anim events — только если появится реальная нужда
(см. FutureComponents/ComboGraph.md).

3. Звук
Текущий подход (AudioSource + клипы прямо в PlayerMovement/MeleeCombat) заменяется
слушателями-презентацией:

MovementAudioListener (на игроке): подписан на MovementEvents — шаги (walk/run loop
как сейчас, но старт/стоп по RunChanged/Landed), приземление.

CombatAudioListener: подписан на CombatEvents/DamageEvents — свинг, попадание
(рандом pitch 0.65–1.35 как сейчас), промах, свист броска, воткнувшийся топор.

Клипы и громкости — в [SerializeField] слушателя или в маленьком SO AudioProfile
(один SO на слушателя — достаточно; общая аудиосистема с пулами — Future).

Правило: звук никогда не стартует из геймплей-класса.

4. Камера
Основная камера: Cinemachine 2.10 (пакет установлен). CinemachineVirtualCamera
с CinemachineTransposer/FramingTransposer, follow = Transform игрока.
Ручной Camera.main.transform в скриптах запрещён.

Тряска при ударе: CinemachineImpulseSource на источниках урона +
CinemachineImpulseListener на виртуальной камере. Сила — из данных урона/событий.

Текущие анимации камеры (Assets/Animation/Camera Shakes, camAnimator.SetTrigger("Hit"))
мигрируют на impulse; аним-клипы камеры можно удалить после миграции.

Диалоговые/катсценные камеры — будущие виртуальные камеры с приоритетами; пока не нужны.

5. UI
UI читает состояние подписками на события (DamageEvents.DamageTaken → HP-бар,
CombatEvents.AxeCountChanged → иконки топоров, стамина — поллинг Stamina.Normalized
раз в кадр). Событие StaminaChanged не существует; при необходимости добавить позже.

StaminaBarFollow/UIFollowNPC остаются как есть (это presentation).

UI никогда не вызывает геймплей; кнопки меню → GameEvents/прямые вызовы меню.

6. Время (задел под Hit Stop / Slow Motion)
Сейчас в целевом геймплейном коде Time.timeScale не используется нигде. Единственное
исключение — временный PauseMenu, который ставит timeScale = 0 для паузы; при переходе
на TimeService он мигрирует. Единственное место, где позже появится управление временем, —
будущий TimeService: сервис в ServiceLocator, регистрируется GameBootstrap и тикается
им (спроектирован в FutureComponents/TimeEffects.md, механизм сервисов — 08-ServiceLocator.md).
Уже сейчас соблюдаем два правила, чтобы подключение было безболезненным:

Вся геймплейная логика использует Time.deltaTime/Time.fixedDeltaTime (не
unscaledDeltaTime) и не опирается на timeScale == 1
(текущая проверка Mathf.Approximately(Time.timeScale, 1f) в PlayerMovement — убрать).

UI/пауза, которым нужен unscaled time, помечаются отдельно в момент реализации паузы
(PauseMenu уже работает через Time.timeScale; при переходе на TimeService — мигрирует).

7. Антипаттерны
Единый EventBus.Publish<T> на всё — запрещён навсегда (события только раздельными
каналами; ServiceLocator — не шина событий, а реестр сервисов, 08).

Геймплей подписывается на свои же события, чтобы выполнить следующую логику.

animator.SetTrigger вне AnimatorDriver.

Боевой стейт вызывает AnimatorDriver напрямую вместо публикации события.

Камера двигается скриптом вместо Cinemachine.

Звук воспроизводится из PlayerCombat/AttackRunner.

8. Чек-лист готовности
□ Четыре канала событий существуют, подписки/отписки парные.
□ Анимация игрока полностью управляется AnimatorDriver по таблице §2.
□ Шаги, свинги, попадания озвучены слушателями без геймплейных ссылок.
□ Камера — Cinemachine; тряска — impulse (можно на этапе 6).
□ Никто не трогает Time.timeScale в геймплее.