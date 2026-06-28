using UnityEngine;
using UnityEngine.InputSystem; // Sử dụng thư viện Input mới để đồng bộ với PlayerController
using ElmanGameDevTools.PlayerSystem; // Gọi namespace chứa PlayerController mới

public class ItemViewer : MonoBehaviour
{
    public static ItemViewer Instance;

    [Header("Giao diện UI Soi Vật Phẩm")]
    [SerializeField] private GameObject viewerUI; // Kéo ô Object "Viewer_UI" vào đây
    [SerializeField] private Transform inspectPoint; // Kéo ô Object trống "Inspect_Point" trước Camera vào đây

    [Header("Tốc độ xoay chuột")]
    [SerializeField] private float rotationSpeed = 150f;

    private GameObject currentInspectItem;
    private bool isInspecting = false;
    private ItemCollectible currentActualCollectible; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (viewerUI != null) viewerUI.SetActive(false);
    }

    private void Update()
    {
        if (!isInspecting) return;

        // 1. Giữ chuột trái và di chuyển chuột để xoay vật phẩm 3D tự do
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && currentInspectItem != null)
        {
            // Thay thế Input.GetAxis cũ bằng Mouse.current.delta của New Input System để không bị đơ
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * 0.05f; 
            float rotX = mouseDelta.x * rotationSpeed * Mathf.Deg2Rad;
            float rotY = mouseDelta.y * rotationSpeed * Mathf.Deg2Rad;

            // Thực hiện xoay theo trục ngang và trục dọc màn hình
            currentInspectItem.transform.Rotate(Vector3.up, -rotX, Space.World);
            currentInspectItem.transform.Rotate(Vector3.right, rotY, Space.World);
        }

        if (Keyboard.current != null)
        {
            // 2. Nhấn E để quyết định nhặt hẳn mảnh này vào túi
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                CollectCurrentItem();
            }

            // 3. Nhấn F hoặc ESC để hủy bỏ, trả mảnh ghép về vị trí cũ
            if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseInspect(false);
            }
        }
    }

    public void StartInspect(GameObject itemPrefab, ItemCollectible actualCollectible)
    {
        if (isInspecting) return;

        gameObject.SetActive(true);

        isInspecting = true;
        currentActualCollectible = actualCollectible;

        // === FIX LỖI 1: TẮT DI CHUYỂN VÀ KHÓA CHUỘT PLAYER GÓC NHÌN THỨ NHẤT ===
        if (Chapter1Manager.Instance != null && Chapter1Manager.Instance.playerController != null)
        {
            Chapter1Manager.Instance.playerController.enabled = false;   // Khóa di chuyển WASD
            Chapter1Manager.Instance.playerController.LockCameraOnly(); // Khóa cứng camera chính không bị lắc theo vật phẩm
        }

        // Mở khóa con trỏ chuột tự do để người chơi click giữ xoay vật phẩm giải đố
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (viewerUI != null) viewerUI.SetActive(true);

        // Tạo ra bản sao 3D trước camera phụ để ngắm nghía
        if (itemPrefab != null && inspectPoint != null)
        {
            currentInspectItem = Instantiate(itemPrefab, inspectPoint.position, inspectPoint.rotation, inspectPoint);
            
            // Vô hiệu hóa collider để không bị va chạm vật lý đè map bay lung tung
            if (currentInspectItem.TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        }
    }

    public void CloseInspect(bool isCollected)
    {
        isInspecting = false;

        if (currentInspectItem != null) Destroy(currentInspectItem);
        if (viewerUI != null) viewerUI.SetActive(false);

        if (!isCollected && currentActualCollectible != null)
        {
            currentActualCollectible.CancelInspect();
        }

        // === FIX LỖI 2: TRẢ LẠI TOÀN BỘ QUYỀN ĐIỀU KHIỂN CHO PLAYER GÓC NHÌN THỨ NHẤT ===
        if (Chapter1Manager.Instance != null && Chapter1Manager.Instance.playerController != null)
        {
            Chapter1Manager.Instance.playerController.UnlockCamera(); // Mở lại camera quay chuột chơi game
            Chapter1Manager.Instance.playerController.enabled = true;   // Mở lại di chuyển WASD
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameObject.SetActive(false);
    }

    private void CollectCurrentItem()
    {
        if (currentActualCollectible != null)
        {
            currentActualCollectible.ConfirmCollect(); 
        }
        CloseInspect(true);
    }
}