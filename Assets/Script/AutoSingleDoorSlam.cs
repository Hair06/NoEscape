using UnityEngine;

public class AutoSingleDoorSlam : MonoBehaviour
{
    [Header("Cấu hình Cánh Cửa")]
    [Tooltip("Kéo 1 cánh cửa muốn sập vào đây")]
    [SerializeField] private Transform doorTransform;

    [Header("Góc xoay khi ĐÓNG HOÀN TOÀN")]
    [Tooltip("Nhìn số độ trục Y lúc cửa đóng trong Inspector rồi điền vào đây")]
    [SerializeField] private float closedRotationY = 0f;

    [Header("Cấu hình Tốc độ")]
    [Tooltip("Tốc độ sập cửa (Số càng cao đóng càng nhanh)")]
    [SerializeField] private float slamSpeed = 10f;

    [Header("Âm thanh (Không bắt buộc)")]
    [SerializeField] private AudioSource slamAudio;

    private bool hasSlammed = false;
    private bool isClosing = false;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu Collider bước vào vùng này là Player và chưa bị sập bao giờ
        if (other.CompareTag("Player") && !hasSlammed)
        {
            hasSlammed = true;
            isClosing = true;

            // Bật âm thanh đập cửa nếu có gắn
            if (slamAudio != null)
            {
                slamAudio.Play();
            }

            Debug.Log("🚪 RẦM! Cửa đơn đã tự động sập lại phía sau Player!");
        }
    }

    private void Update()
    {
        if (isClosing && doorTransform != null)
        {
            // Lấy góc xoay hiện tại của cửa dưới dạng Vector3
            Vector3 currentLocalAngles = doorTransform.localEulerAngles;

            // Dịch chuyển mượt mà góc Y hiện tại về góc Y khi đóng
            float newAngleY = Mathf.MoveTowardsAngle(currentLocalAngles.y, closedRotationY, Time.deltaTime * slamSpeed * 20f);

            // Cập nhật lại góc xoay mới cho cửa
            doorTransform.localRotation = Quaternion.Euler(currentLocalAngles.x, newAngleY, currentLocalAngles.z);

            // Nếu góc xoay đã khít với góc đóng thì dừng Update
            if (Mathf.Abs(Mathf.DeltaAngle(newAngleY, closedRotationY)) < 0.1f)
            {
                doorTransform.localRotation = Quaternion.Euler(currentLocalAngles.x, closedRotationY, currentLocalAngles.z);
                isClosing = false;
            }
        }
    }
}