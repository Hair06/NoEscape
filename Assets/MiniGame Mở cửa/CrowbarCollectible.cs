using UnityEngine;

public class CrowbarCollectible : MonoBehaviour, IInteractable
{
    [Header("Tên item")]
    public string itemName = "Crowbar";

    [Header("Xà beng hiện trên tay Player")]
    [Tooltip("Kéo object xà beng gắn sẵn trên tay Player vào đây (để tắt sẵn trong Scene)")]
    [SerializeField] private GameObject crowbarInHand;

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int questSubQuestIndex = 0;
    [SerializeField, Min(1)] private int requiredProgress = 2;

    [Header("Raycast Settings")]
    [SerializeField] private float pickupRange = 3f;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip collectSound;

    private Camera playerCamera;
    private bool isLookingAt = false;

    private void Start()
    {
        playerCamera = Camera.main;

        // Dam bao luc dau xa beng tren tay dang an
        if (crowbarInHand != null) crowbarInHand.SetActive(false);
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            isLookingAt = false;
            return;
        }

        CheckLookAt();
    }

    private void CheckLookAt()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
            isLookingAt = hit.collider.gameObject == gameObject;
        else
            isLookingAt = false;
    }

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return "";

        return isLookingAt ? "Nhấn [E] để nhặt xà beng" : "";
    }

    public void Interact()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return;

        PlayerInventory.Add(itemName);

        // Hien xa beng tren tay Player
        if (crowbarInHand != null)
        {
            crowbarInHand.SetActive(true);
            Debug.Log("Đã cầm xà beng lên tay!");
        }

        ReportQuestProgress();
        Debug.Log("Đã nhặt Crowbar!");

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

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
}