using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkCard : MonoBehaviour
{
    private Animator anim;
    private GameObject player;

    // TODO: Ýçinde barýndýracaðý perk objesi

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Image perkLogo;
    [SerializeField] private Button selectButton;

    private int _index = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Show()
    {
        anim.SetBool("Open", true);
        selectButton.interactable = true;
    }

    public void Hide()
    {
        anim.SetBool("Open", false);
    }

    public void Setup(string title, string desc, Sprite perkLogo)
    {
        this.title.text = title;
        this.desc.text = desc;
        if (perkLogo != null)
            this.perkLogo.sprite = perkLogo;
    }

    public void Setup(PerkBase perk, int index)
    {
        _index = index;
		Setup(perk.PerkName, perk.Description, perk.Icon);
	}

	public void OnClick()
	{
        selectButton.interactable = false;

        // PerkManager singleton olduðu için direkt ulaþabiliriz
        player.GetComponent<PerkManager>().OnClickPerk(_index);
	}
}
