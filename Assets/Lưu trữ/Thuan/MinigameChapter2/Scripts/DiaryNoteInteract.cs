using UnityEngine;
using TMPro;

/// <summary>
/// Đặt lên Object cuốn nhật ký trên bàn trang điểm.
/// Khi đọc xong → tự động mở khóa DrawerTapeMinigame.
/// </summary>
public class DiaryNoteInteract : MonoBehaviour, IInteractable
{
    [Header("Nội dung nhật ký")]
    [SerializeField] private GameObject diaryUI;           // Panel hiển thị nhật ký
    [SerializeField] private TextMeshProUGUI diaryContentText;  // TMP hiển thị chữ nhật ký
    [SerializeField]
    [TextArea(5, 20)]
    private string diaryText =
        "Con yêu của mẹ, Alex...\n\n" +
        "Nếu con đọc được những dòng này, mẹ không còn ở đây nữa.\n\n" +
        "Mẹ đã cất giữ chiếc hộp nhạc của con trong ngăn kéo này. " +
        "Nó bị hỏng từ lâu rồi — mẹ không có tiền sửa. " +
        "Nhưng mẹ vẫn giữ nó vì tiếng nhạc của nó... là lý do duy nhất " +
        "khiến mẹ tiếp tục những ngày tháng đó.\n\n" +
        "— Yêu con mãi mãi, Kiều Hoa";

    [Header("Mở khóa ngăn kéo sau khi đọc")]
    [SerializeField] private DrawerTapeMinigame targetDrawer;

    [Header("Scripts đóng băng khi đọc nhật ký")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool hasReadDiary = false;

    // ─── IInteractable ────────────────────────────────────────────────────────

    public string GetInteractPrompt() =>
        hasReadDiary ? "Nhật ký của Kiều Hoa" : "Nhấn E để đọc nhật ký";

    public void Interact() => OpenDiary();

    // ─── Logic ───────────────────────────────────────────────────────────────

    private void OpenDiary()
    {
        // Hiện nội dung
        if (diaryContentText != null) diaryContentText.text = diaryText;
        if (diaryUI != null) diaryUI.SetActive(true);

        // Hiện cursor để đọc
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = false;

        // Chỉ mở khóa ngăn kéo lần đầu đọc
        if (!hasReadDiary)
        {
            hasReadDiary = true;
            if (targetDrawer != null) targetDrawer.UnlockFromDiary();
            Debug.Log("[Diary] Đã đọc nhật ký. Ngăn kéo được mở khóa.");
        }
    }

    /// <summary>Gắn hàm này vào nút "Đóng" trên diaryUI.</summary>
    public void CloseDiary()
    {
        if (diaryUI != null) diaryUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = true;
    }
}