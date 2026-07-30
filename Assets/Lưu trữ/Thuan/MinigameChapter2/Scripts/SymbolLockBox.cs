using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Hộp gỗ khóa ký hiệu (Bộ phận 4).
// Luồng: nhấn E mở bảng -> bấm đúng thứ tự ký hiệu -> nhấn E mở nắp hòm
//        -> nắp xoay lên -> chìa vặn hiện ra -> nhấn E nhặt (script WindKeyCollect lo).
public class SymbolLockBox : MonoBehaviour
{
    private const int ChapterIndex = 2;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để mở hộp khóa";
    [SerializeField] private string openLidMessage = "Nhấn [E] để mở nắp hòm";

    [Header("Bảng khóa ký hiệu")]
    [Tooltip("Kéo Panel UI chứa các nút ký hiệu vào đây")]
    [SerializeField] private GameObject lockPanel;

    [Header("Các nút ký hiệu")]
    [Tooltip("Kéo các Button ký hiệu vào đây theo thứ tự (nút 1, 2, 3, 4...)")]
    [SerializeField] private Button[] symbolButtons;

    [Header("Tự sắp nút bằng code")]
    [SerializeField] private bool autoArrangeButtons = true;
    [SerializeField] private float buttonSpacing = 180f;
    [SerializeField] private Vector2 buttonSize = new Vector2(140f, 140f);
    [SerializeField] private float rowYOffset = 0f;

    [Header("Thông báo trạng thái (tùy chọn)")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Khóa camera khi chơi")]
    [SerializeField] private MonoBehaviour cameraScript;

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lidOpenSound;

    [Header("Nắp hòm xoay lên")]
    [Tooltip("Kéo Object LidHinge (bản lề chứa nắp) vào đây")]
    [SerializeField] private Transform lidHinge;
    [Tooltip("Góc mở nắp (độ). Thử -90 hoặc 90 tùy hướng nắp")]
    [SerializeField] private float lidOpenAngle = -100f;
    [Tooltip("Trục xoay nắp: thường là X")]
    [SerializeField] private Vector3 lidRotateAxis = new Vector3(1, 0, 0);
    [SerializeField] private float lidSpeed = 3f;

    [Header("Chìa vặn hiện ra sau khi mở nắp")]
    [Tooltip("Kéo Object Chìa Vặn (tắt sẵn) - bật lên khi nắp đã mở")]
    [SerializeField] private GameObject windKeyReward;

    [Header("Đáp án (thứ tự nút đúng)")]
    [Tooltip("Số thứ tự các nút cần bấm. Ví dụ 3,1,4 = bấm nút thứ 3, rồi 1, rồi 4")]
    [SerializeField] private List<int> correctSequence = new List<int>() { 3, 1, 4 };

    // Trạng thái
    private enum State { Locked, Unlocked, LidOpen }
    private State state = State.Locked;

    private int currentStep = 0;
    private bool isPlayerInside = false;
    private bool isPanelOpen = false;

    private Quaternion lidClosedRot;
    private Quaternion lidTargetRot;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (lockPanel != null) lockPanel.SetActive(false);
        if (windKeyReward != null) windKeyReward.SetActive(false);

        for (int i = 0; i < symbolButtons.Length; i++)
        {
            int buttonNumber = i + 1;
            if (symbolButtons[i] != null)
                symbolButtons[i].onClick.AddListener(() => PressSymbol(buttonNumber));
        }

        if (autoArrangeButtons) ArrangeButtons();

        // Ghi nhớ góc đóng của nắp
        if (lidHinge != null)
        {
            lidClosedRot = lidHinge.localRotation;
            lidTargetRot = lidClosedRot;
        }
    }

    private void ArrangeButtons()
    {
        int n = symbolButtons.Length;
        if (n == 0) return;

        float totalWidth = (n - 1) * buttonSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < n; i++)
        {
            if (symbolButtons[i] == null) continue;
            RectTransform rt = symbolButtons[i].GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = buttonSize;
            rt.anchoredPosition = new Vector2(startX + i * buttonSpacing, rowYOffset);
        }
    }

    private void Update()
    {
        // Xoay nắp mượt về góc mục tiêu
        if (lidHinge != null)
            lidHinge.localRotation = Quaternion.Slerp(lidHinge.localRotation, lidTargetRot, Time.deltaTime * lidSpeed);

        if (!MiniGameFlowManager.IsChapterActive(ChapterIndex))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        if (!isPlayerInside) return;
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        // Nhấn E theo trạng thái hiện tại
        if (state == State.Locked && !isPanelOpen)
        {
            OpenLockPanel();
        }
        else if (state == State.Unlocked)
        {
            OpenLid();
        }
    }

    private void OpenLockPanel()
    {
        if (!MiniGameFlowManager.TryOpen(
                this,
                lockPanel,
                ChapterIndex))
        {
            return;
        }

        isPanelOpen = true;
        currentStep = 0;

        if (lockPanel != null) lockPanel.SetActive(true);
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (statusText != null) statusText.text = "Nhập đúng thứ tự ký hiệu...";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraScript != null) cameraScript.enabled = false;

        Debug.Log("Mở bảng khóa ký hiệu. Hãy bấm đúng thứ tự.");
    }

    private void PressSymbol(int number)
    {
        if (state != State.Locked ||
            !isPanelOpen ||
            !MiniGameFlowManager.CanContinue(this, ChapterIndex))
            return;

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (number == correctSequence[currentStep])
        {
            currentStep++;
            if (statusText != null) statusText.text = "Đúng... tiếp tục";

            if (currentStep >= correctSequence.Count)
            {
                Unlock();
            }
        }
        else
        {
            currentStep = 0;
            if (statusText != null) statusText.text = "Sai rồi... thử lại từ đầu";
            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);
        }
    }

    private void Unlock()
    {
        MiniGameFlowManager.Close(this, lockPanel);

        state = State.Unlocked;
        isPanelOpen = false;
        Debug.Log("Mở khóa đúng! Giờ nhấn E để mở nắp hòm.");

        if (audioSource != null && unlockSound != null)
            audioSource.PlayOneShot(unlockSound);

        // Đóng bảng, khóa chuột lại, trả camera
        if (lockPanel != null) lockPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraScript != null) cameraScript.enabled = true;

        // Hiện chữ "mở nắp hòm" (người chơi vẫn đứng gần)
        if (promptText != null)
        {
            promptText.text = openLidMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OpenLid()
    {
        state = State.LidOpen;
        Debug.Log("Nắp hòm bật lên! Lộ ra Chìa Vặn.");

        if (audioSource != null && lidOpenSound != null)
            audioSource.PlayOneShot(lidOpenSound);

        // Đặt góc đích cho nắp xoay lên
        if (lidHinge != null)
            lidTargetRot = lidClosedRot * Quaternion.AngleAxis(lidOpenAngle, lidRotateAxis.normalized);

        // Hiện chìa vặn để người chơi nhặt
        if (windKeyReward != null) windKeyReward.SetActive(true);

        // Ẩn chữ của hộp (việc nhặt chìa do WindKeyCollect lo)
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") &&
            MiniGameFlowManager.IsChapterActive(ChapterIndex))
        {
            isPlayerInside = true;
            if (promptText != null && !isPanelOpen && state != State.LidOpen)
            {
                promptText.text = (state == State.Locked) ? interactMessage : openLidMessage;
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
