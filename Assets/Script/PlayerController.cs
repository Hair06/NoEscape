using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useRigidbody = false;
    [SerializeField] private Rigidbody rb;

    private Vector2 moveInput = Vector2.zero;
    private Animator anim; // Thêm animator để kích hoạt chuyển động từ câu hỏi trước

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Hàm này sẽ tự động được gọi bởi PlayerInput Component khi có thao tác di chuyển
    // Tên hàm bắt buộc phải là "On" + Tên Action (Ví dụ Action tên là Move -> OnMove)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Điều khiển Animation nếu có thiết lập tham số "isMoving" ở bài trước
        if (anim != null)
        {
            anim.SetBool("isMoving", moveInput.sqrMagnitude > 0.001f);
        }
    }

    private void Update()
    {
        // 1. Tạo vector hướng di chuyển từ Input mặt phẳng phẳng (X, Z)
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (useRigidbody && rb != null)
        {
            // 1. Chỉ ĐỌC vận tốc vật lý hiện tại ĐÚNG 1 LẦN duy nhất ra biến tạm
            Vector3 currentVelocity = rb.linearVelocity;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                // 2. Tính toán trên biến tạm
                Vector3 targetVelocity = moveDirection.normalized * moveSpeed;
                targetVelocity.y = currentVelocity.y; // Lấy trục Y từ biến tạm đã đọc phía trên

                // 3. GHI lại vào Rigidbody
                rb.linearVelocity = targetVelocity;

                transform.forward = moveDirection.normalized;
            }
            else
            {
                // Khi đứng yên: Giữ nguyên Y cũ từ biến tạm, triệt tiêu X và Z
                rb.linearVelocity = new Vector3(0f, currentVelocity.y, 0f);
            }
        }
        else
        {
            // Nếu không dùng Rigidbody (Chế độ di chuyển thường bằng Translate)
            if (moveDirection.sqrMagnitude < 0.001f) return;

            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
            transform.forward = moveDirection.normalized;
        }
    }
}