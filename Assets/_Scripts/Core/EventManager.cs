using System;

public static class EventManager
{
    public static event Action<CharacterDataSO> OnCharacterChanged;
    public static event Action<bool> OnMaskChanged;
    public static event Action<EnemyBase> OnEnemyKilled;
    public static event Action<bool> OnDashStatusChanged;

    public static event Action<float, float> OnHealthChanged;
    public static event Action OnPlayerDeath;

    public static event Action<float, float> OnMadnessChanged;
	public static void RaiseCharacterChanged(CharacterDataSO data) => OnCharacterChanged?.Invoke(data);
    public static void RaiseMaskChanged(bool isActive) => OnMaskChanged?.Invoke(isActive);
    public static void RaiseEnemyKilled(EnemyBase enemy) => OnEnemyKilled?.Invoke(enemy);
    public static void RaiseDashStatusChanged(bool isDashing) => OnDashStatusChanged?.Invoke(isDashing);

    public static void RaiseHealthChanged(float current, float max) => OnHealthChanged?.Invoke(current, max);
    public static void RaisePlayerDeath() => OnPlayerDeath?.Invoke();
    public static void RaiseMadnessChanged(float current, float max) => OnMadnessChanged?.Invoke(current, max);

}
