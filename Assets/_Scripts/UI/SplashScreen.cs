using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DartStyleSplashScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image background;
    [SerializeField] private Image logo;

    [Header("Physics Settings")]
    [SerializeField] private float baseGravity = 2500f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float initialThrust = 1300f;
    [Range(0f, 1f)]
    [SerializeField] private float bounciness = 0.4f;

    [Header("Wobble Effect (Dart Titremesi)")]
    [Tooltip("Saplandýktan sonra ne kadar süre titresin?")]
    [SerializeField] private float wobbleDuration = 0.5f;
    [Tooltip("Titremenin þiddeti (Açý cinsinden).")]
    [SerializeField] private float wobbleStrength = 7f;
    [Tooltip("Titreme hýzý.")]
    [SerializeField] private float wobbleSpeed = 30f;

    [Header("Audio Fix")]
    [Tooltip("Sesi kaç saniye önceden tetikleyelim? (Gecikme önleyici)")]
    [SerializeField] private float preFireSeconds = 0.05f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip whooshSFX;
    [SerializeField] private AudioClip bigBoinkSFX;
    [SerializeField] private AudioClip smallBoinkSFX;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Timing")]
    [SerializeField] private float waitTime = 1.0f;
    [SerializeField] private float dropOutDuration = 0.6f;
    [SerializeField] private string nextSceneName = "MainMenu";

    private RectTransform logoRect;
    private float startYPosition;
    private float currentVelocity = 0f;
    private int bounceCount = 0;

    // Ses kilidi
    private bool hasPlayedSoundForCurrentDrop = false;

    private void Start()
    {
        logoRect = logo.GetComponent<RectTransform>();
        startYPosition = (Screen.height / 2f) + (logoRect.rect.height / 2f) + 150f;

        SetImageAlpha(background, 0f);
        logoRect.anchoredPosition = new Vector2(0, startYPosition);

        // Olasý ilk takýlmayý önle
        if (bigBoinkSFX != null) bigBoinkSFX.LoadAudioData();
        if (smallBoinkSFX != null) smallBoinkSFX.LoadAudioData();

        StartCoroutine(SplashSequence());
    }

    private IEnumerator SplashSequence()
    {
        if (whooshSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(whooshSFX, sfxVolume);

        yield return StartCoroutine(FadeAlpha(background, 0f, 1f, 0.2f));

        bool isBouncing = true;
        currentVelocity = -initialThrust;
        bounceCount = 0;
        hasPlayedSoundForCurrentDrop = false;

        while (isBouncing)
        {
            float dt = Time.deltaTime;
            float appliedGravity = baseGravity;

            if (currentVelocity < 0) appliedGravity = baseGravity * fallMultiplier;

            currentVelocity -= appliedGravity * dt;

            float currentY = logoRect.anchoredPosition.y;
            float nextY = currentY + (currentVelocity * dt);

            if (currentVelocity < 0 && !hasPlayedSoundForCurrentDrop)
            {
                float predictedY = currentY + (currentVelocity * preFireSeconds);
                if (predictedY <= 0)
                {
                    PlayImpactSound(bounceCount + 1);
                    hasPlayedSoundForCurrentDrop = true;
                }
            }

            if (nextY <= 0)
            {
                nextY = 0;

                if (!hasPlayedSoundForCurrentDrop) PlayImpactSound(bounceCount + 1);

                bounceCount++;

                if (bounceCount < 3)
                {
                    currentVelocity = -currentVelocity * bounciness;
                    hasPlayedSoundForCurrentDrop = false;
                }
                else
                {
                    currentVelocity = 0;
                    isBouncing = false;

                    logoRect.anchoredPosition = Vector2.zero;
                    yield return StartCoroutine(WobbleRoutine());
                }
            }

            if (isBouncing)
            {
                logoRect.anchoredPosition = new Vector2(0, nextY);
                yield return null;
            }
        }

        yield return new WaitForSeconds(waitTime);

        float dropVelocity = 0f;
        while (logoRect.anchoredPosition.y > -startYPosition * 1.5f)
        {
            dropVelocity -= baseGravity * 2f * Time.deltaTime;
            logoRect.anchoredPosition += new Vector2(0, dropVelocity * Time.deltaTime);
            yield return null;
        }
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator WobbleRoutine()
    {
        float timer = 0f;

        while (timer < wobbleDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / wobbleDuration;

            float currentStrength = Mathf.Lerp(wobbleStrength, 0f, progress);
            float angle = Mathf.Sin(timer * wobbleSpeed) * currentStrength;

            logoRect.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        logoRect.localRotation = Quaternion.identity;
    }

    private void PlayImpactSound(int currentBounce)
    {
        AudioClip clipToPlay = null;
        float volFactor = 1f;

        if (currentBounce == 1 || currentBounce == 2)
        {
            clipToPlay = bigBoinkSFX;
            volFactor = (currentBounce == 1) ? 1f : 0.9f;
        }
        else if (currentBounce == 3)
        {
            clipToPlay = smallBoinkSFX;
            volFactor = 0.8f;
        }

        if (clipToPlay != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clipToPlay, sfxVolume * volFactor);
    }

    private IEnumerator FadeAlpha(Image img, float start, float end, float duration)
    {
        float timer = 0f;
        Color c = img.color;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, timer / duration);
            img.color = c;
            yield return null;
        }
        c.a = end;
        img.color = c;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}