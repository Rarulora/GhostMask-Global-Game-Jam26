using UnityEngine;
using GameStates;
using UnityEngine.InputSystem;
using Unity.Services.Analytics;

[System.Serializable]
public class GameSettings
{
    [Header("Controls")]
    [Range(0.1f, 5.0f)]
    public float MouseSensitivity = 1.0f;
}

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameSettings gameSettings = new();

    [Header("Databases")]
    [SerializeField] private CharacterDatabase characterDatabase;

    public GameBaseState CurrentState { get; private set; }
    private GameStateFactory _states;

    private SaveData saveData;
    public SaveData SaveData => saveData;

    public CharacterDatabase CharacterDatabase => characterDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        InitDatabases();

        _states = new GameStateFactory();
        CurrentState = _states.MainMenu();
        CurrentState.EnterState();

        saveData = LoadGame();
    }

    private void Update()
    {
        CurrentState?.UpdateState();
        Debug.Log(Time.timeScale);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private void InitDatabases()
    {
        if (characterDatabase == null)
            characterDatabase = Resources.Load<CharacterDatabase>("CharacterDatabase");
    }

	public void ChangeName(string name)
    {
        saveData.Name = name;
        saveData.hasAChosenName = true;
        SaveManager.Save(saveData);
    }

    public bool DidPlayerChooseAName() => saveData.hasAChosenName;

    public void OnTouchBasketLadysBoobs()
    {
        if (!saveData.touchedBoobs)
        {
            AnalyticsService.Instance.RecordEvent("touched_basket_lady_boobs");
            saveData.touchedBoobs = true;
            SaveManager.Save(saveData);
        }
    }

    public void SetSaveData(SaveData save)
    {
        saveData = save;
        SaveGame();
    }

    public void SaveGame()
    {
        SaveManager.Save(saveData);
    }

    public SaveData LoadGame()
    {
        return SaveManager.Load();
    }

    public void SwitchState(GameBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        Debug.Log(newState.ToString());
        CurrentState.EnterState();
        //GameEvents.RaiseGameStateChanged(CurrentState);
    }

    public void Play()
    {
        SwitchState(_states.Play());
        SceneTransitionManager.Instance.OpenScene(SceneTransitionManager.GAMEPLAY_SCENE);
    }

    public void MainMenu()
    {
        SwitchState(_states.MainMenu());
        SceneTransitionManager.Instance.OpenScene(SceneTransitionManager.MAIN_MENU_SCENE);
    }

    public void Pause() => SwitchState(_states.Pause());

    public void Resume() => SwitchState(_states.Play());

    public void StartPerkSelect() => SwitchState(_states.StartPerkSelect());

    public void StopPerkSelect() => SwitchState(_states.Play());

    public bool IsCurrentState<T>() where T : GameBaseState
    {
        return CurrentState is T;
    }
}

namespace GameStates
{
    public abstract class GameBaseState
    {
        protected GameStateFactory Factory;
        public GameBaseState(GameStateFactory factory) => Factory = factory;
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
    }

    public class GameStateFactory
    {
        private GameBaseState _mainMenu, _gameplay, _pause, _gameOver, _perkSelect;

        public GameBaseState MainMenu() => _mainMenu ??= new MainMenuState(this);
        public GameBaseState Play() => _gameplay ??= new GameplayState(this);
        public GameBaseState Pause() => _pause ??= new PauseState(this);
        public GameBaseState GameOver() => _gameOver ??= new GameOverState(this);
        public GameBaseState StartPerkSelect() => _perkSelect ??= new PerkSelectState(this);
    }

    public class MainMenuState : GameBaseState
    {
        public MainMenuState(GameStateFactory factory) : base(factory) { }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
        }
    }

    public class GameplayState : GameBaseState
    {
        public GameplayState(GameStateFactory factory) : base(factory) { }

        public override void EnterState()
        {
            Time.timeScale = 1f;
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GameManager.Instance.Pause();
            }
        }
    }

    public class PauseState : GameBaseState
    {
        public PauseState(GameStateFactory factory) : base(factory) { }

        public override void EnterState()
        {
            Time.timeScale = 0.0f;
            GameUI.Instance.SetMenuState("PauseMenu", true);
        }

        public override void ExitState()
        {
            Time.timeScale = 1.0f;
            GameUI.Instance.SetMenuState("PauseMenu", false);
        }

        public override void UpdateState()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GameManager.Instance.Resume();
            }
        }
    }

    public class PerkSelectState : GameBaseState
    {
        public PerkSelectState(GameStateFactory factory) : base(factory) { }

        public override void EnterState()
        {
            Time.timeScale = 0.0f;
        }

        public override void ExitState()
        {
            Time.timeScale = 1.0f;
        }

        public override void UpdateState()
        {
        }
    }

    public class GameOverState : GameBaseState
    {
        public GameOverState(GameStateFactory factory) : base(factory) { }

        public override void EnterState()
        {
            Time.timeScale = 0.0f;
            GameUI.Instance.SetMenuState("GameOverMenu", true);
        }

        public override void ExitState()
        {
            Time.timeScale = 1.0f;
            GameUI.Instance.SetMenuState("GameOverMenu", false);
        }

        public override void UpdateState()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GameManager.Instance.MainMenu();
            }
        }
    }
}