ForDeveloper/03-CurrentCodeMap — текущий код и план миграции
(Документ не требует правок; приводится для полноты.)

Инвентаризация прототипа (что есть) и карта перевода на целевую архитектуру
(что станет). Миграция идёт по этапам 00-README.md; старые классы живут до замены,
чтобы игра оставалась запускаемой.

1. Игрок (Assets/Scripts/Player)
Сейчас	Проблемы	Станет
CharacterController2D (Standard Assets-style)	универсальный Move/Crouch, flip через scale, OnLandEvent	разбирается на CharacterMotor + GroundDetector + FacingController (04). Класс удалить после этапа 3
PlayerMovement	legacy Input.GetAxisRaw, скорость в «попугаях» (300/500), прямой Animator.SetBool/SetFloat, звук шагов внутри, корутина StopRun	стейты Idle/Run/Crouch (02) + MovementConfig (04); звук → MovementAudioListener (06). Класс удалить после этапа 3
MeleeCombat	Input.GetMouseButtonDown, OverlapCircleAll на клике, прямой SetTrigger("Fight"), кулдаун-корутинa, boost-механика	AttackState + AttackRunner + AttackDefinition + Hitbox/Hurtbox (05). Механику boost сохранить как поле атаки/бафф при переносе. Класс удалить после этапа 4
PlayerHealth	HP + стамина + пауза в одном классе; прямой SetTrigger("Hit") и camAnimator	Health + Stamina (Scripts/Health), пауза → PauseMenu+GameEvents, анимация боли → AnimatorDriver по DamageEvents (06)
Связки, которые должны исчезнуть: MeleeCombat → PlayerMovement.StopRun
(заменяется lockMovement в AttackState), PlayerMovement → PlayerHealth.CanRun
(стейт читает Ctx.Stamina), все Animator из не-Presentation классов.

2. Враги (Assets/Scripts/Enemy)
Сейчас	Станет
EnemyHealth (TakeDamageEnemy)	EnemyHealth : DamageReceiver (реализует IDamageable), HP из SO-конфига
MeleeEnemy (поиск игрока по тегу, атака по дистанции)	тот же MonoBehaviour-мозг, но урон через Hitbox с DamageInfo; поиск игрока — ссылка в инспекторе/через World-сервис вместо FindGameObjectWithTag
EnemyMovement (патруль)	остаётся на этапе врагов; позже заменяется AI из FutureComponents
3. Мир и интеракции (Assets/Scripts/Active objects → Scripts/World)
Сейчас	Станет
InteractableObject	база для Interactable (рецепт ForDeveloper/01 §8)
StairController	мигрирует в World; позже — стейт Climb (этап после топоров)
QuickTrigger, TextingObject, NPCDialogue, test.cs	переносятся в World с неймспейсом; test.cs удалить
CutsceneManager (заглушка 12 строк)	оставить заглушкой до этапа катсцен
4. Меню и сервисы
Сейчас	Станет
PauseMenu (Escape читается в PlayerHealth!)	Escape → InputReader (action Pause) → GameEvents.PauseRequested → PauseMenu. Time.timeScale допустим до появления TimeService
GameProgress (ручной Singleton: Instance + DontDestroyOnLoad, сцены + PlayerPrefs)	этап 0: базовые классы Singleton/PersistentSingleton + ServiceLocator; логика GameProgress разбирается на SceneService (порядок/загрузка сцен) и ProgressService (PlayerPrefs-прогресс), регистрируются GameBootstrap (08). До этапа 0 работает как есть
MainMenu, SettingsMenu	без изменений на текущих этапах; потребители сервисов — через ServiceLocator.Get, не через GameProgress.Instance
UI: StaminaBar(Follow), UIFollowNPC, DisplayText, CustomDropdown	остаются; StaminaBar переводится на Stamina.Normalized/событие вместо полей PlayerHealth
Assets/InfoPanel.cs (одиночный скрипт в корне Assets)	перенести в Scripts/UI (+ неймспейс) либо удалить при ближайших правках UI
5. Инфраструктура проекта
Элемент	Статус
Unity 6000.3.20f1, URP 17, Cinemachine 2.10, Input System 1.19	ок, ничего не добавлять
activeInputHandler = Both	оставляем до полного удаления legacy Input.*, потом можно InputSystemOnly
Анимации: Idle/Walk/Run/Crouch/Fight/Hit	переиспользуются; Fight разбивается на Attack1..N; Jump/Fall-клипы — по мере появления арта
Assets/Animation/Camera Shakes + camAnimator	заменяются Cinemachine Impulse (06 §4) на этапе презентации
6. Порядок миграции (шаги, игра запускается после каждого)
Сервисный каркас: ServiceLocator, Singleton<T>/PersistentSingleton<T>,
GameBootstrap в Prefabs/SystemPrefabs; SceneService + ProgressService из текущего
GameProgress (старый класс пока остаётся, потребители переключаются постепенно). Игра
работает как раньше.

Каркас без смены поведения: папки/неймспейсы по 07; PlayerFacade + PlayerContext

пустой PlayerStateMachine (один LegacyState, который вызывает старый
CharacterController2D.Move как сейчас). Игра работает как раньше.

Ввод: ассет PlayerInputActions, InputReader, InputBuffer; LegacyState читает
ввод из ридера вместо Input.GetAxisRaw. Удалить legacy-вызовы в PlayerMovement.

Движение: CharacterMotor, GroundDetector, стейты Idle/Run/Crouch/Jump/Fall.
Удалить CharacterController2D, PlayerMovement.

Здоровье/стамина: Health, Stamina; пауза через события. PlayerHealth удалить.

Ближний бой: Hitbox/Hurtbox, AttackDefinition, AttackRunner, AttackState.
MeleeCombat удалить.

Бросок топора: AxeThrowDefinition, AxeThrowState, AxeProjectile, подбор,
UI боезапаса.

Враги: EnemyHealth на IDamageable, атака врага через хитбокс.

Презентация: AnimatorDriver полностью, аудио-слушатели, Cinemachine impulse.

Финал сервисов: удалить GameProgress, все потребители — на ServiceLocator.

Каждый шаг = отдельный коммит. Если шаг ломал поведение — чинить в рамках шага,
не откатывая архитектуру.

7. Известные грабли
Player.controller использует параметры Speed, Crouching, Running, Fight, Hit —
менять по таблице 06-EventsAndPresentation.md §2 синхронно с кодом.

Префаб Player используется в двух сценах (SampleScene, SecondLocation) — правки префаба
проверяем в обеих.

Rigidbody2D.velocity в Unity 6 не существует → только linearVelocity.

Старые скорости 300/500 ≠ юниты/с; эквивалент ≈ 6/10 юнит/с — см. MovementConfig (04 §6).