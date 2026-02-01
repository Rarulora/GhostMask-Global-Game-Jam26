using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameplayEntryLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Settings")]
    [Tooltip("Sistemin kendine gelmesi için beklenecek minimum süre")]
    [SerializeField] private float minLoadingTime = 2f;

    // Oyunun gerçekten baþladýðýný diðer scriptlere bildirmek için event
    public static System.Action OnGameReady;

    private void Awake()
    {
        // 1. Oyunun akýþýný (zamaný) durdurabilirsin veya sadece inputlarý kitleyebilirsin.
        // Time.timeScale = 0; // Ýstersen fiziði de durdurmak için bunu aç.

        // Canvas'ýn görünür olduðundan emin ol
        loadingCanvasGroup.alpha = 1;
        loadingCanvasGroup.gameObject.SetActive(true);
    }

    private IEnumerator Start()
    {
        // --- ADIM 1: AÐIR YÜKÜN BÝNMESÝNÝ BEKLE ---

        // Ýlk frame'in geçmesini bekle (Awake/Start metodlarý burada çalýþýr ve biter)
        yield return null;

        // Ýkinci frame (Fizik motoru ve UI layout kendine gelir)
        yield return new WaitForEndOfFrame();

        // --- ADIM 2: MANUEL GARBAGE COLLECTION (Opsiyonel ama önerilir) ---
        // Sahne geçiþinde biriken çöpleri temizle ki oyun ortasýnda takýlma yapmasýn.
        System.GC.Collect();
        yield return null; // Bir frame daha bekle

        // --- ADIM 3: SHADER WARMUP VE ASSET YÜKLEMELERÝ ---
        // Burasý "yalandan" bekleme süresi deðil, sistemin GPU'ya veri atmasý için fýrsattýr.
        float timer = 0f;
        while (timer < minLoadingTime)
        {
            timer += Time.unscaledDeltaTime;
            // Buraya istersen loading bar'ý dolduracak kod yazabilirsin.
            yield return null;
        }

        // --- ADIM 4: YÜKLEME EKRANINI KAPAT ---

        // Time.timeScale kullandýysan burada 1 yapmalýsýn.
        // Time.timeScale = 1;

        // Fade Out Animasyonu
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.unscaledDeltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            yield return null;
        }

        loadingCanvasGroup.gameObject.SetActive(false);

        // --- ADIM 5: OYUNU BAÞLAT ---
        // Düþmanlar, spawnerlar vs. bu eventi dinleyip harekete geçebilir.
        OnGameReady?.Invoke();
        Debug.Log("Gameplay Hazýr, Kasma Bitti!");
    }
}