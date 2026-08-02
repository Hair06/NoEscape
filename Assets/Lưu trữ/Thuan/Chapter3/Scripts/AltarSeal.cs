using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Bàn thờ có phong ấn Chương 3.
// Đặt đủ 4 vật phẩm sẽ hoàn thành nhiệm vụ và chạy cutscene.
public class AltarSeal : MonoBehaviour
{
    public static AltarSeal Instance;

    private static readonly string[] REQUIRED_ITEMS =
    {
        "ConMat",
        "KiTu",
        "TraiTim",
        "GiotMau"
    };

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int altarSubQuestIndex = 4;
    [SerializeField, Min(1)] private int requiredItemCount = 4;

    [Header("Trạng thái 4 vật phẩm (chỉ xem)")]
    [SerializeField] private bool hasConMat = false;
    [SerializeField] private bool hasKiTu = false;
    [SerializeField] private bool hasTraiTim = false;
    [SerializeField] private bool hasGiotMau = false;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string notReadyMessage = "Bàn thờ cổ... còn thiếu vật phẩm";
    [SerializeField] private string placeMessage = "Nhấn [E] để đặt vật phẩm lên bàn thờ";

    [Header("Model vật phẩm hiện trên bàn thờ")]
    [SerializeField] private GameObject conMatVisual;
    [SerializeField] private GameObject kiTuVisual;
    [SerializeField] private GameObject traiTimVisual;
    [SerializeField] private GameObject giotMauVisual;

    [Header("Âm thanh & Hiệu ứng khi hoàn thành")]
    [SerializeField] private AudioSource placeAudio;
    [SerializeField] private AudioSource sealCompleteAudio;
    [SerializeField] private ParticleSystem sealVFX;

    [Header("Xích cửa mở ra khi đủ 4 vật phẩm")]
    [SerializeField] private GameObject doorChain;

    [Header("Cutscene kết thúc Chapter 3")]
    [SerializeField] private MapSealCutscenePlayer endCutscene;

    [Header("Hiệu ứng tối dần trước cutscene")]
    [Tooltip("Kéo tấm Image đen phủ màn hình (FadeScreen) vào đây")]
    [SerializeField] private Image fadeImage;
    [Tooltip("Tốc độ tối/sáng dần. Số nhỏ = chậm hơn")]
    [SerializeField] private float fadeSpeed = 1f;
    [Tooltip("Thời gian giữ màn hình đen (giây) trước khi vào cutscene")]
    [SerializeField] private float holdBlackTime = 1.5f;

    private bool isPlayerInside = false;
    private bool isComplete = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (conMatVisual != null) conMatVisual.SetActive(false);
        if (kiTuVisual != null) kiTuVisual.SetActive(false);
        if (traiTimVisual != null) traiTimVisual.SetActive(false);
        if (giotMauVisual != null) giotMauVisual.SetActive(false);

        // Đảm bảo tấm đen trong suốt và tắt lúc đầu
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        if (isComplete || !isPlayerInside) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            PlaceItems();
    }

    private void PlaceItems()
    {
        int placedThisTime = 0;

        foreach (string item in REQUIRED_ITEMS)
        {
            if (PlayerInventory.Count(item) > 0 && !IsPlaced(item))
            {
                MarkPlaced(item);
                PlayerInventory.RemoveAll(item);
                placedThisTime++;
                Debug.Log("Đã đặt vật phẩm lên bàn thờ: " + item);
            }
        }

        if (placedThisTime == 0)
        {
            Debug.Log("Bạn không cầm vật phẩm nào để đặt.");
            return;
        }

        if (placeAudio != null) placeAudio.Play();

        int totalPlaced = CountPlaced();
        ReportAltarProgress(placedThisTime);
        Debug.Log($"Tiến độ phong ấn: {totalPlaced}/{requiredItemCount}");

        if (promptText != null) promptText.text = GetCurrentPrompt();

        if (totalPlaced >= requiredItemCount) CompleteSeal();
    }

    private void ReportAltarProgress(int placedThisTime)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("AltarSeal: Không tìm thấy QuestManager để cập nhật Chương 3.");
            return;
        }

        QuestManager.Instance.ReportProgressForChapter(
            questChapterIndex,
            altarSubQuestIndex,
            placedThisTime,
            requiredItemCount
        );
    }

    private bool IsPlaced(string item)
    {
        switch (item)
        {
            case "ConMat": return hasConMat;
            case "KiTu": return hasKiTu;
            case "TraiTim": return hasTraiTim;
            case "GiotMau": return hasGiotMau;
            default: return false;
        }
    }

    private void MarkPlaced(string item)
    {
        switch (item)
        {
            case "ConMat":
                hasConMat = true;
                if (conMatVisual != null) conMatVisual.SetActive(true);
                break;
            case "KiTu":
                hasKiTu = true;
                if (kiTuVisual != null) kiTuVisual.SetActive(true);
                break;
            case "TraiTim":
                hasTraiTim = true;
                if (traiTimVisual != null) traiTimVisual.SetActive(true);
                break;
            case "GiotMau":
                hasGiotMau = true;
                if (giotMauVisual != null) giotMauVisual.SetActive(true);
                break;
        }
    }

    private int CountPlaced()
    {
        int count = 0;
        if (hasConMat) count++;
        if (hasKiTu) count++;
        if (hasTraiTim) count++;
        if (hasGiotMau) count++;
        return count;
    }

    public bool IsComplete()
    {
        return hasConMat && hasKiTu && hasTraiTim && hasGiotMau;
    }

    private void CompleteSeal()
    {
        isComplete = true;
        Debug.Log("ĐỦ 4 VẬT PHẨM! Phong ấn đã được mở.");

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (sealCompleteAudio != null) sealCompleteAudio.Play();

        if (sealVFX != null)
        {
            sealVFX.gameObject.SetActive(true);
            sealVFX.Play();
        }

        if (doorChain != null) doorChain.SetActive(false);

        // Chạy chuỗi: tối dần -> giữ đen -> cutscene -> sáng dần
        StartCoroutine(FadeThenPlayCutscene());
    }

    private IEnumerator FadeThenPlayCutscene()
    {
        // 1. Màn hình tối dần
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

        // 2. Giữ màn hình đen một lúc cho có nhịp
        yield return new WaitForSecondsRealtime(holdBlackTime);

        // 3. Khóa chuột chuẩn bị xem phim
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 4. Bắt đầu cutscene
        if (endCutscene != null) endCutscene.PlayCutscene();

        // 5. Màn hình sáng dần lại để xem cutscene
        if (fadeImage != null)
        {
            float alpha = 1f;
            Color c = fadeImage.color;

            while (alpha > 0f)
            {
                alpha -= Time.unscaledDeltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }

            fadeImage.gameObject.SetActive(false);
        }
    }

    private string GetCurrentPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return "";

        if (isComplete) return "";

        int holding = 0;
        foreach (string item in REQUIRED_ITEMS)
        {
            if (PlayerInventory.Count(item) > 0 && !IsPlaced(item)) holding++;
        }

        return holding > 0 ? placeMessage : notReadyMessage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            !isComplete &&
            MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = GetCurrentPrompt();
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}