using UnityEngine;
using GameStates;
using UnityEngine.InputSystem;

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
    public GameBaseState CurrentState { get; private set; }
    private GameStateFactory _states;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        _states = new GameStateFactory();
        CurrentState = _states.Play();
        CurrentState.EnterState();
    }

    private void Update()
    {
        CurrentState?.UpdateState();
    }
    public void SwitchState(GameBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
        //GameEvents.RaiseGameStateChanged(CurrentState);
    }

    public void Play()
    {
        SceneTransitionManager.Instance.OpenScene(SceneTransitionManager.GAMEPLAY_SCENE);
        SwitchState(_states.Play());
    }

    public void MainMenu()
    {
        SceneTransitionManager.Instance.OpenScene(SceneTransitionManager.MAIN_MENU_SCENE);
    }

    public void Pause() => SwitchState(_states.Pause());

    public void Resume() => SwitchState(_states.Play());

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
        private GameBaseState _mainMenu, _gameplay, _pause, _gameOver;

        public GameBaseState MainMenu() => _mainMenu ??= new MainMenuState(this);
        public GameBaseState Play() => _gameplay ??= new GameplayState(this);
        public GameBaseState Pause() => _pause ??= new PauseState(this);
        public GameBaseState GameOver() => _gameOver ??= new GameOverState(this);
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void ExitState()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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