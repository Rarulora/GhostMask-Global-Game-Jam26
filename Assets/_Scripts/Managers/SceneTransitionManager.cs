using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    private List<Animator> animators;
    private bool uiDirty = true;

    public const string MAIN_MENU_SCENE = "MainMenu";
    public  const string GAMEPLAY_SCENE = "Gameplay";
    public const string COSMETICS_SCENE = "Cosmetics";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        GetUIElements();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiDirty = true;
        GetUIElements();
        foreach (var animator in animators)
        {
            animator.SetBool("Open", true);
        }
    }

    private void GetUIElements()
    {
        animators = FindObjectsByType<Animator>(FindObjectsSortMode.None).ToList();
        foreach (var animator in animators)
        {
            if (animator.GetComponent<Menu>() != null)
                animators.Remove(animator);
        }

        uiDirty = false;
    }
    public void OpenScene(string name)
    {
        foreach (var animator in animators)
        {
            animator.SetBool("Open", false);
        }

        SceneManager.LoadScene(name);
    }
}
