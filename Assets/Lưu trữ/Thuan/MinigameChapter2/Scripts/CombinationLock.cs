using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

// Quản lý tổng khóa số 3D (nhập bằng phím số).
// Nhấn E -> ổ khóa bay lên cận cảnh -> gõ phím số 0-9 để nhập mật mã.
// Mỗi số gõ vào: vòng tương ứng xoay mượt tới số đó. Đủ 4 số tự kiểm tra.
public class CombinationLock : MonoBehaviour
{
    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để chỉnh khóa số";

    [Header("Hiển thị số đang nhập (tùy chọn)")]
    [Tooltip("Text hiện dãy số đang gõ, ví dụ '0 4 _ _'")]
    [SerializeField] private TextMeshProUGUI inputDisplay;

    [Header("4 vòng số")]
    [Tooltip("Kéo 4 Object vòng số (có script LockDial) theo thứ tự trái->phải")]
    [SerializeField] private LockDial[] dials;

    [Header("Tổ hợp đúng (theo thứ tự vòng)")]
    [SerializeField] private List<int> correctCombination = new List<int>() { 0, 4, 5, 1 };

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;

    [Header("Khóa di chuyển khi chỉnh")]
    [SerializeField] private MonoBehaviour playerInput;

    [Header("Ổ khóa bay lên cận cảnh")]
    [SerializeField] private Transform lockBody;
    [SerializeField] private float closeUpDistance = 0.3f;
    [SerializeField] private float closeUpYOffset = 0f;
    [SerializeField] private Vector3 closeUpEuler = new Vector3(0, 180, 0);
    [SerializeField] private float moveSpeed = 8f;

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource unlockAudio;
    [SerializeField] private AudioSource wrongAudio;

    [Header("Chìa vặn hiện ra sau khi mở khóa")]
    [SerializeField] private GameObject windKeyReward;

    [Header("Nắp hòm xoay lên (sau khi mở khóa)")]
    [Tooltip("Kéo Object LidHinge (bản lề chứa nắp) vào đây")]
    [SerializeField] private Transform lidHinge;
    [Tooltip("Góc mở nắp (độ). Thử -100 hoặc 100 tùy hướng")]
    [SerializeField] private float lidOpenAngle = -100f;
    [Tooltip("Trục xoay nắp")]
    [SerializeField] private Vector3 lidRotateAxis = new Vector3(1, 0, 0);
    [SerializeField] private float lidSpeed = 3f;
    [Tooltip("Chữ gợi ý mở nắp")]
    [SerializeField] private string openLidMessage = "Nhấn [E] để mở nắp hòm";
    [SerializeField] private AudioSource lidOpenAudio;

    private bool isPlayerInside = false;
    private bool isAdjusting = false;
    private bool isUnlocked = false;
    private bool isMoving = false;
    private bool isUnlockedWaitingLid = false; // đã mở khóa, chờ nhấn E mở nắp
    private bool isLidOpen = false;
    private Quaternion lidClosedRot;
    private Quaternion lidTargetRot;

