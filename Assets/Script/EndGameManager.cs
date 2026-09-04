using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager Instance { get; private set; }

    [Header("UI Tham Chiếu")]
    [Tooltip("Kéo Object Canvas THE END vào đây")]
    [SerializeField] private GameObject endGameUI;

    [Tooltip("Kéo Image Fade đen vào đây (Nếu không dùng thì để None)")]
    [SerializeField] private Image fadeImage;

    [Tooltip("Kéo Button Back to Menu vào đây")]
    [SerializeField] private Button backToMenuButton;

    [Header("Cấu Hình")]
    [SerializeField] private float fadeSpeed = 1.5f;

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
        if (fadeImage != null && fadeImage.gameObject != endGameUI) fadeImage.gameObject.SetActive(false);

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
        // 1. Phủ màn đen (Chỉ chạy nếu có gán fadeImage riêng biệt)
        if (fadeImage != null && fadeImage.gameObject != endGameUI)
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

        // 2. Bật UI THE END cố định
        if (endGameUI != null)
        {
            endGameUI.SetActive(true);
        }

        // 3. Bật con trỏ chuột tương tác nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        SceneManager.LoadScene("SceneMenu");
    }
}