using Enums;
using UnityEngine;

[System.Serializable]
public class SaveData : MonoBehaviour
{
    public string Name;

    public int[] purchasedCosmeticIDs;
    public int[] purchasedCharacterIDs;
    public int[] gainedAchievementIDs;

    public int equippedHatID;
    public int equippedWingID;
    public int equippedCharacterID;

    public int gold;
    public HighScoreData bestRunData;

    public SaveData()
    {
        Name = "Guest";
        purchasedCosmeticIDs = new int[0];
        purchasedCharacterIDs = new int[0];
        gainedAchievementIDs = new int[0];
        equippedHatID = 0; // ID'si 0 olan þapka ve kanat yok, gözükmüyor yani
        equippedWingID = 0;
        equippedCharacterID = 0;
        gold = 0;
        bestRunData = null; // No highscore
    }

    public SaveData(string Name, int[] purchasedCosmeticIDs, int[] purchasedCharacterIDs, int[] gainedAchievementIDs, int equippedHatID, int equippedWingID, int equippedCharacterID, int gold, HighScoreData bestRunData)
    {
        this.Name = Name;
        this.purchasedCosmeticIDs = purchasedCosmeticIDs;
        this.purchasedCharacterIDs = purchasedCharacterIDs;
        this.gainedAchievementIDs = gainedAchievementIDs;
        this.equippedHatID = equippedHatID;
        this.equippedWingID = equippedWingID;
        this.equippedCharacterID = equippedCharacterID;
        this.gold = gold;
        this.bestRunData = bestRunData;
    }

    public SaveData(string Name, int[] purchasedCosmeticIDs, int[] purchasedCharacterIDs, int[] gainedAchievementIDs, int equippedHatID, int equippedWingID, int equippedCharacterID, int gold, float highScore, CharacterType character, AttackType attackType, string[] selectedPerkNames)
    {
        this.Name = Name;
        this.purchasedCosmeticIDs = purchasedCosmeticIDs;
        this.purchasedCharacterIDs = purchasedCharacterIDs;
        this.gainedAchievementIDs = gainedAchievementIDs;
        this.equippedHatID = equippedHatID;
        this.equippedWingID = equippedWingID;
        this.equippedCharacterID = equippedCharacterID;
        this.gold = gold;
        bestRunData = new HighScoreData(highScore, character, attackType, selectedPerkNames);
    }
}
