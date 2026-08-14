# FutureComponents/PassiveAbilities — пассивные способности

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Не реализовывать и не строить заготовок, пока система
не понадобится геймдизайну (после броска топора и врагов).

---

## 1. Что это

Пассивки — постоянные или условные модификаторы персонажа без активного действия:
«+15% скорость бега», «крит при полном HP», «второй шанс раз в бой», «топор возвращается
после 3 попаданий». Не являются стейтами и не занимают кнопку.

## 2. Модель: модификаторы + триггеры (без «способности на всё»)

Два вида пассивок, оба — данные + маленький обработчик:

### 2.1. Модификаторы статов

```csharp
[CreateAssetMenu(menuName = "Nuthouse/Future/Passive Modifier")]
public sealed class PassiveModifierDef : ScriptableObject
{
    public StatTarget target;        // MoveSpeed, RunSpeed, JumpForce, Damage, MaxHealth...
    public ModifyOp op;              // Add, Multiply
    public float value;
    public PassiveConditionDef condition;  // null = всегда
}
```

Применяются через **StatBlock** — обёртку над числом, которую читают мотор/урон:

```csharp
public sealed class StatBlock
{
    private float baseValue;
    private readonly List<PassiveModifierDef> modifiers = new();
    public float Value { get; private set; }
    public void Recalculate();       // base + все модификаторы, кэш до изменения списка
}
```

Точки внедрения в текущей архитектуре: `MovementConfig`-значения читаются через StatBlock
в `CharacterMotor` (walkSpeed/runSpeed/jumpForce), урон — в `DamageInfo` на этапе сборки в
`AttackRunner`. То есть пассивки не лезут в стейты — они меняют числа, которые стейты уже читают.

### 2.2. Триггерные пассивки

Реагируют на события каналов (06): «при DamageTaken с шансом 20% дать неуязвимость».
Класс `PassiveTriggerListener` подписывается на канал, проверяет условие, применяет эффект
через существующие API (`Health.GrantInvulnerability`, `Motor.ApplyImpulse`, ...).

```csharp
public sealed class PassiveTriggerDef : ScriptableObject
{
    public EventSource source;           // OnDamageTaken, OnKill, OnAxePickup...
    public float chance = 1f;
    public EffectDef effect;             // что применить (SO)
    public float cooldown;
}
```

## 3. Хранилище и экипировка

- `PassiveInventory` на игроке (рядом с `PlayerCombat.AxesCount`): список активных
  `PassiveDef`, лимит слотов из конфига.
- Сохранение — вместе с прогрессом (Save system, этап будущий).

## 4. Правила внедрения (когда дойдём)

- Никакого нового «PassiveScheduler». Модификаторы — пересчёт StatBlock; триггеры — слушатели.
- Пассивки не меняют StateMachine (исключение — пассивка, дающая новый стейт: тогда она
  регистрирует стейт через обычный рецепт ForDeveloper/01 §2).
- Баланс — только в SO.

## 5. Что подготовить заранее (сейчас)

Ничего, кроме дисциплины: числа в `CharacterMotor`/`AttackRunner` уже читаются из конфигов —
этого достаточно, StatBlock добавится точечно без переписывания.
