using TMPro;
using UnityEngine;

// Gắn vào Trái Tim / Giọt Máu (vật phẩm Chương 3).
public class Chapter3ItemCollect : MonoBehaviour
{
    private const int ChapterIndex = 3;
    private const int CollectionSubQuestIndex = 3;
    private const int RequiredItems = 2;

    [Header("Cấu hình vật phẩm")]
    [Tooltip("Tên item trên hotbar: TraiTim hoặc GiotMau")]
    [SerializeField] private string itemName = "TraiTim";

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside;
    private bool taken;

    private void Start()
    {
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(ChapterIndex))
        {
            SetPromptVisible(false);
            return;
        }

        if (!isPlayerInside || taken)
        {
            return;
        }

        SetPromptVisible(true);

        if (GameInputBridge.GetKeyDown(KeyCode.E))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (taken)
        {
            return;
        }

        taken = true;

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(
                collectSound,
                transform.position
            );
        }

        PlayerInventory.Add(itemName);
        ReportQuestProgress();

        Debug.Log("Đã nhặt: " + GetDisplayName());

        SetPromptVisible(false);
        Destroy(gameObject);
    }

    private void ReportQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning(
                "Chapter3ItemCollect: Không tìm thấy QuestManager để cập nhật Chương 3."
            );
            return;
        }

        // Trái Tim và Giọt Máu luôn cùng báo về nhiệm vụ số 3.
        // Dùng hằng số để dữ liệu cũ trong Scene không thể trỏ nhầm sang nhiệm vụ số 2.
        QuestManager.Instance.ReportProgressForChapter(
            ChapterIndex,
            CollectionSubQuestIndex,
            1,
            RequiredItems
        );
    }

    private string GetDisplayName()
    {
        return itemName == "GiotMau"
            ? "Giọt Máu Giáo Phái"
            : "Trái Tim Giáo Phái";
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText == null)
        {
            return;
        }

        if (visible)
        {
            promptText.text =
                "Nhấn [E] để nhặt " + GetDisplayName();
        }

        promptText.gameObject.SetActive(visible);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerInside = true;

        if (!taken &&
            MiniGameFlowManager.IsChapterActive(ChapterIndex))
        {
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerInside = false;
        SetPromptVisible(false);
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }
}
