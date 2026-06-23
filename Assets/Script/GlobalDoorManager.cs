using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class GlobalDoorManager : MonoBehaviour
{
    public static GlobalDoorManager Instance;

    [Header("Tham chiếu UI dùng chung")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultMessage = "Nhấn [E] để mở cửa";

    [Header("DANH SÁCH TẤT CẢ CÁC CỬA TRONG MAP (Đổ xuống)")]
    [Tooltip("Ấn dấu + để thêm ô và kéo thả các Object có gắn DoorData vào đây")]
    [SerializeField] private List<DoorData> allDoors = new List<DoorData>();

    private DoorData currentActiveDoor = null; // Cửa mà người chơi đang đứng gần nhất

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 1. Nhận diện phím E của hệ thống Input mới
        if (currentActiveDoor != null && !currentActiveDoor.isOpened && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenSpecificDoor(currentActiveDoor);
        }

        // 2. Duyệt qua danh sách để tự động xoay mượt mà NHỮNG CỬA NÀO ĐÃ ĐƯỢC MỞ
        // Cửa nào chưa mở hoặc đã mở xong thì bỏ qua, hoàn toàn độc lập với nhau
        foreach (DoorData door in allDoors)
        {
            if (door != null && door.isOpened)
            {
                // Chỉ xoay đúng cánh cửa có biến isOpened = true
                door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, door.targetRotation, Time.deltaTime * door.openSpeed);

                // Nếu sát góc đích thì khóa vị trí lại để tối ưu hóa hiệu năng
                if (Quaternion.Angle(door.transform.localRotation, door.targetRotation) < 0.1f)
                {
                    door.transform.localRotation = door.targetRotation;
                }
            }
        }
    }

    // Hàm mở một cánh cửa cụ thể
    private void OpenSpecificDoor(DoorData door)
    {
        door.isOpened = true;

        // Phát âm thanh riêng của cánh cửa đó
        if (door.doorOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(door.doorOpenSound, door.transform.position);
        }

        // Tắt BoxCollider vùng Trigger của riêng cánh cửa đó để không bắt va chạm nữa
        Collider doorCollider = door.GetComponent<Collider>();
        if (doorCollider != null) doorCollider.enabled = false;

        // Xóa trạng thái cửa kích hoạt hiện tại và ẩn chữ UI
        if (promptText != null) promptText.gameObject.SetActive(false);
        currentActiveDoor = null;
    }

    // Hiển thị UI chữ E khi đứng gần cửa
    public void ShowPrompt(DoorData door)
    {
        currentActiveDoor = door;
        if (promptText != null)
        {
            promptText.text = defaultMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    // Ẩn UI chữ E khi đi xa cửa
    public void HidePrompt(DoorData door)
    {
        if (currentActiveDoor == door)
        {
            currentActiveDoor = null;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}