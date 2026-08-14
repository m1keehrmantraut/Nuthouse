# FutureComponents/AI — Blackboard + Utility AI

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Текущие враги — простые MonoBehaviour (патруль/ближний бой).
Полноценный AI внедряется после броска топора, когда появятся 2+ типа врагов с разным поведением.

---

## 1. Архитектура врага

```text
Enemy (GameObject)
├── AIController          — тикает Brain, владеет Blackboard
├── Blackboard            — общие данные (цель, память, таймеры)
├── UtilityBrain          — выбор действия по оценкам
├── Sensors               — «органы чувств», пишут в Blackboard
├── EnemyMotor            — движение (общий CharacterMotor или упрощённый)
├── EnemyCombat           — исполнение атак через общие AttackRunner/Hitbox
└── Health (общий класс)
```

Принцип тот же, что у игрока: AI **выбирает намерение**, исполняют его те же системы
(Motor, AttackRunner). Никаких прямых вызовов Animator из AI.

## 2. Blackboard

Централизованное хранилище знаний врага. НЕ синглтон — инстанс на врага.

```csharp
public sealed class Blackboard
{
    public Vector2 SelfPosition;
    public Transform Target;              // текущая цель (игрок)
    public float DistanceToTarget;
    public bool CanSeeTarget;             // от LineOfSightSensor
    public float TimeSinceTargetSeen;
    public float Aggro;                   // накопленное раздражение
    public Vector2 LastKnownTargetPos;

    public T Get<T>(BlackboardKey key);   // расширяемые ключи при нужде
    public void Set<T>(BlackboardKey key, T value);
}
```

Правила:

- Пишут в Blackboard только Sensors и система боя (факты: «получил урон»).
- Читают — Brain и действия.
- Blackboard не содержит логики принятия решений, только данные.

## 3. Sensors

Отдельные компоненты, тикаются в `AIController.UpdateSense()`:

| Сенсор | Что пишет |
|--------|-----------|
| `LineOfSightSensor` | `CanSeeTarget` (raycast), `LastKnownTargetPos` |
| `ProximitySensor` | `DistanceToTarget` (OverlapCircle) |
| `DamageSensor` | подписан на `DamageEvents.DamageTaken` своего Health → `Aggro += X`, цель = атакующий |
| `HearingSensor` | (будущее) реакция на звук шагов/броска топора |

## 4. Utility AI (оценки)

Каждое возможное действие — `UtilityAction` с функцией оценки от 0 до 1:

```csharp
public abstract class UtilityAction : ScriptableObject
{
    public float Cooldown;                 // общий кулдаун действия
    public abstract float Score(Blackboard bb, EnemyContext ctx);
    public abstract bool CanExecute(Blackboard bb, EnemyContext ctx);
    public abstract void Execute(AIController owner);
}
```

Примеры:

- **Approach** — score растёт с дистанцией, если цель видна.
- **MeleeAttack** — score = 1 если в радиусе атаки и кулдаун готов.
- **Retreat** — score растёт при низком HP врага.
- **Idle/Patrol** — базовый score 0.1.

`UtilityBrain.Tick`: выбирает действие с максимальным score, исполняет.
Решения — только через Blackboard, никакого `GameObject.Find("Player")`.

## 5. Поведение и стейты

Действия Utility AI транслируются в состояние врага:

- Движение → `EnemyMotor.MoveHorizontal(...)` (тот же `CharacterMotor`).
- Атака → `EnemyCombat.StartAttack(attackDef)` → тот же `AttackRunner`.
- Получение урона → `HurtState` врага (прерывает действие).

Враг использует упрощённую FSM: `Idle / Patrol / Chase / Attack / Hurt / Dead`.

## 6. Точки внедрения (когда дойдём)

- `Scripts/Enemies/AI/` — Blackboard, Sensors, UtilityBrain.
- Общие `CharacterMotor`, `AttackRunner`, `Health`, `Hurtbox` переиспользуются без изменений.
- Игрок как цель: `DamageEvents.DamageTaken` даёт ссылку на атакующего.

## 7. Антипаттерны

- Behavior Tree с сотней узлов — Utility проще для 2D-экшена.
- Blackboard как глобальный синглтон на всех врагов.
- AI напрямую вызывает Animator или Combat.
