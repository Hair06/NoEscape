using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using ElmanGameDevTools.PlayerSystem;

// Khoa so 3D. Nhan E -> CAMERA BAY XUONG diem moc truoc o khoa.
// Giai xong -> camera BAY LEN tra ve vi tri cu.
public class CombinationLock : MonoBehaviour, IInteractable
{
    private const int ChapterIndex = 2;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để chỉnh khóa số";

    [Header("Hiển thị số đang nhập (tùy chọn)")]
    [SerializeField] private TextMeshProUGUI inputDisplay;

    [Header("4 vòng số")]
    [SerializeField] private LockDial[] dials;

    [Header("Tổ hợp đúng (theo thứ tự vòng)")]
    [SerializeField] private List<int> correctCombination = new List<int>() { 1, 2, 3, 4 };

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;

    [Header("Khóa di chuyển khi chỉnh")]
    [SerializeField] private MonoBehaviour playerInput;
    private PlayerController autoFoundPlayer;

    // ============================================================
    [Header("═══ ĐIỂM MỐC CAMERA (CÁCH CHÍNH) ═══")]
    [Tooltip("Kéo object CamAnchor_Lock vào đây. Camera bay CHÍNH XÁC tới vị trí + góc của nó.\n" +
             "Mẹo: chọn object mốc rồi dùng GameObject > Align View to Selected để xem trước.")]
    [SerializeField] private Transform cameraAnchor;

    [Header("═══ ĐIỂM NGẮM (chỉ dùng khi KHÔNG có mốc) ═══")]
    [Tooltip("Kéo object ổ khóa vào đây")]
    [SerializeField] private Transform lockBody;
    [Tooltip("Lệch điểm ngắm theo X/Y/Z (mét)")]
    [SerializeField] private Vector3 lookPointOffset = Vector3.zero;

    [Header("═══ VỊ TRÍ TỰ ĐỘNG (chỉ dùng khi KHÔNG có mốc) ═══")]
    [SerializeField] private float viewDistance = 1.2f;
    [SerializeField] private float viewHeight = 0.6f;
    [SerializeField] private float viewSideOffset = 0f;
    [SerializeField] private bool useLockForwardDirection = false;
    [SerializeField] private float extraPitch = 0f;
    [SerializeField] private float extraYaw = 0f;
    [SerializeField] private float extraRoll = 0f;

    [Header("═══ TỐC ĐỘ BAY ═══")]
    [SerializeField] private float flyDownSpeed = 4f;
    [SerializeField] private float flyUpSpeed = 4f;

    [Header("═══ XEM TRƯỚC TRONG SCENE ═══")]
    [Tooltip("Cầu VÀNG = điểm ngắm. Cầu XANH = vị trí camera sẽ dừng")]
    [SerializeField] private bool showAimGizmo = true;
    // ============================================================

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource unlockAudio;
    [SerializeField] private AudioSource wrongAudio;

    [Header("Chìa vặn hiện ra sau khi mở khóa")]
    [SerializeField] private GameObject windKeyReward;

    [Header("Nắp hòm xoay lên (sau khi mở khóa)")]
    [SerializeField] private Transform lidHinge;
    [SerializeField] private float lidOpenAngle = -100f;
    [SerializeField] private Vector3 lidRotateAxis = new Vector3(1, 0, 0);
    [SerializeField] private float lidSpeed = 3f;
    [SerializeField] private string openLidMessage = "Nhấn [E] để mở nắp hòm";
    [SerializeField] private AudioSource lidOpenAudio;

    private bool isAdjusting = false;
    private bool isUnlocked = false;
    private bool isUnlockedWaitingLid = false;
    private bool isLidOpen = false;
    private Quaternion lidClosedRot;
    private Quaternion lidTargetRot;

    private int currentDigitIndex = 0;
    private int[] enteredDigits;

    // ===== CAMERA STATE =====
    private bool isFlyingDown = false;
    private bool isFlyingUp = false;
    private Vector3 camHomePosition;
    private Quaternion camHomeRotation;
    private Transform camHomeParent;

