using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private float holdDuration = 3f; // Kaç saniye basýlý tutulacak?
    [SerializeField] private Image fillImage; // Dolacak olan görsel

    [Header("Events")]
    public UnityEvent OnHoldComplete; // Süre dolunca ne olacak?

    private bool isHolding = false;
    private float timer = 0f;
    private bool completed = false;

    private void Start()
    {
        if (fillImage != null) fillImage.fillAmount = 0;
    }

    private void Update()
    {
        if (isHolding && !completed)
        {
            timer += Time.unscaledDeltaTime; // Oyun dursa bile çalýþsýn istersen unscaled kullan

            // Görseli güncelle (0 ile 1 arasýnda)
            if (fillImage != null)
                fillImage.fillAmount = timer / holdDuration;

            // Süre doldu mu?
            if (timer >= holdDuration)
            {
                CompleteHold();
            }
        }
    }

    // Mouse butona bastýðýnda
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable) return; // Buton pasifse çalýþma

        isHolding = true;
        completed = false;
        timer = 0f;
    }

    // Mouse butonu býraktýðýnda
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHold();
    }

    // Mouse basýlýyken butonun dýþýna çýkarsa (Ýptal etmek için)
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHold();
    }

    private void CompleteHold()
    {
        completed = true;
        isHolding = false;

        // Ýþlem tamamlandý eventini tetikle
        OnHoldComplete?.Invoke();

        // Görseli sýfýrla
        if (fillImage != null) fillImage.fillAmount = 0;
    }

    private void ResetHold()
    {
        isHolding = false;
        timer = 0f;
        completed = false;
        if (fillImage != null) fillImage.fillAmount = 0;
    }
}