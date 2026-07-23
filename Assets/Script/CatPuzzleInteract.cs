using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Thư viện để nhận diện TextMesh Pro

public class CatPuzzleInteract : MonoBehaviour
{
    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText; 
    [SerializeField] private string interactMessage = "Nhấn [E] để dịch chuyển tượng mèo";

    [Header("Tham chiếu Mảnh ảnh 2")]
    [SerializeField] private GameObject puzzlePiece2;       

    [Header("Cấu hình Di Chuyển Con Mèo")]
    [SerializeField] private Transform catTransform;         
    [Tooltip("Dịch chuyển theo hướng cục bộ của Mèo. X: Sang phải/trái, Y: Lên/xuống, Z: Tiến/lùi")]
    [SerializeField] private Vector3 localMoveOffset = new Vector3(0f, 0f, -0.6f); 
    [SerializeField] private float moveSpeed = 3f;           

    private bool isPlayerInside = false;
    private bool isInteracted = false;
    
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        // 1. Mặc định vừa vào game sẽ ẩn chữ gợi ý E đi
        if (promptText != null) promptText.gameObject.SetActive(false);
        
        // 2. Ý ĐỒ CỦA BẠN: Mặc định vừa vào game là mảnh ảnh 2 PHẢI ẨN ĐI hoàn toàn
        if (puzzlePiece2 != null)
        {
            puzzlePiece2.SetActive(false); 
        }

        if (catTransform != null)
        {
            initialPosition = catTransform.position;
            // Tính toán tọa độ đích đến của con mèo theo hướng cục bộ
            targetPosition = initialPosition + catTransform.TransformDirection(localMoveOffset);
        }
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(1))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        // 1. Đọc sự kiện nhấn nút E để kích hoạt dịch chuyển mèo
        if (isPlayerInside && !isInteracted && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerCatEvent();
        }

        // 2. Khi đã nhấn E, con mèo di chuyển VÀ mảnh ảnh 2 lập tức hiện ra
        if (isInteracted && catTransform != null)
        {
            // Di chuyển con mèo mượt mà bằng Lerp
            catTransform.position = Vector3.Lerp(catTransform.position, targetPosition, Time.deltaTime * moveSpeed);
            
            // BẬT ẢNH HIỆN RA NGAY KHI MÈO VỪA DI CHUYỂN (Không cần đợi mèo dừng lại hẳn)
            if (puzzlePiece2 != null && !puzzlePiece2.activeSelf)
            {
                puzzlePiece2.SetActive(true);
                Debug.Log("Mèo đang di chuyển! Mảnh ảnh 2 đã được hiện ra để chuẩn bị nhặt.");
            }

            // Khi mèo đã trượt sát sạt vị trí đích, khóa vị trí lại cho chuẩn
            if (Vector3.Distance(catTransform.position, targetPosition) < 0.01f)
            {
                catTransform.position = targetPosition;
            }
        }
    }

    private void TriggerCatEvent()
    {
        isInteracted = true;
        // Tương tác xong thì ẩn dòng chữ "Nhấn E để dịch chuyển" đi
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            !isInteracted &&
            MiniGameFlowManager.IsChapterActive(1))
        {
            isPlayerInside = true;
            if (promptText != null) 
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
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}
