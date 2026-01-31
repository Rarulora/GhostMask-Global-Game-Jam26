using System.Collections;
using UnityEngine;

public enum CollectableType
{
    Gold,
    XP
}

public class Collectable : MonoBehaviour
{
    [Header("Flags")]
    private bool detected = false;
    private bool knockedBack = false;
    private bool collected = false;

    [Header("Movement")]
    public float knockBackSpeed = 25f;
    public float startMoveSpeed = 5f;
    public float accelerationAmount = 25f; // per second
    private float currentFollowSpeed;

    [Header("Properties")]
    [SerializeField] public CollectableType type;
    public float knockbackDistance = 2f;
    public float distanceThreshold = 0.2f;
    public int collectableValue = 1;
    private PlayerController player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (detected && !knockedBack && !collected)
        {
            knockedBack = true;
            StartCoroutine(KnockbackRoutine());
        }
    }

    private IEnumerator KnockbackRoutine()
    {
        Vector3 direction = (transform.position - player.transform.position).normalized;
        if (direction == Vector3.zero) direction = Random.insideUnitSphere.normalized;

        float duration = knockbackDistance / knockBackSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position += direction * knockBackSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FollowPlayerRoutine());
    }

    private IEnumerator FollowPlayerRoutine()
    {
        currentFollowSpeed = startMoveSpeed;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        while (distanceToPlayer > distanceThreshold)
        {
            currentFollowSpeed += accelerationAmount * Time.deltaTime;

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.transform.position,
                currentFollowSpeed * Time.deltaTime
            );

            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            yield return null;
        }

        OnReachedPlayer();
    }

    private void OnReachedPlayer()
    {
        switch (type)
        {
            case CollectableType.Gold:
                player.IncreaseGold(collectableValue);
                break;
            case CollectableType.XP:
                player.IncreaseXP(collectableValue);
                break;
        }
        collected = true;

        // TODO: pop effect
        Destroy(gameObject);
    }

    public void MarkAsDetected()
    {
        detected = true;
    }

    public void ConfigureCollectableValue(int newValue)
    {
        collectableValue = newValue;
    }
}
