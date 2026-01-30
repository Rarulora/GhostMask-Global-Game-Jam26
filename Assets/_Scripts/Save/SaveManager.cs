using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public const string SAVEDATA = "SaveData";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVEDATA, json);
    }

    public static SaveData Load()
    {
        string json = PlayerPrefs.GetString(SAVEDATA, string.Empty);
        if (json == string.Empty) 
            return new SaveData();

        return JsonUtility.FromJson<SaveData>(json);
    }
}
