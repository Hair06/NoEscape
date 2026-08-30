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

    [Header("Cấu Hình")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private string menuSceneName = "MainMenu"; // Đặt tên Scene Menu của bạn

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
        // 1. Tối màn hình (Fade to Black)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float alpha = 0f;
            Color c = fadeImage.color;

            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeImage.color = c;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Bật hình ảnh THE END
        if (endGameUI != null)
        {
            endGameUI.SetActive(true);
        }

        // 3. Mờ dần màn hình đen để hiện ảnh THE END
        if (fadeImage != null)
        {
            float alpha = 1f;
            Color c = fadeImage.color;

            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }
            fadeImage.gameObject.SetActive(false);
        }

        // 4. Mở khóa chuột và dừng thời gian Game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("== HOÀN THÀNH GAME (THE END) ==");
    }

    // Nút bấm chuyển về Main Menu (Nếu có tạo nút trên UI THE END)
    public void BackToMainMenu()
    {
        Time.timeScale = 1f; // Trả lại thời gian bình thường trước khi chuyển Scene
        SceneManager.LoadScene(menuSceneName);
    }
}