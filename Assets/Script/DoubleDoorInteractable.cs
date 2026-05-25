using UnityEngine;

public class DoubleDoorInteractable : MonoBehaviour, IInteractable
{
    public string promptMessage = "Nhấn E để Mở/Đóng cửa";

    [Header("Door Components")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Rotation Settings")]
    public float openAngle = 90f;   // Góc mở cửa
    public float speed = 3f;        // Tốc độ mở cửa

    private bool isOpen = false;
    
    // Lưu lại góc ban đầu của 2 cánh cửa
    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;
    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

    private void Start()
    {
        // Ghi nhớ góc đóng ban đầu
        leftInitialRotation = leftDoor.localRotation;
        rightInitialRotation = rightDoor.localRotation;

        // Đặt mục tiêu ban đầu là đóng
        leftTargetRotation = leftInitialRotation;
        rightTargetRotation = rightInitialRotation;
    }

    private void Update()
    {
        // Xoay mượt mà cánh cửa về phía góc mục tiêu
        leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftTargetRotation, Time.deltaTime * speed);
        rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightTargetRotation, Time.deltaTime * speed);
    }

    public string GetInteractPrompt()
    {
        return promptMessage;
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
}