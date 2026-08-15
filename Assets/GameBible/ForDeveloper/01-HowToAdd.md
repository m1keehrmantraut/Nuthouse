ForDeveloper/01-HowToAdd — как добавлять компоненты и механики
Версия: 1.1 (исправлена)
Целевая аудитория: разработчик (или LLM), который добавляет функционал в готовый каркас
(этапы 1–3 из 00-README.md выполнены: есть PlayerFacade, StateMachine, InputBuffer, Motor).

Каждый рецепт = какие классы писать, где класть, куда подключать в инспекторе/коде.
Если задача не похожа ни на один рецепт — сначала прочитать 01-Architecture.md §5.

1. Сборка префаба Player (базовая комплектация)
GameObject Player (тег Player, слои: тело — Player, отдельный ребёнок с коллайдером-хуртбоксом — слой Hurtbox):

text
Player (Rigidbody2D Dynamic, freezeRotation Z, gravityScale 0; CapsuleCollider2D — тело)
├── Sprite (SpriteRenderer, аниматор на родителе или здесь)
├── GroundCheck (пустой Transform под ногами)
├── HandAnchor (пустой Transform — точка спавна топора)
├── HitboxHolder (пустой; сюда AttackRunner создаёт/кладёт Hitbox-объекты)
└── Hurtbox (Collider2D isTrigger, слой Hurtbox, компонент Hurtbox.cs → PlayerFacade.Health)
На корне компоненты: PlayerFacade (в него перетащить: MovementConfig, InputConfig,
HealthConfig, StaminaConfig, AttackDefinition, AxeThrowDefinition, groundLayer,
interactableLayer, groundCheck, hitboxRoot, hurtbox), PlayerAnimatorDriver,
MovementAudioListener, CombatAudioListener. Всё остальное (InputReader, Motor, стейты...)
создаётся кодом в PlayerFacade.Awake() — в инспектор ничего больше не тащить.
Привязка урона: Hurtbox игрока получает Health runtime-вызовом Hurtbox.Bind(ctx.Health)
в фасаде (05 §5); у врагов — [SerializeField] DamageReceiver в инспекторе.

Камера: CinemachineVirtualCamera в сцене, Follow = Player. Слои коллизий в
Project Settings → Physics 2D: матрица Hitbox × Hurtbox включена, Hitbox × Hitbox
выключена. Слои: Ground, Player, Hurtbox, Hitbox, Enemy, Projectile, Interactable.

2. Новое движение / стейт
Задача: «добавить Wall Slide / рывок / новое состояние».

Класс: Scripts/Player/States/XState.cs, public sealed class XState : StateBase
(или наследник GroundedState/AirborneState, если подходит по иерархии).
Реализовать Enter/Exit/Tick/FixedTick. Движение — только через Ctx.Motor.

Регистрация: в PlayerFacade.RegisterStates() рядом с остальными:
ctx.States.Register(new XState(ctx));

Вход в стейт: добавить условие в Tick того стейта, откуда переход
(обычно Ctx.Buffer.TryConsume(...) или данные Ctx.Ground). Обновить таблицу
переходов в 02-StateMachine.md §6 — документация и код должны совпадать.

Выход: условие в Tick нового стейта → To<IdleState>() и т.п.

Анимация: параметр в Player.controller + обработка в PlayerAnimatorDriver
(подписка на StateChanged или событие).

Данные: все числа — в MovementConfig (новое поле в SO), не в стейте.

Проверка: стейт достижим вводом, выход из него гарантирован (нет застреваний),
Exit ничего не оставляет включённым.

3. Новая кнопка / действие
Карта Gameplay строится в коде (03 §5): gameplay.AddAction("X", тип, первый биндинг),
доп. клавиши — action.AddBinding(...) (НЕ повторный AddAction с тем же именем — исключение
дубликата убивает PlayerFacade.Awake).

биндинги клавиатура/геймпад.

enum InputAction → новое значение; InputConfig → поле TTL + ветка в GetTtl.

InputReader.BindCallbacks → строка map["X"].performed += _ => ActionPressed?.Invoke(InputAction.X);
(для удержаний — started/canceled в свойство). Иногда фаза решается по отпусканию клавиши,
а не по performed — например hold-charge тяжёлой атаки: map["Attack"].started/canceled +
hold ≥ heavyAttackHold → Invoke(HeavyAttack) иначе Attack (03 §5).

Потребитель: стейт (Buffer.TryConsume) либо система вне FSM (Interact, Pause).

Если действие требует нового поведения — см. рецепт 2 (стейт) или 4 (атака).

4. Новая атака ближнего боя
Данные: Assets/Data/Attacks/ → Create → Nuthouse → Attack Definition.
Заполнить фазы/урон/хитбокс/animTrigger. Для комбо — связать nextInCombo.

Вход в атаку: первая атака цепочки подставляется в поле
[SerializeField] AttackDefinition primaryAttack на PlayerFacade (канон 02 §4) —
перетащить ассет в инспекторе. Остальные звенья цепочки — через nextInCombo,
регистрировать их нигде не нужно.

Анимация: добавить trigger с именем из animTrigger в Player.controller

переходы из боевых стейтов.

Звук/VFX: при необходимости подписаться в слушателях на CombatEvents.AttackStarted/AttackHit.

