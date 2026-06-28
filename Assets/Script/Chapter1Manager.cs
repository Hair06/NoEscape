using UnityEngine;
using UnityEngine.Playables; 
using UnityEngine.InputSystem; // Thư viện Input bắt buộc để đọc trạng thái chuột trái
using ElmanGameDevTools.PlayerSystem;

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    private bool isPuzzleFinished = false;

    [Header("Tham chiếu UI Mini Game")]
    public GameObject puzzleMiniGameUI; 

    [Header("Tham chiếu Player Góc Nhìn Thứ Nhất")]
    public PlayerController playerController;     

    [Header("Cấu hình Cutscene Sau Khi Thắng")]
    [SerializeField] private PlayableDirector victoryCutsceneTimeline; 
    [SerializeField] private GameObject victoryCutsceneObject;

    // === HỆ THỐNG CON TRỎ CHUỘT CO NẮM BÀN TAY ===
    [Header("Cấu hình Con Trỏ Chuột Custom")]
    [Tooltip("Ảnh bàn tay xòe ra khi rê chuột bình thường")]
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

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        // Vừa vào game: Ép hệ thống dùng ảnh bàn tay mở làm con trỏ chuột luôn
        ResetCursorToDefaultHand();
    }

    private void Update()
    {
        // Kiểm tra xem con trỏ chuột có đang hiển thị trên màn hình không (khi mở UI/Mini-game)
        if (Cursor.visible && Mouse.current != null)
        {
            // Nếu người chơi nhấn GIỮ chuột trái -> Bàn tay nắm lại
            if (Mouse.current.leftButton.isPressed)
            {
                if (handClosedTexture != null)
                {
                    Cursor.SetCursor(handClosedTexture, Vector2.zero, CursorMode.Auto);
                }
            }
            // Nếu người chơi THẢ chuột trái ra -> Quay về bàn tay mở mặc định
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

            if (playerController != null) 
            {
                playerController.enabled = false;   
                playerController.LockCameraOnly(); 
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Đảm bảo lúc mở UI lên là bàn tay mở sẵn sàng
            ResetCursorToDefaultHand();
        }
    }

    public void ClosePuzzleGame(bool isWin = false)
    {
        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false); 

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

            if (playerController != null) 
            {
                playerController.UnlockCamera(); 
                playerController.enabled = true;   
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnCutsceneFinished()
    {
        Debug.Log("🎬 Cutscene kết thúc thành công! Trả lại quyền điều khiển cho Player.");
        
        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) 
        {
            playerController.UnlockCamera(); 
            playerController.enabled = true;   
        }
    }

    /// <summary>
    /// Hàm tiện ích để ép con trỏ chuột quay về hình bàn tay mở mặc định game
    /// </summary>
    public void ResetCursorToDefaultHand()
    {
        if (handOpenTexture != null)
        {
            Cursor.SetCursor(handOpenTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}