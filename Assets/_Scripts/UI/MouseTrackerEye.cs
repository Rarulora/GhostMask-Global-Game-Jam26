using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MouseTrackerEye : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform eyeWhiteRect;
    RectTransform pupilRect;

    [Header("Movement")]
    public float maxRadiusX = 50f;
    public float maxRadiusY = 30f;
    public float smoothSpeed = 15f;

    [Header("Blink Settings")]
    public float blinkDuration = 0.15f; // Tam kapanma veya açýlma süresi
    private float initialHeight;
    private float initialWidth;
    private Coroutine currentBlinkCoroutine;

    private Canvas parentCanvas;

    void Start()
    {
        pupilRect = GetComponent<RectTransform>();

        if (eyeWhiteRect == null && transform.parent != null)
            eyeWhiteRect = transform.parent.GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();

        if (eyeWhiteRect == null)
        {
            enabled = false;
            return;
        }

        // Baþlangýç boyutlarýný kaydet
        initialHeight = eyeWhiteRect.sizeDelta.y;
        initialWidth = eyeWhiteRect.sizeDelta.x;
    }

    void Update()
    {
        // 1. Göz Bebeði Takibi
        Vector2 targetLocalPosition = GetClampedMousePosition();
        pupilRect.anchoredPosition = Vector2.Lerp(pupilRect.anchoredPosition, targetLocalPosition, Time.deltaTime * smoothSpeed);

        // 2. Týklama ile Kýrpma Kontrolü

        // Mouse'a BASILDIÐINDA -> Kapatmaya baþla
        if (Input.GetMouseButtonDown(0))
        {
            if (currentBlinkCoroutine != null) StopCoroutine(currentBlinkCoroutine);
            currentBlinkCoroutine = StartCoroutine(AnimateEye(0f)); // Hedef: 0 (Kapalý)
        }

        // Mouse BIRAKILDIÐINDA -> Açmaya baþla
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentBlinkCoroutine != null) StopCoroutine(currentBlinkCoroutine);
            currentBlinkCoroutine = StartCoroutine(AnimateEye(initialHeight)); // Hedef: Orijinal Yükseklik (Açýk)
        }
    }

    // Tek bir Coroutine hem açma hem kapama iþini yapar
    // targetHeight: 0 gelirse kapanýr, initialHeight gelirse açýlýr
    private IEnumerator AnimateEye(float targetHeight)
    {
        float timer = 0f;
        float startHeight = eyeWhiteRect.sizeDelta.y; // Harekete þimdiki boyuttan baþla (Kaldýðý yerden)

        // Animasyonun ne kadar süreceðini mesafeye göre ayarlayabiliriz 
        // ama sabit süre genellikle daha "snappy" hissettirir.

        while (timer < blinkDuration)
        {
            timer += Time.deltaTime;
            float t = timer / blinkDuration;

            // Daha doðal bir hareket için SmoothStep (Yavaþ baþla, yavaþ bitir)
            // Ýstersen Mathf.Lerp olarak býrakabilirsin.
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            float newHeight = Mathf.Lerp(startHeight, targetHeight, smoothedT);

            // Geniþliði koru, yüksekliði deðiþtir
            eyeWhiteRect.sizeDelta = new Vector2(initialWidth, newHeight);

            yield return null;
        }

        // Döngü bitince tam hedefe kilitle
        eyeWhiteRect.sizeDelta = new Vector2(initialWidth, targetHeight);
        currentBlinkCoroutine = null;
    }

    Vector2 GetClampedMousePosition()
    {
        if (parentCanvas == null) return Vector2.zero;

        Vector2 localMousePos;
        Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            eyeWhiteRect,
            Input.mousePosition,
            cam,
            out localMousePos
        );

        if (maxRadiusX <= 0 || maxRadiusY <= 0) return Vector2.zero;

        Vector2 normalizedPos = new Vector2(localMousePos.x / maxRadiusX, localMousePos.y / maxRadiusY);
        float magnitude = normalizedPos.magnitude;

        if (magnitude > 1f)
            localMousePos /= magnitude;

        return localMousePos;
    }
}