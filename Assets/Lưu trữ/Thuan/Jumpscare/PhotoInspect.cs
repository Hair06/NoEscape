using UnityEngine;
using UnityEngine.InputSystem;
using ElmanGameDevTools.PlayerSystem;

// Cầm lá thư lên xem cận cảnh, kéo chuột để xoay tự do.
// Bấm 1 lần nữa để cất -> lá thư biến mất vĩnh viễn.
// Gắn vào một object luôn bật (ví dụ Canvas hoặc GameObject rỗng).
public class PhotoInspect : MonoBehaviour
{
    [Header("Tên vật phẩm trong túi")]
    [Tooltip("Phải trùng tên lúc nhặt, ví dụ: LaThu")]
    [SerializeField] private string itemName = "LaThu";

    [Header("Model lá thư cầm trên tay")]
    [Tooltip("Kéo object LaThuModel vào đây - để tắt sẵn")]
    [SerializeField] private GameObject photoObject;

    [Header("Vị trí cầm trước mặt")]
    [SerializeField] private float distance = 0.45f;
    [SerializeField] private float yOffset = 0f;
    [Tooltip("Góc xoay ban đầu, để mặt trước quay về người chơi")]
    [SerializeField] private Vector3 startEuler = new Vector3(0f, 180f, 0f);

    [Header("Tốc độ xoay khi kéo chuột")]
    [SerializeField] private float rotateSpeed = 0.35f;

    [Header("Xóa lá thư sau khi xem xong")]
    [Tooltip("Bật: cất xuống là mất luôn. Tắt: giữ lại xem nhiều lần.")]
    [SerializeField] private bool destroyAfterViewing = true;

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource pickupAudio;
    [SerializeField] private AudioSource discardAudio;

    private bool isInspecting = false;
    private bool isUsedUp = false;
    private Camera cam;
    private PlayerController playerCtrl;

    private void Start()
    {
        if (photoObject != null) photoObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Bấm phím 1 để cầm lên / cất xuống
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (isInspecting) ExitInspect();
            else TryEnterInspect();
        }

        // Bấm Esc cũng cất xuống
        if (isInspecting && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitInspect();
        }

        if (isInspecting) HandleRotate();
    }

    private void TryEnterInspect()
    {
        // Đã xem xong và bỏ đi rồi thì không cầm lại được nữa
        if (isUsedUp) return;

        // Chưa nhặt lá thư thì không làm gì
        if (PlayerInventory.Count(itemName) <= 0) return;

        if (photoObject == null)
        {
            Debug.LogWarning("[PhotoInspect] Chưa gán Photo Object!");
            return;
        }

        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[PhotoInspect] Không tìm thấy Main Camera!");
            return;
        }

        isInspecting = true;

        // Khóa di chuyển + camera của Player
        playerCtrl = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null) playerCtrl.enabled = false;

        // Đưa lá thư ra trước mặt camera
        photoObject.SetActive(true);
        photoObject.transform.SetParent(null);
        photoObject.transform.position =
            cam.transform.position
            + cam.transform.forward * distance
            + cam.transform.up * yOffset;
        photoObject.transform.rotation =
            Quaternion.LookRotation(cam.transform.forward)
            * Quaternion.Euler(startEuler);

        if (pickupAudio != null) pickupAudio.Play();

        Debug.Log("[PhotoInspect] Đang xem lá thư. Kéo chuột trái để xoay, bấm 1 để cất.");
    }

    private void HandleRotate()
    {
        if (Mouse.current == null || photoObject == null) return;

        // Giữ chuột trái và kéo để xoay lá thư
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            photoObject.transform.Rotate(
                cam.transform.up,
                -delta.x * rotateSpeed,
                Space.World
            );

            photoObject.transform.Rotate(
                cam.transform.right,
                delta.y * rotateSpeed,
                Space.World
            );
        }
    }

    private void ExitInspect()
    {
        isInspecting = false;

        // Trả lại quyền điều khiển cho Player
        if (playerCtrl != null) playerCtrl.enabled = true;

        if (destroyAfterViewing)
        {
            // Xóa khỏi túi đồ và hủy hẳn model
            isUsedUp = true;
            PlayerInventory.RemoveAll(itemName);

            if (discardAudio != null) discardAudio.Play();

            if (photoObject != null) Destroy(photoObject);

            Debug.Log("[PhotoInspect] Đã bỏ lá thư đi. Không xem lại được nữa.");
        }
        else
        {
            // Chỉ cất đi, vẫn giữ trong túi
            if (photoObject != null) photoObject.SetActive(false);
            Debug.Log("[PhotoInspect] Đã cất lá thư.");
        }
    }
}