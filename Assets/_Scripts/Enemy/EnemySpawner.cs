using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Player")]
    private PlayerController player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Settings")]
    public int baseBudget = 20;
    public float budgetGrowth = 1.2f;
    public float difficultyGrowth = 1.1f; // Düþmanlar her level %10 güçlenir (ve pahalanýr)

    private EnemyDatabase enemyDatabase;
    private const int MAX_ATTEMPTS = 50;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.onLevelChanged += SpawnEnemiesForLevel;

        enemyDatabase = Resources.Load<EnemyDatabase>("EnemyDatabase");
        if (enemyDatabase == null) 
            Debug.LogError("EnemyDatabase couldn't be found!");
    }

    public void SpawnEnemiesForLevel(int currentLevel)
    {
        if (enemyDatabase == null) return;

        int currentBudget = Mathf.RoundToInt(baseBudget * Mathf.Pow(budgetGrowth, currentLevel - 1));
        float difficultyMultiplier = Mathf.Pow(difficultyGrowth, currentLevel - 1);

        int attempts = 0;
        while (currentBudget > 0 && attempts < MAX_ATTEMPTS)
        {
            var affordableEnemies = enemyDatabase.data
                .Where(data => (data.SpawnValue * difficultyMultiplier) <= currentBudget)
                .ToList();

            if (affordableEnemies.Count == 0) break;

            EnemyData selectedData = affordableEnemies[Random.Range(0, affordableEnemies.Count)];
            int cost = Mathf.RoundToInt(selectedData.SpawnValue * difficultyMultiplier);
            SpawnEnemy(selectedData, difficultyMultiplier);

            currentBudget -= cost;
            attempts++;
        }
    }

    private void SpawnEnemy(EnemyData data, float difficultyMultiplier)
    {
        if (data.Prefab == null)
        {
            Debug.LogError($"There are no prefab in {data.name} data!");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(data.Prefab, point.position, Quaternion.identity);
        
        EnemyStats stats = newEnemy.GetComponent<EnemyStats>();
        if (stats != null)
            stats.Initialize(difficultyMultiplier);
        else
            Debug.LogWarning("No EnemyStats script on instantiated enemies!");
    }
}