using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzlePiece[] allPieces;
    [SerializeField] private GameObject puzzleUI;

    [Header("Cutscene sau khi mở Phong Ấn 1")]
    [SerializeField] private MapSealCutscenePlayer seal1Cutscene;

    private bool completed;

    public void CheckWinCondition()
{
    foreach (PuzzlePiece piece in allPieces)
    {
        if (completed) return;

        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.IsSnapped()) return;
        }

        completed = true;

        Debug.Log("Bạn đã giải mã xong bức tranh cổ! Phong ấn 1 đã được mở.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        puzzleUI.SetActive(false);

        if (seal1Cutscene != null)
        {
            seal1Cutscene.PlayCutscene();
        }
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

    // SỬA TẠI ĐÂY: Thêm chữ true vào đây để báo cho Chapter1Manager biết bạn đã WIN để nó khóa phím E
    Chapter1Manager.Instance.ClosePuzzleGame(true); 
}
}