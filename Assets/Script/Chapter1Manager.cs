using UnityEngine;

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    [Header("Tham chiếu UI")]
    public GameObject puzzleMiniGameUI; // Kéo Canvas Mini-game vào đây

    [Header("Tham chiếu Player Invector")]
    // SỬA TẠI ĐÂY: Chỉ đích danh Script của Invector để Unity nhận diện được ngay
    public Invector.vCharacterController.vThirdPersonInput playerInputSystem;     

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm gọi khi đến gần vị trí nhiệm vụ và ấn E
    public void TryTriggerPuzzle()
    {
        // Nếu bạn muốn test nhanh không cần nhặt đủ 4 mảnh, hãy tạm thời bỏ điều kiện if này
        if (collectedPieces >= totalPiecesRequired)
        {
            OpenPuzzleGame();
        }
        else
        {
            Debug.Log("Chưa tìm đủ số mảnh ảnh nhiệm vụ!");
            // Để test nhanh tính năng đóng băng, bạn có thể gọi thẳng OpenPuzzleGame(); ở đây luôn
            OpenPuzzleGame(); 
        }
    }

    private void OpenPuzzleGame()
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(true); 

            // 1. ĐÓNG BĂNG PLAYER: Tắt toàn bộ hệ thống nhận phím di chuyển và quay chuột
            if (playerInputSystem != null) playerInputSystem.enabled = false;

            // 2. HIỆN CON TRỎ CHUỘT
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Hàm gọi khi bấm nút Thoát/Đóng trên UI Mini-game
    public void ClosePuzzleGame()
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false); // Ẩn bảng UI đi

            //1. MỞ BĂNG PLAYER: Bật lại điều khiển
            if (playerInputSystem != null) playerInputSystem.enabled = true;

            // 2. KHÓA LẠI CHUỘT
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}