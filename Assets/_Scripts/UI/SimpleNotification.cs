using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleNotification : MonoBehaviour
{
    private Animator anim;

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float defaultDuration = 3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Show(string message)
    {
        Show(message, defaultDuration);
    }

    public void Show(string message, float duration)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        notificationPanel.SetActive(true);
        notificationText.text = message;

        anim.SetBool("Open", true);

        currentRoutine = StartCoroutine(HideRoutine(duration));
    }

    private IEnumerator HideRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        anim.SetBool("Open", false);
        yield return new WaitForSeconds(3);

        notificationPanel.SetActive(false);
    }
}