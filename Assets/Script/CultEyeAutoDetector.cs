using UnityEngine;

public class CultEyeAutoDetector : MonoBehaviour
{
    [Header("THAM CHIẾU VẬT THỂ TRÊN TAY PLAYER")]
    [SerializeField] private GameObject eyeInHandObject;

    [Header("GIAO DIỆN KÍNH NHÌN ẨN (UI)")]
    [SerializeField] private GameObject scopeMaskUI;

    [Header("CẤU HÌNH VẬT THỂ ẨN & ÁNH SÁNG")]
    [SerializeField] private GameObject hiddenSignObject;
    [SerializeField] private Light roomLight;
    [SerializeField] private Color horrorColor = new Color(0.4f, 0f, 0f);

    private Color originalRoomColor;
    private bool isHoldingEye = false;
    private bool isAiming = false;

    void Start()
    {
        if (roomLight != null)
        {
            originalRoomColor = roomLight.color;
        }

        if (hiddenSignObject != null)
        {
            hiddenSignObject.SetActive(false);
        }

        if (scopeMaskUI != null)
        {
            scopeMaskUI.SetActive(false);
        }
    }

    void Update()
    {
        if (eyeInHandObject == null) return;

        // Tự động kiểm tra xem con mắt trên tay đang Bật hay Tắt
        isHoldingEye = eyeInHandObject.activeInHierarchy;

        if (isHoldingEye)
        {
            if (CheckRightClick())
            {
                if (!isAiming)
                {
                    isAiming = true;
                    ApplyCultVision(true);
                }
            }
            else
            {
                if (isAiming)
                {
                    isAiming = false;
                    ApplyCultVision(false);
                }
            }
        }
        else
        {
            if (isAiming)
            {
                isAiming = false;
                ApplyCultVision(false);
            }
        }
    }

    private bool CheckRightClick()
    {
        // 1. Nếu dự án đang dùng Input System mới
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
        }
        return false;
        // 2. Nếu dự án dùng Input System cũ (để đề phòng lỗi build)
        #else
        return Input.GetMouseButton(1);
        #endif
    }

    private void ApplyCultVision(bool state)
    {
        if (scopeMaskUI != null)
        {
            scopeMaskUI.SetActive(state);
        }

        if (hiddenSignObject != null)
        {
            hiddenSignObject.SetActive(state);
        }

        if (roomLight != null)
        {
            roomLight.color = state ? horrorColor : originalRoomColor;
        }
    }
}