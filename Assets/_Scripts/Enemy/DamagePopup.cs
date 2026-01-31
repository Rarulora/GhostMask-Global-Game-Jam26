using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private static DamagePopup prefab;

    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 1f;

    public static DamagePopup Create(Vector3 position, float damageAmount, bool isCritical)
    {
        if (prefab == null)
            prefab = Resources.Load<DamagePopup>("DamagePopup");

        Vector3 spawnPosition = position + new Vector3(-0.5f, 0.8f, 0);
        spawnPosition += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), 0);

        Transform damagePopupTransform = Instantiate(prefab.transform, spawnPosition, Quaternion.identity);

        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCritical);

        return damagePopup;
    }

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount, bool isCritical)
    {
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();

        if (!isCritical)
        {
            // Normal (Red)
            textMesh.fontSize = 4;
            textColor = new Color(1f, 0.2f, 0.2f, 1f);
        }
        else
        {
            // Critical
            textMesh.fontSize = 6;
            textColor = new Color(1f, 0.8f, 0.2f, 1f);
            textMesh.text += "!";
        }

        textMesh.color = textColor;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        moveVector = new Vector3(0.5f, 1f) * 5f;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}