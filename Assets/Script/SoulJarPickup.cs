using UnityEngine;

public class SoulJarPickup : MonoBehaviour, IInteractable
{
    private const int ChapterIndex = 4;
    private const int SoulJarSubQuestIndex = 2;
    public const string InventoryItemName = "BinhLinhHon";

    [Header("Chữ tương tác")]
    [SerializeField]
    private string interactPrompt =
        "Nhấn [E] để nhặt Bình Linh Hồn";

    [Header("Âm thanh và hiệu ứng")]
    [SerializeField] private AudioClip pickupClip;
    [Tooltip("Có thể gán prefab hoặc Particle System mẫu trong scene.")]
    [SerializeField] private ParticleSystem pickupVfxPrefab;

    private bool collected;

    public string GetInteractPrompt()
    {
        return CanCollect() ? interactPrompt : "";
    }

    public void Interact()
    {
        if (!CanCollect())
        {
            return;
        }

        collected = true;
        PlayerInventory.Add(InventoryItemName);

        if (pickupClip != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupClip,
                transform.position
            );
        }

        if (pickupVfxPrefab != null)
        {
            ParticleSystem spawnedVfx = Instantiate(
                pickupVfxPrefab,
                transform.position,
                pickupVfxPrefab.transform.rotation
            );

            spawnedVfx.Play();
            Destroy(
                spawnedVfx.gameObject,
                spawnedVfx.main.duration +
                spawnedVfx.main.startLifetime.constantMax
            );
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                SoulJarSubQuestIndex
            );
        }

        Debug.Log("Đã nhặt Bình Linh Hồn.");
        gameObject.SetActive(false);
    }

    private bool CanCollect()
    {
        QuestManager questManager = QuestManager.Instance;

        return !collected &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex ==
                   SoulJarSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }
}
