using UnityEngine;
using UnityEngine.InputSystem; 

public class FPSCameraFollow : MonoBehaviour
{
    [Header("Kéo object Camera_Target vào đây")]
    public Transform target; 
    
    [Header("Độ mượt mà khi xoay chuột")]
    public float mouseSensitivity = 0.05f; 

    [Header("Cấu hình góc nhìn ban đầu")]
    [Tooltip("Nhập 45 để nhìn xéo sang phải, hoặc -45 để nhìn xéo sang trái")]
    public float startingYawAngle = 45f; 
    
    [Tooltip("Góc cúi xuống mặc định khi vào game. Nhập số dương để cúi xuống (Ví dụ: 45)")]
    public float startingPitchAngle = 45f; // Đặt 45 độ ở đây để vừa vào game mắt sẽ nhìn chúc xuống đất luôn

    [Header("Giới hạn góc nhìn lên/xuống khi chơi")]
    [Tooltip("Góc ngước lên trời tối đa (Số âm vì ngược chiều trục X của Unity)")]
    public float maxUpperAngle = -60f;
    [Tooltip("Góc cúi xuống đất tối đa (Số dương)")]
    public float maxLowerAngle = 80f; 

    private float xRotation = 0f;
    private float yRotation = 0f; 

    private void Start()
    {
        // Khóa con trỏ chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;

        // Thiết lập góc nhìn xiên (Y) và góc cúi xuống (X) ban đầu
        yRotation = startingYawAngle;
        xRotation = startingPitchAngle; // Gán góc cúi xuống mặc định

        // Áp dụng ngay lập tức khi bắt đầu game
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void Update()
    {
        if (target == null) return;

        // Giữ vị trí Camera luôn dính chặt vào điểm mốc mắt cố định
        transform.position = target.position;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            float mouseX = mouseDelta.x;
            float mouseY = mouseDelta.y;

            // Tính toán góc ngước lên/cúi xuống dựa trên di chuyển chuột (Hệ thống mới delta dương khi chuột đi lên)
            // Trong góc quay Unity, cộng thêm vào X sẽ làm camera cúi xuống, trừ đi sẽ ngước lên
            xRotation -= mouseY;

            // Giới hạn góc nhìn để người chơi không quay vòng tròn camera theo chiều dọc
            // maxUpperAngle thường là số âm (ngước lên), maxLowerAngle là số dương (cúi xuống)
            xRotation = Mathf.Clamp(xRotation, maxUpperAngle, maxLowerAngle); 

            // Tính toán góc quay trái/phải (Trục Y)
            yRotation += mouseX;

            // Áp dụng góc xoay tổng hợp vào Camera
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            
            // Xoay toàn bộ thân Player theo chiều ngang của chuột
            target.root.Rotate(Vector3.up * mouseX);
        }
    }
}