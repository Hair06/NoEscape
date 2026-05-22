using UnityEngine;

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
}