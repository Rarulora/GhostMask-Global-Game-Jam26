using UnityEngine;
using Enums;

public enum SkillType
{
    Attack,
    Movement,
    Mask,
    Health,
    Economy
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Database/Skill Data")]
public class SkillData : ScriptableObject
{
    public int ID;
    public float value;
    public int price;
    public Sprite icon;
    public string title;
    public string desc;
    public StatType affectedStat;
    public StatModType affectType;
    public SkillType type;
}
