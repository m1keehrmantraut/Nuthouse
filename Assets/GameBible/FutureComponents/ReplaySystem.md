# FutureComponents/ReplaySystem — запись и воспроизведение

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Требует завершённого детерминированного ввода (этапы 1–3)
и стабильной физики. Внедряется последним из Future-систем.

---

## 1. Идея

Записывать поток команд из `InputBuffer` с таймстампами и воспроизводить его, подавая
в тот же буфер. Если детерминизм соблюдён — персонаж повторит действия 1-в-1.

Это не «видео», а именно ввод. Преимущество архитектуры: пайплайн ввода (03) уже абстрагирован
от устройства, поэтому Replay — это просто ещё один источник команд.

## 2. Детерминизм — обязательные условия

- Вся физика в `FixedUpdate` с фиксированным `fixedDeltaTime` (у нас так).
- Никакого `Random.Range` в геймплее без seed'а (для критов/разброса — `System.Random` с
  сохраняемым seed в момент записи).
- Никаких `Time.deltaTime` в решениях, влияющих на исход (только таймеры/анимация).
- Все входные данные — из `InputBuffer` и контекста, не из `Time.realtimeSinceStartup`.

## 3. Формат записи

```csharp
public readonly struct ReplayFrame
{
    public readonly float Timestamp;      // игровое время (Time.time)
    public readonly InputAction Action;
    public readonly float AxisX;          // только если меняется Move
}
```

Запись: слушатель на `InputReader.ActionPressed` + изменения осей → список `ReplayFrame`.
Хранение: бинарный/JSON файл в `Application.persistentDataPath`.

## 4. Воспроизведение

```csharp
public sealed class ReplayPlayer
{
    public void Play(ReplayData data, InputBuffer targetBuffer);
    public void Tick(float gameTime);   // подаёт кадры, чей Timestamp <= gameTime
}
```

- Режим Replay: управление игроку выключено (`SetControlEnabled(false)`), но буфер принимает
  команды от `ReplayPlayer`.
- Камера — отдельная Cinemachine-камера со свободным обзором.

## 5. Точки внедрения

- `InputReader` уже публикует команды — добавить опциональный рекордер.
- `InputBuffer.Add(action)` — тот же, источник не важен.
- Seed генератора случайных чисел сохранять в заголовок Replay-файла.

## 6. Антипаттерны

- Записывать позиции/скорости (ломается при малейших изменениях физики).
- Пытаться воспроизводить `MonoBehaviour.Update`-логику.
- Полагаться на `Time.time` без синхронизации с `FixedUpdate`.
