# 00-README — GameBible проекта Nuthouse

**Версия:** 3.0
**Проект:** Nuthouse — 2D платформер, Unity 6000.3.x (URP), стадия ранней разработки (2 уровня).
**Назначение папки:** единственный источник архитектурной правды для людей и LLM.

Изменения 3.0: в архитектуру введён сервисный слой — **Service Locator + Singletons**
(`08-ServiceLocator.md`); исправлены противоречия сигнатур между документами 01–07
(PlayerContext, HurtRequest→PendingHurt, AnimatorDriver, coyote, дедупликация урона,
InteractController).

---

## Что это такое

GameBible описывает **целевую архитектуру** игры. Текущий код в `Assets/Scripts` — прототип
(legacy Input Manager, прямые вызовы Animator, связанные компоненты). Он должен быть переписан
по этой документации. Карта "старый класс → новая система" и порядок миграции:
`ForDeveloper/03-CurrentCodeMap.md`.

Документы написаны так, чтобы LLM могла по ним написать рабочую реализацию без дополнительных
вопросов: везде, где важно, даны сигнатуры классов и правил подключения.

## Порядок чтения

| # | Файл | О чём |
|---|------|-------|
| 1 | `01-Architecture.md` | Слои, правила зависимостей, порядок обновления, принципы |
| 2 | `02-StateMachine.md` | Иерархическая стейт-машина игрока (ядро геймплея) |
| 3 | `03-InputAndBuffer.md` | Input Reader, команды, Input Buffer |
| 4 | `04-Movement.md` | Мотор, физика, прыжок, параметры движения |
| 5 | `05-Combat.md` | Атаки, фазы, хитбоксы, урон, **бросок топора** |
| 6 | `06-EventsAndPresentation.md` | Раздельные каналы событий, анимация, звук, камера |
| 7 | `07-ProjectStructure.md` | Папки, неймспейсы, где что лежит |
| 8 | `08-ServiceLocator.md` | **Service Locator, Singletons, GameBootstrap, сервисы** |
| — | `ForDeveloper/` | Прикладные инструкции: как добавлять компоненты |
| — | `FutureComponents/` | Продуманные, но отложенные системы |

## Ключевые архитектурные решения (кратко)

1. **Ядро геймплея — иерархическая State Machine + Input Buffer.** Не единый Ability Scheduler,
   не гигантская FSM на всё. Стейт-машина управляет локомоцией и боевыми состояниями игрока;
   Input Buffer делает управление отзывчивым.
2. **Нет единой шины событий.** Собыства разбиты на маленькие статические каналы по доменам
   (`MovementEvents`, `CombatEvents`, `DamageEvents`, `GameEvents`). Каждый канал — один класс,
   никаких глобальных диспетчеров и реестров подписок.
3. **Данные — в ScriptableObjects.** Параметры движения, атак, здоровья настраиваются ассетами,
   не кодом. SO не хранят runtime-состояние.
4. **Физика только через CharacterMotor.** Никто кроме мотора не трогает Rigidbody2D игрока.
5. **Анимация только через AnimatorDriver.** Игровая логика не вызывает `Animator.SetTrigger`
   напрямую; драйвер подписан на события/состояния и сам рулит параметрами.
6. **Сервисы приложения — через Service Locator + Singletons.** Глобальные сервисы (Scene,
   Progress, позже Time/Audio/Save) регистрирует единственный `GameBootstrap` (`PersistentSingleton`)
   в `ServiceLocator`. **Ядро геймплея локатор не читает** — его зависимости приходят через
   `PlayerContext`. Singletons — только для системных объектов, не для игрока/врагов/стейтов.
   Подробности и запреты — `08-ServiceLocator.md`.
7. **Всё отложенное — в `FutureComponents/`.** Пассивки, полный Combo Graph, AI с Blackboard и
   Utility AI, Hit Stop / Slow Motion, мульти-девайсы, Replay — продуманы, но НЕ реализуются,
   пока не дойдём до них. Не строить под них инфраструктуру заранее.

## Текущий фокус разработки (дорожная карта)

| Этап | Задача | Документ |
|------|--------|----------|
| 0 | Каркас сервисов: `ServiceLocator`, `Singleton/PersistentSingleton`, `GameBootstrap`, `SceneService`, `ProgressService` | 08 |
| 1 | Каркас: `PlayerFacade`, `PlayerContext`, `StateMachine` | 02 |
| 2 | Ввод: Input System asset, `InputReader`, `InputBuffer` | 03 |
| 3 | Движение: `CharacterMotor`, `GroundDetector`, стейты Idle/Run/Jump/Fall/Crouch | 04 |
| 4 | Ближняя атака: `AttackDefinition`, `AttackRunner`, Hitbox/Hurtbox | 05 |
| 5 | **Бросок топора**: `AxeThrowState`, `AxeProjectile`, подбор | 05 |
| 6 | Враги на новой системе урона | 05, ForDeveloper |

Этап 0 — тонкий и делается за один заход (механизм + 2 сервиса из текущего `GameProgress`).
Этапы 1–3 — фундамент. Этапы 4–5 — главная цель текущего спринта. Всё остальное — потом.

## Правила для LLM

- Перед написанием кода прочитать документы из "порядка чтения" (минимум 01, 02, 03;
  при работе с сервисами/меню/временем — ещё 08).
- Не добавлять системы из `FutureComponents/` и не строить под них заготовок.
- Не отступать от правил зависимостей `01-Architecture.md §5`. Если кажется, что правило мешает —
  сначала перечитать `ForDeveloper/01-HowToAdd.md`, обычно есть штатный путь.
- `ServiceLocator.Get` — только для сервисов приложения и только вне геймплейного ядра;
  регистрировать сервисы можно лишь в `GameBootstrap` (`08-ServiceLocator.md`).
- Код писать под Unity 6: `Rigidbody2D.linearVelocity` (не `velocity`), Input System 1.19.
- Стандарты кода: `ForDeveloper/02-CodingStandards.md`.
