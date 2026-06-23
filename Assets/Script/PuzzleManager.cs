using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzlePiece[] allPieces;
    
    [Header("Cấu hình ẩn UI Minigame")]
    [Tooltip("Kéo Object 'UI_GiaoDienChoi' vừa tạo vào đây")]
    [SerializeField] private GameObject puzzleUI;

    [Header("Cutscene sau khi mở Phong Ấn 1")]
    [SerializeField] private MapSealCutscenePlayer seal1Cutscene;

    // =========================================================================
    // CODE BỔ SUNG MỚI: Bật ảnh trên tường khi thắng
    // =========================================================================
    [Header("Cấu hình Ảnh Trên Tường")]
    [Tooltip("Kéo Game Object bức ảnh hoàn chỉnh gắn trên tường vào đây")]
    [SerializeField] private GameObject photoOnWall;
    // =========================================================================

    private bool completed;

    public void CheckWinCondition()
    {
        if (completed) return;

        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.IsSnapped()) return;
        }

        completed = true;
        if (QuestManager.Instance != null)
{
    QuestManager.Instance.CompleteSubQuest(2);
}

        Debug.Log("Bạn đã giải mã xong bức tranh cổ! Phong ấn 1 đã được mở.");

        // 1. Tắt phần UI hiển thị bàn xếp hình
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false); 
        }

        // =========================================================================
        // THỰC HIỆN: Bật bức ảnh hoàn chỉnh trên tường lên lập tức khi giải xong
        // =========================================================================
        if (photoOnWall != null)
        {
            photoOnWall.SetActive(true);
            Debug.Log("Đã kích hoạt hiển thị bức ảnh hoàn chỉnh trên khung tranh treo tường!");
        }
        // =========================================================================

        // 2. Chuyển tiếp trạng thái thắng sang Chapter1Manager
        if (Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.ClosePuzzleGame(true);
        }

        // 3. Kích hoạt phát đoạn phim cutscene cốt truyện chạy chữ
        if (seal1Cutscene != null)
        {
            seal1Cutscene.PlayCutscene();
        }
        
    }
}