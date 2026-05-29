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
}