using UnityEngine;
using UnityEngine.InputSystem;

public class CustomHandCursor : MonoBehaviour
{
    [Header("Cấu hình Con Trỏ Chuột Custom")]
    [Tooltip("Ảnh bàn tay xòe ra khi rê chuột bình thường")]
    [SerializeField] private Texture2D handOpenTexture;

    [Tooltip("Ảnh bàn tay nắm chặt lại khi nhấn giữ chuột trái")]
    [SerializeField] private Texture2D handClosedTexture;

    [Tooltip("Tọa độ điểm nhấp trên con trỏ chuột (Ví dụ X: 32, Y: 10 cho đầu ngón trỏ)")]
    [SerializeField] private Vector2 cursorHotspot = new Vector2(32f, 10f);

    [Header("Tùy chọn dành cho Scene Menu")]
    [Tooltip("Tự động mở khóa và hiện chuột ngay khi load Scene Menu")]
    [SerializeField] private bool autoUnlockCursor = true;

    private void Start()
    {
        if (autoUnlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ResetCursorToDefaultHand();
    }

    private void Update()
    {
        if (Cursor.visible && Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                if (handClosedTexture != null)
                {
                    Cursor.SetCursor(handClosedTexture, cursorHotspot, CursorMode.Auto);
                }
            }
            else
            {
                if (handOpenTexture != null)
                {
                    Cursor.SetCursor(handOpenTexture, cursorHotspot, CursorMode.Auto);
                }
            }
        }
    }

    public void ResetCursorToDefaultHand()
    {
        if (handOpenTexture != null)
        {
            Cursor.SetCursor(handOpenTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    private void OnDisable()
    {
        // Tra lại con trỏ chuột mặc định của Windows khi ẩn UI hoặc chuyển Scene
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}