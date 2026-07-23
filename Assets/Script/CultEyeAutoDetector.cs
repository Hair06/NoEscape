using UnityEngine;

public class CultEyeAutoDetector : MonoBehaviour
{
    public static CultEyeAutoDetector Instance;

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

    // Cho script khac biet dang soi ky tu hay khong
    public bool IsAiming => isAiming;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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
        if (!MiniGameFlowManager.IsChapterActive(3))
        {
            if (isAiming)
            {
                isAiming = false;
                ApplyCultVision(false);
            }
            return;
        }

        if (eyeInHandObject == null) return;

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
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            return UnityEngine.InputSystem.Mouse.current.rightButton.isPressed;
        }
        return false;
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
