using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Database/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    public EnemyData[] data;

    public EnemyData GetEnemyByID(int ID)
    {
        foreach (EnemyData enemy in data)
        {
            if (enemy.ID == ID)
                return enemy;
        }

        return null;
    }
}
