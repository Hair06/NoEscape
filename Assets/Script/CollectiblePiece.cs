using UnityEngine;
using UnityEngine.InputSystem; 
using TMPro; // Bắt buộc phải có để nhận diện TextMesh Pro

public class CollectiblePiece : MonoBehaviour
{
    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText; 
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt mảnh ảnh";

    [Header("Âm thanh khi nhặt")]
    [SerializeField] private AudioClip collectSound;

    [Header("Bán kính quét phát hiện Player đứng sẵn")]
    [Tooltip("Khoảng cách (mét) xung quanh mảnh giấy để tự động phát hiện người chơi")]
    [SerializeField] private float detectionRadius = 1.2f; 

    private bool isPlayerInside = false;
    private bool promptSuppressedUntilExit = false;

    private void Start()
    {
        // 1. Vừa vào game, đảm bảo tắt chữ hướng dẫn đi trước để tránh rác UI
        if (promptText != null) 
            promptText.gameObject.SetActive(false);
            
        // 2. TỰ ĐỘNG QUÉT: Dành cho mảnh để sẵn ở ngoài map (Mảnh 1, Mảnh 3)
        // Nếu vừa vào game mà Player đã đứng sát nó, chữ E sẽ hiện lên luôn
        CheckIfPlayerIsAlreadyInside();
    }

    private void OnEnable()
    {
        // 3. TỰ ĐỘNG KÍCH HOẠT: Dành cho mảnh ẩn giấu (Mảnh 2 ẩn sau con mèo hoặc ngăn kéo)
        // Khi con mèo/ngăn kéo đẩy ra và gọi SetActive(true), hàm này sẽ lập tiếp chạy để quét Player
        CheckIfPlayerIsAlreadyInside();
    }

    private void Update()
    {
        // Nhận diện phím E theo Input System mới, không lo xung đột Input cũ
        if (isPlayerInside && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            CollectThisPiece();
        }
    }

    public void SuppressPromptUntilExit()
    {
        promptSuppressedUntilExit = true;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void CollectThisPiece()
    {
        // === ĐOẠN CODE LIÊN KẾT MỚI: Tắt đèn Proximity Light trước khi xử lý nhặt ===
        ProximityLightGlow proximityLight = GetComponent<ProximityLightGlow>();
        if (proximityLight != null)
        {
            proximityLight.TurnOffLightOnPickup(); // Gọi hàm tắt đèn và khóa Update vĩnh viễn
        }

        // Cộng tiến độ nhiệm vụ vào Chapter1Manager toàn cục
        if (Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.collectedPieces++;
            Debug.Log($"Đã nhặt được mảnh ảnh! Tiến độ hiện tại: {Chapter1Manager.Instance.collectedPieces}/{Chapter1Manager.Instance.totalPiecesRequired}");
        }

        // Phát âm thanh sột soạt tại vị trí mảnh ảnh nếu có gán Audio
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Ẩn chữ gợi ý và phá hủy Object mảnh ảnh để hoàn thành việc nhặt
        if (promptText != null) promptText.gameObject.SetActive(false);
        
        // Phá hủy mảnh giấy (Point Light con nằm trong nó cũng sẽ bị tự động xóa theo sạch sẽ)
        if (ItemInfoUI.IsVisible) ItemInfoUI.Instance.HideInfo();
        Destroy(gameObject);
        if (QuestManager.Instance != null)
{
    QuestManager.Instance.CompleteSubQuest(0);
}
    }

    // --- XỬ LÝ VA CHẠM KHI NGƯỜI CHƠI DI CHUYỂN RA/VÀO VÙNG TRIGGER ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            promptSuppressedUntilExit = false;
            if (promptText != null) 
            {
                promptText.text = interactMessage; // Gán nội dung chữ hướng dẫn
                promptText.gameObject.SetActive(true); // Bật UI hiển thị công khai
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Đảm bảo trạng thái luôn sẵn sàng nhặt khi người chơi đang đứng trong vùng Collider
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            
            // Đề phòng trường hợp chữ bị script khác tắt mất, tự động kích hoạt lại UI
            if (!promptSuppressedUntilExit && promptText != null && !promptText.gameObject.activeSelf)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            promptSuppressedUntilExit = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }

    // --- THUẬT TOÁN FIX LỖI: QUÉT ĐA DIỆN 3D BẰNG HÌNH CẦU ---
    private void CheckIfPlayerIsAlreadyInside()
    {
        // Tạo một vùng quét hình cầu xung quanh mảnh ảnh với bán kính detectionRadius
        // Cách này loại bỏ hoàn toàn việc Sprite bị dẹt trục Z khiến hộp quét không chạm tới Player
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                isPlayerInside = true;
                promptSuppressedUntilExit = false;
                if (promptText != null) 
                {
                    promptText.text = interactMessage;
                    promptText.gameObject.SetActive(true);
                }
                break;
            }
        }
    }

    // Hiển thị một vòng tròn xanh lá trong cửa sổ Scene (chỉ Admin nhìn thấy) để dễ căn chỉnh khoảng cách quét
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
}
