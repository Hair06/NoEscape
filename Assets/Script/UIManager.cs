using UnityEngine;
using UnityEngine.SceneManagement;
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
        SceneManager.LoadScene("Cutscene1");
    }
       public void ExitGame()
    {
        Application.Quit();

        Debug.Log("Game Closed");
    }
}