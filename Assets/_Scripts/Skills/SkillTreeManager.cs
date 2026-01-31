using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private SkillTreeNode[] attackNodes;
    [SerializeField] private SkillTreeNode[] movementNodes;
    [SerializeField] private SkillTreeNode[] healthNodes;
    [SerializeField] private SkillTreeNode[] maskNodes;
    [SerializeField] private SkillTreeNode[] economyNodes;

    [SerializeField] private DetailMenu detailMenu;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var attackNode in attackNodes)
        {
            if (GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(attackNode.skill.ID))
                attackNode.gained = true;
        }

        foreach (var movementNode in movementNodes)
        {
            if (GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(movementNode.skill.ID))
                movementNode.gained = true;
        }

        foreach (var healthNode in healthNodes)
        {
            if (GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(healthNode.skill.ID))
               healthNode.gained = true;
        }

        foreach (var maskNode in maskNodes)
        {
            if (GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(maskNode.skill.ID))
                maskNode.gained = true;
        }

        foreach (var economyNode in economyNodes)
        {
            if (GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(economyNode.skill.ID))
                economyNode.gained = true;
        }
    }
}

public class SkillTreeNode : MonoBehaviour
{
    public int ID;
    public SkillData skill;
    public bool gained = false;

    public void Purchase()
    {
        gained = true;

        int[] newGainedSkills = new int[GameManager.Instance.SaveData.gainedSkillIDs.Length + 1];
        int i;
        for (i = 0; i < GameManager.Instance.SaveData.gainedSkillIDs.Length; i++) 
        {
            newGainedSkills[i] = GameManager.Instance.SaveData.gainedSkillIDs[i];
        }

        newGainedSkills[i] = skill.ID;
        SaveData newSaveData = GameManager.Instance.SaveData;
        newSaveData.gainedSkillIDs = newGainedSkills;

        GameManager.Instance.SetSaveData(newSaveData);
        GameManager.Instance.SaveGame();
    }
}
