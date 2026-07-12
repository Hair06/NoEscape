using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Cấu hình UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 1.5f; // Tốc độ tối dần (giây)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Đảm bảo lúc đầu game màn hình không bị đen
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hàm kích hoạt màn hình tối dần, sau đó thực hiện hành động gì đó (như bật Cutscene)
    /// </summary>
    public void FadeToBlack(System.Action onFadeComplete)
    {
        if (fadeImage == null) return;
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeOutRoutine(onFadeComplete));
    }

    private IEnumerator FadeOutRoutine(System.Action onFadeComplete)
    {
        float alpha = 0f;
        Color c = fadeImage.color;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
            yield return null; // Chờ sang khung hình tiếp theo
        }

        // Đã tối đen hoàn toàn -> Kích hoạt hành động kế tiếp (Bật Cutscene)
        if (onFadeComplete != null)
        {
            onFadeComplete.Invoke();
        }
    }

    /// <summary>
    /// Hàm làm màn hình sáng lên trở lại sau khi hết Cutscene
    /// </summary>
    public void FadeFromBlack()
    {
        if (fadeImage == null) return;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
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
}