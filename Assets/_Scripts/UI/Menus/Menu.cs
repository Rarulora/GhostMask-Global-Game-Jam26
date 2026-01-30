using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Menu : MonoBehaviour
{
    [Tooltip("Unique ID for menus.")]
    public string menuID;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetState(bool isOpen)
    {
        anim.SetBool("Open", isOpen);
    }
}
