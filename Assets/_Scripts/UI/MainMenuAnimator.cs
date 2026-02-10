using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimator : MonoBehaviour
{
    public static bool IsIntroFinished { get; private set; } = false;

    [System.Serializable]
    public struct MenuElement
    {
        public RectTransform rectTransform;
        public Direction enterFrom;
        public float delay;
    }

    public enum Direction { Left, Right, Top, Bottom }

    [Header("Animation Settings")]
    [Tooltip("Animasyonun ne kadar süreceði (Saniye).")]
    [SerializeField] private float duration = 0.7f;

    [Header("Elements")]
    [SerializeField] private List<MenuElement> uiElements;

    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();

    private void Awake()
    {
        IsIntroFinished = false;

        foreach (var item in uiElements)
        {
            if (item.rectTransform != null)
            {
                originalPositions[item.rectTransform] = item.rectTransform.anchoredPosition;
                item.rectTransform.anchoredPosition = GetOffScreenPosition(item.rectTransform, item.enterFrom);
            }
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (var item in uiElements)
        {
            if (item.rectTransform != null)
            {
                StartCoroutine(AnimateItem(item));
            }
        }

        float maxDelay = 0;
        foreach (var item in uiElements)
        {
            if (item.delay > maxDelay) maxDelay = item.delay;
        }

        yield return new WaitForSeconds(maxDelay + duration);

        IsIntroFinished = true;
    }

    private IEnumerator AnimateItem(MenuElement item)
    {
        yield return new WaitForSeconds(item.delay);

        Vector2 startPos = item.rectTransform.anchoredPosition;
        Vector2 targetPos = originalPositions[item.rectTransform];

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            item.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }

        item.rectTransform.anchoredPosition = targetPos;
    }

    private Vector2 GetOffScreenPosition(RectTransform rect, Direction dir)
    {
        Vector2 pos = originalPositions[rect];
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float offsetCheck = 100f;

        switch (dir)
        {
            case Direction.Left:
                pos.x = -(screenWidth / 2 + rect.rect.width / 2 + offsetCheck);
                break;
            case Direction.Right:
                pos.x = (screenWidth / 2 + rect.rect.width / 2 + offsetCheck);
                break;
            case Direction.Top:
                pos.y = (screenHeight / 2 + rect.rect.height / 2 + offsetCheck);
                break;
            case Direction.Bottom:
                pos.y = -(screenHeight / 2 + rect.rect.height / 2 + offsetCheck);
                break;
        }
        return pos;
    }
}