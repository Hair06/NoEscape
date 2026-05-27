using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzlePiece[] allPieces; // Kéo cả 4 Object Piece vào danh sách này
    [SerializeField] private GameObject puzzleUI;      // Kéo chính Puzzle_Canvas vào đây để ẩn khi thắng

    public void CheckWinCondition()
    {
        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.IsSnapped()) 
                return; // Nếu có dù chỉ 1 mảnh chưa xong thì chưa thắng
        }

        // --- CHÚC MỪNG CHIẾN THẮNG ---
        Debug.Log("Bạn đã giải mã xong bức tranh cổ! Phong ấn 1 đã được mở.");
        
        // Trả lại trạng thái game bình thường
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        // Tắt UI Mini-game đi
        puzzleUI.SetActive(false);

        // Kích hoạt các logic tiếp theo ở đây (Vd: Mở cửa tầng 2, phát tiếng động kinh dị...)
    }
}