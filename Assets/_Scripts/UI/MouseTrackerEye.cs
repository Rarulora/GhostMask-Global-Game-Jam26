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
    public float blinkSpeed = 0.15f;
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
            enabled = false;

        initialHeight = eyeWhiteRect.sizeDelta.y;
        initialWidth = eyeWhiteRect.sizeDelta.x;
    }

    void Update()
    {
        Vector2 targetLocalPosition = GetClampedMousePosition();
        pupilRect.anchoredPosition = Vector2.Lerp(pupilRect.anchoredPosition, targetLocalPosition, Time.deltaTime * smoothSpeed);

        if (Input.GetMouseButtonDown(0))
        {
            if (currentBlinkCoroutine != null) StopCoroutine(currentBlinkCoroutine);
            currentBlinkCoroutine = StartCoroutine(Blink());
        }
    }

    Vector2 GetClampedMousePosition()
    {
        Vector2 localMousePos;
        Camera cam = null;

        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = parentCanvas.worldCamera;

        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
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

    private IEnumerator Blink()
    {
        float timer = 0f;
        float startH = eyeWhiteRect.sizeDelta.y;

        while (timer < blinkSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / blinkSpeed;

            float newHeight = Mathf.Lerp(startH, 0f, t);
            eyeWhiteRect.sizeDelta = new Vector2(initialWidth, newHeight);

            yield return null;
        }

        eyeWhiteRect.sizeDelta = new Vector2(initialWidth, 0f);

        timer = 0f;
        while (timer < blinkSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / blinkSpeed;

            float newHeight = Mathf.Lerp(0f, initialHeight, t);
            eyeWhiteRect.sizeDelta = new Vector2(initialWidth, newHeight);

            yield return null;
        }

        eyeWhiteRect.sizeDelta = new Vector2(initialWidth, initialHeight);
        currentBlinkCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (eyeWhiteRect != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = eyeWhiteRect.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(maxRadiusX * 2, maxRadiusY * 2, 1));
        }
    }
}