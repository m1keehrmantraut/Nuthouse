03-InputAndBuffer — ввод и буфер намерений
Версия: 2.1 · Зависит от: 01-Architecture
Приоритет: реализуется вторым, сразу после StateMachine.
Изменения 2.1: конструктор BufferedCommand, JumpHeld для jump-cut, InputReader без
конфига (TTL — в InputBuffer), InputBuffer.Tick() без аргумента, snapshot для debug.

(Документ 03 не требует правок, кроме уточнения, что он остаётся без изменений.)

1. Назначение
Input слой фиксирует намерение игрока («нажата кнопка атаки») и складывает его в буфер.
Он не знает, можно ли сейчас атаковать, и не запускает ничего сам. Решение принимает
StateMachine, читая буфер. Это даёт отзывчивое управление: нажатие за 0.15 с до конца
восстановления всё равно выполнится.

Технология: Unity Input System (пакет 1.19 уже установлен, activeInputHandler = Both).
Legacy Input.GetAxis/GetButton в новом коде запрещены.

2. Пайплайн
text
Устройство (клавиатура/геймпад)
    ↓
PlayerInputActions.inputactions (ассет, Action Maps: Gameplay, UI)
    ↓
InputReader (подписка на callbacks → нормализованные данные)
    ↓
InputBuffer (команды с TTL; непрерывные оси хранятся отдельно)
    ↓
StateMachine.Tick (TryConsume / чтение осей)
AI и будущий Replay подают команды в тот же InputBuffer (см. FutureComponents/ReplaySystem.md),
поэтому буфер не знает об источнике команд.

3. Ассет ввода
Assets/Input/PlayerInputActions.inputactions — один ассет, две карты:

Карта Gameplay:

Action	Type	Привязки (стартовые)	Назначение
Move	Value, Vector2	A/D, ←/→ (для 2D только ось X, Y зарезервирована под лестницы/прицел)	движение
Jump	Button	Space	прыжок
Run	Button	Left Shift	бег (удержание)
Crouch	Button	S / Ctrl	присед (удержание)
Attack	Button	ЛКМ / J	ближняя атака
Throw	Button	ПКМ / K	бросок топора
Interact	Button	E	взаимодействие/подбор топора
Pause	Button	Esc	пауза
Карта UI: Navigate, Submit, Cancel (для меню, работает через EventSystem как сейчас).

Rebinding (переназначение клавиш) сейчас не реализуется — отложено
(FutureComponents/InputDevices.md), но ассет должен использовать composite-биндинги,
чтобы rebinding добавился без переделки.

4. Команды
csharp
// Nuthouse.Input
public enum InputAction { Jump, Run, Crouch, Attack, Throw, Interact, Pause }

public enum InputPhase { Pressed, Held, Released }

public readonly struct BufferedCommand
{
    public readonly InputAction Action;
    public readonly float Time;      // Time.time в момент нажатия
    public readonly float Ttl;       // окно жизни из InputConfig

    public BufferedCommand(InputAction action, float time, float ttl)
    {
        Action = action;
        Time = time;
        Ttl = ttl;
    }

    public bool IsExpired(float now) => now - Time > Ttl;
}
Непрерывные данные (Move-ось, удержания Run/Crouch/Jump) не буферизуются — они
хранятся в InputReader как актуальное состояние и читаются стейтами напрямую.
Буферизуются только дискретные намерения (Jump-нажатие, Attack, Throw, Interact).
Удержание Jump нужно для переменной высоты прыжка (jump-cut, 04 §3).

5. InputReader
csharp
// Nuthouse.Input
public sealed class InputReader
{
    private readonly PlayerInputActions actions;

    public Vector2 MoveAxis { get; private set; }      // непрерывно
    public bool RunHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool JumpHeld { get; private set; }         // для jump-cut (04 §3)

    public event System.Action<InputAction> ActionPressed;   // → Buffer (через фасад)

    public InputReader()
    {
        actions = new PlayerInputActions();
        BindCallbacks(actions.Gameplay);
        actions.Gameplay.Enable();
    }

    private void BindCallbacks(InputActionMap map)
    {
        map.Move.performed   += c => MoveAxis = c.ReadValue<Vector2>();
        map.Move.canceled    += _ => MoveAxis = Vector2.zero;
        map.Run.started      += _ => RunHeld = true;
        map.Run.canceled     += _ => RunHeld = false;
        map.Crouch.started   += _ => CrouchHeld = true;
        map.Crouch.canceled  += _ => CrouchHeld = false;

        // Jump: и нажатие (в буфер), и удержание (для jump-cut).
        map.Jump.started      += _ => { JumpHeld = true;  ActionPressed?.Invoke(InputAction.Jump); };
        map.Jump.canceled     += _ => JumpHeld = false;

        map.Attack.performed  += _ => ActionPressed?.Invoke(InputAction.Attack);
        map.Throw.performed   += _ => ActionPressed?.Invoke(InputAction.Throw);
        map.Interact.performed+= _ => ActionPressed?.Invoke(InputAction.Interact);
        map.Pause.performed   += _ => ActionPressed?.Invoke(InputAction.Pause);
    }