    private int currentDigitIndex = 0;       // đang nhập vòng thứ mấy
    private int[] enteredDigits;              // các số đã nhập

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private Transform homeParent;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool returningHome = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);
        if (windKeyReward != null) windKeyReward.SetActive(false);

        if (gameCamera == null) gameCamera = Camera.main;
        if (lockBody == null) lockBody = transform;

        enteredDigits = new int[dials.Length];

        homePosition = lockBody.position;
        homeRotation = lockBody.rotation;
        homeParent = lockBody.parent;

        if (lidHinge != null)
        {
            lidClosedRot = lidHinge.localRotation;
            lidTargetRot = lidClosedRot;
        }
    }

    private void Update()
    {
        // Xoay nắp hòm mượt về góc đích
        if (lidHinge != null)
            lidHinge.localRotation = Quaternion.Slerp(lidHinge.localRotation, lidTargetRot, Time.deltaTime * lidSpeed);

        // Sau khi mở khóa: nhấn E để mở nắp hòm (bỏ điều kiện isPlayerInside vì
        // người chơi chắc chắn đang đứng đó vừa giải khóa xong)
        if (isUnlockedWaitingLid && !isLidOpen
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("[CombinationLock] Nhấn E mở nắp hòm.");
            OpenLid();
        }

        // Di chuyển ổ khóa mượt
        if (isMoving)
        {
            lockBody.position = Vector3.Lerp(lockBody.position, targetPosition, Time.deltaTime * moveSpeed);
            lockBody.rotation = Quaternion.Slerp(lockBody.rotation, targetRotation, Time.deltaTime * moveSpeed);

            if (Vector3.Distance(lockBody.position, targetPosition) < 0.01f)
            {
                lockBody.position = targetPosition;
                lockBody.rotation = targetRotation;
                isMoving = false;
                if (returningHome)
                {
                    lockBody.SetParent(homeParent);
                    returningHome = false;
                }
            }
        }

        // Vào chế độ chỉnh
        if (isPlayerInside && !isUnlocked && !isAdjusting
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterAdjustMode();
        }

        if (isAdjusting)
        {
            HandleNumberInput();

            // Backspace: xóa số vừa nhập
            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
                DeleteLastDigit();

            // Esc: thoát
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitAdjustMode();
        }
    }

    private void EnterAdjustMode()
    {
        isAdjusting = true;
        currentDigitIndex = 0;

        // Tắt chữ hướng dẫn cho gọn, bật ô hiển thị số nhập
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (inputDisplay != null) inputDisplay.gameObject.SetActive(true);
        UpdateInputDisplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null) playerInput.enabled = false;

        MoveToCloseUp();
        Debug.Log("Vào chế độ chỉnh khóa số. Gõ phím số để nhập mật mã.");
    }

    private void HandleNumberInput()
    {
        if (Keyboard.current == null) return;
        if (currentDigitIndex >= dials.Length) return;

        int pressed = GetPressedDigit();
        if (pressed < 0) return; // không có phím số nào vừa bấm

        // Chỉ chấp nhận nếu gõ ĐÚNG số của vòng hiện tại
        if (currentDigitIndex < correctCombination.Count
            && pressed == correctCombination[currentDigitIndex])
        {
            // Đúng -> vòng xoay tới số, chuyển sang vòng kế tiếp
            enteredDigits[currentDigitIndex] = pressed;
            if (dials[currentDigitIndex] != null)
                dials[currentDigitIndex].SetNumber(pressed);

            currentDigitIndex++;
            UpdateInputDisplay();

            // Gõ đúng hết các vòng -> mở khóa
            if (currentDigitIndex >= dials.Length)
                Unlock();
        }
        else
        {
            // Sai -> kêu tiếng, đứng yên (không qua vòng)
            if (wrongAudio != null) wrongAudio.Play();
            Debug.Log($"Sai số ở vòng {currentDigitIndex + 1}. Thử lại.");
        }
    }

    // Trả về số 0-9 vừa bấm (cả phím hàng trên lẫn numpad), hoặc -1
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

        // Reset cho nhập lại
        currentDigitIndex = 0;
        for (int i = 0; i < dials.Length; i++)
        {
            enteredDigits[i] = 0;
            if (dials[i] != null) dials[i].ResetDial();
        }
        UpdateInputDisplay();
    }

    private void MoveToCloseUp()
    {
        if (gameCamera == null) return;
        lockBody.SetParent(null);

        // Sau khi đổi parent, ghi lại góc gốc các vòng cho chuẩn
        foreach (LockDial d in dials)
        {
            if (d != null) d.CaptureBase();
        }

        Transform cam = gameCamera.transform;
        targetPosition = cam.position + cam.forward * closeUpDistance + cam.up * closeUpYOffset;
        targetRotation = Quaternion.LookRotation(cam.forward, cam.up) * Quaternion.Euler(closeUpEuler);

        isMoving = true;
        returningHome = false;
    }

    private void ExitAdjustMode()
    {
        isAdjusting = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInput != null) playerInput.enabled = true;

        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);
        if (promptText != null) promptText.text = interactMessage;

        ReturnHome();
        Debug.Log("Thoát chế độ chỉnh khóa.");
    }

    private void ReturnHome()
    {
        targetPosition = homePosition;
        targetRotation = homeRotation;
        isMoving = true;
        returningHome = true;
    }

    private void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Đúng tổ hợp! Khóa số đã mở.");

        if (unlockAudio != null) unlockAudio.Play();

        isAdjusting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInput != null) playerInput.enabled = true;

        if (inputDisplay != null) inputDisplay.gameObject.SetActive(false);

        ReturnHome();

        // Chưa hiện chìa. Chuyển sang trạng thái chờ người chơi nhấn E mở nắp.
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

        // Giờ mới hiện chìa vặn để nhặt (WindKeyCollect lo việc nhặt)
        if (windKeyReward != null) windKeyReward.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isUnlocked)
        {
            isPlayerInside = true;
            if (promptText != null && !isAdjusting)
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
            if (!isAdjusting && promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}