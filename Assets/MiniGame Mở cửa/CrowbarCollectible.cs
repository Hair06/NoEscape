using UnityEngine;
using TMPro;

public class CrowbarCollectible : MonoBehaviour, IInteractable
{
    [Header("Tên item")]
    public string itemName = "Crowbar";

    [Header("Xà beng hiện trên tay Player")]
    [Tooltip("Kéo object xà beng gắn sẵn trên tay Player vào đây")]
    [SerializeField] private GameObject crowbarInHand;

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int questSubQuestIndex = 0;
    [SerializeField, Min(1)] private int requiredProgress = 2;

    [Header("Raycast Settings")]
    [SerializeField] private float pickupRange = 3.5f;
    [SerializeField] private LayerMask ignoreLayers; // Chọn layer của Player để Raycast không bị đụng vào người

    [Header("UI & Âm thanh")]
    [SerializeField] private TextMeshProUGUI promptText; // Kéo UI Text gợi ý vào đây
    [SerializeField] private AudioClip collectSound;

    private Camera playerCamera;
    private bool isLookingAt = false;
    private bool isPlayerNearby = false; // Nhận diện thêm bằng Trigger vùng đứng

    private void Start()
    {
        playerCamera = Camera.main;

        if (crowbarInHand != null) crowbarInHand.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 1. Kiểm tra Chapter active
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
        {
            isLookingAt = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
            return;
        }

        CheckLookAt();

        // 2. BẮT PHÍM [E] ĐỂ NHẶT (Đã bổ sung phần bị thiếu)
        if ((isLookingAt || isPlayerNearby) && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void CheckLookAt()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Raycast xuyên qua layer bị ignore
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ~ignoreLayers))
        {
            // Kiểm tra xem hit trúng chính nó HOẶC object con của nó
            isLookingAt = (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform));
        }
        else
        {
            isLookingAt = false;
        }

        // Cập nhật dòng chữ UI gợi ý
        if (promptText != null)
        {
            if (isLookingAt || isPlayerNearby)
            {
                promptText.text = GetInteractPrompt();
                promptText.gameObject.SetActive(true);
            }
            else if (!isPlayerNearby)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
            return "";

        return "Nhấn [E] để nhặt xà beng";
    }

    public void Interact()
    {
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
            return;

        PlayerInventory.Add(itemName);

        if (crowbarInHand != null)
        {
            crowbarInHand.SetActive(true);
            Debug.Log("Đã cầm xà beng lên tay!");
        }

        ReportQuestProgress();

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        if (promptText != null) promptText.gameObject.SetActive(false);

        Destroy(gameObject);
    }

    private void ReportQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("CrowbarCollectible: Không tìm thấy QuestManager.");
            return;
        }

        QuestManager.Instance.ReportProgressForChapter(
            questChapterIndex,
            questSubQuestIndex,
            1,
            requiredProgress
        );
    }

    // Tự động nhận diện khi Player đi vào vùng Trigger của xà beng
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}