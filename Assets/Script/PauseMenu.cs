using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private InputSystem_Actions input;
    private bool isPaused;

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
        pausePanel.SetActive(false);
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0 : 1;

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ?
            CursorLockMode.None :
            CursorLockMode.Locked;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene("SceneMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}