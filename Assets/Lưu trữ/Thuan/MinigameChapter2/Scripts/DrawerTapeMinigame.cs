using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BỘ PHẬN 1 — Con Thoi Nhạc.
/// Đặt lên cùng GameObject với ngăn kéo 3D (hoặc một Trigger riêng).
/// Yêu cầu: DiaryNoteInteract phải gọi UnlockFromDiary() trước.
/// Khi nhấn E → mini game gỡ băng keo → ngăn kéo mở → thu thập "shuttle".
/// </summary>
public class DrawerTapeMinigame : MonoBehaviour, IInteractable
{
    // ─── Trạng thái ───────────────────────────────────────────────────────────
    private bool diaryRead = false;
    private bool isCompleted = false;
    private bool drawerOpen = false;

    // ─── Mini Game Băng Keo ───────────────────────────────────────────────────
    [Header("UI Mini Game (Băng Keo)")]
    [SerializeField] private GameObject tapeMiniGameUI;   // Panel tổng mini-game
    [SerializeField] private Button[] tapeButtons;      // 3–4 nút hình dải băng
    [SerializeField] private TextMeshProUGUI instructionText;  // Hướng dẫn phía trên

    // ─── Ngăn Kéo 3D ─────────────────────────────────────────────────────────
    [Header("Ngăn Kéo 3D")]
    [SerializeField] private Transform drawerTransform;
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.35f);
    [SerializeField] private float drawerSpeed = 3f;

    // ─── Âm Thanh ─────────────────────────────────────────────────────────────
    [Header("Âm thanh")]
    [SerializeField] private AudioClip tapePeelSound;    // Tiếng xé băng keo
    [SerializeField] private AudioClip drawerOpenSound;  // Tiếng ngăn kéo mở ra

    // ─── Scripts đóng băng ────────────────────────────────────────────────────
    [Header("Scripts đóng băng khi chơi mini game")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    // ─── Private ──────────────────────────────────────────────────────────────
    private int tapeRemaining;
    private Vector3 closedPos, openPos;
    private AudioSource sfxSource;

    private void Start()
    {
        if (drawerTransform != null)
        {
            closedPos = drawerTransform.localPosition;
            openPos = closedPos + openOffset;
        }

        if (tapeMiniGameUI != null) tapeMiniGameUI.SetActive(false);

        tapeRemaining = tapeButtons != null ? tapeButtons.Length : 0;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;

        // Gắn listener cho từng nút băng keo
        for (int i = 0; i < tapeButtons.Length; i++)
        {
            int idx = i;
            if (tapeButtons[i] != null)
                tapeButtons[i].onClick.AddListener(() => PeelTape(idx));
        }
    }

    private void Update()
    {
        // Trượt ngăn kéo mượt mà theo thời gian thực
        if (drawerTransform != null)
        {
            Vector3 target = drawerOpen ? openPos : closedPos;
            drawerTransform.localPosition = Vector3.Lerp(
                drawerTransform.localPosition, target, Time.deltaTime * drawerSpeed);
        }
    }

    // ─── IInteractable ────────────────────────────────────────────────────────

    public string GetInteractPrompt()
    {
        if (isCompleted) return "";
        if (!diaryRead) return "Đọc nhật ký của Kiều Hoa trước";
        return "Nhấn E để gỡ băng keo ngăn kéo";
    }

    public void Interact()
    {
        if (isCompleted) return;

        if (!diaryRead)
        {
            Debug.Log("[Drawer] Cần đọc nhật ký của Kiều Hoa trước!");
            return;
        }

        OpenTapeMinigame();
    }

    // ─── Mở khóa từ ngoài ────────────────────────────────────────────────────

    /// <summary>Gọi từ DiaryNoteInteract sau khi người chơi đọc nhật ký xong.</summary>
    public void UnlockFromDiary()
    {
        diaryRead = true;
        Debug.Log("[Drawer] Ngăn kéo đã được mở khóa bởi nhật ký.");
    }

    // ─── Mini Game Logic ──────────────────────────────────────────────────────

    private void OpenTapeMinigame()
    {
        if (tapeMiniGameUI != null) tapeMiniGameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = false;

        UpdateInstruction();
    }

    private void PeelTape(int index)
    {
        if (tapeButtons[index] == null || !tapeButtons[index].gameObject.activeSelf) return;

        // Ẩn dải băng vừa gỡ
        tapeButtons[index].gameObject.SetActive(false);

        if (sfxSource != null && tapePeelSound != null)
            sfxSource.PlayOneShot(tapePeelSound);

        tapeRemaining--;
        UpdateInstruction();

        if (tapeRemaining <= 0)
            Invoke(nameof(FinishTapeGame), 0.7f);
    }

    private void UpdateInstruction()
    {
        if (instructionText == null) return;
        instructionText.text = tapeRemaining > 0
            ? $"Nhấp vào từng dải băng keo để gỡ — còn {tapeRemaining} dải"
            : "Tất cả băng keo đã được gỡ!";
    }

    private void FinishTapeGame()
    {
        isCompleted = true;
        drawerOpen = true;

        if (tapeMiniGameUI != null) tapeMiniGameUI.SetActive(false);

        if (sfxSource != null && drawerOpenSound != null)
            sfxSource.PlayOneShot(drawerOpenSound);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        foreach (var s in scriptsToDisable)
            if (s != null) s.enabled = true;

        if (Chapter2Manager.Instance != null)
            Chapter2Manager.Instance.CollectPart("shuttle");

        Debug.Log("[Drawer] Đã lấy Con Thoi Nhạc!");
    }
}