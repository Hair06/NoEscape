using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private AudioSource pickupSound;

    [Header("Chữ tương tác")]
    [SerializeField]
    private string bluePrompt = "Nhấn [E] để nhặt Đá Xanh";

    [SerializeField]
    private string redPrompt = "Nhấn [E] để nhặt Đá Đỏ";

    public static bool HasBlueStone { get; set; }
    public static bool HasRedStone { get; set; }

    private bool isPlayerNearby;
    private bool taken;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        HasBlueStone = false;
        HasRedStone = false;
    }

    private void Update()
    {
        if (!isPlayerNearby ||
            taken ||
            !MiniGameFlowManager.IsChapterActive(
                ChapterIndex) ||
            Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        InteractPickup();
    }

    public string GetInteractPrompt()
    {
        if (taken ||
            !MiniGameFlowManager.IsChapterActive(
                ChapterIndex))
        {
            return "";
        }

        return stoneType == StoneType.Blue
            ? bluePrompt
            : redPrompt;
    }

    public void Interact()
    {
        InteractPickup();
    }

    public void InteractPickup()
    {
        if (taken ||
            !MiniGameFlowManager.IsChapterActive(
                ChapterIndex))
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

        if (pickupSound != null)
        {
            pickupSound.Play();
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

        Debug.Log(
            "Đã nhặt " + GetStoneDisplayName() + "."
        );

        gameObject.SetActive(false);
    }

    private string GetStoneDisplayName()
    {
        return stoneType == StoneType.Blue
            ? "Đá Xanh"
            : "Đá Đỏ";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            !taken &&
            MiniGameFlowManager.IsChapterActive(
                ChapterIndex))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
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
}