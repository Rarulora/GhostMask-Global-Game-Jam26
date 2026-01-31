using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cosmetic Database", menuName = "Database/CosmeticDatabase")]
public class CosmeticDatabase : ScriptableObject
{
    public CosmeticData[] data;

    public CosmeticData GetDataByID(int id)
    {
        foreach (var datum in data)
        {
            if (datum.ID == id)
                return datum;
        }

        return null;
    }

    public CosmeticData[] GetDataByType(CosmeticType type)
    {
        List<CosmeticData> typeData = new List<CosmeticData>();
        foreach (var datum in data)
        {
            if (datum.type == type)
                typeData.Add(datum);
        }

        return typeData.ToArray();
    }
}
