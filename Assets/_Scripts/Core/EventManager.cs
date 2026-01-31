using System;

public static class EventManager
{
    public static event Action<CharacterDataSO> OnCharacterChanged;
    public static event Action<bool> OnMaskChanged;
    public static event Action<EnemyBase> OnEnemyKilled;
	public static void RaiseCharacterChanged(CharacterDataSO data) => OnCharacterChanged?.Invoke(data);
    public static void RaiseMaskChanged(bool isActive) => OnMaskChanged?.Invoke(isActive);
    public static void RaiseEnemyKilled(EnemyBase enemy) => OnEnemyKilled?.Invoke(enemy);


}
