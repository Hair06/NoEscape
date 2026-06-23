using UnityEngine;
using UnityEngine.Playables; // Thư viện bắt buộc để điều khiển Timeline phim cắt cảnh

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    // Biến ghi nhớ xem người chơi đã giải xong mini-game này chưa
    private bool isPuzzleFinished = false;

    [Header("Tham chiếu UI Mini Game")]
    public GameObject puzzleMiniGameUI; // Kéo Canvas Mini-game vào đây

    [Header("Tham chiếu Player Invector")]
    public Invector.vCharacterController.vThirdPersonInput playerInputSystem;     

    [Header("Cấu hình Cutscene Sau Khi Thắng")]
    [Tooltip("Kéo Object chứa Timeline Playable Director làm Cutscene vào đây")]
    [SerializeField] private PlayableDirector victoryCutsceneTimeline; 
    [Tooltip("Kéo Object Canvas_Seal1Cutscene (Bức ảnh to đùng đè màn hình) vào đây")]
    [SerializeField] private GameObject victoryCutsceneObject;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Đảm bảo ẩn UI Mini-game khi mới vào màn chơi
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false);
        }

        // Mặc định ẩn phân cảnh Cutscene khi bắt đầu game
        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(false);
    }

    // Hàm gọi khi đến gần vị trí bàn/bức tranh nhiệm vụ và ấn E
    public void TryTriggerPuzzle()
    {
        // 1. Nếu đã hoàn thành phong ấn từ trước thì chặn đứng
        if (isPuzzleFinished)
        {
            Debug.Log("Bạn đã hoàn thành phong ấn bức tranh này rồi, không thể mở lại!");
            return; 
        }

        // 2. Kiểm tra số lượng mảnh ghép thu thập (CHỈ CHO CHƠI KHI ĐỦ 4 MẢNH)
        if (collectedPieces >= totalPiecesRequired)
        {
            OpenPuzzleGame();
        }
        else
        {
            Debug.Log($"Chưa tìm đủ số mảnh ảnh nhiệm vụ! Tiến độ hiện tại: {collectedPieces}/{totalPiecesRequired}");
        }
    }

    private void OpenPuzzleGame()
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(true); 

            // ĐÓNG BĂNG PLAYER: Tắt nhận phím và nhả khóa chuột của Invector
            if (playerInputSystem != null) 
            {
                playerInputSystem.enabled = false;
            }

            // HIỆN CON TRỎ CHUỘT
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Hàm đóng game và chuyển tiếp logic sang Cutscene
    public void ClosePuzzleGame(bool isWin = false)
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false); // Ẩn bảng UI xếp hình đi ngay lập tức

            // NẾU LÀ CHIẾN THẮNG (Tự động chuyển tiếp sang Cutscene)
            if (isWin)
            {
                isPuzzleFinished = true;
                Debug.Log("Chúc mừng! Bạn đã thắng Mini-game. Đang chuyển giao phân cảnh Cutscene...");

                // 1. Khóa và ẩn tâm chuột lại chuẩn bị xem phim
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // 2. KÍCH HOẠT PHIM CUTSCENE
                if (victoryCutsceneTimeline != null)
                {
                    victoryCutsceneTimeline.Play();
                }
                
                if (victoryCutsceneObject != null)
                {
                    victoryCutsceneObject.SetActive(true);
                }

                return; 
            }

            // Trường hợp nếu có nút thoát ngang xương (Bật lại điều khiển nhân vật)
            if (playerInputSystem != null) 
            {
                playerInputSystem.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // =========================================================================
    // HÀM SỬA ĐỔI CHÍNH: Chỉ sửa hàm này để tự động ẩn bức ảnh kẹt khi hết Cutscene
    // =========================================================================
    public void OnCutsceneFinished()
    {
        Debug.Log("🎬 Cutscene kết thúc thành công! Trả lại quyền điều khiển cho Player.");
        
        // Ép ẩn cái Canvas_Seal1Cutscene đè màn hình đi để người chơi tiếp tục chơi game
        if (victoryCutsceneObject != null)
        {
            victoryCutsceneObject.SetActive(false);
            Debug.Log("Đã ẩn bức ảnh Cutscene phóng to thành công.");
        }
        
        // Mở khóa cho phép nhân vật Invector tiếp tục đi lại bình thường
        if (playerInputSystem != null) 
        {
            playerInputSystem.enabled = true;
        }
    }
}