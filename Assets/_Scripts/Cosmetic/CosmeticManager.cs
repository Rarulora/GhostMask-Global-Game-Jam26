using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CosmeticManager : MonoBehaviour
{
    private Dictionary<int, bool> cosmeticPurchaseStatus = new Dictionary<int, bool>();
    private Dictionary<int, bool> characterPurchaseStatus = new Dictionary<int, bool>();
    private CosmeticDatabase cosmeticDatabase;
    private CharacterDatabase characterDatabase;

    private void Awake()
    {
        InitializeManager();
    }

    private void InitializeManager()
    {
        cosmeticDatabase = Resources.Load<CosmeticDatabase>("CosmeticDatabase");
        characterDatabase = Resources.Load<CharacterDatabase>("CharacterDatabase");

        if (cosmeticDatabase == null)
        {
            Debug.LogError("CosmeticDatabase couldn't be found in Resources folder!");
            return;
        }

        if (characterDatabase == null) 
        {
            Debug.LogError("CharacterDatabase couldn't be found in Resources folder!");
            return;
        }

        foreach (var item in cosmeticDatabase.data)
        {
            if (!cosmeticPurchaseStatus.ContainsKey(item.ID))
                cosmeticPurchaseStatus.Add(item.ID, false);
        }

        foreach (var character in characterDatabase.data)
        {
            if (!cosmeticPurchaseStatus.ContainsKey(character.ID))
                cosmeticPurchaseStatus.Add(character.ID, false);
        }
    }

    public void SetCosmeticAsPurchased(int cosmeticID)
    {
        cosmeticPurchaseStatus[cosmeticID] = true;
    }

    public void SetCharacterAsPurchased(int characterID)
    {
        characterPurchaseStatus[characterID] = true;
    }

    public int[] GetNonPurchasedCosmeticIDs()
    {
        List<int> list = new List<int>();
        foreach (var cosmeticID in cosmeticPurchaseStatus.Keys)
        {
            if (cosmeticPurchaseStatus[cosmeticID] == false && cosmeticDatabase.GetDataByID(cosmeticID).sprite != null)
                list.Add(cosmeticID);
        }

        return list.ToArray();
    }

    public CosmeticData[] GetNonPurchasedCosmetics()
    {
        CosmeticDatabase db = Resources.Load<CosmeticDatabase>("CosmeticDatabase");

        return db.data
                 .Where(item => cosmeticPurchaseStatus.ContainsKey(item.ID) && cosmeticPurchaseStatus[item.ID] == false && cosmeticDatabase.GetDataByID(item.ID).sprite != null)
                 .ToArray();
    }

    public int[] GetPurchasedCosmeticIDs()
    {
        List<int> list = new List<int>();
        foreach (var cosmeticID in cosmeticPurchaseStatus.Keys)
        {
            if (cosmeticPurchaseStatus[cosmeticID] == true && cosmeticDatabase.GetDataByID(cosmeticID).sprite != null)
                list.Add(cosmeticID);
        }

        return list.ToArray();
    }

    public CosmeticData[] GetPurchasedCosmetics()
    {
        CosmeticDatabase db = Resources.Load<CosmeticDatabase>("CosmeticDatabase");

        return db.data
                 .Where(item => cosmeticPurchaseStatus.ContainsKey(item.ID) && cosmeticPurchaseStatus[item.ID] == true && cosmeticDatabase.GetDataByID(item.ID).sprite != null)
                 .ToArray();
    }

    public void LoadPurchasedCosmetics(int[] purchasedIDs)
    {
        if (cosmeticPurchaseStatus.Count == 0) 
            InitializeManager();

        if (purchasedIDs == null || purchasedIDs.Length <= 0) 
            return;

        foreach (int id in purchasedIDs)
        {
            if (cosmeticPurchaseStatus.ContainsKey(id))
                cosmeticPurchaseStatus[id] = true;
            else
                Debug.LogWarning($"Save file contains ID: {id} but database doesn't for cosmetics!");
        }
    }

    public void LoadPurchasedCharacters(int[] purchasedIDs)
    {
        if (characterPurchaseStatus.Count == 0)
            InitializeManager();

        if (purchasedIDs == null || purchasedIDs.Length <= 0)
            return;

        foreach (int id in purchasedIDs)
        {
            if (characterPurchaseStatus.ContainsKey(id))
                characterPurchaseStatus[id] = true;
            else
                Debug.LogWarning($"Save file contains ID: {id} but database doesn't for characters!");
        }
    }

    public CosmeticData[] GetDataByType(CosmeticType type)
    {
        return cosmeticDatabase.GetDataByType(type);
    }
}
