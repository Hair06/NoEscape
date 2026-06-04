using UnityEngine;
using UnityEngine.InputSystem; 
using TMPro; // Bắt buộc phải có để nhận diện TextMesh Pro

public class DoubleDoorInteractable : MonoBehaviour
{
    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText; 
    [SerializeField] private string interactMessage = "Nhấn [E] để mở/đóng cửa";

    [Header("Door Components")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Rotation Settings")]
    public float openAngle = 90f;   // Góc mở cửa
    public float speed = 3f;        // Tốc độ mở cửa

    private bool isOpen = false;
    private bool isPlayerInside = false;
    
    // Lưu lại góc ban đầu của 2 cánh cửa
    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;
    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

    private void Start()
    {
        // Đảm bảo chữ gợi ý ẩn đi khi bắt đầu game
        if (promptText != null) promptText.gameObject.SetActive(false);

        // Ghi nhớ góc đóng ban đầu
        if (leftDoor != null) leftInitialRotation = leftDoor.localRotation;
        if (rightDoor != null) rightInitialRotation = rightDoor.localRotation;

        // Đặt mục tiêu ban đầu là đóng
        leftTargetRotation = leftInitialRotation;
        rightTargetRotation = rightInitialRotation;
    }

    private void Update()
    {
        // Xoay mượt mà cánh cửa về phía góc mục tiêu
        if (leftDoor != null)
            leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftTargetRotation, Time.deltaTime * speed);
        
        if (rightDoor != null)
            rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightTargetRotation, Time.deltaTime * speed);

        // Đọc phím E theo chuẩn Input System mới khi người chơi đang đứng trong vùng va chạm
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // Cánh trái xoay góc âm, cánh phải xoay góc dương (hoặc ngược lại tùy tâm góc của model)
            leftTargetRotation = leftInitialRotation * Quaternion.Euler(0, -openAngle, 0);
            rightTargetRotation = rightInitialRotation * Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            // Quay về góc đóng ban đầu
            leftTargetRotation = leftInitialRotation;
            rightTargetRotation = rightInitialRotation;
        }
    }

    // --- CƠ CHẾ PHÁT HIỆN VÀ BẬT CHỮ KHI ĐẾN GẦN ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (promptText != null) 
            {
                promptText.text = interactMessage; // Gán nội dung chữ
                promptText.gameObject.SetActive(true); // Bật chữ lên công khai
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) 
            {
                promptText.gameObject.SetActive(false); // Đi ra xa thì ẩn chữ đi
            }
        }
    }
}