using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using ElmanGameDevTools.PlayerSystem; // tự tìm PlayerController Elman

public class DiaryRead : MonoBehaviour
{
    private const int ChapterIndex = 2;

    // 3 trạng thái của chuỗi tương tác
    private enum State
    {
        DrawerClosed,   // ngăn kéo đang đóng
        DrawerOpen,     // ngăn kéo đã mở, thấy nhật ký
        ReadingDiary    // đang xem nhật ký to đè màn hình
    }

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string openDrawerMessage = "Nhấn [E] để mở ngăn kéo";
    [SerializeField] private string readDiaryMessage = "Nhấn [E] để xem nhật ký";
    [SerializeField] private string closeDiaryMessage = "Nhấn [E] để đóng nhật ký";
    [SerializeField] private string peelTapeMessage = "Nhấn [E] để gỡ băng keo";

    [Header("Ngăn kéo trượt ra")]
    [Tooltip("Kéo Object ngăn kéo (model trượt) vào đây")]
    [SerializeField] private Transform drawer;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 0.3f);
    [SerializeField] private float drawerSpeed = 3f;

    [Header("Tấm ảnh nhật ký to đè màn hình")]
    [Tooltip("Kéo Panel/Image nhật ký vào đây (lúc đầu để tắt)")]
    [SerializeField] private GameObject diaryPanel;

    [Header("Mini game băng keo (nối tiếp sau khi đọc nhật ký)")]
    [Tooltip("BẮT BUỘC kéo object TapePeelPuzzle vào đây, nếu trống mini game sẽ không tự mở")]
    [SerializeField] private TapePeelPuzzle tapePuzzle;

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource drawerAudio;   // tiếng kéo ngăn
    [SerializeField] private AudioSource pageAudio;     // tiếng lật giấy

    [Header("Khóa camera khi xem nhật ký (có thể để trống)")]
    [Tooltip("Để trống sẽ tự tìm PlayerController trong scene")]
    [SerializeField] private MonoBehaviour cameraScript;
    private PlayerController autoFoundPlayer;

    // Biến cho bước sau (mini game băng keo) kiểm tra
    [HideInInspector] public bool hasReadDiary = false;

    private State currentState = State.DrawerClosed;
    private bool isPlayerInside = false;

    private Vector3 closedPos;
    private Vector3 openPos;

    private void Start()
    {
        // Lúc đầu ẩn cả chữ hướng dẫn lẫn tấm nhật ký
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (diaryPanel != null) diaryPanel.SetActive(false);

        // Ghi nhớ vị trí đóng/mở của ngăn kéo
        if (drawer != null)
        {
            closedPos = drawer.localPosition;
            openPos = closedPos + openOffset;
        }

        // Cảnh báo nếu quên kéo TapePeelPuzzle vào Inspector
        if (tapePuzzle == null)
            Debug.LogError("[DiaryRead] Ô 'Tape Puzzle' đang TRỐNG! Kéo object TapePeelPuzzle vào Inspector, nếu không mini game sẽ không tự mở.");
    }

    private void Update()
    {
        // Luôn trượt ngăn kéo mượt về đúng vị trí mục tiêu
        if (drawer != null)
        {
            Vector3 targetPos = (currentState == State.DrawerClosed) ? closedPos : openPos;
            drawer.localPosition = Vector3.Lerp(drawer.localPosition, targetPos, Time.deltaTime * drawerSpeed);
        }

        if (!MiniGameFlowManager.CanContinue(
                this,
                ChapterIndex))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        // Nhận phím E
        if (isPlayerInside
            && Keyboard.current != null
            && Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleInteract();
        }
    }

    private void HandleInteract()
    {
        switch (currentState)
        {
            case State.DrawerClosed:
                OpenDrawer();
                break;
            case State.DrawerOpen:
                // Nếu đã đọc nhật ký rồi -> mở mini game gỡ băng keo
                if (hasReadDiary && tapePuzzle != null)
                {
                    tapePuzzle.OpenPuzzle();
                    if (promptText != null) promptText.gameObject.SetActive(false);
                }
                else
                {
                    OpenDiary();
                }
                break;
            case State.ReadingDiary:
                CloseDiary();
                break;
        }
    }

    private void OpenDrawer()
    {
        currentState = State.DrawerOpen;
        if (drawerAudio != null) drawerAudio.Play();

        // Đổi chữ sang "xem nhật ký"
        if (promptText != null) promptText.text = readDiaryMessage;

        Debug.Log("Đã mở ngăn kéo. Thấy cuốn nhật ký bên trong.");
    }

    private void OpenDiary()
    {
        if (!MiniGameFlowManager.TryOpen(
                this,
                diaryPanel,
                ChapterIndex))
        {
            return;
        }

        currentState = State.ReadingDiary;

        if (diaryPanel != null) diaryPanel.SetActive(true);
        if (pageAudio != null) pageAudio.Play();

        // Mở chuột để người chơi xem thoải mái
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Khóa điều khiển Player (di chuyển + camera)
        if (cameraScript != null)
        {
            cameraScript.enabled = false;
        }
        else
        {
            autoFoundPlayer = FindFirstObjectByType<PlayerController>();
            if (autoFoundPlayer != null) autoFoundPlayer.enabled = false;
        }

        // Ẩn chữ prompt chung (chữ "đóng nhật ký" nên đặt riêng trong DiaryPanel)
        if (promptText != null) promptText.gameObject.SetActive(false);

        Debug.Log("Đang đọc nhật ký Kiều Hoa...");
    }

    private void CloseDiary()
    {
        currentState = State.DrawerOpen;

        MiniGameFlowManager.Close(this, diaryPanel);

        if (diaryPanel != null) diaryPanel.SetActive(false);

        // Đánh dấu đã đọc xong
        hasReadDiary = true;
        Debug.Log("Đã đọc xong nhật ký. Vào thẳng mini game gỡ băng keo.");

        // Ẩn chữ hướng dẫn
        if (promptText != null) promptText.gameObject.SetActive(false);

        // ===== VÀO THẲNG MINI GAME, KHÔNG CẦN BẤM E LẦN NỮA =====
        if (tapePuzzle != null)
        {
            tapePuzzle.OpenPuzzle();
            return;   // TapePeelPuzzle tự lo chuột và camera
        }

        // Nếu ô Tape Puzzle bị trống thì trả lại điều khiển như cũ
        Debug.LogError("[DiaryRead] Không mở được mini game vì ô 'Tape Puzzle' đang TRỐNG!");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraScript != null) cameraScript.enabled = true;
        else if (autoFoundPlayer != null) autoFoundPlayer.enabled = true;

        if (promptText != null)
        {
            promptText.text = peelTapeMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            MiniGameFlowManager.IsChapterActive(ChapterIndex))
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = GetCurrentPrompt();
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }

    // Chọn chữ hướng dẫn đúng theo trạng thái hiện tại
    private string GetCurrentPrompt()
    {
        if (currentState == State.DrawerClosed) return openDrawerMessage;
        if (hasReadDiary) return peelTapeMessage;
        return readDiaryMessage;
    }
}
