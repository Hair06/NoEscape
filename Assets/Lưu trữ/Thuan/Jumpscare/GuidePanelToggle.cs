using UnityEngine;
using UnityEngine.InputSystem;

// Bật/tắt bảng hướng dẫn bằng phím G.
// Gắn script này vào một object luôn bật (ví dụ Canvas hoặc GameObject rỗng).
public class GuidePanelToggle : MonoBehaviour
{
    [Header("Bảng hướng dẫn (Panel to)")]
    [Tooltip("Kéo Image bảng hướng dẫn vào đây - để tắt sẵn lúc đầu")]
    [SerializeField] private GameObject guidePanel;

    [Header("Icon nhắc bấm G (góc màn hình)")]
    [Tooltip("Kéo Image icon nhỏ vào đây - hiện khi bảng đang đóng")]
    [SerializeField] private GameObject keyHintIcon;

    [Header("Âm thanh lật giấy (có thể để trống)")]
    [SerializeField] private AudioSource pageAudio;

    private bool isOpen = false;

    private void Start()
    {
        // Lúc đầu: bảng đóng, icon nhắc hiện
        if (guidePanel != null) guidePanel.SetActive(false);
        if (keyHintIcon != null) keyHintIcon.SetActive(true);

        Debug.Log("[GuidePanelToggle] Đã khởi động. Nhấn G để mở bảng hướng dẫn.");
    }

    private void Update()
    {
        // Đọc thẳng từ Input System mới, không qua GameInputBridge
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        isOpen = !isOpen;

        if (guidePanel != null) guidePanel.SetActive(isOpen);

        // Đang mở bảng thì ẩn icon nhắc đi cho đỡ vướng
        if (keyHintIcon != null) keyHintIcon.SetActive(!isOpen);

        if (pageAudio != null) pageAudio.Play();

        Debug.Log("[GuidePanelToggle] Bảng hướng dẫn: " + (isOpen ? "MỞ" : "ĐÓNG"));
    }
}