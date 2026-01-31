using Enums; // Enumlarýn olduðu namespace
using UnityEngine;

[System.Serializable]
public class HighScoreData
{
    public float highScore;
    public CharacterType character;
    public AttackType attackType;
    public string[] selectedPerkNames;

    public HighScoreData() { }

    public HighScoreData(float highScore, CharacterType character, AttackType attackType, string[] selectedPerkNames)
    {
        this.highScore = highScore;
        this.character = character;
        this.attackType = attackType;
        this.selectedPerkNames = selectedPerkNames;
    }

    public HighScoreData(float highScore, CharacterType character, AttackType attackType)
    {
        this.highScore = highScore;
        this.character = character;
        this.attackType = attackType;
        this.selectedPerkNames = null;
    }
}