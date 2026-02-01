using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] private Image closeButton;
    [SerializeField] private VideoPlayer video;

    [Header("Properties")]
    private float closeButtonFadeInOutTime = 1f;
    private float closeButtonActiveTime = 5f;

    private float cutsceneTimer = 0f;

    [Header("Flags")]
    private bool hasEnded = false;
    private bool closeButtonActive = false;

    private void Awake()
    {
        if (closeButton != null)
        {
            Color c = closeButton.color;
            c.a = 0f;
            closeButton.color = c;
        }
    }

    private void Start()
    {
        closeButton.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.MainMenu());
    }

    private void Update()
    {
        cutsceneTimer += Time.unscaledDeltaTime;

        if (cutsceneTimer >= 32f)
        {
            hasEnded = true;
        }

        if (Input.anyKeyDown)
        {
            if (!closeButtonActive && !hasEnded)
                StartCoroutine(FadeCloseButton());
        }

        if (hasEnded)
            GameManager.Instance.MainMenu();
    }

    private IEnumerator FadeCloseButton()
    {
        closeButtonActive = true;

        closeButton.GetComponent<Button>().interactable = true;
        yield return StartCoroutine(FadeAlpha(closeButton, 0f, 1f, closeButtonFadeInOutTime));

        yield return new WaitForSecondsRealtime(closeButtonActiveTime);

        yield return StartCoroutine(FadeAlpha(closeButton, 1f, 0f, closeButtonFadeInOutTime));
        closeButton.GetComponent<Button>().interactable = false;

        closeButtonActive = false;
    }

    private IEnumerator FadeAlpha(Image img, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            c.a = alpha;
            img.color = c;
            yield return null;
        }

        c.a = to;
        img.color = c;
    }
}
