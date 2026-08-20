using System.Collections;
using TMPro;
using UnityEngine;

public class RitualAltar : MonoBehaviour
{
    private const int ChapterIndex = 4;
    private const int FinalSubQuestIndex = 3;

    [Header("Các đối tượng liên quan")]
    public GameObject phongAn;
    public GameObject jarOnAltar;
    public ParticleSystem soulVFX;

    [Header("Cấu hình hiệu ứng")]
    [Tooltip("Thời gian hiệu ứng linh hồn chạy trước khi bắt đầu Cutscene 4")]
    public float soulVFXDuration = 5f;

    [Header("Giao diện UI")]
    public TextMeshProUGUI promptText;
    public string promptMessage =
        "Nhấn [E] để đặt Bình Linh Hồn lên Bệ Cổ";

    [Header("Cutscene kết thúc Chương 4")]
    [Tooltip("Kéo object chứa MapSealCutscenePlayer của cutscene kết thúc Chương 4 vào đây.")]
    [SerializeField] private MapSealCutscenePlayer endCutscene;

    private bool hasJar;
    private bool isPlaced;
    private bool isNearPlayer;
    private bool isPlayingEnding;

    public bool IsConfiguredForSoulJar =>
        phongAn != null ||
        jarOnAltar != null ||
        soulVFX != null;

    private void Start()
    {
        if (phongAn != null)
        {
            phongAn.SetActive(false);
        }

        if (jarOnAltar != null)
        {
            jarOnAltar.SetActive(false);
        }

        if (soulVFX != null)
        {
            soulVFX.Stop();
        }

        SetPromptVisible(false);
    }

    // Được SoulJar gọi đúng một lần khi người chơi nhặt bình.
    public void OnPickUpJar()
    {
        if (hasJar || isPlaced)
        {
            return;
        }

        hasJar = true;

        if (phongAn != null)
        {
            phongAn.SetActive(true);
        }

        RefreshPrompt();

        Debug.Log(
            "[RitualAltar] Đã nhận Bình Linh Hồn; bật chỉ dẫn về Bệ Cổ."
        );
    }

    private void Update()
    {
        // Không cho tương tác trong lúc đang chạy hiệu ứng kết thúc.
        if (isPlayingEnding)
        {
            return;
        }

        // Scene hiện có nhiều RitualAltar dùng chung PromptText.
        // Chỉ altar mà Player đang đứng gần mới được điều khiển prompt.
        if (!isNearPlayer)
        {
            return;
        }

        RefreshPrompt();

        if (CanPlaceJar() &&
            GameInputBridge.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PlaceJarRoutine());
        }
    }

    private bool CanPlaceJar()
    {
        QuestManager questManager = QuestManager.Instance;

        return hasJar &&
               !isPlaced &&
               !isPlayingEnding &&
               isNearPlayer &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex == FinalSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }

    private IEnumerator PlaceJarRoutine()
    {
        if (!CanPlaceJar())
        {
            yield break;
        }

        // Khóa không cho người chơi nhấn E lần nữa.
        isPlaced = true;
        isPlayingEnding = true;

        // Ẩn prompt ngay lập tức.
        SetPromptVisible(false);

        // Xóa Bình Linh Hồn khỏi hotbar vì đã đặt lên bệ.
        PlayerInventory.RemoveAll("BinhLinhHon");

        // Tắt bình đang cầm / vật thể cũ.
        if (phongAn != null)
        {
            phongAn.SetActive(false);
        }

        // Hiển thị Bình Linh Hồn trên Bệ Cổ.
        if (jarOnAltar != null)
        {
            jarOnAltar.SetActive(true);
        }

        Debug.Log(
            "[RitualAltar] Đã đặt Bình Linh Hồn lên Bệ Cổ."
        );

        // =========================================================
        // CHẠY HIỆU ỨNG LINH HỒN
        // =========================================================

        if (soulVFX != null)
        {
            soulVFX.Play();

            Debug.Log(
                "[RitualAltar] Bắt đầu hiệu ứng linh hồn. " +
                "Chờ " + soulVFXDuration + " giây trước Cutscene 4."
            );

            // Chờ đúng thời gian hiệu ứng.
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, soulVFXDuration)
            );

            // Dừng hiệu ứng sau khi hết thời gian.
            soulVFX.Stop();

            Debug.Log(
                "[RitualAltar] Hiệu ứng linh hồn kết thúc."
            );
        }
        else
        {
            Debug.LogWarning(
                "[RitualAltar] Soul VFX chưa được gán. " +
                "Cutscene 4 sẽ bắt đầu ngay."
            );
        }

        // =========================================================
        // SAU KHI HIỆU ỨNG KẾT THÚC -> BẮT ĐẦU CUTSCENE 4
        // =========================================================

        if (endCutscene != null)
        {
            Debug.Log(
                "[RitualAltar] Bắt đầu Cutscene 4."
            );

            endCutscene.PlayFinalChapterCutscene();
        }
        else
        {
            Debug.LogWarning(
                "[RitualAltar] Chưa gán End Cutscene. " +
                "Nhiệm vụ cuối sẽ được hoàn thành trực tiếp."
            );

            CompleteFinalQuestFallback();
        }

        isPlayingEnding = false;
    }

    private static void CompleteFinalQuestFallback()
    {
        if (QuestManager.Instance == null)
        {
            return;
        }

        QuestManager.Instance.CompleteSubQuestForChapter(
            ChapterIndex,
            FinalSubQuestIndex
        );
    }

    private void RefreshPrompt()
    {
        SetPromptVisible(CanPlaceJar());
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText == null)
        {
            return;
        }

        if (visible)
        {
            promptText.text = promptMessage;
        }

        promptText.gameObject.SetActive(visible);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = true;
            RefreshPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isNearPlayer = false;
        SetPromptVisible(false);
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }
}