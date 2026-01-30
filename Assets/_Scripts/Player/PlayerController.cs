using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int goldCollected = 0;
    private int xpCollected = 0;

    [Header("Collectable")]
    public float collectRadius = 2f;
    public LayerMask collectableLayer;

    private void Update()
    {
        CheckForCollectables();
    }

    public void IncreaseGold(int amount = 1)
    {
        goldCollected += amount;
    }

    public void IncreaseXP(int amount = 1)
    {
        xpCollected += amount;
    }

    public void DecreaseGold(int amount = 1)
    {
        goldCollected -= amount;
        if (goldCollected < 0)
            goldCollected = 0;
    }

    public void DecreaseXP(int amount = 1)
    {
        xpCollected -= amount;
        if (xpCollected < 0)
            xpCollected = 0;
    }

    private void CheckForCollectables()
    {
        Collider[] hitColliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, collectRadius, hitColliders, collectableLayer);
        Debug.Log(numColliders);

        for (int i = 0; i < numColliders; i++)
        {
            Collectable collectable = hitColliders[i].GetComponent<Collectable>();
            if (collectable != null)
            {
                collectable.MarkAsDetected();
                Debug.Log("detected");
            }
                
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}
