# FutureComponents/InputDevices — несколько устройств ввода и Rebinding

**Статус:** ПРОДУМАНО, ОТЛОЖЕНО. Сейчас поддержка клавиатуры + базовые биндинги в ассете.
Полный мульти-девайс и rebinding — после вертикального среза геймплея.

---

## 1. Требования

- Одновременная поддержка Keyboard + Mouse и Gamepad (переключение без перезапуска).
- Rebinding: игрок переназначает клавиши в настройках, изменения сохраняются.
- Индикация текущего устройства (подсказки «нажми Space» vs «нажми A»).
- Архитектура ввода (03) уже рассчитана на это: `InputReader` нормализует устройства,
  геймплей видит только `InputAction` — не знает, откуда пришёл сигнал.

## 2. Реализация в Input System

### 2.1. Мульти-девайс

- В `PlayerInputActions` для каждого Action несколько биндингов (клавиатура, геймпад) —
  уже делаем так с этапа 2.
- Определение активного устройства: `InputUser` + `InputUser.listenForUnpairedDeviceActivity`.
  При первом нажатии с геймпада — устройство становится активным.

### 2.2. Rebinding

```csharp
// Nuthouse.Input (когда дойдём)
public sealed class RebindService
{
    public void StartRebind(InputAction action, int bindingIndex, System.Action onDone);
    public string SaveOverrides();                  // JSON → PlayerPrefs/Save
    public void LoadOverrides(string json);
    public void ResetToDefaults();
}
```

- `action.PerformInteractiveRebinding(bindingIndex)` — штатный API Input System.
- Сохранение через `actionMap.SaveBindingOverridesAsJson()` → в Save-систему.

### 2.3. Индикация устройства

- Событие `InputEvents.DeviceChanged(DeviceKind)` (новый канал или расширение GameEvents).
- UI подписывается и меняет иконки подсказок.

## 3. Точки внедрения

- `InputReader` остаётся как есть — меняется только то, как создаются/настраиваются ассеты.
- Геймплей не меняется вообще.
- Меню настроек (`SettingsMenu`) получает раздел Rebind.

## 4. Антипаттерны

- Отдельные «клавиатурный» и «геймпадный» пайплайны ввода.
- Хардкод `KeyCode` в UI-подсказках (брать из биндингов через `GetBindingDisplayString`).
