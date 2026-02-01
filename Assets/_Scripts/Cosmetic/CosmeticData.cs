using UnityEngine;
using Enums;


[CreateAssetMenu(fileName = "Cosmetic Data", menuName = "Database/CosmeticData")]
public class CosmeticData : ScriptableObject
{
    public int ID;
    public string Name;
    public string description;
    public float price;
    public Sprite sprite;
    public CosmeticBuyMethod method;
    public CosmeticType type;
}