    private Vector3 flyTargetPosition;
    private Quaternion flyTargetRotation;

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(ChapterIndex))
            return "";

        if (isUnlocked && !isUnlockedWaitingLid && !isLidOpen) return "";
        if (isUnlockedWaitingLid && !isLidOpen) return openLidMessage;
        if (isAdjusting) return "";
        return interactMessage;
    }

    public void Interact()
    {
        if (!MiniGameFlowManager.IsChapterActive(ChapterIndex)) return;

        if (!isUnlocked && !isAdjusting)
        {
            EnterAdjustMode();
            return;
        }

        if (isUnlockedWaitingLid && !isLidOpen)
        {
            OpenLid();
            return;
        }
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);
        if (windKeyReward != null) windKeyReward.SetActive(false);

        if (gameCamera == null) gameCamera = Camera.main;
        if (lockBody == null) lockBody = transform;

        enteredDigits = new int[dials.Length];

        if (lidHinge != null)
        {
            lidClosedRot = lidHinge.localRotation;
            lidTargetRot = lidClosedRot;
        }

        if (cameraAnchor == null)
            Debug.LogWarning("[CombinationLock] Chưa gán 'Camera Anchor'. Đang dùng cách tính tự động (có thể lệch). " +
                             "Nên tạo object mốc và kéo vào ô Camera Anchor.");
    }

    private Vector3 GetLookPoint()
    {
        if (lockBody == null) return transform.position;
        return lockBody.position + lookPointOffset;
    }

    private Vector3 GetCameraTargetPosition()
    {
        // Uu tien diem moc dat tay
        if (cameraAnchor != null) return cameraAnchor.position;

        Vector3 lookPoint = GetLookPoint();

        Vector3 backDir;
        if (useLockForwardDirection && lockBody != null)
        {
            backDir = lockBody.forward;
        }
        else
        {
            Camera cam = gameCamera != null ? gameCamera : Camera.main;
            if (cam == null) return lookPoint;

            Vector3 d = cam.transform.position - lookPoint;
            d.y = 0f;
            backDir = d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.back;
        }

        Vector3 sideDir = Vector3.Cross(Vector3.up, backDir).normalized;

        return lookPoint
             + backDir * viewDistance
             + Vector3.up * viewHeight
             + sideDir * viewSideOffset;
    }

    private Quaternion GetCameraTargetRotation(Vector3 fromPosition)
    {
        // Uu tien goc cua diem moc
        if (cameraAnchor != null) return cameraAnchor.rotation;

        Vector3 dir = GetLookPoint() - fromPosition;
        if (dir.sqrMagnitude < 0.0001f) return Quaternion.identity;

        Quaternion baseRot = Quaternion.LookRotation(dir, Vector3.up);
        return baseRot * Quaternion.Euler(extraPitch, extraYaw, extraRoll);
    }

    private void Update()
    {
        if (lidHinge != null)
            lidHinge.localRotation = Quaternion.Slerp(lidHinge.localRotation, lidTargetRot, Time.deltaTime * lidSpeed);

        // ===== CAMERA BAY XUONG =====
        if (isFlyingDown && gameCamera != null)
        {
            Transform ct = gameCamera.transform;

            ct.position = Vector3.Lerp(ct.position, flyTargetPosition, Time.deltaTime * flyDownSpeed);
            ct.rotation = Quaternion.Slerp(ct.rotation, flyTargetRotation, Time.deltaTime * flyDownSpeed);
        }
        // ===== CAMERA BAY LEN TRA VE =====
        else if (isFlyingUp && gameCamera != null)
        {
            Transform ct = gameCamera.transform;

            ct.position = Vector3.Lerp(ct.position, camHomePosition, Time.deltaTime * flyUpSpeed);
            ct.rotation = Quaternion.Slerp(ct.rotation, camHomeRotation, Time.deltaTime * flyUpSpeed);

            bool posDone = Vector3.Distance(ct.position, camHomePosition) < 0.02f;
            bool rotDone = Quaternion.Angle(ct.rotation, camHomeRotation) < 0.5f;

            if (posDone && rotDone)
            {
                ct.position = camHomePosition;
                ct.rotation = camHomeRotation;

                if (camHomeParent != null)
                    ct.SetParent(camHomeParent, true);

                isFlyingUp = false;
                RestorePlayerControl();
            }
        }

        if (!MiniGameFlowManager.CanContinue(this, ChapterIndex))
        {
            return;
        }

        if (isAdjusting)
        {
            HandleNumberInput();

            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
                DeleteLastDigit();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitAdjustMode();
        }
    }

    private void EnterAdjustMode()
    {
        GameObject modalRoot = inputDisplay != null ? inputDisplay.gameObject : null;

        if (!MiniGameFlowManager.TryOpen(this, modalRoot, ChapterIndex)) return;

        isAdjusting = true;
        currentDigitIndex = 0;

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (inputDisplay != null) inputDisplay.gameObject.SetActive(true);
        UpdateInputDisplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
        else
        {
            autoFoundPlayer = FindFirstObjectByType<PlayerController>();
            if (autoFoundPlayer != null) autoFoundPlayer.enabled = false;
            else Debug.LogWarning("[CombinationLock] Không tìm thấy PlayerController để khóa camera!");
        }

        if (gameCamera != null)
        {
            Transform ct = gameCamera.transform;

            camHomeParent = ct.parent;
            camHomePosition = ct.position;
            camHomeRotation = ct.rotation;

            ct.SetParent(null, true);

            // TINH DIEM DICH 1 LAN DUY NHAT
            if (cameraAnchor != null)
            {
                // Uu tien dung diem moc dat tay trong Scene
                flyTargetPosition = cameraAnchor.position;
                flyTargetRotation = cameraAnchor.rotation;
            }
            else
            {
                flyTargetPosition = GetCameraTargetPosition();
                flyTargetRotation = GetCameraTargetRotation(flyTargetPosition);
            }
        }

        foreach (LockDial d in dials)
            if (d != null) d.CaptureBase();

        isFlyingDown = true;
        isFlyingUp = false;

        Debug.Log("Camera đang bay xuống ổ khóa. Gõ phím số để nhập mật mã.");
    }

    private void HandleNumberInput()
    {
        if (Keyboard.current == null) return;
        if (currentDigitIndex >= dials.Length) return;

        int pressed = GetPressedDigit();
        if (pressed < 0) return;

        if (currentDigitIndex < correctCombination.Count
            && pressed == correctCombination[currentDigitIndex])
        {
            enteredDigits[currentDigitIndex] = pressed;
            if (dials[currentDigitIndex] != null)
                dials[currentDigitIndex].SetNumber(pressed);

            currentDigitIndex++;
            UpdateInputDisplay();

            if (currentDigitIndex >= dials.Length)
                Unlock();
        }
        else
        {
            if (wrongAudio != null) wrongAudio.Play();
            Debug.Log($"Sai số ở vòng {currentDigitIndex + 1}. Thử lại.");
        }
    }

    private int GetPressedDigit()
    {
        var k = Keyboard.current;
        if (k.digit0Key.wasPressedThisFrame || k.numpad0Key.wasPressedThisFrame) return 0;
        if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) return 1;
        if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) return 2;
        if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) return 3;
        if (k.digit4Key.wasPressedThisFrame || k.numpad4Key.wasPressedThisFrame) return 4;
        if (k.digit5Key.wasPressedThisFrame || k.numpad5Key.wasPressedThisFrame) return 5;
        if (k.digit6Key.wasPressedThisFrame || k.numpad6Key.wasPressedThisFrame) return 6;
        if (k.digit7Key.wasPressedThisFrame || k.numpad7Key.wasPressedThisFrame) return 7;
        if (k.digit8Key.wasPressedThisFrame || k.numpad8Key.wasPressedThisFrame) return 8;
        if (k.digit9Key.wasPressedThisFrame || k.numpad9Key.wasPressedThisFrame) return 9;
        return -1;
    }

    private void DeleteLastDigit()
    {
        if (currentDigitIndex <= 0) return;
        currentDigitIndex--;
        enteredDigits[currentDigitIndex] = 0;
        if (dials[currentDigitIndex] != null)
            dials[currentDigitIndex].SetNumber(0);
        UpdateInputDisplay();
    }

    private void UpdateInputDisplay()
    {
        if (inputDisplay == null) return;
        string s = "";
        for (int i = 0; i < dials.Length; i++)
        {
            if (i < currentDigitIndex) s += enteredDigits[i] + " ";
            else s += "_ ";
        }
        inputDisplay.text = s.Trim();
    }

    public void CheckCombination()
    {
        for (int i = 0; i < dials.Length; i++)
        {
            if (i >= correctCombination.Count) return;
            if (enteredDigits[i] != correctCombination[i])
            {
                WrongCode();
                return;
            }
        }
        Unlock();
    }

    private void WrongCode()
    {
        Debug.Log("Sai mật mã! Thử lại.");
        if (wrongAudio != null) wrongAudio.Play();

        currentDigitIndex = 0;
        for (int i = 0; i < dials.Length; i++)
        {
            enteredDigits[i] = 0;
            if (dials[i] != null) dials[i].ResetDial();
        }
        UpdateInputDisplay();
    }

    private void ExitAdjustMode()
    {
        MiniGameFlowManager.Close(this, inputDisplay != null ? inputDisplay.gameObject : null);

        isAdjusting = false;

        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);
        if (promptText != null) promptText.text = interactMessage;

        StartCameraReturn();
        Debug.Log("Thoát chế độ chỉnh khóa.");
    }

    private void StartCameraReturn()
    {
        isFlyingDown = false;
        isFlyingUp = true;
    }

    private void RestorePlayerControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null) playerInput.enabled = true;
        else if (autoFoundPlayer != null) autoFoundPlayer.enabled = true;

        Debug.Log("[CombinationLock] Camera đã bay về vị trí cũ. Trả lại điều khiển cho Player.");
    }

    private void Unlock()
    {
        MiniGameFlowManager.Close(this, inputDisplay != null ? inputDisplay.gameObject : null);

        isUnlocked = true;
        Debug.Log("Đúng tổ hợp! Khóa số đã mở.");

        if (unlockAudio != null) unlockAudio.Play();

        isAdjusting = false;

        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);

        StartCameraReturn();

        isUnlockedWaitingLid = true;
        if (promptText != null)
        {
            promptText.text = openLidMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OpenLid()
    {
        isLidOpen = true;
        Debug.Log("Nắp hòm bật lên! Lộ ra Chìa Vặn.");

        if (lidOpenAudio != null) lidOpenAudio.Play();

        if (lidHinge != null)
            lidTargetRot = lidClosedRot * Quaternion.AngleAxis(lidOpenAngle, lidRotateAxis.normalized);

        if (promptText != null) promptText.gameObject.SetActive(false);

        if (windKeyReward != null) windKeyReward.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        if (!showAimGizmo) return;

        // Vi tri camera se dung
        Vector3 camPos = GetCameraTargetPosition();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(camPos, 0.08f);

        if (cameraAnchor != null)
        {
            // Ve huong nhin cua diem moc
            Gizmos.color = new Color(0.3f, 1f, 1f, 0.8f);
            Gizmos.DrawRay(cameraAnchor.position, cameraAnchor.forward * 1.5f);
        }
        else if (lockBody != null)
        {
            Vector3 lookPoint = GetLookPoint();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lookPoint, 0.06f);
            Gizmos.color = new Color(0.3f, 1f, 1f, 0.7f);
            Gizmos.DrawLine(camPos, lookPoint);
        }
    }
}
