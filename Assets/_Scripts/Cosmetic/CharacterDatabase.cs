using UnityEngine;

[CreateAssetMenu(fileName = "Character Database", menuName = "Database/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public CharacterDataSO[] data;

    public CharacterDataSO GetCharacterByID(int id)
    {
        foreach (var datum in data)
        {
            if (datum.ID == id)
                return datum;
        }

        return null;
    }
}
