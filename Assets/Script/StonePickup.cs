using UnityEngine;
using TMPro;

public enum StoneType
{
    Blue,
    Red,
}

public class StonePickup : MonoBehaviour, IInteractable
{
    [Header("Cấu hình Chapter & Quest")]
    [SerializeField] private int chapterIndex = 4;
    [SerializeField] private int findStonesSubQuestIndex = 0;
    [SerializeField] private int requiredStones = 2;

    [Header("Loại đá")]
    [SerializeField] private StoneType stoneType;

    [Header("Âm thanh nhặt")]
    [SerializeField] private AudioClip pickupClip;

    [Header("Chữ tương tác")]
    [SerializeField] private string bluePrompt = "Nhấn [E] để nhặt Đá Xanh";
    [SerializeField] private string redPrompt = "Nhấn [E] để nhặt Đá Đỏ";

    [Header("UI Gợi ý (Tùy chọn)")]
    [SerializeField] private TextMeshProUGUI promptText;

    public static bool HasBlueStone { get; private set; }
    public static bool HasRedStone { get; private set; }

    private bool taken;
    private bool isPlayerNearby;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        HasBlueStone = false;
        HasRedStone = false;
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Bắt phím E khi đứng gần đá
        if (isPlayerNearby && !taken && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public string GetInteractPrompt()
    {
        if (!CanPickup())
        {
            return "";
        }

        return stoneType == StoneType.Blue ? bluePrompt : redPrompt;
    }

    public void Interact()
    {
        if (!CanPickup())
        {
            Debug.LogWarning($"[StonePickup] Chưa đủ điều kiện nhặt viên đá {GetStoneDisplayName()} (Kiểm tra lại Chapter/Quest Active)");
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
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportProgressForChapter(
                chapterIndex,
                findStonesSubQuestIndex,
                1,
                requiredStones
            );
        }

        if (promptText != null) promptText.gameObject.SetActive(false);

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
        if (taken) return false;

        // Cho phép nhặt nếu Chapter tương ứng đang active
        if (MiniGameFlowManager.IsChapterActive(chapterIndex))
        {
            return true;
        }

        // Nếu QuestManager chưa khởi chạy hoàn chỉnh thì vẫn cho phép nhặt thử nghiệm
        if (QuestManager.Instance == null) return true;

        return QuestManager.Instance.CurrentChapterIndex == chapterIndex;
    }

    private string GetStoneDisplayName()
    {
        return stoneType == StoneType.Blue ? "Đá Xanh" : "Đá Đỏ";
    }

    // Tự động bật UI và nhận diện khi Player đứng gần viên đá
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CanPickup())
        {
            isPlayerNearby = true;
            if (promptText != null)
            {
                promptText.text = GetInteractPrompt();
                promptText.gameObject.SetActive(true);
            }
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