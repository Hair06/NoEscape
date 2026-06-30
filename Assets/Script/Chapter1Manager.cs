using UnityEngine;
using UnityEngine.Playables; 
using UnityEngine.InputSystem; // Thư viện bắt buộc để đọc tín hiệu nhấn giữ chuột trái
using ElmanGameDevTools.PlayerSystem; // Gọi hệ thống PlayerController góc nhìn thứ nhất

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    private bool isPuzzleFinished = false;
    private bool isMiniGameOpen = false; // Biến kiểm tra trạng thái mở mini-game

    [Header("Tham chiếu UI Mini Game")]
    public GameObject puzzleMiniGameUI; 

    [Header("Tham chiếu Player Góc Nhìn Thứ Nhất")]
    public PlayerController playerController;     

    [Header("Cấu hình Cutscene Sau Khi Thắng")]
    [SerializeField] private PlayableDirector victoryCutsceneTimeline; 
    [SerializeField] private GameObject victoryCutsceneObject;

    // === HỆ THỐNG HIỆU ỨNG CO NẮM BÀN TAY ===
    [Header("Cấu hình Con Trỏ Chuột Custom")]
    [Tooltip("Ảnh bàn tay xòe ra khi rê chuột đi lại bình thường")]
    [SerializeField] private Texture2D handOpenTexture; 
    [Tooltip("Ảnh bàn tay nắm chặt lại khi nhấn giữ chuột trái để kéo ảnh")]
    [SerializeField] private Texture2D handClosedTexture; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (puzzleMiniGameUI != null) puzzleMiniGameUI.SetActive(false);
        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(false);

        // Tự động tìm Player Controller góc nhìn thứ nhất đời mới trên Map
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        // Vừa vào game: Ép hệ thống dùng ảnh bàn tay mở làm con trỏ chuột mặc định luôn
        ResetCursorToDefaultHand();
    }

    private void Update()
    {
        // Chỉ xử lý hiệu ứng co/nắm ngón tay khi bảng Mini-game xếp hình đang mở
        if (!isMiniGameOpen) return;

        if (Mouse.current != null)
        {
            // Nếu người chơi nhấn GIỮ chuột trái để kéo thả mảnh tranh -> Bàn tay nắm lại
            if (Mouse.current.leftButton.isPressed)
            {
                if (handClosedTexture != null)
                {
                    Cursor.SetCursor(handClosedTexture, Vector2.zero, CursorMode.Auto);
                }
            }
            // Nếu người chơi THẢ chuột trái ra -> Quay về bàn tay mở xòe ngón bình thường
            else
            {
                if (handOpenTexture != null)
                {
                    Cursor.SetCursor(handOpenTexture, Vector2.zero, CursorMode.Auto);
                }
            }
        }
    }

    public void TryTriggerPuzzle()
    {
        if (isPuzzleFinished)
        {
            Debug.Log("Bạn đã hoàn thành phong ấn bức tranh này rồi, không thể mở lại!");
            return; 
        }

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
            isMiniGameOpen = true; // Kích hoạt bộ kiểm tra Update nhận diện bấm giữ chuột

            // KHÓA CỨNG PLAYER GÓC NHÌN THỨ NHẤT
            if (playerController != null) 
            {
                playerController.enabled = false;   // Khóa di chuyển WASD không cho đi lung tung
                playerController.LockCameraOnly(); // Khóa cứng camera chính, chuột di giải đố không bị lắc camera
            }

            // HIỆN CON TRỎ CHUỘT LÊN TRÊN MÀN HÌNH
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Đảm bảo lúc mới hiện lên là bàn tay xòe mở sẵn sàng
            ResetCursorToDefaultHand();
        }
    }

    public void ClosePuzzleGame(bool isWin = false)
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false); 
            isMiniGameOpen = false; // Tắt bộ kiểm tra chuột khi đóng bảng xếp hình

            // Trả con trỏ chuột về mặc định của hệ thống
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            if (isWin)
            {
                isPuzzleFinished = true;
                Debug.Log("Chúc mừng! Bạn đã thắng Mini-game. Đang chuyển giao phân cảnh Cutscene...");

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (victoryCutsceneTimeline != null) victoryCutsceneTimeline.Play();
                if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(true);

                return; 
            }

            // Trường hợp THOÁT NGANG XƯƠNG (Mở lại camera và WASD để đi tiếp)
            if (playerController != null) 
            {
                playerController.UnlockCamera(); // Nhả khóa camera quay nhìn bình thường
                playerController.enabled = true;   // Nhả khóa di chuyển WASD
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnCutsceneFinished()
    {
        Debug.Log("🎬 Cutscene kết thúc thành công! Trả lại quyền điều khiển cho Player.");
        
        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(false);
        
        // Ẩn chuột hoàn toàn khi quay lại góc nhìn chơi game bình thường
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) 
        {
            playerController.UnlockCamera(); 
            playerController.enabled = true;   
        }
    }

    /// <summary>
    /// Hàm tiện ích ép con trỏ chuột hiển thị hình dáng bàn tay mở
    /// </summary>
    public void ResetCursorToDefaultHand()
    {
        if (handOpenTexture != null)
        {
            Cursor.SetCursor(handOpenTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}