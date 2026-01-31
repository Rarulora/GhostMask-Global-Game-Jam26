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

    private Canvas parentCanvas;

    void Start()
    {
        pupilRect = GetComponent<RectTransform>();

        if (eyeWhiteRect == null && transform.parent != null)
            eyeWhiteRect = transform.parent.GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();

        if (eyeWhiteRect == null)
            enabled = false;
    }

    void Update()
    {
        Vector2 targetLocalPosition = GetClampedMousePosition();
        pupilRect.anchoredPosition = Vector2.Lerp(pupilRect.anchoredPosition, targetLocalPosition, Time.deltaTime * smoothSpeed);
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