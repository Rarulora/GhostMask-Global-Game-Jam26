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

    private int _index = 0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Show()
    {
        anim.SetBool("Open", true);
        selectButton.interactable = true;
    }

    public void Setup(string title, string desc, Sprite perkLogo)
    {
        this.title.text = title;
        this.desc.text = desc;
        this.perkLogo.sprite = perkLogo;
    }

    public void Setup(PerkBase perk, int index)
    {
        _index = index;
		Setup(perk.PerkName, perk.Description, perk.Icon);
	}

	public void OnClick()
	{
		// PerkManager singleton olduðu için direkt ulaþabiliriz
		PerkManager.I.OnClickPerk(_index);

		// Týklanýnca butonlarý kapatabiliriz çift týklamayý önlemek için
		selectButton.interactable = false;
	}
}
