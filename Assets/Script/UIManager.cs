using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public GameObject settingsPanel;

    // Mở setting
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Tắt setting
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    public void PlayGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        SceneManager.LoadScene("Cutscene1");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

        Debug.Log("Game Closed");
    }
}
