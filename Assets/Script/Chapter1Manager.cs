using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using ElmanGameDevTools.PlayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Chapter1Manager : MonoBehaviour
{
    public static Chapter1Manager Instance;

    [Header("Tiến trình Nhiệm vụ")]
    public int collectedPieces = 0;
    public int totalPiecesRequired = 4;

    private readonly HashSet<string> collectedPieceIds =
        new HashSet<string>();

    private bool isPuzzleFinished = false;

    [Header("Tham chiếu UI Mini Game")]
    public GameObject puzzleMiniGameUI;

    [Header("Tham chiếu Player Góc Nhìn Thứ Nhất")]
    public PlayerController playerController;

    [Header("Cấu hình Cutscene Sau Khi Thắng")]
    [SerializeField] private PlayableDirector victoryCutsceneTimeline;
    [SerializeField] private GameObject victoryCutsceneObject;

    [Header("Cấu hình Con Trỏ Chuột Custom")]
    [Tooltip("Ảnh bàn tay xòe ra khi rê chuột bình thường")]
    [SerializeField] private Texture2D handOpenTexture;
    [Tooltip("Ảnh bàn tay nắm chặt lại khi nhấn giữ chuột trái để kéo ảnh")]
    [SerializeField] private Texture2D handClosedTexture;
    [Tooltip("Tọa độ điểm nhấp trên con trỏ chuột (Ví dụ X: 32, Y: 10 cho đầu ngón trỏ)")]
    [SerializeField] private Vector2 cursorHotspot = new Vector2(32f, 10f);

    [Header("CẤU HÌNH HIỆU ỨNG CHUYỂN CẢNH (FADE EFFECT)")]
    [Tooltip("Kéo Object ảnh đen (FadeScreen) vào đây")]
    [SerializeField] private Image fadeImage;
    [Tooltip("Tốc độ tối dần của màn hình")]
    [SerializeField] private float fadeSpeed = 1.5f;
    [Tooltip("Thời gian giữ màn hình đen (giây) trước khi vào cutscene")]
    [SerializeField] private float holdBlackTime = 1.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (puzzleMiniGameUI != null) puzzleMiniGameUI.SetActive(false);
        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(false);

        // Đảm bảo lúc đầu game ảnh đen tàng hình
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        ResetCursorToDefaultHand();
    }

    private void Update()
    {
        if (Cursor.visible && Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                if (handClosedTexture != null)
                {
                    // Đã sửa Vector2.zero -> cursorHotspot (32, 10)
                    Cursor.SetCursor(handClosedTexture, cursorHotspot, CursorMode.Auto);
                }
            }
            else
            {
                if (handOpenTexture != null)
                {
                    // Đã sửa Vector2.zero -> cursorHotspot (32, 10)
                    Cursor.SetCursor(handOpenTexture, cursorHotspot, CursorMode.Auto);
                }
            }
        }
    }

    public bool TryTriggerPuzzle()
    {
        if (isPuzzleFinished)
        {
            Debug.Log("Bạn đã hoàn thành phong ấn bức tranh này rồi, không thể mở lại!");
            return false;
        }

        if (collectedPieces >= totalPiecesRequired)
        {
            return OpenPuzzleGame();
        }

        Debug.Log($"Chưa tìm đủ số mảnh ảnh nhiệm vụ! Tiến độ hiện tại: {collectedPieces}/{totalPiecesRequired}");
        return false;
    }

    public bool RegisterCollectedPiece(string pieceId)
    {
        string safeId = string.IsNullOrWhiteSpace(pieceId)
            ? "Piece_" + (collectedPieces + 1)
            : pieceId.Trim();

        if (!collectedPieceIds.Add(safeId))
        {
            Debug.LogWarning("Mảnh ảnh đã được tính trước đó: " + safeId);
            return false;
        }

        collectedPieces = Mathf.Min(
            collectedPieces + 1,
            totalPiecesRequired
        );

        Debug.Log(
            $"Đã nhặt mảnh ảnh {safeId}. Tiến độ: " +
            $"{collectedPieces}/{totalPiecesRequired}"
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportProgressForChapter(
                1,
                0,
                1,
                totalPiecesRequired
            );
        }

        return true;
    }

    private bool OpenPuzzleGame()
    {
        if (!MiniGameFlowManager.TryOpen(
                this,
                puzzleMiniGameUI,
                1))
        {
            return false;
        }

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

            ResetCursorToDefaultHand();
        }

        return true;
    }

    public void ClosePuzzleGame(bool isWin = false)
    {
        MiniGameFlowManager.Close(
            this,
            puzzleMiniGameUI,
            !isWin
        );

        if (puzzleMiniGameUI != null)
        {
            puzzleMiniGameUI.SetActive(false);

            if (playerController != null)
            {
                playerController.UnlockCamera();
                playerController.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    public void StartGlitchFadeTransition(GameObject puzzleUI, GameObject photoOnWall, MapSealCutscenePlayer seal1Cutscene)
    {
        isPuzzleFinished = true;
        StartCoroutine(UltimateWinRoutine(puzzleUI, photoOnWall, seal1Cutscene));
    }

    private IEnumerator UltimateWinRoutine(GameObject puzzleUI, GameObject photoOnWall, MapSealCutscenePlayer seal1Cutscene)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float alpha = 0f;
            Color c = fadeImage.color;

            while (alpha < 1f)
            {
                alpha += Time.deltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(holdBlackTime);

        MiniGameFlowManager.Close(
            this,
            puzzleMiniGameUI,
            false
        );

        if (puzzleMiniGameUI != null) puzzleMiniGameUI.SetActive(false);
        if (puzzleUI != null) puzzleUI.SetActive(false);

        if (photoOnWall != null) photoOnWall.SetActive(true);

        PlayerInventory.RemoveAll("ManhGiay1");
        PlayerInventory.RemoveAll("ManhGiay2");
        PlayerInventory.RemoveAll("ManhGiay3");
        PlayerInventory.RemoveAll("ManhGiay4");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (victoryCutsceneObject != null) victoryCutsceneObject.SetActive(true);
        if (victoryCutsceneTimeline != null) victoryCutsceneTimeline.Play();
        if (seal1Cutscene != null) seal1Cutscene.PlayCutscene();

        if (fadeImage != null)
        {
            float alpha = 1f;
            Color c = fadeImage.color;

            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                yield return null;
            }
            fadeImage.gameObject.SetActive(false);
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

    public void ResetCursorToDefaultHand()
    {
        if (handOpenTexture != null)
        {
            // Đã sửa Vector2.zero -> cursorHotspot (32, 10)
            Cursor.SetCursor(handOpenTexture, cursorHotspot, CursorMode.Auto);
        }
    }
}