using UnityEngine;

// Quản lý tổng mini game gỡ băng keo.
// Gắn vào Panel UI chứa các miếng băng keo (TapePiece).
public class TapePeelPuzzle : MonoBehaviour
{
    [Header("Bảng mini game")]
    [Tooltip("Kéo chính Panel UI mini game vào đây (cái chứa các miếng băng)")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Các miếng băng keo")]
    [Tooltip("Để trống sẽ tự tìm tất cả TapePiece con bên trong Panel")]
    [SerializeField] private TapePiece[] tapePieces;

    [Header("Rải băng ngẫu nhiên mỗi lần mở?")]
    [SerializeField] private bool scatterOnOpen = true;

    [Header("Khóa camera khi chơi")]
    [Tooltip("Kéo Player (script vThirdPersonInput) vào đây để khóa lúc gỡ băng")]
    [SerializeField] private MonoBehaviour cameraScript;

    [Header("Phần thưởng sau khi gỡ hết")]
    [Tooltip("Kéo Object Con Thoi Nhạc (để sẵn trong ngăn kéo) vào đây")]
    [SerializeField] private GameObject shuttleReward;

    private int peeledCount = 0;
    private int totalPieces = 0;
    private bool isComplete = false;
    private bool hasScattered = false;

    private void Start()
    {
        // Tự tìm các miếng băng nếu chưa gán
        if (tapePieces == null || tapePieces.Length == 0)
        {
            tapePieces = GetComponentsInChildren<TapePiece>(true);
        }

        totalPieces = tapePieces.Length;

        // Gán manager cho từng miếng để chúng báo về đây
        foreach (TapePiece piece in tapePieces)
        {
            if (piece != null) piece.manager = this;
        }

        // Lúc đầu ẩn bảng mini game và ẩn phần thưởng
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        if (shuttleReward != null) shuttleReward.SetActive(false);
    }

    // Hàm cho DiaryRead gọi để bật mini game lên
    public void OpenPuzzle()
    {
        if (isComplete) return;

        if (puzzlePanel != null) puzzlePanel.SetActive(true);

        // Rải băng ngẫu nhiên (chỉ lần đầu mở, để không xáo trộn miếng đã gỡ)
        if (scatterOnOpen && !hasScattered)
        {
            foreach (TapePiece piece in tapePieces)
            {
                if (piece != null) piece.ScatterRandom();
            }
            hasScattered = true;
        }

        // Mở chuột để người chơi kéo-lột băng keo
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Khóa camera để không xoay theo chuột khi đang gỡ băng
        if (cameraScript != null) cameraScript.enabled = false;

        Debug.Log("Mở mini game gỡ băng keo. Hãy kéo từng miếng để lột.");
    }

    // Mỗi miếng băng gọi hàm này khi bong ra
    public void OnPiecePeeled()
    {
        peeledCount++;
        Debug.Log($"Đã gỡ {peeledCount}/{totalPieces} miếng băng.");

        if (peeledCount >= totalPieces)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        isComplete = true;
        Debug.Log("Đã gỡ hết băng keo! Con Thoi Nhạc lộ ra.");

        // Đóng bảng mini game, khóa chuột lại để tiếp tục chơi
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Bật lại camera cho người chơi đi tiếp
        if (cameraScript != null) cameraScript.enabled = true;

        // Hiện Con Thoi Nhạc để người chơi nhặt
        if (shuttleReward != null) shuttleReward.SetActive(true);
    }

    public bool IsComplete() => isComplete;
}