using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    private Dictionary<string, Menu> _sceneMenus = new Dictionary<string, Menu>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneMenus();
    }

    public void RefreshSceneMenus()
    {
        _sceneMenus.Clear();
        Menu[] foundMenus = FindObjectsByType<Menu>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var menu in foundMenus)
        {
            string finalID = menu.menuID;

            // If there are menus with the same name
            while (_sceneMenus.ContainsKey(finalID))
                finalID += "_";

            menu.menuID = finalID;
            _sceneMenus.Add(finalID, menu);
        }
    }

    public void SetMenuState(string id, bool isOpen)
    {
        if (_sceneMenus.TryGetValue(id, out Menu menu))
            menu.SetState(isOpen);
        else
            Debug.LogWarning($"Menu with ID = '{id}' couldn't be found.");
    }
}
