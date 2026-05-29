using UnityEngine;

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    // BIẾN MỚI: Dùng để ghi nhớ xem người chơi đã giải xong mini-game này chưa
    private bool isPuzzleFinished = false;

    [Header("Tham chiếu UI")]
    public GameObject puzzleMiniGameUI; // Kéo Canvas Mini-game vào đây

    [Header("Tham chiếu Player Invector")]
    public Invector.vCharacterController.vThirdPersonInput playerInputSystem;     

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm gọi khi đến gần vị trí nhiệm vụ và ấn E
    public void TryTriggerPuzzle()
    {
        // THÊM KIỂM TRA: Nếu đã hoàn thành nhiệm vụ này rồi thì chặn đứng, không cho hiện lại nữa
        if (isPuzzleFinished)
        {
            Debug.Log("Bạn đã hoàn thành phong ấn bức tranh này rồi, không thể mở lại!");
            return; 
        }

        // Kiểm tra số lượng mảnh ghép thu thập
        if (collectedPieces >= totalPiecesRequired)
        {
            OpenPuzzleGame();
        }
        else
        {
            Debug.Log("Chưa tìm đủ số mảnh ảnh nhiệm vụ!");
            // Giữ lại dòng test nhanh của bạn
            OpenPuzzleGame(); 
        }
    }

    private void OpenPuzzleGame()
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(true); 

            // 1. ĐÓNG BĂNG PLAYER: Tắt nhận phím và nhả khóa chuột của Invector
            if (playerInputSystem != null) 
            {

                playerInputSystem.enabled = false;
            }

            // 2. HIỆN CON TRỎ CHUỘT
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // THAY ĐỔI: Thêm tham số 'isWin' vào hàm để phân biệt giữa việc bấm nút "Thoát" và việc "Win game"
    public void ClosePuzzleGame(bool isWin = false)
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false); // Ẩn bảng UI đi

            // NẾU LÀ CHIẾN THẮNG: Khóa vĩnh viễn không cho ấn E mở lại nữa
            if (isWin)
            {
                isPuzzleFinished = true;
                Debug.Log("Chúc mừng! Mini-game đã bị khóa vĩnh viễn vì bạn đã thắng.");
            }

            // 1. MỞ BĂNG PLAYER: Bật lại điều khiển và khóa tâm chuột
            if (playerInputSystem != null) 
            {
                playerInputSystem.enabled = true;
            }

            // 2. KHÓA LẠI CHUỘT THEO CHUẨN UNITY
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}