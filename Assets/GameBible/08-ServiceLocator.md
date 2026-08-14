08-ServiceLocator — Service Locator, Singleton и композиция сервисов
Версия: 3.0 · Зависит от: 01-Architecture
Статус: базовая линия. Вводит сервисный слой как единственную точку сборки
приложения. Правила из других документов про Singleton/Service Locator согласованы с этим.

Документ не требует изменений, кроме уточнения, что он остаётся без правок.

1. Назначение и место в архитектуре
Документ описывает сервисный слой — механизмы, которыми собирается и живёт
всё, что относится к приложению в целом, а не к конкретному игроку/сцене:
течение времени, поток сцен, сохранение прогресса, настройки, аудио.

Два механизма:

Service Locator — статический реестр сервисов. Единственное место, где
приложение узнаёт о своих сервисах. Заменяет FindFirstObjectByType,
статические поля-магниты и протягивание ссылок через десять конструкторов.

Singleton / PersistentSingleton — базовые MonoBehaviour для глобальных
объектов, у которых ровно один экземпляр (точка входа, бутстрап).

Важно разграничить: Service Locator — это про сервисы приложения.
Он не заменяет PlayerContext (композицию игрока) и не является
шиной событий (события — раздельные каналы, 06-EventsAndPresentation.md).

text
GameBootstrap (PersistentSingleton)      ← точка входа, живёт вечно
      │  в Awake регистрирует сервисы
      ▼
ServiceLocator  ──►  ISceneService, IProgressService, ITimeService(буд.), IAudioService(буд.)
      ▲
      │ читают сервисы (но НЕ геймплейные системы)
      └── Меню, UI, World, презентация, PlayerFacade (только для сервисов)
2. Что такое сервис (критерии)
Класс попадает в Service Locator, только если отвечает всем критериям:

Живёт всё время работы приложения (или всю сцену), а не создаётся на игрока/врага.

Экземпляр ровно один (или один на сцену).

Нужен нескольким независимым потребителям из разных слоёв.

Не является геймплейной сущностью (игрок, враг, снаряд, стейт).

Сервисы сейчас (этапы 1–3)
Интерфейс	Реализация	Что делает
ISceneService	SceneService	загрузка/порядок сцен, переход в меню
IProgressService	ProgressService	сохранение прогресса (PlayerPrefs), завершённые сцены
Обе реализуются на базе текущего GameProgress (см. ForDeveloper/03-CurrentCodeMap.md),
разбитого на два сервиса. Это единственные сервисы, которые регистрируются сразу.

Сервисы потом (НЕ строить заготовки)
Интерфейс	Когда появится
ITimeService	FutureComponents/TimeEffects.md, после этапа 6
IAudioService	когда появится общая аудиосистема с пулами (06 §3)
ISaveService	когда сохранение выйдет за рамки PlayerPrefs
ISettingsService	когда появится меню настроек с persist
Правило: сервис из «потом» не создаётся и не регистрируется заранее.
Регистрация добавляется в GameBootstrap в момент реализации сервиса.

3. Service Locator (полная реализация)
csharp
// Nuthouse.Core
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<TService>(TService instance) where TService : class
    {
        var key = typeof(TService);
        if (services.ContainsKey(key))
            throw new InvalidOperationException($"ServiceLocator: {key.Name} уже зарегистрирован.");
        services[key] = instance;
    }

    public static TService Get<TService>() where TService : class
    {
        var key = typeof(TService);
        if (!services.TryGetValue(key, out var svc))
            throw new InvalidOperationException(
                $"ServiceLocator: {key.Name} не зарегистрирован. Проверь GameBootstrap.");
        return (TService)svc;
    }

    public static bool TryGet<TService>(out TService service) where TService : class
    {
        if (services.TryGetValue(typeof(TService), out var svc))
        {
            service = (TService)svc;
            return true;
        }
        service = null;
        return false;
    }

    public static bool Has<TService>() where TService : class
        => services.ContainsKey(typeof(TService));

    public static void Unregister<TService>() where TService : class
        => services.Remove(typeof(TService));

    public static void Clear() => services.Clear();   // смена сцены / тесты
}
Правила использования:

Регистрирует только GameBootstrap (одна точка сборки). Никакой
саморегистрации сервисов в собственных Awake.

Регистрация — по интерфейсу (Register<ISceneService>(sceneService)),
не по конкретному классу. Это позволяет подменять реализацию в тестах.

Get<T>() бросает исключение, если сервиса нет — fail fast, чтобы
пропущенная регистрация падала сразу, а не превращалась в NullReference позже.

Потребитель вызывает Get один раз (в конструкторе/Awake/Start) и
кэширует ссылку в поле. Не вызывать Get в Update/FixedUpdate.

Clear() вызывается при полной перезагрузке приложения/тестах, не в обычном
геймплее. Сервисы живут дольше сцены.

