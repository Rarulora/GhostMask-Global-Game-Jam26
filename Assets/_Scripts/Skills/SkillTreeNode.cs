using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNode : MonoBehaviour
{
    public int ID;
    public SkillData skill;
    public bool gained = false;
    public bool canBeGained = false;
    public SkillTreeNode parent = null;

    [SerializeField] private Image darkPart;
    private Button button;

    public Action onGained;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (parent == null)
            canBeGained = true;

        if (parent != null)
            parent.onGained += SetAsGainable;
    }

    private void Update()
    {
        if (!canBeGained)
            darkPart.gameObject.SetActive(true);
        else
            darkPart.gameObject.SetActive(false);

        button.interactable = canBeGained;
    }

    private void SetAsGainable()
    {
        canBeGained = true;
    }

    public void Purchase()
    {
        GameManager.Instance.SaveData.gold -= skill.price;

        gained = true;
        onGained?.Invoke();

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