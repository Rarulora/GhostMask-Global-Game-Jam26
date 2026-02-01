using UnityEngine;
using UnityEngine.UI;

public class GameplayBars : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider madnessBar;

    private PlayerController player;
    private MaskController maskController;
    private PlayerHealthController healthController;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        maskController = player.GetComponent<MaskController>();
        healthController = player.GetComponent<PlayerHealthController>();
    }

    private void Start()
    {
        healthBar.maxValue = healthController.MaxHealth;
        healthBar.value = healthController.CurrentHealth;
        madnessBar.maxValue = maskController.MaxMadness;
        madnessBar.value = maskController.CurrentMadness;   
    }

    private void OnEnable()
    {
        EventManager.OnHealthChanged += OnHealthChange;
        EventManager.OnMadnessChanged += OnMadnessChange;
    }

    private void OnDisable()
    {
        EventManager.OnHealthChanged -= OnHealthChange;
        EventManager.OnMadnessChanged -= OnMadnessChange;
    }

    private void OnHealthChange(float current, float max)
    {
        healthBar.maxValue = max;
        healthBar.value = current;
    }

    private void OnMadnessChange(float current, float max)
    {
        madnessBar.maxValue = max;
        madnessBar.value = current;
    }
}
