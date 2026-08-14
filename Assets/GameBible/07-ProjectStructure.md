07-ProjectStructure — папки и неймспейсы
Версия: 2.2 · Зависит от: 01-Architecture
Изменения 2.2: исправлен комментарий для Scripts/Player/Combat.

1. Структура Assets
text
Assets/
├── GameBible/               # эта документация (в код не компилируется)
│   ├── ForDeveloper/
│   └── FutureComponents/
├── Data/                    # ScriptableObjects-конфиги (по подпапкам)
│   ├── Movement/            #   MovementConfig
│   ├── Input/               #   InputConfig
│   ├── Attacks/             #   AttackDefinition-ассеты (Attack1, Attack2, AxeThrow...)
│   └── Characters/          #   HealthConfig, StaminaConfig, конфиги врагов
├── Input/                   # PlayerInputActions.inputactions + сгенерированный класс
├── Scripts/
│   ├── Core/
│   │   ├── StateMachine/    # IState, StateBase, PlayerStateMachine, StateRegistry
│   │   ├── Events/          # MovementEvents, CombatEvents, DamageEvents, GameEvents
│   │   ├── ServiceLocator/  # ServiceLocator, Singleton<T>, PersistentSingleton<T>,
│   │   │                    # GameBootstrap (08-ServiceLocator)
│   │   └── Extensions/      # мелкие помощники (таймеры, математика)
│   ├── Services/            # ISceneService/SceneService, IProgressService/ProgressService
│   │                        # (база — текущий GameProgress); новые сервисы — сюда же
│   ├── Input/               # InputReader, InputBuffer, BufferedCommand, InputConfig
│   ├── Player/
│   │   ├── States/          # IdleState, RunState, ... AxeThrowState, HurtState
│   │   ├── Movement/        # CharacterMotor, GroundDetector, FacingController, MovementConfig
│   │   ├── Combat/          # PlayerCombat (боезапас топоров, точка входа в атаки)
│   │   ├── Interact/        # InteractController (подсистема игрока, 05 §7.5)
│   │   └── Presentation/    # PlayerAnimatorDriver, MovementAudioListener, CombatAudioListener
│   ├── Combat/              # общее для игрока и врагов
│   │   ├── Hitbox.cs, Hurtbox.cs, DamageInfo.cs, IDamageable.cs, HurtRequest.cs
│   │   ├── AttackDefinition.cs, AttackRunner.cs
│   │   └── Projectiles/     # AxeProjectile + будущие снаряды
│   ├── Enemies/             # MeleeEnemy и т.д. (пока тонкий слой поверх Combat/)
│   ├── Health/              # Health, Stamina, DamageReceiver, HealthConfig, StaminaConfig
│   ├── Presentation/
│   │   ├── Camera/          # Cinemachine-хелперы, impulse-источники
│   │   └── Effects/         # VFX-спавнеры, слушатели событий
│   ├── World/               # интеракции, двери, лестницы, чекпоинты
│   │   └── Interact/        # Interactable (база), AxeInteractable, InteractableObject-миграция
│   ├── UI/                  # StaminaBar, HUD, инвентарь-иконки топоров
│   └── Menus/               # MainMenu, PauseMenu, SettingsMenu
├── Prefabs/
│   ├── Player/              # Player.prefab
│   ├── Enemies/
│   ├── Projectiles/         # AxeProjectile.prefab
│   ├── SystemPrefabs/       # GameBootstrap, системные объекты (08 §5)
│   └── ... (двери, препятствия — как сейчас)
├── Animation/Player/        # клипы + Player.controller
├── Scenes/                  # MainMenu, SampleScene, SecondLocation
├── Sound/                   # клипы (как сейчас)
└── Sprites/                 # (как сейчас)
Миграция: папка Scripts/Active objects → Scripts/World (пробел в имени убрать);
GameProgress из Scripts/Menus → разбивается на SceneService/ProgressService
в Scripts/Services (позже, до этого работает как есть); старый CharacterController2D
удаляется после замены мотором. Отдельного Assets/Scripts/SceneManager в проекте нет —
загрузка сцен живёт в GameProgress и переезжает в SceneService.

2. Неймспейсы
Папка	Неймспейс
Scripts/Core	Nuthouse.Core
Scripts/Services	Nuthouse.Services
Scripts/Input	Nuthouse.Input
Scripts/Player	Nuthouse.Player (+ .States, .Movement, .Combat, .Interact, .Presentation)
Scripts/Combat	Nuthouse.Combat (+ .Projectiles)
Scripts/Enemies	Nuthouse.Enemies
Scripts/Health	Nuthouse.Health
Scripts/Presentation	Nuthouse.Presentation
Scripts/World	Nuthouse.World
Scripts/UI, Menus	Nuthouse.UI
Правила:

Один неймспейс на корневую папку; вложенные папки — вложенные неймспейсы.

Глобальных (без неймспейса) классов не должно остаться — текущие скрипты мигрируют
с добавлением неймспейса.

using Nuthouse.* в инспекторных полях Unity работает без проблем; если сериализация
ругается на смену неймспейса — использовать [MovedFrom] или перебиндить поля в инспекторе
(миграция префаба Player — отдельный шаг, см. ForDeveloper/03).

3. Assembly Definitions (опционально)
Пока один дефолтный assembly достаточно. Если время компиляции станет проблемой:
Nuthouse.Core, Nuthouse.Gameplay (Player+Combat+Enemies+Health), Nuthouse.Presentation.
Сейчас asmdef не создаём — не оверинжинирим.

4. Сцены и сборка
Player.prefab — единственный источник игрока; в сценах только экземпляр префаба.

GameBootstrap (PersistentSingleton, 08 §5) — в Prefabs/SystemPrefabs, присутствует
в каждой сцене; регистрирует сервисы. Прочие системные объекты — там же.

Игровые конфиги (SO) подключаются сериализованными полями на PlayerFacade — никаких
Resources.Load для геймплея. Сервисы приложения — через ServiceLocator, не Resources.

