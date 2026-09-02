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
    [SerializeField] private string menuSceneName = "MainMenu"; // Đặt đúng tên Scene Menu của bạn

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

        // Bắt sự kiện Click nút Back To Menu bằng code nếu có gán Button
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMainMenu);
        }
    }

    /// <summary>
    /// Gọi hàm này từ bất kỳ Chapter nào để kết thúc game
    /// </summary>
    public void TriggerEndGame()
    {
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // 1. Phủ màn hình đen (dùng unscaledDeltaTime phòng trường hợp TimeScale bị chỉnh)
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
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // 2. Bật UI THE END (bao gồm cả nút Back to Menu)
        if (endGameUI != null)
        {
            endGameUI.SetActive(true);
        }

        // 3. Tắt lớp fadeImage cũ để nhường chỗ cho endGameUI
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

        // 4. Dừng Game Loop & Bật con trỏ chuột tương tác UI
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("== THE END ==");
    }

    /// <summary>
    /// Hàm gọi khi bấm nút Quay về Main Menu
    /// </summary>
    public void BackToMainMenu()
    {
        // 1. Phát tiếng click nút UI (nếu dự án dùng AudioManager Singleton)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // 2. Khôi phục lại tốc độ thời gian bình thường
        Time.timeScale = 1f;

        // 3. Mở con trỏ chuột cho Scene Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 4. Load Scene Main Menu
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("EndGameManager: Chưa gán tên Menu Scene trong Inspector!");
        }
    }
}