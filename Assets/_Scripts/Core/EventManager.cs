using System;

public static class EventManager
{
    public static event Action<CharacterDataSO> OnCharacterChanged;

    public static void RaiseCharacterChanged(CharacterDataSO data) => OnCharacterChanged?.Invoke(data);
}
