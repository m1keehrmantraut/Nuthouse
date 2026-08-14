# FutureComponents/ComboGraph — полный граф комбо

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Текущей цепочки через `AttackDefinition.nextInCombo`
(05-Combat §3) достаточно для этапа 4–5. Граф внедряется, когда появятся ветвления
(«после Attack2 можно и Attack3, и Throw»), контекстные комбо или комбо в воздухе.

---

## 1. Модель

Граф — SO-ассет на оружие/персонажа:

```text
ComboGraphAsset
└── nodes: List<ComboNodeDef>
        ├── attack: AttackDefinition
        ├── enterWindow: (min, max) — доля recovery, в которую узел достижим
        ├── transitions: List<ComboTransitionDef>
        │       ├── input: InputAction (Attack / Throw / Jump...)
        │       ├── condition: GameplayTagQuery | null   (Grounded, Moving...)
        │       └── target: ComboNodeDef
        └── resetDelay — время без ввода, после которого комбо сбрасывается
```

Стартовый узел — поле графа. Один граф на оружие; смена оружия = смена графа.

## 2. Runtime

```csharp
public sealed class ComboRunner
{
    public ComboNodeDef Current { get; }
    public void Start(ComboGraphAsset graph);
    public ComboNodeDef ResolveInput(InputAction action, PlayerContext ctx); // null = нет перехода
    public void Reset();
}
```

- `AttackState` вместо поля `nextInCombo` спрашивает `ComboRunner.ResolveInput(Attack, ctx)`
  в cancel-окне. Становится универсальным исполнителем любого узла.
- Сброс: по таймеру `resetDelay` после Recovery или при смене стейта.
- Буфер ввода остаётся прежним (03) — граф читает только уже буферизованные команды.

## 3. Условия переходов

Минимальный набор условий (не строить универсальный Rule Engine):

- `RequiresGrounded`, `RequiresAirborne`, `RequiresMoving`, `RequiresTag(string)`.
- Проверки читают `PlayerContext` (Ground, Motor.Velocity, теги).

## 4. Точки внедрения в текущую архитектуру

| Что меняется | Как |
|---|---|
| `AttackDefinition.nextInCombo` | помечается `[Obsolete]`, цепочка переезжает в граф |
| `AttackState` | использует `ComboRunner` вместо ручного next |
| `PlayerCombat` | хранит активный `ComboGraphAsset` (сейчас — цепочка) |
| Animator | триггеры уже приходят из `AttackDefinition.animTrigger` — без изменений |

Ничего в Motor, Buffer, Health менять не нужно — поэтому отложить безопасно.

## 5. Animation Events

Если кадры анимаций начнут диктовать тайминги фаз (вместо таймеров 05 §4):
`AnimationEventReceiver` на объекте с Animator транслирует события в `AttackRunner`
(`OnHitboxStart`, `OnHitboxEnd`, `OnCancelWindowStart`). Переход только при реальной нужде —
таймеры проще дебажить.

## 6. Отладка

При внедрении добавить debug-вывод текущего узла графа и последнего отказа перехода
(в стиле 03 §9) — комбо-графы главный источник «почему не сработало».
