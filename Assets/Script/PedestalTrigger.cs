using UnityEngine;
using TMPro;

public class PedestalTrigger : MonoBehaviour, IInteractable
{
    public enum PedestalType
    {
        BluePedestal,
        RedPedestal,
    }

    [Header("Loại bệ đá")]
    [SerializeField] private PedestalType pedestalType;

    [Header("Tham chiếu câu đố")]
    [SerializeField] private StoneDoorPuzzle puzzleManager;

    [Header("UI Gợi ý (Kéo Text UI vào nếu muốn)")]
    [SerializeField] private TextMeshProUGUI promptText;

    private bool isPlayerNearby;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Tự động bắt phím [E] khi Player đi vào vùng bệ đá
        if (isPlayerNearby && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public string GetInteractPrompt()
    {
        // 1. Kiểm tra trạng thái Puzzle
        if (puzzleManager == null) return "";

        // Cho phép tương tác nếu bước đặt đá active (hoặc nếu bệ đá chưa hoàn thành)
        if (!puzzleManager.IsPlacementStepActive())
        {
            // Nếu puzzleManager bị kẹt bước, tạm thời kiểm tra xem đã đặt đá chưa
            if (pedestalType == PedestalType.BluePedestal && puzzleManager.IsBluePlaced) return "";
            if (pedestalType == PedestalType.RedPedestal && puzzleManager.IsRedPlaced) return "";
        }

        // 2. Bệ Đá Xanh
        if (pedestalType == PedestalType.BluePedestal)
        {
            if (puzzleManager.IsBluePlaced) return "";

            bool hasBlue = StonePickup.HasBlueStone || PlayerInventory.Count("DaXanh") > 0;
            return hasBlue ? "Nhấn [E] để đặt Đá Xanh" : "Cần Đá Xanh cho bệ này";
        }

        // 3. Bệ Đá Đỏ
        if (puzzleManager.IsRedPlaced) return "";

        bool hasRed = StonePickup.HasRedStone || PlayerInventory.Count("DaDo") > 0;
        return hasRed ? "Nhấn [E] để đặt Đá Đỏ" : "Cần Đá Đỏ cho bệ này";
    }

    public void Interact()
    {
        if (puzzleManager == null)
        {
            Debug.LogWarning("PedestalTrigger: Chưa gán StoneDoorPuzzle Manager!");
            return;
        }

        if (pedestalType == PedestalType.BluePedestal)
        {
            if (puzzleManager.IsBluePlaced) return;

            bool hasBlue = StonePickup.HasBlueStone || PlayerInventory.Count("DaXanh") > 0;
            if (hasBlue)
            {
                puzzleManager.TryPlaceBlueStone();
                if (promptText != null) promptText.gameObject.SetActive(false);
                Debug.Log("Đã gọi TryPlaceBlueStone thành công!");
            }
            else
            {
                Debug.Log("Không thể đặt: Chưa có Đá Xanh trong túi!");
            }
        }
        else
        {
            if (puzzleManager.IsRedPlaced) return;

            bool hasRed = StonePickup.HasRedStone || PlayerInventory.Count("DaDo") > 0;
            if (hasRed)
            {
                puzzleManager.TryPlaceRedStone();
                if (promptText != null) promptText.gameObject.SetActive(false);
                Debug.Log("Đã gọi TryPlaceRedStone thành công!");
            }
            else
            {
                Debug.Log("Không thể đặt: Chưa có Đá Đỏ trong túi!");
            }
        }
    }

    // Nhận diện Player đi lại gần bệ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (promptText != null)
            {
                string prompt = GetInteractPrompt();
                if (!string.IsNullOrEmpty(prompt))
                {
                    promptText.text = prompt;
                    promptText.gameObject.SetActive(true);
                }
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