4. Singleton и PersistentSingleton
csharp
// Nuthouse.Core
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    /// Жить ли между сценами (DontDestroyOnLoad).
    protected virtual bool PersistAcrossScenes => false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);          // дубликат убивается
            return;
        }
        Instance = (T)this;
        if (PersistAcrossScenes) DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    /// Переопределяется вместо Awake.
    protected virtual void OnAwake() { }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

// PersistentSingleton = Singleton с PersistAcrossScenes == true по умолчанию
public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
{
    protected override bool PersistAcrossScenes => true;
}
Где уместны Singleton-объекты:

Объект	База	Зачем
GameBootstrap	PersistentSingleton	точка входа, регистрирует сервисы
Системные префабы (Prefabs/SystemPrefabs)	PersistentSingleton	живут между сценами
Где Singleton запрещён: игрок, враги, снаряды, стейты, presentation-компоненты,
геймплейные системы. У них много экземпляров, и они собираются через PlayerContext/
инспектор, а не через Instance.

5. GameBootstrap — точка сборки
csharp
// Nuthouse.Core
public sealed class GameBootstrap : PersistentSingleton<GameBootstrap>
{
    protected override void OnAwake()
    {
        // Единственное место регистрации сервисов.
        ServiceLocator.Register<ISceneService>(new SceneService());
        ServiceLocator.Register<IProgressService>(new ProgressService());
        // Новые сервисы добавляются сюда по мере реализации (см. §2 «потом»).
    }
}
GameBootstrap лежит в Prefabs/SystemPrefabs и присутствует в каждой
сцене (благодаря PersistentSingleton переживает переходы; дубликаты убиваются).

Если сервису нужен тик (будущий ITimeService), GameBootstrap вызывает его в
Update/FixedUpdate — сервисы не полагаются на собственный MonoBehaviour.Update
(аналогично правилу для подсистем игрока, 02-StateMachine.md §4).

6. Кто и что может читать из Service Locator
Потребитель	Может Get	Не может Get
Меню / UI	любые сервисы	геймплейные системы
World / Interact	любые сервисы	—
Presentation	любые сервисы	—
PlayerFacade	только сервисы приложения	—
StateMachine / Motor / Combat / States / Input	ничего	всё
Сервисы	другие сервисы (осторожно)	игрока, врагов, сцены напрямую
Ключевые запреты:

Ядро геймплея (StateMachine, CharacterMotor, PlayerCombat, стейты, InputReader,
InputBuffer) не обращается к ServiceLocator. Их зависимости приходят через
PlayerContext (конструкторы) — это сохраняет тестируемость и граф-дерево (01 §10).

Сервис не хранит ссылки на игрока/врагов и не принимает геймплейных решений.
Сервис — инфраструктура (время, сцены, сохранение), не геймплей.

Service Locator не используется как шина событий и не заменяет раздельные
каналы 06. События по-прежнему публикуются геймплеем и слушаются презентацией.

7. Соотношение с PlayerContext
PlayerContext остаётся контейнером зависимостей игрока (01 §3). Он создаётся
в PlayerFacade.Awake() и передаётся в подсистемы.

Если подсистеме игрока нужен сервис приложения (например, будущему UI- following
нужен ITimeService), она получает его через ServiceLocator.Get, но только на
уровне presentation/фасада, не внутри стейтов и мотора.

Service Locator и PlayerContext — разные уровни: первый для приложения, второй для
конкретного игрока. Они не дублируют и не заменяют друг друга.

8. Антипаттерны
Регистрировать сервис вне GameBootstrap (саморегистрация в Awake).

ServiceLocator.Get в Update/FixedUpdate без кэширования.

Сервис как геймплейная сущность (Player.Instance через Singleton).

Singleton на игрока/врага/стейт.

Использовать Service Locator для передачи данных между геймплейными системами
(это должен делать PlayerContext/события).

Глобальный статический класс-«магнит» с кучей полей вместо типизированных сервисов.

Строить заготовки сервисов из раздела «потом» заранее.

9. Расширение (добавление нового сервиса)
Определить интерфейс IXService и реализацию XService (Scripts/Services/,
неймспейс Nuthouse.Services).

Зарегистрировать в GameBootstrap.OnAwake():
ServiceLocator.Register<IXService>(new XService());

Потребители: private readonly IXService x = ServiceLocator.Get<IXService>();
(один раз, в конструкторе/Awake).

Если сервису нужен тик — добавить вызов в GameBootstrap.Update.

Обновить таблицу сервисов в §2 этого документа.

Ядро и другие сервисы при этом не меняются. Пошагово — ForDeveloper/01-HowToAdd.md §9.

10. Чек-лист готовности
□ ServiceLocator, Singleton<T>, PersistentSingleton<T> реализованы (Nuthouse.Core).
□ GameBootstrap : PersistentSingleton существует и регистрирует сервисы в OnAwake.
□ ISceneService + IProgressService зарегистрированы (база — текущий GameProgress).
□ Ни одна геймплейная система (стейты/мотор/комбат/ввод) не вызывает ServiceLocator.
□ Get вызывается один раз и кэшируется; нет Get в Update.
□ Нет сервисов-заготовок из раздела «потом».