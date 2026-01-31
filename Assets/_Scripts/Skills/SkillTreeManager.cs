using System;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        goldText.text = GameManager.Instance.SaveData.gold.ToString();
    }

    private void Initialize()
    {
        foreach (var attackNode in attackNodes)
        {
            if (attackNode.skill != null && GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(attackNode.skill.ID))
                attackNode.gained = true;
        }

        foreach (var movementNode in movementNodes)
        {
            if (movementNode.skill != null && GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(movementNode.skill.ID))
                movementNode.gained = true;
        }

        foreach (var healthNode in healthNodes)
        {
            if (healthNode.skill != null && GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(healthNode.skill.ID))
               healthNode.gained = true;
        }

        foreach (var maskNode in maskNodes)
        {
            if (maskNode.skill != null && GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(maskNode.skill.ID))
                maskNode.gained = true;
        }

        foreach (var economyNode in economyNodes)
        {
            if (economyNode.skill != null && GameManager.Instance.SaveData.gainedSkillIDs.Contains<int>(economyNode.skill.ID))
                economyNode.gained = true;
        }
    }
}
