using UnityEngine;

public class CrowbarCollectible : MonoBehaviour, IInteractable
{
    [Header("Tên item")]
    public string itemName = "Crowbar";

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
    }

    private void Update()
    {
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
        return isLookingAt ? "Nhấn [E] để nhặt xà beng" : "";
    }

    public void Interact()
    {
        PlayerInventory.Add(itemName);
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
