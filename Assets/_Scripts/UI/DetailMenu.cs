using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailMenu : MonoBehaviour
{
    private Animator anim;
    private SkillTreeNode node;

    private bool onScreen = false;

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Button buyButton;

    [SerializeField] private UIHoldButton holdButtonLogic;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (holdButtonLogic == null && buyButton != null)
            holdButtonLogic = buyButton.GetComponent<UIHoldButton>();
    }

    public void OnPurchaseHoldCompleted()
    {
        if (node != null && !node.gained)
        {
            if (GameManager.Instance.SaveData.gold >= node.skill.price)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cash);
                node.Purchase();
                Close();
            }
            else
            {
                buyButton.GetComponent<Animator>().SetTrigger("Vibrate");
            }
        }
    }

    private void Initialize(SkillTreeNode node)
    {
        this.node = node;
        title.text = node.skill.title;
        desc.text = node.skill.desc;
        price.text = node.skill.price.ToString();

        // Eðer yetenek zaten alýnmýþsa butonu kapat
        if (node.gained)
        {
            buyButton.interactable = false;
            price.text = "Owned";
        }
        else
        {
            buyButton.interactable = true;
        }
    }

    public void Open(SkillTreeNode node)
    {
        onScreen = true;
        Initialize(node);
        anim.SetBool("Open", true);
    }

    public void Close()
    {
        onScreen = false;
        anim.SetBool("Open", false);
    }
}
