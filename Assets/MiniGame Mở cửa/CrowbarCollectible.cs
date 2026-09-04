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
    [SerializeField] private LayerMask ignoreLayers;

    [Header("UI & Âm thanh")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private AudioClip collectSound;

    private Camera playerCamera;
    private bool isLookingAt = false;
    private bool isPlayerNearby = false;
    private bool isPickedUp = false; // Cờ chống nhặt trùng 2 lần

    private void Start()
    {
        playerCamera = Camera.main;

        if (crowbarInHand != null) crowbarInHand.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPickedUp) return; // Nếu đã nhặt rồi thì bỏ qua mọi logic Update

        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
        {
            isLookingAt = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
            return;
        }

        CheckLookAt();

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

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, ~ignoreLayers))
        {
            isLookingAt = (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform));
        }
        else
        {
            isLookingAt = false;
        }

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
        if (isPickedUp || !MiniGameFlowManager.IsChapterActive(questChapterIndex))
            return "";

        return "Nhấn [E] để nhặt xà beng";
    }

    public void Interact()
    {
        // Kiểm tra chống nhặt trùng
if (isPickedUp || !MiniGameFlowManager.IsChapterActive(questChapterIndex))
            return;

        isPickedUp = true; // Đánh dấu đã nhặt thành công ngay lập tức

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

    private void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && other.CompareTag("Player"))
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