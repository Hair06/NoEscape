using UnityEngine;

public class ProximityLightGlow : MonoBehaviour
{
    [Header("Cấu hình bán kính quét")]
    [Tooltip("Khoảng cách (mét) để Light bắt đầu sáng lên")]
    [SerializeField] private float activationRadius = 5f;

    [Header("Liên kết Component Light")]
    [Tooltip("Kéo Object Point Light con vào đây")]
    [SerializeField] private Light itemLight;

    private Transform playerTransform;
    private bool isLightActive = false;
    private bool isPickedUp = false; // Biến cờ: Đánh dấu nếu mảnh giấy đã bị nhặt

    void Start()
    {
        // Tìm Player qua Tag mặc định của Invector
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Mới vào game thì tắt Light đi để giấu mảnh giấy
        if (itemLight != null)
        {
            itemLight.enabled = false;
        }
    }

    void Update()
    {
        // Nếu đã nhặt mảnh giấy rồi thì không chạy logic tính khoảng cách nữa
        if (isPickedUp || playerTransform == null || itemLight == null) return;

        // Tính khoảng cách giữa Player và mảnh giấy
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Nếu Player đi vào bán kính set trước
        if (distanceToPlayer <= activationRadius)
        {
            if (!isLightActive)
            {
                itemLight.enabled = true; // Bật đèn
                isLightActive = true;
            }
        }
        else // Nếu Player đi ra ngoài bán kính
        {
            if (isLightActive)
            {
                itemLight.enabled = false; // Tắt đèn
                isLightActive = false;
            }
        }
    }

    /// <summary>
    /// HÀM MỚI BỔ SUNG: Gọi hàm này khi Player nhấn nhặt mảnh giấy thành công
    /// </summary>
    public void TurnOffLightOnPickup()
    {
        isPickedUp = true; // Khóa logic Update lại
        if (itemLight != null)
        {
            itemLight.enabled = false; // Tắt đèn ngay lập tức
        }
    }

    // Vẽ vòng tròn giả lập vùng kích hoạt trong cửa sổ Scene để bro dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}