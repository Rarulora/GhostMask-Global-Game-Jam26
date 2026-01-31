using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderboardIcons", menuName = "ScriptableObjects/LeaderboardIcons")]
public class LeaderboardIconDatabase : ScriptableObject
{
    [Header("Character Icons")]
    public List<Sprite> characterIcons;

    [Header("Attack Type Icons")]
    public List<Sprite> attackTypeIcons;

    public Sprite GetCharacterIcon(int id)
    {
        if (id >= 0 && id < characterIcons.Count) return characterIcons[id];
        return null;
    }

    public Sprite GetAttackTypeIcon(int id)
    {
        if (id >= 0 && id < attackTypeIcons.Count) return attackTypeIcons[id];
        return null;
    }
}