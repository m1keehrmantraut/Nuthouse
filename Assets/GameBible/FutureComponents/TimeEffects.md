# FutureComponents/TimeEffects — Hit Stop и Slow Motion

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Сейчас `Time.timeScale` никто не трогает (правило 06 §6).
Внедряется после этапа 6 (враги), когда появится «мясо» в ударах.

---

## 1. Требования

- **Hit Stop** — мгновенная заморозка на 50–150 мс при подтверждённом ударе (для обоих участников).
  Самый сильный и самый простой эффект «веса» удара.
- **Slow Motion** — плавное замедление (`timeScale 0.3–0.5`) по геймплейным поводам:
  добивание, низкое HP, бросок топора в критический момент.
- Эффекты должны **стекаться** и корректно отпускать время обратно (несколько источников
  одновременно — частая причина багов).

## 2. Решение: TimeService с приоритетными источниками

`TimeService` — сервис приложения: интерфейс `ITimeService`, регистрация в
`ServiceLocator` единственной строкой в `GameBootstrap.OnAwake()` (механизм —
`08-ServiceLocator.md`), тик — из `GameBootstrap.Update`.

```csharp
// Nuthouse.Services (когда дойдём)
public enum TimeScalePriority { Low = 0, Gameplay = 100, HitStop = 200, Cutscene = 300 }

public sealed class TimeService : ITimeService
{
    private struct Source { public float Scale; public TimeScalePriority Priority; public float ExpiresAt; }
    private readonly List<Source> sources = new();

    public void Apply(float scale, TimeScalePriority priority, float duration);
    public void Tick(float dt);                // RemoveExpired + ComputeScale → Time.timeScale
    public float ComputeScale();               // минимальный scale среди активных источников
                                               // с наивысшим приоритетом
}
```

Правила:

- **Единственный** класс, пишущий в `Time.timeScale` и `Time.fixedDeltaTime`
  (`fixedDeltaTime = baseFixed * timeScale`, чтобы физика оставалась стабильной).
- Hit Stop = `Apply(0f, HitStop, 0.08f)`. Slow Motion = `Apply(0.4f, Gameplay, 1.5f)`.
- Источники истекают по времени; принудительно снимать можно по ID токена
  (`Apply` возвращает токен).

## 3. Точки внедрения

| Эффект | Триггер | Куда подписаться |
|--------|---------|------------------|
| Hit Stop | подтверждённый удар | `CombatEvents.AttackHit` → `TimeService.Apply(0, HitStop, def.hitStopTime)` |
| Hit Stop | бросок топора попал | `CombatEvents.AxeStuck` (поле в `AxeThrowDefinition`) |
| Slow Motion | добивание | будущая система добиваний |
| Slow Motion | смерть босса | `DamageEvents.Died` + условие |

Данные эффекта — поля в существующих SO: `AttackDefinition.hitStopTime` (добавить при
внедрении), `AxeThrowDefinition.hitStopTime`.

## 4. Совместимость с текущей архитектурой (уже обеспечена)

- Геймплей тикается `PlayerFacade` с `Time.deltaTime` — замедлится автоматически.
- UI/пауза — `unscaledDeltaTime` (учесть при реализации паузы через `TimeService`).
- Анимации Animator зависят от `Time.timeScale` по умолчанию — замедлятся сами.
- `AudioSource.pitch` при slow motion можно опционально занижать (AudioMixer-настройка).

## 5. Антипаттерны

- `Time.timeScale = X` в случайных местах (главный источник «время не вернулось»).
- Корутины `WaitForSeconds` в геймплее для таймингов эффектов (зависят от scale —
  иногда нужно, иногда нет; выбирать осознанно).
- Hit Stop через `enabled = false` на компонентах.
