using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkCard : MonoBehaviour
{
    private Animator anim;

    // TODO: Ýçinde barýndýracaðý perk objesi

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Image perkLogo;
    [SerializeField] private Button selectButton;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Show()
    {
        anim.SetBool("Open", true);
        selectButton.interactable = true;
    }

    public void Initialize(string title, string desc, Sprite perkLogo)
    {
        this.title.text = title;
        this.desc.text = desc;
        this.perkLogo.sprite = perkLogo;
    }

    public void Initialize(/*Perk nesnesi*/)
    {
        
    }

    public void OnClick()
    {

    }
}
