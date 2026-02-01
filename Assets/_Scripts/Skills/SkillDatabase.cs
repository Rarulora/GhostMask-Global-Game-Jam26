using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Database/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    public SkillData[] data;

    public SkillData GetSkillByID(int ID)
    {
        foreach (var datum in data)
        {
            if (datum.ID == ID)
                return datum;
        }

        return null;
    }

    public SkillData[] GetSkillsByType(SkillType type)
    {
        List<SkillData> list = new List<SkillData>();
        foreach (var datum in data)
        {
            if (datum.type == type)
                list.Add(datum);
        }

        return list.ToArray();
    }
}
