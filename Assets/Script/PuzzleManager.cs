using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzlePiece[] allPieces;
    
    [Header("Cấu hình ẩn UI Minigame")]
    [SerializeField] private GameObject puzzleUI;

    [Header("Cutscene sau khi mở Phong Ấn 1")]
    [SerializeField] private MapSealCutscenePlayer seal1Cutscene;

    [Header("Cấu hình Ảnh Trên Tường")]
    [SerializeField] private GameObject photoOnWall;

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
            QuestManager.Instance.CompleteSubQuestForChapter(1, 2);
        }

        Debug.Log("🧩 PuzzleManager: Đã xong tranh. Giao toàn quyền cho Chapter1Manager!");

        // Truyền thẳng lệnh và các Object cần thiết qua Chapter1Manager xử lý chuỗi Coroutine
        if (Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.StartGlitchFadeTransition(puzzleUI, photoOnWall, seal1Cutscene);
        }
    }
}