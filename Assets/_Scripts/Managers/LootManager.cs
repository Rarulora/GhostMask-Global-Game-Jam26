using UnityEngine;

public class LootManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Collectable goldPrefab;
    [SerializeField] private Collectable xpPrefab;

    [Header("Settings")]
    [SerializeField] private float dropRadius = 1.5f; // Ganimetler düþmandan ne kadar uzaða saçýlsýn

    private void OnEnable()
    {
        EventManager.OnEnemyKilled += DropLoot;
    }

    private void OnDisable()
    {
        EventManager.OnEnemyKilled -= DropLoot;
    }

    private void DropLoot(EnemyBase enemy)
    {
        Vector3 spawnOrigin = enemy.transform.position;

        SpawnCollectable(goldPrefab, spawnOrigin, enemy.Stats.GoldReward);
        SpawnCollectable(xpPrefab, spawnOrigin, enemy.Stats.XPReward);
    }

    private void SpawnCollectable(Collectable prefab, Vector3 origin, int value)
    {
        Collectable loot = Instantiate(prefab, origin, Quaternion.identity);
        loot.ConfigureCollectableValue(value);

        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        Vector3 targetPos = origin + new Vector3(randomCircle.x, randomCircle.y, 0);

        loot.InitializeDrop(origin, targetPos);
    }
}