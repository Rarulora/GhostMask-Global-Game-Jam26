using UnityEngine;

public enum CosmeticBuyMethod
{
    Gold,
    Achievement
}

public enum CosmeticType
{
    Hat,
    Wing
}

[CreateAssetMenu(fileName = "Cosmetic Data", menuName = "Database/CosmeticData")]
public class CosmeticData : ScriptableObject
{
    public int ID;
    public string Name;
    public string description;
    public float price; // achievement ise 0
    public string achievementID; // gold ise null/empty
    public Sprite sprite;
    public CosmeticBuyMethod method;
    public CosmeticType type;
}
