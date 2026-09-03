using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager Instance { get; private set; }

    [Header("UI Tham Chiếu")]
    [Tooltip("Kéo Object UI chứa tấm ảnh THE END vào đây")]
    [SerializeField] private GameObject endGameUI;

    [Tooltip("Kéo Image Fade màn hình đen vào đây")]
    [SerializeField] private Image fadeImage;

    [Tooltip("Kéo Button 'Back to Menu' trên UI EndGame vào đây")]
    [SerializeField] private Button backToMenuButton;

    [Header("Cấu Hình")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private string menuSceneName = "SceneMenu"; // Đã đổi thành SceneMenu

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (endGameUI != null) endGameUI.SetActive(false);

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMainMenu);
        }
    }

    public void TriggerEndGame()
    {
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // 1. Phủ màn hình đen (Dùng unscaledDeltaTime để không bị đứng do timeScale)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float alpha = 0f;
            Color c = fadeImage.color;

            while (alpha < 1f)
            {
                alpha += Time.unscaledDeltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }
            
            c.a = 1f;
            fadeImage.color = c;
        }

        // Chờ 0.3 giây thực tế (Realtime)
        yield return new WaitForSecondsRealtime(0.3f);

        // 2. Bật UI THE END cố định
        if (endGameUI != null)
        {
            endGameUI.SetActive(true);
        }

        // 3. Tắt màn đen fadeImage đi ĐỂ LỘ ẢNH THE END BÊN DƯỚI
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

        // 4. Mở khóa chuột hoàn toàn và giữ vững màn hình
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tạm thời KHÔNG chỉnh Time.timeScale = 0f nếu game của bạn có script UI Update tự động ẩn Canvas khi Pause
        // Time.timeScale = 0f; 

        Debug.Log("== THE END ĐÃ HIỆN CỐ ĐỊNH ==");
    }

    public void BackToMainMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}