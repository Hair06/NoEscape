using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    public static bool IsPaused { get; private set; }

    private InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();

        input.UI.Esc.performed += ctx =>
        {
            TogglePause();
        };
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        MiniGameFlowManager.ApplyPauseState(IsPaused);

        pausePanel.SetActive(IsPaused);

        Time.timeScale = IsPaused ? 0 : 1;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGamePaused(IsPaused);
        }

        ApplyCursorState();
    }

    private void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGamePaused(false);
        }
    }

    private void ApplyCursorState()
    {
        bool needsCursor =
            IsPaused ||
            MiniGameFlowManager.HasActiveMiniGame;

        Cursor.visible = needsCursor;
        Cursor.lockState = needsCursor ?
            CursorLockMode.None :
            CursorLockMode.Locked;
    }



    public void ExitToMenu()
    {
        IsPaused = false;
        Time.timeScale = 1;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGamePaused(false);
        }

        SceneManager.LoadScene("SceneMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
  
}
