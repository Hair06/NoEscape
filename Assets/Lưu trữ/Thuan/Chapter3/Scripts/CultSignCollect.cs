using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Gắn vào object Ký Tự Giáo Phái (hiddenSignObject).
// Chỉ nhặt được khi đang soi bằng Con Mắt (giữ chuột phải).
public class CultSignCollect : MonoBehaviour
{
    [Header("Cấu hình vật phẩm")]
    [Tooltip("Tên item trên hotbar - phải khớp AltarSeal")]
    [SerializeField] private string itemName = "KiTu";
    [SerializeField] private float pickupDistance = 3f;

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int questSubQuestIndex = 2;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Ký Tự Giáo Phái";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    private Transform playerTransform;
    private bool taken = false;
    private bool promptShowing = false;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            ShowPrompt(false);
            return;
        }

        if (taken || playerTransform == null || !gameObject.activeInHierarchy) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= pickupDistance)
        {
            ShowPrompt(true);

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                CollectItem();
        }
        else
        {
            ShowPrompt(false);
        }
    }

    private void ShowPrompt(bool state)
    {
        if (promptShowing == state) return;
        promptShowing = state;

        if (promptText != null)
        {
            if (state) promptText.text = interactMessage;
            promptText.gameObject.SetActive(state);
        }
    }

    private void OnDisable()
    {
        ShowPrompt(false);
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

        ShowPrompt(false);
        Destroy(gameObject);
    }

    private void ReportQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("CultSignCollect: Không tìm thấy QuestManager để cập nhật Chương 3.");
            return;
        }

        QuestManager.Instance.CompleteSubQuestForChapter(
            questChapterIndex,
            questSubQuestIndex
        );
    }
}
