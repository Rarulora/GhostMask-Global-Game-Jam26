using System.Collections;
using UnityEngine;

public enum CollectableType
{
    Gold,
    XP
}

public class Collectable : MonoBehaviour
{
    [Header("Drop Animation (Parabolic)")]
    [SerializeField] private AnimationCurve dropCurve; // Editörden ayarlanacak "Tümsek" eðrisi
    [SerializeField] private float dropHeight = 1.5f; // Ne kadar yükseðe zýplayacak
    [SerializeField] private float dropDuration = 0.5f; // Yere düþmesi ne kadar sürecek

    [Header("Flags")]
    private bool detected = false;
    private bool knockedBack = false;
    private bool collected = false;
    private bool isSpawning = false; // Havada süzülürken toplanmasýn diye

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
        if (dropCurve == null || dropCurve.length == 0)
        {
            dropCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        }
    }

    private void Update()
    {
        if (detected && !knockedBack && !collected && !isSpawning)
        {
            knockedBack = true;
            StartCoroutine(KnockbackRoutine());
        }
    }

    public void InitializeDrop(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(DropAnimationRoutine(startPos, targetPos));
    }

    private IEnumerator DropAnimationRoutine(Vector3 startPos, Vector3 targetPos)
    {
        isSpawning = true;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / dropDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
            float heightOffset = dropCurve.Evaluate(percent) * dropHeight;
            currentPos.y += heightOffset;

            transform.position = currentPos;

            yield return null;
        }

        transform.position = targetPos;
        isSpawning = false;
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
