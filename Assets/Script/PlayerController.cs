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
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude < 0.001f) return;

        if (useRigidbody && rb != null)
        {
            rb.MovePosition(rb.position + move * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }

        transform.forward = move.normalized;
    }
}