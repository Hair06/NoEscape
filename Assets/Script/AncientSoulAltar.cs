using UnityEngine;

public class AncientSoulAltar : MonoBehaviour, IInteractable
{
    private const int ChapterIndex = 4;
    private const int FinalSubQuestIndex = 3;

    [Header("Chữ tương tác")]
    [SerializeField]
    private string placePrompt =
        "Nhấn [E] để đặt Bình Linh Hồn lên bệ cổ";

    [SerializeField]
    private string missingPrompt =
        "Cần có Bình Linh Hồn";

    [Header("Model Bình Linh Hồn trên bệ")]
    [SerializeField] private GameObject soulJarVisual;

    [Header("Âm thanh và hiệu ứng")]
    [SerializeField] private AudioSource placementAudio;
    [SerializeField] private ParticleSystem altarVfx;

    [Header("Cutscene kết thúc Chương 4")]
    [Tooltip(
        "Cutscene sẽ hoàn thành nhiệm vụ cuối sau khi phát xong.")]
    [SerializeField]
    private MapSealCutscenePlayer finalCutscene;

    private bool activated;

    private void Start()
    {
        if (soulJarVisual != null)
        {
            soulJarVisual.SetActive(false);
        }
    }

    public string GetInteractPrompt()
    {
        if (!IsFinalStepActive())
        {
            return "";
        }

        return HasSoulJar()
            ? placePrompt
            : missingPrompt;
    }

    public void Interact()
    {
        if (!IsFinalStepActive() || !HasSoulJar())
        {
            return;
        }

        activated = true;
        PlayerInventory.RemoveAll(
            SoulJarPickup.InventoryItemName
        );

        if (soulJarVisual != null)
        {
            soulJarVisual.SetActive(true);
        }

        if (placementAudio != null)
        {
            placementAudio.Play();
        }

        if (altarVfx != null)
        {
            altarVfx.Play();
        }

        if (finalCutscene != null)
        {
            finalCutscene.PlayCutscene();
        }
        else
        {
            Debug.LogWarning(
                "AncientSoulAltar: Chưa gán Final Cutscene; " +
                "nhiệm vụ sẽ được hoàn thành trực tiếp."
            );

            CompleteFinalQuestFallback();
        }
    }

    private bool IsFinalStepActive()
    {
        QuestManager questManager = QuestManager.Instance;

        return !activated &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex ==
                   FinalSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }

    private static bool HasSoulJar()
    {
        return PlayerInventory.Count(
            SoulJarPickup.InventoryItemName
        ) > 0;
    }

    private static void CompleteFinalQuestFallback()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                FinalSubQuestIndex
            );
        }
    }
}
