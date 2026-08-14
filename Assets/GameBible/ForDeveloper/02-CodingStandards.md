ForDeveloper/02-CodingStandards — стандарты кода
(Документ не требует правок; приводится для полноты.)

Краткий свод. Полные архитектурные правила — в 01-Architecture.md.

Язык и версии
C# 9+ (Unity 6), .NET Standard 2.1. Unity API: Rigidbody2D.linearVelocity (не velocity).

Input: только новый Input System (InputReader), legacy Input.* запрещён.

Классы
Один класс — одна ответственность. Класс > ~250 строк — повод разбить.

Чистые C#-классы предпочтительнее MonoBehaviour. MonoBehaviour только если нужно:
сериализуемое поле в инспекторе, коллайдер-события, жизненный цикл Unity.

Игровая логика в MonoBehaviour не должна зависеть от порядка Update — тикает владелец
(PlayerFacade), либо логика выносится в чистый класс.

Наследование — только для реальной иерархии (GroundedState → IdleState).
Поведение — композицией.

Именование
Сущность	Стиль	Пример
Классы, методы, свойства	PascalCase	CharacterMotor, TryConsume
Приватные поля	camelCase	float jumpForce; (без _ и без m_)
Сериализуемые поля	camelCase + [SerializeField] private	
Константы	PascalCase	const float MinTtl = 0.05f;
Интерфейсы	I-префикс	IDamageable
События	прошедшее время	Landed, AttackFinished
SO-ассеты	имя типа + суть	MovementConfig.asset, Attack_AxeThrow.asset
Данные
Параметры геймплея — в ScriptableObject. SO никогда не хранит runtime-состояние.

Runtime-состояние (HP, таймеры, боезапас) — только в классах-владельцах.

Дефолтные значения допустимы в SO; в коде магических чисел быть не должно.

События
Подписка/отписка парно: OnEnable/OnDisable.

Публикация — через Publish* методы каналов, не инвоком извне.

Аргументы событий — неизменяемые (struct/readonly).

Unity-специфика
[RequireComponent] для обязательных соседей; зависимости — [SerializeField] или
через конструктор/контекст.

Не использовать: FindFirstObjectByType, GameObject.Find, FindGameObjectWithTag,
Resources.Load для геймплейных данных.

Singleton (Singleton<T>/PersistentSingleton<T>) — только системные объекты
(GameBootstrap); геймплейные классы — без синглтонов. ServiceLocator.Get — только для
сервисов приложения и только вне геймплейного ядра (полные правила — 08-ServiceLocator.md).

Update/FixedUpdate в новых классах — только у фасадов (PlayerFacade, слушатели
presentation, GameBootstrap). Физика строго в FixedUpdate.

Корутины: допустимы в presentation (затухания, эффекты). В геймплее — таймеры в Tick
(предсказуемо для паузы и будущего time-control).

Слои/теги: игровые проверки — по LayerMask из конфига, не по строковым тегам.

Сервисы
Имя интерфейса — I<Суть>Service, реализации — <Суть>Service; папка Scripts/Services,
неймспейс Nuthouse.Services.

Регистрация — только в GameBootstrap.OnAwake(), по интерфейсу.

Get<T>() один раз в конструкторе/Awake, результат — в поле; не в Update.

Сервис не хранит ссылки на игрока/врагов и не принимает геймплейных решений.

Организация
Неймспейсы — по 07-ProjectStructure.md §2.

Новый файл = новый класс (кроме маленьких struct/enum рядом с владельцем, например
DamageInfo рядом с IDamageable).

#region не использовать; большие классы — разбивать.

Комментарии и документация
Комментарии — только там, где код неочевиден (почему, а не что).

Публичные API систем — XML-doc в одну строку достаточно.

Каждая новая механика = обновление GameBible (таблицы/списки в соответствующем файле).

Тестирование
Ручные проверки в сцене обязательны для каждой фичи (чек-листы в документах систем).

Unit-тесты (Test Framework установлен): для чистых классов — InputBuffer, AttackRunner,
StateMachine-переходы. Минимум — покрыть буфер и фазы атаки.