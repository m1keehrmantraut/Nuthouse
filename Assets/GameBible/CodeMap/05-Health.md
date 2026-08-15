# 05 — Health (здоровье и стамина)

Папка `Assets/Scripts/Health/`. Здоровье игрока и врагов, приём урона, усталость.

- `HealthConfig.cs` `[новый, SO]` — макс. HP, время неуязвимости после удара (`invulnTime`), длительность оглушения (`staggerTime`).
- `Health.cs` `[новый]` — здоровье ИГРОКА (чистый класс, реализует `IDamageable`). Получив урон: вычитает HP, публикует `DamageTaken`; если выжил — кладёт запрос `PendingHurt` в контекст (→ `HurtState`) и включает короткую неуязвимость; если HP≤0 — флаг `IsDead` + событие `Died`.
- `Stamina.cs` `[новый]` — стамина: расход на бег, реген, порог восстановления. `RunState` через `SetSprinting` сообщает, бежит ли игрок.
- `StaminaConfig.cs` `[новый, SO]` — макс. стамины, скорость расхода/регена, порог восстановления.
- `DamageReceiver.cs` `[новый]` — обёртка-MonoBehaviour для ВРАГОВ: реализует `IDamageable`, HP из SO-конфига, публикует `DamageEvents.Died`. `EnemyHealth` наследуется от него.

У игрока `DamageReceiver` нет: его `Hurtbox` привязывается к чистому `Health` через `Bind()` в фасаде.