Код не меняется вообще, если атака укладывается в схему фаз. Нужна особая логика
(множественные хитбоксы, зарядка) — подкласс AttackRunner-обвязки в AttackState,
но сначала обсудить: возможно, это уже уровень Combo Graph (Future).

5. Новый метательный снаряд (по образцу топора)
SO: подкласс или аналог AxeThrowDefinition (если поведение 1-в-1 — сам ассет).

Префаб: Prefabs/Projectiles/X.prefab: Rigidbody2D, Collider2D (у снаряда это
Hitbox-триггер + Hitbox.cs), спрайт-ребёнок для визуального вращения, скрипт
XProjectile (наследовать AxeProjectile либо написать по его образцу).

Контакты: переопределить HandleHit (рикошет, взрыв, прилипание — по §7.3 05-Combat).

Бросок: стейт броска (клон AxeThrowState) или существующий, если снарядов
несколько: PlayerCombat.SpawnProjectile(def) — точка спавна HandAnchor,
направление Facing.Direction.

Подбор: на застрявшем снаряде компонент-наследник Interactable (Scripts/World/Interact),
в Interact(PlayerFacade player) вызывает публичный API фасада
(player.ReturnAxe(1) → внутри PlayerCombat.ReturnAxe) и уничтожает/пулит снаряд
(канон — 05 §7.5). Внешний мир не трогает PlayerCombat напрямую.

6. Новый враг
Префаб: спрайт, Collider2D-тело (слой Enemy), ребёнок Hurtbox (isTrigger, слой
Hurtbox, Hurtbox.cs → ссылка на EnemyHealth).

EnemyHealth : DamageReceiver (базовый класс в Scripts/Health): HP из SO-конфига,
Died → событие DamageEvents.Died(gameObject).

Поведение: пока простая логика в MonoBehaviour по образцу текущего MeleeEnemy
(полноценный AI — FutureComponents/AI-Blackboard-UtilityAI.md). Атака врага — его
Hitbox с DamageInfo (через AttackRunner или таймерный триггер).

Смерть/дроп: подписки presentation; геймплейные последствия — в обработчике уровня
(World), не в самом враге.

7. Реакция презентации (звук / тряска / VFX)
Выбрать канал (CombatEvents, DamageEvents, MovementEvents); нужного события
нет — добавить его в канал и публикацию в геймплее (одно поле event + один Publish-вызов).

Слушатель: существующий (CombatAudioListener, ...) или новый MonoBehaviour в
Presentation/. Подписка в OnEnable, отписка в OnDisable.

Слушатель не имеет ссылок на геймплейные классы (максимум — читать данные из аргументов
события).

8. Взаимодействие с миром (Interact)
Канон: 05-Combat.md §7.5 (Interactable + InteractController).

Объект: наследник Interactable (Scripts/World/Interact) — задать radius,
при необходимости управлять IsAvailable, реализовать Interact(PlayerFacade player).

Детекцию делает InteractController (подсистема игрока, тикается фасадом):
потребляет InputAction.Interact из буфера и выбирает ближайший доступный объект.
Конструктор принимает LayerMask для слоя Interactable.
Сам объект не ищет игрока и не слушает ввод.

Эффект взаимодействия — только через публичный API фасада (player.ReturnAxe(...),
player.SetControlEnabled(false) для диалога и т.п.), не через внутренние компоненты.

9. Новый сервис приложения
Канон: 08-ServiceLocator.md.

Интерфейс + реализация: Scripts/Services/IXService.cs и XService.cs
(неймспейс Nuthouse.Services). Сервис — чистый класс, если ему не нужен
MonoBehaviour; иначе — Singleton<XService>/PersistentSingleton<XService>.

Регистрация: в GameBootstrap.OnAwake() одной строкой
ServiceLocator.Register<IXService>(new XService()); — единственное место регистрации.

Потребление: private readonly IXService x = ServiceLocator.Get<IXService>();
в конструкторе/Awake потребителя (один раз, кэшировать). Если сервису нужен тик —
добавить вызов в GameBootstrap.Update.

Обновить таблицу сервисов в 08-ServiceLocator.md §2.

10. Чего НЕ делать
Не писать FindFirstObjectByType/FindGameObjectWithTag в геймплее — зависимости через
контекст/инспектор/события.

Не создавать Singleton/ServiceLocator для геймплея (игрок, враги, стейты, комбат).
Синглтоны и локатор — только системные сервисы приложения (08-ServiceLocator.md).

Не регистрировать сервис вне GameBootstrap (никакой саморегистрации в Awake).

Не вызывать ServiceLocator.Get в Update/FixedUpdate без кэширования.

Не добавлять поля «на будущее» и системы из FutureComponents/.

Не править PlayerStateMachine/CharacterMotor ради одной новой механики — механика
добавляется стейтом/конфигом/классом.

Не дублировать числа из SO в [SerializeField] компонентов.

11. Дефинишн готовности любой новой фичи
□ Работает на SO-конфигах, без магических чисел.
□ Нет новых прямых ссылок между системами сверх 01-Architecture.md §5.
□ Presentation реагирует событиями, а не прямыми вызовами.
□ Сервисы (если появились) зарегистрированы в GameBootstrap, ядро геймплея их не читает.
□ Обновлён соответствующий документ GameBible (таблица переходов / список событий / сервисов).
□ Скомпилировано без warnings; проверено в сцене вручную.