    public void SetMapEnabled(string mapName, bool enabled) { /* Gameplay/UI */ }
    public void Dispose() => actions.Dispose();
}
PlayerFacade связывает ввод и буфер с фильтрами (единый обработчик — канон в
02-StateMachine.md §4, метод OnActionPressed): пауза уходит в GameEvents мимо буфера,
выключенное управление и недоступные действия (Throw без топора) игнорируются, остальное —
ctx.Buffer.Add(action). Подписка/отписка — в Awake/OnDestroy фасада.

6. InputBuffer
csharp
// Nuthouse.Input
public sealed class InputBuffer
{
    private readonly InputConfig config;
    private readonly List<BufferedCommand> commands = new(8);

    public InputBuffer(InputConfig config) => this.config = config;

    public void Add(InputAction action)
    {
        commands.RemoveAll(c => c.Action == action);        // одно действие — одна команда
        commands.Add(new BufferedCommand(action, Time.time, config.GetTtl(action)));
    }

    public bool Has(InputAction action)
        => commands.Exists(c => c.Action == action);

    public bool TryConsume(InputAction action)
    {
        int i = commands.FindIndex(c => c.Action == action);
        if (i < 0) return false;
        commands.RemoveAt(i);
        return true;
    }

    public void Tick()   // вызывается из PlayerFacade.Update; время — Time.time
    {
        float now = Time.time;
        commands.RemoveAll(c => c.IsExpired(now));
    }

    public void Clear() => commands.Clear();

    public IReadOnlyList<BufferedCommand> Snapshot => commands;   // debug (03 §9)
}
Правила буфера:

Команда живёт Ttl секунд (окно буферизации), затем молча удаляется.

Буфер не проверяет кулдауны, стамины, состояния — он «глупый». Проверки делает стейт.

Повторное нажатие того же действия перезаписывает команду (не копит очередь).
Очереди (комбо-цепочки) — задача Combat, а не Buffer.

Clear() вызывается при потере управления (диалог, катсцена, смерть).

InputConfig (ScriptableObject)
csharp
[CreateAssetMenu(menuName = "Nuthouse/Input Config")]
public sealed class InputConfig : ScriptableObject
{
    public float jumpBuffer    = 0.12f;   // сек
    public float attackBuffer  = 0.15f;
    public float throwBuffer   = 0.15f;
    public float interactBuffer= 0.25f;

    public float GetTtl(InputAction a) => a switch
    {
        InputAction.Jump     => jumpBuffer,
        InputAction.Attack   => attackBuffer,
        InputAction.Throw    => throwBuffer,
        InputAction.Interact => interactBuffer,
        _ => 0.1f,
    };
}
7. Фильтры действий
Не всякое нажатие должно попадать в буфер:

PlayerFacade подписывается на ActionPressed и применяет фильтры:
управление выключено → игнор; действие недоступно в принципе (например Throw без топора) →
игнор (или оставить в буфере, если окно маленькое — решается геймдизайном; базово — игнор).

Pause не попадает в буфер вообще — уходит напрямую в GameEvents.PauseRequested →
PauseMenu (как сейчас PauseMenu.Pause).

8. Контексты ввода
Минимальная схема на сейчас — переключение Action Map:

Контекст	Активная карта	Кто переключает
Gameplay	Gameplay	по умолчанию
Menu/Pause	UI	PauseMenu
Dialogue	Gameplay выключена	DialogueSystem (когда появится)
Переключение — через InputReader.SetMapEnabled. Никаких отдельных классов контекстов пока
не нужно (не оверинжинирить): одной выключенной карты достаточно.

9. Debug
В InputReader и InputBuffer — публичные read-only снапшоты (IReadOnlyList<BufferedCommand>)
и, при желании, отладочный вывод через Debug.Log/ImGui под #if UNITY_EDITOR/define
NUTHOUSE_DEBUG. Формат строки: [12.304] Attack Pressed → buffered (ttl 0.15).

10. Расширение
Новая кнопка = новый Action в ассете + значение в enum InputAction + TTL в InputConfig +
подписка в BindCallbacks + потребление в нужном стейте/системе. Читатели буфера и мотор
не меняются.

11. Антипаттерны
Input.GetButton* в геймплее (legacy).

Стейт подписывается на ActionPressed напрямую, минуя буфер.

Буфер проверяет кулдауны/стамину.

Buffer хранит очередь из 10 команд (пока нужен один слот на действие).

Геймпад-специфичная логика в стейтах (всё нормализуется в ассете/ридере).

12. Чек-лист готовности
□ Ассет PlayerInputActions.inputactions создан, карта Gameplay по таблице §3.
□ InputReader отдаёт MoveAxis/удержания и события нажатий.
□ InputBuffer хранит/потребляет/чистит команды по TTL из SO.
□ Прыжок через буфер: нажатие чуть до приземления даёт прыжок сразу после приземления.
□ SetControlEnabled(false) очищает буфер и игнорирует ввод.
□ Пауза работает через UI-карту и не зависит от буфера.