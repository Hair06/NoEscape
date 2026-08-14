using UnityEngine;

public enum StoneType
{
    Blue,
    Red,
}

public class StonePickup : MonoBehaviour, IInteractable
{
    private const int ChapterIndex = 4;
    private const int FindStonesSubQuestIndex = 0;
    private const int RequiredStones = 2;

    [Header("Loại đá")]
    [SerializeField] private StoneType stoneType;

    [Header("Âm thanh nhặt")]
    [SerializeField] private AudioClip pickupClip;

    [Header("Chữ tương tác")]
    [SerializeField]
    private string bluePrompt = "Nhấn [E] để nhặt Đá Xanh";

    [SerializeField]
    private string redPrompt = "Nhấn [E] để nhặt Đá Đỏ";

    public static bool HasBlueStone { get; private set; }
    public static bool HasRedStone { get; private set; }

    private bool taken;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        HasBlueStone = false;
        HasRedStone = false;
    }

    public string GetInteractPrompt()
    {
        if (!CanPickup())
        {
            return "";
        }

        return stoneType == StoneType.Blue
            ? bluePrompt
            : redPrompt;
    }

    public void Interact()
    {
        if (!CanPickup())
        {
            return;
        }

        taken = true;

        string inventoryName;

        if (stoneType == StoneType.Blue)
        {
            HasBlueStone = true;
            inventoryName = "DaXanh";
        }
        else
        {
            HasRedStone = true;
            inventoryName = "DaDo";
        }

        PlayerInventory.Add(inventoryName);

        if (pickupClip != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupClip,
                transform.position
            );
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportProgressForChapter(
                ChapterIndex,
                FindStonesSubQuestIndex,
                1,
                RequiredStones
            );
        }

        Debug.Log("Đã nhặt " + GetStoneDisplayName() + ".");
        gameObject.SetActive(false);
    }

    public static void Consume(StoneType type)
    {
        if (type == StoneType.Blue)
        {
            HasBlueStone = false;
            PlayerInventory.RemoveAll("DaXanh");
            return;
        }

        HasRedStone = false;
        PlayerInventory.RemoveAll("DaDo");
    }

    private bool CanPickup()
    {
        QuestManager questManager = QuestManager.Instance;

        return !taken &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex ==
                   FindStonesSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }

    private string GetStoneDisplayName()
    {
        return stoneType == StoneType.Blue
            ? "Đá Xanh"
            : "Đá Đỏ";
    }
}
