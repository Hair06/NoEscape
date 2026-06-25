using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using ElmanGameDevTools.PlayerSystem; // để tự tìm PlayerController Elman

// Quản lý tổng mini game ghép đĩa nhạc.
// Gắn vào Object đĩa vỡ gần cửa sổ (có Collider trigger để bắt [E]).
public class DiscPuzzleManager : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để sửa đĩa nhạc";

    [Header("Bảng mini game ghép mảnh")]
    [Tooltip("Kéo Panel UI chứa các mảnh DiscPiece vào đây")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Các mảnh đĩa")]
    [Tooltip("Để trống sẽ tự tìm tất cả DiscPiece con bên trong Panel")]
    [SerializeField] private DiscPiece[] discPieces;

    [Header("Rải mảnh ngẫu nhiên mỗi lần mở?")]
    [SerializeField] private bool scatterOnOpen = true;

    [Header("Khóa camera khi chơi")]
    [Tooltip("Có thể để trống - script sẽ tự tìm PlayerController trong scene")]
    [SerializeField] private MonoBehaviour cameraScript;
    private PlayerController autoFoundPlayer; // tự tìm nếu cameraScript trống

    [Header("Âm thanh khi ghép xong")]
    [Tooltip("Giai điệu đĩa nhạc - giống tiếng trong băng cassette")]
    [SerializeField] private AudioSource discMelody;

    [Header("Đĩa hoàn chỉnh 3D hiện ra để nhặt")]
    [Tooltip("Kéo Object đĩa hoàn chỉnh (tắt sẵn) - sẽ bật lên sau khi ghép xong")]
    [SerializeField] private GameObject completedDisc3D;

    [Header("Đĩa vỡ cũ (ẩn đi sau khi ghép xong)")]
    [Tooltip("Kéo model đĩa vỡ ngoài scene vào đây để ẩn khi ghép xong (có thể để trống)")]
    [SerializeField] private GameObject brokenDisc3D;

    private bool isPlayerInside = false;
    private bool isOpen = false;
    private bool isComplete = false;
    private bool hasScattered = false;

    private void Start()
    {
        if (discPieces == null || discPieces.Length == 0)
        {
            if (puzzlePanel != null)
                discPieces = puzzlePanel.GetComponentsInChildren<DiscPiece>(true);
        }

        foreach (DiscPiece piece in discPieces)
        {
            if (piece != null) piece.manager = this;
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (puzzlePanel != null) puzzlePanel.SetActive(false);

        // Đĩa hoàn chỉnh ẩn sẵn, chỉ hiện khi ghép xong
        if (completedDisc3D != null) completedDisc3D.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && !isOpen && !isComplete
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenPuzzle();
        }
    }

    private void OpenPuzzle()
    {
        isOpen = true;

        if (puzzlePanel != null) puzzlePanel.SetActive(true);
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (scatterOnOpen && !hasScattered)
        {
            foreach (DiscPiece piece in discPieces)
            {
                if (piece != null) piece.ScatterRandom();
            }
            hasScattered = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraScript != null)
        {
            cameraScript.enabled = false;
        }
        else
        {
            autoFoundPlayer = FindFirstObjectByType<PlayerController>();
            if (autoFoundPlayer != null) autoFoundPlayer.enabled = false;
            else Debug.LogWarning("[DiscPuzzleManager] Không tìm thấy PlayerController để khóa camera!");
        }

        Debug.Log("Mở mini game ghép đĩa nhạc. Hãy kéo các mảnh vào đúng vị trí.");
    }

    public void CheckComplete()
    {
        if (isComplete) return;

        foreach (DiscPiece piece in discPieces)
        {
            if (piece != null && !piece.IsSnapped()) return;
        }

        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        isComplete = true;
        Debug.Log("Đã ghép xong đĩa nhạc! Giai điệu vang lên. Hãy tới nhặt đĩa.");

        // Đóng bảng, khóa chuột lại
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Bật lại camera
        if (cameraScript != null) cameraScript.enabled = true;
        else if (autoFoundPlayer != null) autoFoundPlayer.enabled = true;

        // Phát giai điệu đĩa nhạc
        if (discMelody != null) discMelody.Play();

        // Ẩn đĩa vỡ cũ, hiện đĩa hoàn chỉnh 3D để người chơi tới nhặt
        if (brokenDisc3D != null) brokenDisc3D.SetActive(false);
        if (completedDisc3D != null) completedDisc3D.SetActive(true);

        // LƯU Ý: KHÔNG báo hộp nhạc ở đây nữa.
        // Việc báo MusicBoxRestore chuyển sang script DiscPartCollect (lúc nhấn E nhặt đĩa).
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isComplete)
        {
            isPlayerInside = true;
            if (promptText != null && !isOpen)
            {
                promptText.text = interactMessage;
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
}