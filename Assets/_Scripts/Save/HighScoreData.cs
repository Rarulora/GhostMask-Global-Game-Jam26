using Enums;
using UnityEngine;

[System.Serializable]
public class HighScoreData : MonoBehaviour
{
    private float highScore;
    private CharacterType character;
    private AttackType attackType;
    private string[] selectedPerkNames;

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
        selectedPerkNames = null;
    }
}
