using UnityEngine;

public class ItemViewer : MonoBehaviour
{
    public static ItemViewer Instance;

    [Header("Giao diện UI Soi Vật Phẩm")]
    [SerializeField] private GameObject viewerUI; // Kéo ô Object "Viewer_UI" (chứa chữ hướng dẫn nút bấm) vào đây
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

        // 1. Giữ chuột trái và di chuyển chuột để xoay vật phẩm 3D tự do các hướng để soi mặt trước/sau
        if (Input.GetMouseButton(0) && currentInspectItem != null)
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

            // Thực hiện xoay theo trục ngang và trục dọc màn hình
            currentInspectItem.transform.Rotate(Vector3.up, -rotX, Space.World);
            currentInspectItem.transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // 2. Nhấn E để quyết định nhặt hẳn mảnh này vào túi bỏ vào tiến trình game
        if (Input.GetKeyDown(KeyCode.E))
        {
            CollectCurrentItem();
        }

        // 3. Nhấn F hoặc ESC để hủy bỏ, trả mảnh ghép về vị trí cũ ngoài môi trường
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInspect(false);
        }
    }

    public void StartInspect(GameObject itemPrefab, ItemCollectible actualCollectible)
    {
        if (isInspecting) return;

        // TỰ ĐỘNG BẬT: Kích hoạt chính nó và Camera phụ lên ngay khi bắt đầu soi
        gameObject.SetActive(true);

        isInspecting = true;
        currentActualCollectible = actualCollectible;

        // Khóa di chuyển nhân vật Invector để không bị đi lung tung khi đang ngắm ảnh
        if (Chapter1Manager.Instance != null && Chapter1Manager.Instance.playerInputSystem != null)
        {
            Chapter1Manager.Instance.playerInputSystem.enabled = false;
        }

        // Mở khóa chuột để người chơi tương tác xoay
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

        // Nếu người chơi nhấn thoát chứ không nhặt, ta hiện lại mảnh đó ở ngoài map
        if (!isCollected && currentActualCollectible != null)
        {
            currentActualCollectible.CancelInspect();
        }

        // Trả lại quyền di chuyển cho Player Invector
        if (Chapter1Manager.Instance != null && Chapter1Manager.Instance.playerInputSystem != null)
        {
            Chapter1Manager.Instance.playerInputSystem.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // TỰ ĐỘNG TẮT: Ẩn trạm soi đi sau khi hoàn thành để trả lại màn hình góc nhìn gốc cho Player
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