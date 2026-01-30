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

    [Header("Movement")]
    public float knockBackSpeed = 5f;
    public float startMoveSpeed = 1f;
    public float accelerationAmount = 5f; // per second
    private float currentFollowSpeed;

    [Header("Properties")]
    [SerializeField] public CollectableType type;
    public float knockbackDistance = 3.5f;
    public float distanceThreshold = 0.2f;
    private PlayerController player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (detected && !knockedBack)
        {
            Debug.Log("Knockedback");
            knockedBack = true;
            KnockBack();
        }
    }

    private void KnockBack()
    {
        Vector3 moveDir = InputManager.I.MoveInput.normalized;
        if (moveDir == Vector3.zero) moveDir = -transform.forward;
        StartCoroutine(KnockbackRoutine(moveDir));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        Debug.Log("In knocback coroutine");
        float duration = knockbackDistance / knockBackSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position += direction * knockBackSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartFollowingPlayer();
    }

    private void StartFollowingPlayer()
    {
        if (player.transform == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        while (distanceToPlayer > distanceThreshold)
        {
            currentFollowSpeed += accelerationAmount * Time.deltaTime;

            Vector3 nextPosition = Vector3.MoveTowards(
                transform.position,
                player.transform.position,
                currentFollowSpeed * Time.deltaTime
            );

            transform.position = nextPosition;
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);;
        }

        OnReachedPlayer();
    }

    private void OnReachedPlayer()
    {
        switch (type)
        {
            case CollectableType.Gold:
                player.IncreaseGold();
                break;
            case CollectableType.XP:
                player.IncreaseXP();
                break;
        }

        // TODO: pop effect
        Destroy(gameObject);
    }

    public void MarkAsDetected()
    {
        detected = true;
    }
}
