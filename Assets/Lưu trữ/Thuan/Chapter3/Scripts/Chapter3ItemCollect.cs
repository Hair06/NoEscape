using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gắn vào Trái Tim / Giọt Máu (vật phẩm Chương 3).
public class Chapter3ItemCollect : MonoBehaviour
{
    [Header("Cấu hình vật phẩm")]
    [Tooltip("Tên item trên hotbar: TraiTim hoặc GiotMau")]
    [SerializeField] private string itemName = "TraiTim";

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [Tooltip("Trái Tim và Giọt Máu cùng thuộc nhiệm vụ khám phá mê cung.")]
    [SerializeField, Min(0)] private int questSubQuestIndex = 3;
    [SerializeField, Min(1)] private int requiredProgress = 2;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Trái Tim Giáo Phái";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside = false;
    private bool taken = false;

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        if (isPlayerInside && !taken
            && Keyboard.current != null
            && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (taken) return;
        taken = true;

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        PlayerInventory.Add(itemName);
        ReportQuestProgress();
        Debug.Log("Đã nhặt: " + itemName);

        if (promptText != null) promptText.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void ReportQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("Chapter3ItemCollect: Không tìm thấy QuestManager để cập nhật Chương 3.");
            return;
        }

        QuestManager.Instance.ReportProgressForChapter(
            questChapterIndex,
            questSubQuestIndex,
            1,
            requiredProgress
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            !taken &&
            MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = interactMessage;
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
