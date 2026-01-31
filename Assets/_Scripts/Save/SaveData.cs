using Enums;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string Name;
    public bool hasAChosenName;
    public bool touchedBoobs;

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
        Name = "";
        hasAChosenName = false;
        touchedBoobs = false;
        purchasedCosmeticIDs = new int[0];
        purchasedCharacterIDs = new int[0];
        gainedAchievementIDs = new int[0];
        equippedHatID = 0; // ID'si 0 olan þapka ve kanat yok, gözükmüyor yani
        equippedWingID = 0;
        equippedCharacterID = 0;
        gold = 0;
        bestRunData = new HighScoreData(0, (CharacterType)0, (AttackType)0);
    }

    public SaveData(string Name, bool hasAChosenName, bool touchedBoobs, int[] purchasedCosmeticIDs, int[] purchasedCharacterIDs, int[] gainedAchievementIDs, int equippedHatID, int equippedWingID, int equippedCharacterID, int gold, HighScoreData bestRunData)
    {
        this.Name = Name;
        this.hasAChosenName = hasAChosenName;
        this.touchedBoobs = touchedBoobs;
        this.purchasedCosmeticIDs = purchasedCosmeticIDs;
        this.purchasedCharacterIDs = purchasedCharacterIDs;
        this.gainedAchievementIDs = gainedAchievementIDs;
        this.equippedHatID = equippedHatID;
        this.equippedWingID = equippedWingID;
        this.equippedCharacterID = equippedCharacterID;
        this.gold = gold;
        this.bestRunData = bestRunData;
    }

    public SaveData(string Name, bool hasAChosenName, bool touchedBoobs, int[] purchasedCosmeticIDs, int[] purchasedCharacterIDs, int[] gainedAchievementIDs, int equippedHatID, int equippedWingID, int equippedCharacterID, int gold, float highScore, CharacterType character, AttackType attackType, string[] selectedPerkNames)
    {
        this.Name = Name;
        this.hasAChosenName = hasAChosenName;
        this.touchedBoobs = touchedBoobs;
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
